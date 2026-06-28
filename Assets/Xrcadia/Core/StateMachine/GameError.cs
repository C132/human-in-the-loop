using System;

namespace Xrcadia.Core.StateMachine
{
    /// <summary>
    /// How bad a failure is. Recoverable failures pause the current state behind an error
    /// overlay and can resume; fatal failures abandon the current state and safe-exit to the
    /// Main Menu (XRC-99).
    /// </summary>
    public enum ErrorSeverity
    {
        Recoverable,
        Fatal,
    }

    /// <summary>A failure surfaced to the player, with enough context for the error screens.</summary>
    public sealed class GameError
    {
        public ErrorSeverity Severity;
        public string Title;
        public string Message;
        public string Source;       // e.g. "Tracking", "Save", "Load", "Transition"
        public Exception Exception; // underlying cause, if any (for logs)
    }

    /// <summary>
    /// Observable holder for the active error, shared between the FSM (writer) and the error
    /// screens (reader). Lives on <see cref="StateContext"/>, mirroring <c>LoadingProgress</c>.
    /// </summary>
    public sealed class ErrorContext
    {
        public GameError Current { get; private set; }

        public event Action<GameError> Changed;

        public void Set(GameError error)
        {
            Current = error;
            Changed?.Invoke(error);
        }

        public void Clear()
        {
            Current = null;
            Changed?.Invoke(null);
        }
    }

    /// <summary>
    /// Base for exceptions that carry their own severity, so throwing work (e.g. a save load)
    /// classifies itself — the transition service routes off <see cref="Severity"/> rather than
    /// hard-coding which exception types are fatal.
    /// </summary>
    public abstract class GameErrorException : Exception
    {
        protected GameErrorException(string source, string title, string message, Exception inner = null)
            : base(message, inner)
        {
            Source = source;
            Title = title;
        }

        public abstract ErrorSeverity Severity { get; }
        public string Source { get; }
        public string Title { get; }

        public GameError ToError() => new GameError
        {
            Severity = Severity,
            Title = Title,
            Message = Message,
            Source = Source,
            Exception = this,
        };
    }

    /// <summary>A failure the player can recover from (tracking loss, transient load failure).</summary>
    public sealed class RecoverableError : GameErrorException
    {
        public RecoverableError(string source, string title, string message, Exception inner = null)
            : base(source, title, message, inner) { }

        public override ErrorSeverity Severity => ErrorSeverity.Recoverable;
    }

    /// <summary>An unrecoverable failure (corrupt save, unsupported data) — safe-exit only.</summary>
    public class FatalError : GameErrorException
    {
        public FatalError(string source, string title, string message, Exception inner = null)
            : base(source, title, message, inner) { }

        public override ErrorSeverity Severity => ErrorSeverity.Fatal;
    }
}

namespace Xrcadia.Core.StateMachine
{
    public enum StateChangeKind
    {
        /// <summary>A base state replaced the previous base state.</summary>
        Replace,
        /// <summary>An overlay was pushed above the current state.</summary>
        Push,
        /// <summary>An overlay was popped, restoring the state beneath it.</summary>
        Pop,
    }

    /// <summary>
    /// Rich transition payload the UI router uses to decide whether to swap the base
    /// screen or manage the overlay stack. The simpler <c>Action&lt;GameState,GameState&gt;
    /// StateChanged</c> event is also emitted for audio/analytics consumers that only
    /// care about from/to.
    /// </summary>
    public readonly struct StateChange
    {
        public readonly GameState From;
        public readonly GameState To;
        public readonly StateChangeKind Kind;

        public StateChange(GameState from, GameState to, StateChangeKind kind)
        {
            From = from;
            To = to;
            Kind = kind;
        }

        public override string ToString() => $"{Kind}: {From} -> {To}";
    }
}

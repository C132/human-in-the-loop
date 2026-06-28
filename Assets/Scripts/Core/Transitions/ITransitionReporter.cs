namespace Xrcadia.Core.Transitions
{
    /// <summary>
    /// Handed to async transition work so it can report determinate progress to the
    /// Loading overlay (XRC-98). Implementations are expected to be safe to call from the
    /// main thread only.
    /// </summary>
    public interface ITransitionReporter
    {
        /// <param name="progress01">Normalized progress in the range [0, 1].</param>
        /// <param name="status">Optional human-readable status line; null leaves it unchanged.</param>
        void Report(float progress01, string status = null);
    }
}

using System;

namespace Xrcadia.Core.Transitions
{
    /// <summary>
    /// Observable progress model shared between the transition service (writer) and the
    /// Loading screen (reader). Lives on the <c>StateContext</c> so the UI layer can bind
    /// without a hard reference to the state machine internals.
    /// </summary>
    public sealed class LoadingProgress : ITransitionReporter
    {
        public float Value { get; private set; }
        public string Status { get; private set; } = string.Empty;

        /// <summary>Fired on every <see cref="Report"/> with (value, status).</summary>
        public event Action<float, string> Changed;

        public void Report(float progress01, string status = null)
        {
            Value = progress01 < 0f ? 0f : (progress01 > 1f ? 1f : progress01);
            if (status != null)
            {
                Status = status;
            }

            Changed?.Invoke(Value, Status);
        }

        public void Reset()
        {
            Value = 0f;
            Status = string.Empty;
            Changed?.Invoke(Value, Status);
        }
    }
}

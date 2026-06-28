using Xrcadia.Core.Services;

namespace TheLoop.App
{
    /// <summary>
    /// Pure phase cursor for MR setup (XRC-94): check the space, then either guide the player to
    /// fix it (and re-check) or confirm the anchor. UI-free so the branching is unit-testable
    /// without a rendered screen; <see cref="Screens.MRSetupScreen"/> drives it.
    /// </summary>
    public sealed class MRSetupFlow
    {
        public enum Phase
        {
            Checking,
            Guidance,       // space not ready — guide setup, then re-check
            ConfirmAnchor,  // space ready — confirm the tabletop and begin
        }

        public Phase Current { get; private set; } = Phase.Checking;
        public MRSpaceStatus Status { get; private set; }

        /// <summary>Begin (or re-run) a check — back to the Checking phase.</summary>
        public void BeginCheck() => Current = Phase.Checking;

        /// <summary>Apply a check result, routing to ConfirmAnchor when ready or Guidance when not.</summary>
        public void Apply(MRSpaceStatus status)
        {
            Status = status;
            Current = status.IsReady ? Phase.ConfirmAnchor : Phase.Guidance;
        }

        /// <summary>Only a confirmed, ready space may begin the Session.</summary>
        public bool CanBegin => Current == Phase.ConfirmAnchor;
    }
}

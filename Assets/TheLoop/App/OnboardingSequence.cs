namespace TheLoop.App
{
    /// <summary>
    /// Pure step cursor for the first-run onboarding sequence (XRC-90). Deliberately free of
    /// UI so the advance/clamp behaviour is unit-testable without a rendered screen; the
    /// <see cref="Screens.OnboardingScreen"/> owns presentation and drives this.
    /// </summary>
    public sealed class OnboardingSequence
    {
        private readonly int _count;

        public OnboardingSequence(int count) => _count = count < 1 ? 1 : count;

        public int Index { get; private set; }

        public bool IsFirst => Index == 0;
        public bool IsLast => Index >= _count - 1;

        public void Next()
        {
            if (!IsLast) Index++;
        }

        public void Back()
        {
            if (!IsFirst) Index--;
        }

        public void Reset() => Index = 0;
    }
}

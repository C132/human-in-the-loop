using NUnit.Framework;
using TheLoop.App;

namespace TheLoop.Tests.EditMode
{
    /// <summary>
    /// XRC-90 sequence logic. The once-only routing and skip→flag→Main Menu behaviour are
    /// covered by <see cref="LoadingFlowTests"/> (FirstLaunch_ShowsOnboardingOnce_ThenSkips,
    /// which drives OnboardingComplete — the same trigger Skip and Finish both fire). These
    /// cover the step cursor and that the placeholder content matches the teaching beats.
    /// </summary>
    public sealed class OnboardingTests
    {
        [Test]
        public void Sequence_AdvancesAndClampsAtBothEnds()
        {
            var s = new OnboardingSequence(3);
            Assert.IsTrue(s.IsFirst);
            Assert.IsFalse(s.IsLast);

            s.Back();                                   // clamps at the first step
            Assert.AreEqual(0, s.Index);

            s.Next();
            Assert.AreEqual(1, s.Index);
            Assert.IsFalse(s.IsFirst);

            s.Next();
            Assert.AreEqual(2, s.Index);
            Assert.IsTrue(s.IsLast);

            s.Next();                                   // clamps at the last step
            Assert.AreEqual(2, s.Index);

            s.Back();
            Assert.AreEqual(1, s.Index);

            s.Reset();
            Assert.AreEqual(0, s.Index);
        }

        [Test]
        public void Sequence_SingleStep_IsFirstAndLast()
        {
            var s = new OnboardingSequence(1);
            Assert.IsTrue(s.IsFirst);
            Assert.IsTrue(s.IsLast);
        }

        [Test]
        public void Content_CoversTheTeachingBeats()
        {
            Assert.GreaterOrEqual(OnboardingContent.Steps.Count, 4,
                "framing → shape → run → refine");

            foreach (var step in OnboardingContent.Steps)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(step.Title), "every step has a title");
                Assert.IsFalse(string.IsNullOrWhiteSpace(step.Body), "every step has body copy");
            }
        }
    }
}

using System.Collections.Generic;

namespace TheLoop.App
{
    /// <summary>One teaching beat in the onboarding sequence.</summary>
    public readonly struct OnboardingStep
    {
        public readonly string Title;
        public readonly string Body;

        public OnboardingStep(string title, string body)
        {
            Title = title;
            Body = body;
        }
    }

    /// <summary>
    /// Placeholder first-run teaching beats (XRC-90). Copy and VO are design-owned and will be
    /// replaced; the sequence shape mirrors the flow in the issue — framing → shape → run →
    /// refine — so the routing and pacing can be built and tested against real steps.
    /// </summary>
    public static class OnboardingContent
    {
        public static readonly IReadOnlyList<OnboardingStep> Steps = new[]
        {
            new OnboardingStep(
                "You train the agent",
                "You don't play the hero here. You shape the world it learns in — then it runs on its own."),
            new OnboardingStep(
                "Shape the loop",
                "Lay out terrain, hazards and rewards. Every choice teaches the agent what to value."),
            new OnboardingStep(
                "The agent runs itself",
                "Set it loose. It acts on the policy it has learned — your job is to watch where that leads."),
            new OnboardingStep(
                "Refine between runs",
                "Read the debrief, adjust the loop, and send it back a little sharper each time."),
        };
    }
}

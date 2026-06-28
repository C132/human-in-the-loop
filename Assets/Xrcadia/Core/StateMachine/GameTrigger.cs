namespace Xrcadia.Core.StateMachine
{
    /// <summary>
    /// User/system intents that the <see cref="TransitionTable"/> resolves to a target
    /// state given the current state. Keeping intents separate from concrete targets lets
    /// the same button ("Start") route to different states (Onboarding vs MainMenu) without
    /// scattering conditionals across the UI layer.
    /// </summary>
    public enum GameTrigger
    {
        None = 0,
        StartPressed,
        OnboardingComplete,
        Continue,
        NewGame,
        OpenSettings,
        CloseSettings,
        Quit,
    }
}

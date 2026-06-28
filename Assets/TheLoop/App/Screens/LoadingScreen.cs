using UnityEngine.UIElements;
using Xrcadia.Core.StateMachine;
using Xrcadia.UI;

namespace TheLoop.App.Screens
{
    /// <summary>
    /// World-space Loading overlay (XRC-98 slice). Binds to the shared
    /// <c>StateContext.Loading</c> progress model while visible.
    /// </summary>
    public sealed class LoadingScreen : ScreenBase
    {
        VisualElement _fill;
        Label _status;
        bool _subscribed;

        public override GameState State => GameState.Loading;

        protected override VisualElement Build()
        {
            var root = Ui.Scrim();

            var panel = Ui.Panel();
            panel.Add(Ui.Heading("Loading"));

            var track = new VisualElement();
            track.AddToClassList("progress-track");
            _fill = new VisualElement();
            _fill.AddToClassList("progress-fill");
            track.Add(_fill);
            panel.Add(track);

            _status = new Label(string.Empty);
            _status.AddToClassList("status-text");
            panel.Add(_status);

            root.Add(panel);
            return root;
        }

        protected override void OnShow()
        {
            if (!_subscribed)
            {
                Context.Loading.Changed += OnProgress;
                _subscribed = true;
            }

            OnProgress(Context.Loading.Value, Context.Loading.Status);
        }

        protected override void OnHide()
        {
            if (_subscribed)
            {
                Context.Loading.Changed -= OnProgress;
                _subscribed = false;
            }
        }

        void OnProgress(float value, string status)
        {
            _fill.style.width = Length.Percent(value * 100f);
            _status.text = status ?? string.Empty;
        }
    }
}

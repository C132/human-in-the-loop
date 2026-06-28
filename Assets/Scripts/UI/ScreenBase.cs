using UnityEngine.UIElements;
using Xrcadia.Core.StateMachine;

namespace Xrcadia.UI
{
    /// <summary>
    /// A world-space screen/overlay (XRC-86). Each screen owns a VisualElement subtree built
    /// in <see cref="Build"/>, shown/hidden by the <see cref="UIRouter"/> in response to FSM
    /// transitions. Screens never drive navigation directly other than by issuing triggers
    /// through the <see cref="StateContext"/> they are bound to.
    /// </summary>
    public abstract class ScreenBase
    {
        /// <summary>The FSM state this screen represents.</summary>
        public abstract GameState State { get; }

        public VisualElement Root { get; private set; }

        protected StateContext Context { get; private set; }

        /// <summary>Build the (initially hidden) subtree. Called once at registration.</summary>
        protected abstract VisualElement Build();

        public void Initialize(StateContext context)
        {
            Context = context;
            Root = Build();
            Root.AddToClassList("screen");
            Root.pickingMode = PickingMode.Position;
            // Fill the panel; screens stack and are shown/hidden by the router.
            Root.style.position = Position.Absolute;
            Root.style.left = 0;
            Root.style.top = 0;
            Root.style.right = 0;
            Root.style.bottom = 0;
            Hide();
        }

        /// <summary>Refresh bindings against current state (e.g. enable/disable Continue).</summary>
        public virtual void Bind() { }

        public void Show()
        {
            if (Root == null) return;
            Root.style.display = DisplayStyle.Flex;
            Bind();
            Root.BringToFront();
            OnShow();
        }

        public void Hide()
        {
            if (Root == null) return;
            Root.style.display = DisplayStyle.None;
            OnHide();
        }

        protected virtual void OnShow() { }
        protected virtual void OnHide() { }
    }
}

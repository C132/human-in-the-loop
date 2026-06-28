using System;
using UnityEngine.UIElements;

namespace Xrcadia.UI
{
    /// <summary>
    /// Small factory helpers so screens build consistent, themed elements without repeating
    /// USS class names everywhere. Named <c>Ui</c> (not <c>UI</c>) to avoid colliding with the
    /// enclosing <c>Xrcadia.UI</c> namespace at call sites under the <c>Xrcadia.*</c> hierarchy.
    /// </summary>
    public static class Ui
    {
        public static VisualElement Scrim()
        {
            var v = new VisualElement();
            v.AddToClassList("scrim");
            v.AddToClassList("screen");
            return v;
        }

        public static VisualElement Panel()
        {
            var v = new VisualElement();
            v.AddToClassList("panel");
            return v;
        }

        public static Label Title(string text)
        {
            var l = new Label(text);
            l.AddToClassList("title");
            return l;
        }

        public static Label Subtitle(string text)
        {
            var l = new Label(text);
            l.AddToClassList("subtitle");
            return l;
        }

        public static Label Heading(string text)
        {
            var l = new Label(text);
            l.AddToClassList("heading");
            return l;
        }

        public static Label Prompt(string text)
        {
            var l = new Label(text);
            l.AddToClassList("prompt");
            return l;
        }

        public static Button MenuButton(string text, Action onClick)
        {
            var b = new Button(onClick) { text = text };
            b.AddToClassList("menu-button");
            return b;
        }
    }
}

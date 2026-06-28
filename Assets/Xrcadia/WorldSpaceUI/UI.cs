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

        public static Label Body(string text)
        {
            var l = new Label(text);
            l.AddToClassList("body");
            return l;
        }

        public static Label Caption(string text)
        {
            var l = new Label(text);
            l.AddToClassList("caption");
            return l;
        }

        public static Label Eyebrow(string text)
        {
            var l = new Label(text);
            l.AddToClassList("eyebrow");
            return l;
        }

        public static VisualElement Divider()
        {
            var v = new VisualElement();
            v.AddToClassList("divider");
            return v;
        }

        /// <summary>A grouped block with an uppercase section title; add rows/controls to it.</summary>
        public static VisualElement Section(string title)
        {
            var section = new VisualElement();
            section.AddToClassList("section");

            var label = new Label(title);
            label.AddToClassList("section-title");
            section.Add(label);

            return section;
        }

        /// <summary>A label-left / control-right row.</summary>
        public static VisualElement Row(string label, VisualElement control)
        {
            var row = new VisualElement();
            row.AddToClassList("row");

            var l = new Label(label);
            l.AddToClassList("row__label");
            row.Add(l);
            row.Add(control);

            return row;
        }

        public static Button MenuButton(string text, Action onClick)
        {
            var b = new Button(onClick) { text = text };
            b.AddToClassList("menu-button");
            return b;
        }

        public static Button PrimaryButton(string text, Action onClick)
        {
            var b = MenuButton(text, onClick);
            b.AddToClassList("button--primary");
            return b;
        }

        public static Button GhostButton(string text, Action onClick)
        {
            var b = MenuButton(text, onClick);
            b.AddToClassList("button--ghost");
            return b;
        }

        public static Button DangerButton(string text, Action onClick)
        {
            var b = MenuButton(text, onClick);
            b.AddToClassList("button--danger");
            return b;
        }

        /// <summary>A vertical stack of full-width buttons (the standard menu column).</summary>
        public static VisualElement ButtonBar()
        {
            var v = new VisualElement();
            v.AddToClassList("button-bar");
            return v;
        }

        /// <summary>A horizontal row of buttons (e.g. confirm / cancel).</summary>
        public static VisualElement ButtonRow()
        {
            var v = new VisualElement();
            v.AddToClassList("button-row");
            return v;
        }
    }
}

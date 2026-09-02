
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Galactic1.UI.Text
{
    public class TextBuilder
    {
        private readonly StringBuilder _sb = new();
        private readonly Stack<string> _tagStack = new();

        public static TextBuilder Start()
        {
            return new TextBuilder();
        }

        // ------------------------------------------------
        // TEXT
        // ------------------------------------------------

        public TextBuilder Text(string value)
        {
            _sb.Append(value);
            return this;
        }

        public TextBuilder Text(int value)
        {
            _sb.Append(value);
            return this;
        }

        public TextBuilder Space()
        {
            _sb.Append(" ");
            return this;
        }

        public TextBuilder LineBreak()
        {
            _sb.Append("\n");
            return this;
        }

        // ------------------------------------------------
        // COLOR
        // ------------------------------------------------

        public TextBuilder Color(Color color)
        {
            var hex = ColorUtility.ToHtmlStringRGB(color);

            _sb.Append("<color=#");
            _sb.Append(hex);
            _sb.Append(">");

            _tagStack.Push("color");

            return this;
        }

        // ------------------------------------------------
        // SIZE
        // ------------------------------------------------

        public TextBuilder Size(int percent)
        {
            _sb.Append("<size=");
            _sb.Append(percent);
            _sb.Append("%>");

            _tagStack.Push("size");

            return this;
        }

        // ------------------------------------------------
        // STYLE
        // ------------------------------------------------

        public TextBuilder Bold()
        {
            _sb.Append("<b>");
            _tagStack.Push("b");
            return this;
        }

        public TextBuilder Italic()
        {
            _sb.Append("<i>");
            _tagStack.Push("i");
            return this;
        }

        // ------------------------------------------------
        // TMP SPRITE
        // ------------------------------------------------

        public TextBuilder Sprite(string name)
        {
            _sb.Append("<sprite name=\"");
            _sb.Append(name);
            _sb.Append("\">");

            return this;
        }

        public TextBuilder Sprite(int index)
        {
            _sb.Append("<sprite index=");
            _sb.Append(index);
            _sb.Append(">");

            return this;
        }

        // ------------------------------------------------
        // TAG CONTROL
        // ------------------------------------------------

        /// <summary>
        /// Закрыть последний тег
        /// </summary>
        public TextBuilder End()
        {
            if (_tagStack.Count == 0)
                return this;

            var tag = _tagStack.Pop();

            _sb.Append("</");
            _sb.Append(tag);
            _sb.Append(">");

            return this;
        }

        /// <summary>
        /// Закрыть все теги
        /// </summary>
        public TextBuilder EndAll()
        {
            while (_tagStack.Count > 0)
            {
                End();
            }

            return this;
        }

        // ------------------------------------------------
        // BUILD
        // ------------------------------------------------

        public override string ToString()
        {
            EndAll();
            return _sb.ToString();
        }
    }
}
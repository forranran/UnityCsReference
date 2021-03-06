// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using System.Collections.Generic;

namespace UnityEngine.UIElements
{
    internal interface ITextElement
    {
        string text { get; set; }
    }

    public class TextElement : VisualElement, ITextElement
    {
        public new class UxmlFactory : UxmlFactory<TextElement, UxmlTraits> {}
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlStringAttributeDescription m_Text = new UxmlStringAttributeDescription { name = "text" };

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                ((ITextElement)ve).text = m_Text.GetValueFromBag(bag, cc);
            }
        }

        public static readonly string ussClassName = "unity-text-element";

        public TextElement()
        {
            requireMeasureFunction = true;
            AddToClassList(ussClassName);
        }

        [SerializeField]
        private string m_Text;
        public virtual string text
        {
            get { return m_Text ?? String.Empty; }
            set
            {
                if (m_Text == value)
                    return;

                m_Text = value;
                IncrementVersion(VersionChangeType.Layout | VersionChangeType.Repaint);

                if (!string.IsNullOrEmpty(viewDataKey))
                    SaveViewData();
            }
        }

        internal override void DoRepaint(IStylePainter painter)
        {
            var stylePainter = (IStylePainterInternal)painter;
            stylePainter.DrawText(text);
        }

        public Vector2 MeasureTextSize(string textToMeasure, float width, MeasureMode widthMode, float height,
            MeasureMode heightMode)
        {
            return MeasureVisualElementTextSize(this, textToMeasure, width, widthMode, height, heightMode);
        }

        internal static Vector2 MeasureVisualElementTextSize(VisualElement ve, string textToMeasure, float width, MeasureMode widthMode, float height, MeasureMode heightMode)
        {
            float measuredWidth = float.NaN;
            float measuredHeight = float.NaN;

            Font usedFont = ve.computedStyle.unityFont.value;
            if (textToMeasure == null || usedFont == null)
                return new Vector2(measuredWidth, measuredHeight);

            var elementScaling = ve.ComputeGlobalScale();

            float pixelsPerPoint = (ve.elementPanel != null) ? ve.elementPanel.currentPixelsPerPoint : GUIUtility.pixelsPerPoint;
            float scaling = (elementScaling.x + elementScaling.y) * 0.5f * pixelsPerPoint;

            if (widthMode == MeasureMode.Exactly)
            {
                measuredWidth = width;
            }
            else
            {
                var textParams = TextStylePainterParameters.GetDefault(ve, textToMeasure);
                textParams.text = textToMeasure;
                textParams.font = usedFont;
                textParams.wordWrapWidth = 0.0f;
                textParams.wordWrap = false;
                textParams.richText = true;

                //we make sure to round up as yoga could decide to round down and text would start wrapping
                measuredWidth = Mathf.Ceil(TextNative.ComputeTextWidth(textParams.GetTextNativeSettings(scaling)));

                if (widthMode == MeasureMode.AtMost)
                {
                    measuredWidth = Mathf.Min(measuredWidth, width);
                }
            }

            if (heightMode == MeasureMode.Exactly)
            {
                measuredHeight = height;
            }
            else
            {
                var textParams = TextStylePainterParameters.GetDefault(ve, textToMeasure);
                textParams.text = textToMeasure;
                textParams.font = usedFont;
                textParams.wordWrapWidth = measuredWidth;
                textParams.richText = true;

                measuredHeight = Mathf.Ceil(TextNative.ComputeTextHeight(textParams.GetTextNativeSettings(scaling)));

                if (heightMode == MeasureMode.AtMost)
                {
                    measuredHeight = Mathf.Min(measuredHeight, height);
                }
            }

            return new Vector2(measuredWidth, measuredHeight);
        }

        protected internal override Vector2 DoMeasure(float desiredWidth, MeasureMode widthMode, float desiredHeight,
            MeasureMode heightMode)
        {
            return MeasureTextSize(text, desiredWidth, widthMode, desiredHeight, heightMode);
        }
    }
}

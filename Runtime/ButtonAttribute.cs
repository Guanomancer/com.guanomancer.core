using UnityEngine;

namespace Guanomancer
{
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class ButtonAttribute : PropertyAttribute
    {
        public string Label;
        public string Tooltip;
        public string TintRGB;

        public Color TintColor =>
                (!string.IsNullOrEmpty(TintRGB) && ColorUtility.TryParseHtmlString((TintRGB[0] == '#' ? TintRGB : '#' + TintRGB), out Color color)) ?
            color : Color.white;

        public ButtonAttribute(string overrideLabel = null, string tooltip = null, string tintRGB = "FFF")
        {
            Label = overrideLabel;
            Tooltip = tooltip;
            TintRGB = tintRGB;
        }
    }
}
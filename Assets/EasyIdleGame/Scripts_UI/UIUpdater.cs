using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EasyIdleGame.UI
{
    public enum ColorType
    {
        Red,
        Green,
        Blue,
        Yellow,
        Cyan,
        Magenta,
        White,
        Black,
        Gray,
        Orange,
        Purple,
        Brown
    }

    public static class UIUpdater
    {
        public static void UpdateText(TMP_Text t, string v)
        {
            if (t == null) return;
            t.text = v;
        }

        public static void UpdateText(ButtonHolder t, string v)
        {
            if (t == null) return;
            t.SetText(v);
        }

        public static void UpdateTextOpacity(TMP_Text t, float opacity)
        {
            if (t == null) return;
            t.color = ChangeColorOpacity(t.color, opacity);
        }

        public static void SetButtonInteractable(Button b, bool v)
        {
            if (b == null) return;
            b.interactable = v;
        }

        public static void SetButtonInteractable(ButtonHolder b, bool v)
        {
            if (b == null) return;
            b.SetButtonInteractible(v);
        }

        public static void UpdateImage(Image i, Sprite s)
        {
            if (i == null) return;
            i.sprite = s;
        }

        public static void UpdateImageColor(Image i, Color color)
        {
            if (i == null) return;
            i.color = color;
        }

        public static void SetSliderValues(Slider s, float min, float max, double value) => SetSliderValues(s, min, max, (float)value);

        public static void SetSliderValues(Slider s, float min, float max, float value)
        {
            if (s == null) return;
            s.minValue = min;
            s.maxValue = max;
            s.value = value;
        }

        public static void UpdateImageOpacity(Image i, float opacity)
        {
            if (i == null) return;
            i.color = ChangeColorOpacity(i.color, opacity);
        }

        public static Color ChangeColorOpacity(Color c, float opacity)
        {
            c.a = opacity;
            return c;
        }

        public static string GetColorfulText(string text, ColorType type)
        {
            string color = type switch
            {
                ColorType.Green => "green",
                ColorType.Red => "red",
                ColorType.Blue => "blue",
                ColorType.Yellow => "yellow",
                ColorType.Cyan => "cyan",
                ColorType.Magenta => "magenta",
                ColorType.White => "white",
                ColorType.Black => "black",
                ColorType.Gray => "gray",
                ColorType.Orange => "orange",
                ColorType.Purple => "purple",
                ColorType.Brown => "brown",
                _ => "white"
            };

            return $"<color={color}>{text}</color>";
        }
    }
}
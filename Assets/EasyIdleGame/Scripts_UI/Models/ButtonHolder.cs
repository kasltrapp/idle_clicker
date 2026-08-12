using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EasyIdleGame.UI
{
    [System.Serializable]
    public class ButtonHolder
    {
        public Button button;
        public TMP_Text buttonText;

        /// <summary>
        /// Sets the button interactable and updates the text color
        /// </summary>
        public void SetButtonInteractible(bool v)
        {
            if (button != null)
                button.interactable = v;

            if (buttonText != null)
                buttonText.color = new Color(buttonText.color.r, buttonText.color.g, buttonText.color.b, v ? 1 : 0.5f);
        }

        public void SetText(string v)
        {
            if (buttonText != null)
                buttonText.text = v;
        }
    }
}

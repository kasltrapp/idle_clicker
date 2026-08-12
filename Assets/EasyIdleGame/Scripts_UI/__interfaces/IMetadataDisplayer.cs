using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EasyIdleGame.UI
{
    public interface IMetadataDisplayer
    {
        public Image iconImage { get; }
        public TMP_Text nameText { get; }
        public TMP_Text descriptionText { get; }

        public void Redraw_Metadata(Metadata metadata) => Redraw_Metadata(metadata?.name, metadata?.description, metadata?.icon);

        public void Redraw_Metadata(string name, string description, Sprite icon)
        {
            UpdateText(nameText, name);
            UpdateText(descriptionText, description);
            UpdateImage(iconImage, icon);
        }

        protected virtual void UpdateText(TMP_Text t, string v) => UIUpdater.UpdateText(t, v);
        protected virtual void UpdateImage(Image i, Sprite s) => UIUpdater.UpdateImage(i, s);
    }
}

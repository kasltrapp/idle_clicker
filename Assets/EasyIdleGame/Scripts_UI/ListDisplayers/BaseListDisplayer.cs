using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyIdleGame.UI
{
    [Obsolete("Use Unity Building Layout Handling instead")]
    public class BaseListDisplayer<T> : MonoBehaviour where T : BaseDisplayer
    {
        public int gap = 150;

        public GameObject prefab;

        public List<T> displayers;

        protected Vector3 GetVector(int i, bool x)
        {
            float height = prefab.transform is RectTransform r ? r.rect.height : 0;

            if (x) return new Vector3(-i * gap - height / 2, 60, 0);
            return new Vector3(0, -i * gap - height / 2, 0);
        }

        public float GetAutoHeight(int itemCount)
        {
            float prefabHeight = prefab.GetComponent<RectTransform>().rect.height;

            return prefabHeight * itemCount + (itemCount - 1) * (gap - prefabHeight);
        }

        protected void ClearDisplayers()
        {
            foreach (var displayer in displayers)
            {
                Destroy(displayer.gameObject);
            }
        }

        protected GameObject SpawnPrefab(
            int i,
            bool x,
            AnchorTypeY yAnchor = AnchorTypeY.Top,
            AnchorTypeX xAnchor = AnchorTypeX.Stretch
        )
        {
            GameObject clone = Instantiate(prefab, transform);

            // set anchors to center just for the spawn so the position is correct
            ((RectTransform)clone.transform).anchorMax = new Vector2(0.5f, 0.5f);
            ((RectTransform)clone.transform).anchorMin = new Vector2(0.5f, 0.5f);

            clone.transform.localPosition = GetVector(i, x);

            var (min, max) = GetAnchors(yAnchor, xAnchor);

            ((RectTransform)clone.transform).anchorMax = max;
            ((RectTransform)clone.transform).anchorMin = min;

            ((RectTransform)clone.transform).offsetMin = new Vector2(0, ((RectTransform)clone.transform).offsetMin.y);
            ((RectTransform)clone.transform).offsetMax = new Vector2(0, ((RectTransform)clone.transform).offsetMax.y);

            return clone;
        }

        public enum AnchorTypeY
        {
            Top,
            Bottom,
            Center,
            Stretch
        }

        public enum AnchorTypeX
        {
            Left,
            Right,
            Center,
            Stretch
        }

        public (Vector2 min, Vector2 max) GetAnchors(AnchorTypeY y, AnchorTypeX x)
        {
            Vector2 min = new Vector2();
            Vector2 max = new Vector2();

            switch (y)
            {
                case AnchorTypeY.Top:
                    min.y = 1;
                    max.y = 1;
                    break;
                case AnchorTypeY.Bottom:
                    min.y = 0;
                    max.y = 0;
                    break;
                case AnchorTypeY.Center:
                    min.y = 0.5f;
                    max.y = 0.5f;
                    break;
                case AnchorTypeY.Stretch:
                    min.y = 0;
                    max.y = 1;
                    break;
            }

            switch (x)
            {
                case AnchorTypeX.Left:
                    min.x = 0;
                    max.x = 0;
                    break;
                case AnchorTypeX.Right:
                    min.x = 1;
                    max.x = 1;
                    break;
                case AnchorTypeX.Center:
                    min.x = 0.5f;
                    max.x = 0.5f;
                    break;
                case AnchorTypeX.Stretch:
                    min.x = 0;
                    max.x = 1;
                    break;
            }

            return (min, max);

        }
    }
}
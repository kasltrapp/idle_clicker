using UnityEngine;

namespace EasyIdleGame
{
    [System.Serializable]
    public class HolderBase<T> where T : ScriptableObject
    {
        public SaveableScriptableObject<T> itemSaveable;

        /// <summary> Item that this holder holds (like business, currency, ...) </summary>
        public T Item
        {
            get => itemSaveable.Value;
            set => itemSaveable = new SaveableScriptableObject<T>(value);
        }

        public HolderBase(T item)
        {
            Item = item;
        }

        public void Load()
        {
            Item = itemSaveable.LoadScriptableObject();
        }

        public static implicit operator bool(HolderBase<T> holder)
        {
            return holder != null;
        }
    }
}
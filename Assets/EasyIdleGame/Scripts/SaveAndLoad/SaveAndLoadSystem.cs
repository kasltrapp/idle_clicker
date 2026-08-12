using UnityEngine;

namespace EasyIdleGame
{
    /// <summary>
    /// Class that handles saving and loading of the game
    /// </summary>
    public static class SaveAndLoadSystem<T> where T : SaveFile, new()
    {
        public static void DeleteSave(string path)
        {
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
        }

        /// <summary>
        /// Creates new SaveFile and saves it to the drive
        /// </summary>
        /// <param name="path"></param>
        public static void Save(string path) => Save(new T(), path);

        /// <summary>
        /// Saves the object to the drive
        /// </summary>
        public static void Save(object obj, string path)
        {
            string json = JsonUtility.ToJson(obj);
            System.IO.File.WriteAllText(path, json);
        }

        /// <summary>
        /// Loads save file from the drive and calls Load method on it
        /// </summary>
        public static void Load(string path, out T usedFile)
        {
            usedFile = Load<T>(path);

            if (usedFile == null) return;

            usedFile.Load();
        }

        /// <returns> Returns loaded file from the drive </returns>
        public static F Load<F>(string path)
        {
            if (!System.IO.File.Exists(path)) return default;

            string json = System.IO.File.ReadAllText(path);
            return JsonUtility.FromJson<F>(json);
        }
    }
}
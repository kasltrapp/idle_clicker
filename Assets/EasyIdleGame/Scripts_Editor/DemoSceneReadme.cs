using System;
using UnityEngine;

namespace EasyIdleGame.Demos
{
    [CreateAssetMenu(fileName = "ReadMe", menuName = "EasyIdleGame/Demos/Demo Scene Readme")]
    public class DemoSceneReadme : ScriptableObject
    {
        public DemoSceneReadmeData data = new DemoSceneReadmeData();

        public string title => data?.title ?? string.Empty;
        public string summary => data?.summary ?? string.Empty;
        public string resourcePrefix => data?.resourcePrefix ?? string.Empty;
        public DemoReadmeSection[] sections => data?.sections ?? Array.Empty<DemoReadmeSection>();
    }

    [Serializable]
    public class DemoSceneReadmeData
    {
        public string title;

        [TextArea(3, 8)]
        public string summary;

        public string resourcePrefix;
        public DemoReadmeSection[] sections = Array.Empty<DemoReadmeSection>();
    }

    [Serializable]
    public class DemoReadmeSection
    {
        public string heading;

        [TextArea(3, 10)]
        public string body;
    }
}

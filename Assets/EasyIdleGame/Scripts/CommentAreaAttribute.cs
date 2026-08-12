using UnityEngine;

namespace EasyIdleGame
{
    /// <summary>
    /// Displays a read-only text box in the Inspector for in-context documentation.
    /// Place on a private dummy string field above the field you are documenting.
    /// The text argument is shown as a styled help box; the field value is hidden.
    /// Pass exampleUsage when the inspector should also show where and how to use the feature.
    /// Usage:
    ///     [CommentArea("This explains what the field below does.")]
    ///     [SerializeField] private string _myNote;
    /// </summary>
    public class CommentAreaAttribute : PropertyAttribute
    {
        public readonly string Title;
        public readonly string Content;
        public readonly string ExampleUsage;

        public CommentAreaAttribute(string title, string content, string exampleUsage = null)
        {
            Title = title;
            Content = content;
            ExampleUsage = exampleUsage;
        }
    }
}

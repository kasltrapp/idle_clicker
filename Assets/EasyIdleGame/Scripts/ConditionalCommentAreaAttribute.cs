using UnityEngine;

namespace EasyIdleGame
{
    public enum ConditionalCommentAreaMode
    {
        EmptyList,
        BigNumberGreaterThan,
        AnyListBigNumberLessThanOrEqual,
        AllListsEmpty
    }

    /// <summary>
    /// Displays a CommentArea-style info box only when a serialized condition is met.
    /// Place on a private dummy string field above the field you are documenting.
    /// </summary>
    public class ConditionalCommentAreaAttribute : PropertyAttribute
    {
        public readonly string Title;
        public readonly string Content;
        public readonly string FieldName;
        public readonly ConditionalCommentAreaMode Mode;
        public readonly string CompareFieldName;
        public readonly string[] CompareFieldNames;
        public readonly string ChildFieldName;
        public readonly double CompareValue;

        public ConditionalCommentAreaAttribute(string title, string content, string fieldName, ConditionalCommentAreaMode mode)
        {
            Title = title;
            Content = content;
            FieldName = fieldName;
            Mode = mode;
        }

        public ConditionalCommentAreaAttribute(string title, string content, string fieldName, ConditionalCommentAreaMode mode, string compareFieldName)
            : this(title, content, fieldName, mode)
        {
            CompareFieldName = compareFieldName;
            CompareFieldNames = new[] { compareFieldName };
        }

        public ConditionalCommentAreaAttribute(string title, string content, string fieldName, ConditionalCommentAreaMode mode, string compareFieldName, string secondCompareFieldName)
            : this(title, content, fieldName, mode)
        {
            CompareFieldName = compareFieldName;
            CompareFieldNames = new[] { compareFieldName, secondCompareFieldName };
        }

        public ConditionalCommentAreaAttribute(string title, string content, string fieldName, ConditionalCommentAreaMode mode, string compareFieldName, string secondCompareFieldName, string thirdCompareFieldName)
            : this(title, content, fieldName, mode)
        {
            CompareFieldName = compareFieldName;
            CompareFieldNames = new[] { compareFieldName, secondCompareFieldName, thirdCompareFieldName };
        }

        public ConditionalCommentAreaAttribute(string title, string content, string fieldName, ConditionalCommentAreaMode mode, string childFieldName, double compareValue)
            : this(title, content, fieldName, mode)
        {
            ChildFieldName = childFieldName;
            CompareValue = compareValue;
        }
    }
}

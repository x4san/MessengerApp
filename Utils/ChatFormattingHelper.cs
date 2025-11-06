using System;
using System.Globalization;
using System.Text;

namespace MessengerApp.Utils
{
    public static class ChatFormattingHelper
    {
        private static readonly string[] Palette = new[]
        {
            "#5B8FB9", "#FF7B72", "#4F9D69", "#9C6ADE", "#E0A458",
            "#3F8EFC", "#FF9AA2", "#70C1B3", "#B28DFF", "#F4A261",
            "#3DA5D9", "#FFB4A2", "#84A59D", "#A2D2FF", "#FFAFCC"
        };

        public static string BuildInitials(string? displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                return "?";
            }

            var words = displayName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .ToArray();

            if (words.Length == 0)
            {
                return "?";
            }

            if (words.Length == 1)
            {
                return TakeFirstLetter(words[0]);
            }

            var builder = new StringBuilder();
            builder.Append(TakeFirstLetter(words[0]));
            builder.Append(TakeFirstLetter(words[^1]));
            return builder.ToString();
        }

        private static string TakeFirstLetter(string word)
        {
            if (string.IsNullOrEmpty(word))
            {
                return string.Empty;
            }

            var textInfo = CultureInfo.CurrentCulture.TextInfo;
            var normalized = word.Normalize(NormalizationForm.FormKC);
            var firstTextElement = new StringInfo(normalized).SubstringByTextElements(0, 1);
            return textInfo.ToUpper(firstTextElement);
        }

        public static string PickColor(string seed)
        {
            if (string.IsNullOrEmpty(seed))
            {
                seed = Guid.NewGuid().ToString();
            }

            unchecked
            {
                int hash = 23;
                foreach (var ch in seed)
                {
                    hash = hash * 31 + ch;
                }

                var index = Math.Abs(hash % Palette.Length);
                return Palette[index];
            }
        }

        public static string BuildReplySnippet(string content, int length = 80)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return string.Empty;
            }

            var trimmed = content.Trim();
            if (trimmed.Length <= length)
            {
                return trimmed;
            }

            return trimmed.Substring(0, length).TrimEnd() + "…";
        }
    }
}

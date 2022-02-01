using System;
using System.Linq;

namespace GamesBot.Utils
{
    public static class StringUtils
    {
        static readonly Random random = new();

        public static string RemoveAccents(string text)
        {
            var normalizedString = text.Normalize(System.Text.NormalizationForm.FormD);
            var stringBuilder = new System.Text.StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC);
        }

        public static string GetAllRemainderTextAfter(string[] array, int index, bool hasSpace = true)
        {
            string output = string.Empty;

            for (int i = 0; i < array.Length; i++)
            {
                if (i <= index) continue;

                output += array[i];
                if (hasSpace)
                    output += " ";
            }

            return output.Trim();
        }

        public static string RandomString(int length = 12)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}

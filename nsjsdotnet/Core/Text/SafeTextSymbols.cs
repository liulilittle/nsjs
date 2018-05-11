namespace nsjsdotnet.Core.Text
{
    using System.Text.RegularExpressions;

    public static class SafeTextSymbols
    {
        public static string GetSafeText(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }
            value = Regex.Replace(value, @"[^(\u4e00-\u9fa5|A-z|0-9|\u002e)]", string.Empty);
            if (value.Length > 0)
            {
                string[] alphabets = { ",", "{", "}", "[", "]", "?", "'", "(", ")", "_", "~", "!" };
                foreach (string alphabet in alphabets)
                {
                    value = value.Replace(alphabet, string.Empty);
                }
            }
            return value;
        }
    }
}

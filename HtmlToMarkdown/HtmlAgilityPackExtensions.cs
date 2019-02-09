using HtmlAgilityPack;

namespace HtmlToMarkdown
{
    internal static class HtmlAgilityPackExtensions
    {
        public static string GetAttributeOrEmpty(
            this HtmlAttributeCollection collection,
            string attributeName)
        {
            return collection[attributeName] != null ? collection[attributeName].Value : "";
        }
    }
}
using System.Text.RegularExpressions;
using Html2Markdown.Replacement;

namespace HtmlToMarkdown
{
    public class PatternReplacer
    {
        public string Pattern { get; set; }

        public string Replacement { get; set; }

        public string Replace(string html)
        {
            return new Regex(Pattern).Replace(html, Replacement);
        }
    }
}
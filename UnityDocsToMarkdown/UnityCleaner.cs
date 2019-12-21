using System.Linq;
using HtmlAgilityPack;
using UnityDocsToMarkdown.Core.Extensions;

namespace HtmlToMarkdown
{
    public static class UnityCleaner
    {
        public static string CleanupDocument(string html)
        {
            var node = HtmlNode.CreateNode(html);
            node.Descendants("head").RemoveAll();
            node.Descendants().Where(x => x.HasClass("header-wrapper")).RemoveAll();
            node.Descendants().Where(x => x.Id == "sidebar").RemoveAll();
            node.Descendants().Where(x => x.Id == "feedbackbox").RemoveAll();
            node.Descendants().Where(x => x.HasClass("footer-wrapper")).RemoveAll();
            node.Descendants().Where(x => x.HasClass("suggest")).RemoveAll();
            node.Descendants().Where(x => x.HasClass("scrollToFeedback")).RemoveAll();

            return node.OuterHtml;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace UnityDocsToMarkdown.Core.Extensions
{
    public static class HtmlExtensions
    {
        public static void RemoveAll(this IEnumerable<HtmlNode> nodes)
        {
            var nodesCopy = nodes.ToArray();
            foreach (var node in nodesCopy)
            {
                node.Remove();
            }
        }
    }
}
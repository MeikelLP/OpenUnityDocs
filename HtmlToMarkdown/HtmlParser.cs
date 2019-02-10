using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using HtmlAgilityPack;

namespace HtmlToMarkdown
{
    internal static class HtmlParser
    {
        private static readonly Regex AnchorRegex = new Regex("(.+)\\.html$");

        public static string ReplacePre(string html)
        {
            var xElement = XElement.Parse(html, LoadOptions.PreserveWhitespace);
            var pres = xElement.XPathSelectElements(".//pre");
            foreach (var pre in pres)
            {
                var anchors = pre.XPathSelectElements(".//a");
                foreach (var anchor in anchors)
                {
                    anchor.ReplaceWith(anchor.Value);
                }
                var breaks = pre.XPathSelectElements(".//br").ToArray();
                foreach (var br in breaks)
                {
                    br.ReplaceWith(Environment.NewLine);
                }

                var text = pre.Value.Trim();
                text = text.Replace("&lt;", "<").Replace("&gt;", ">");
                var str = $"{Environment.NewLine}{Environment.NewLine}```csharp{Environment.NewLine}{TabsToSpaces(text)}{Environment.NewLine}```";
                pre.ReplaceWith(str);
            }

            return xElement.ToString();
        }
        
        public static string ReplaceParagraph(string html)
        {
            var xElement = XElement.Parse(html, LoadOptions.PreserveWhitespace);
            var paragraphs = xElement.XPathSelectElements(".//p").ToArray();
            foreach (var paragraph in paragraphs)
            {
                var fixedParagraph = ReplaceAnchors(paragraph);
                
                var text = fixedParagraph.Value.Trim();
                var str = $"{Environment.NewLine}{Environment.NewLine}{text}{Environment.NewLine}{Environment.NewLine}";
                paragraph.ReplaceWith(str);
            }

            return xElement.ToString();
        }

        private static XElement ReplaceAnchors(XElement node)
        {
            var anchors = node.XPathSelectElements(".//a");
            
            foreach (var anchor in anchors)
            {
                ReplaceAnchor(anchor);
            }

            return node;
        }

        private static void ReplaceAnchor(XElement anchor)
        {
            var text = anchor.Value;
            var href = AnchorRegex.Replace(anchor.Attribute("href")?.Value ?? "", "$1.md");
            var title = anchor.Attribute("title")?.Value;

            if (string.IsNullOrWhiteSpace(title))
            {
                title = text;
            }

            anchor.ReplaceWith($"[{text}]({href} \"{title}\")");
        }

        public static string ReplaceAnchors(string html)
        {
            var xElement = XElement.Parse(html, LoadOptions.PreserveWhitespace);
            var anchors = xElement.XPathSelectElements(".//a");

            foreach (var anchor in anchors)
            {
                ReplaceAnchor(anchor);
            }

            return xElement.ToString();
        }

        private static string TabsToSpaces(string tag)
        {
            return tag.Replace("\t", "    ");
        }
        
        public static string ReplaceImg(string html)
        {
            var xElement = XElement.Parse(html, LoadOptions.PreserveWhitespace);
            var images = xElement.XPathSelectElements("//img").ToArray();
            foreach (var img in images)
            {
                var src = img.Attribute("src")?.Value;
                var alt = img.Attribute("alt")?.Value ?? Path.GetFileNameWithoutExtension(src);
                var title = img.Attribute("title")?.Value ?? Path.GetFileNameWithoutExtension(src);
                
                // TODO reference path must be valid (copy file)
                
                var str = $"![{alt}]({src}{(title?.Length > 0 ? $" \"{title}\"" : "")})";
                img.ReplaceWith(str);
            }

            return xElement.ToString();
        }

        public static string ReplaceEscapedHtml(string str)
        {
            return str.Replace("&lt;", "<").Replace("&gt;", ">");
        }
    }
}
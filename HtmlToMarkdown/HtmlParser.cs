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
                var latestWhiteSpace = "";
                foreach (var br in breaks)
                {
//                    var whiteSpace = latestWhiteSpace;
//                    var brPreviousNode = (XText)br.PreviousNode;
//                    if (brPreviousNode != null)
//                    {
//                        var regexCount = new Regex("^[\\ ]+", RegexOptions.Multiline | RegexOptions.Compiled);
//                        var count = regexCount.Matches(brPreviousNode.Value.Split(Environment.NewLine).Last()).Count;
//                        whiteSpace = new string(Enumerable.Range(0, count).Select(x => ' ').ToArray());
//                        if (whiteSpace != "")
//                        {
//                            latestWhiteSpace = whiteSpace;
//                        }
//                    }

//                    br.ReplaceWith(Environment.NewLine + whiteSpace);
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
            var regex = new Regex("(.+)\\.html$");
            
            foreach (var anchor in anchors)
            {
                var text = anchor.Value;
                var href = regex.Replace(anchor.Attribute("href")?.Value ?? "", "$1.md");
                var title = anchor.Attribute("title")?.Value;

                if (string.IsNullOrWhiteSpace(title))
                {
                    title = text;
                }
                
                anchor.ReplaceWith($"[{text}]({href} \"{title}\")");
            }

            return node;
        }

        public static string ReplaceAnchors(string html)
        {
            var xElement = XElement.Parse(html, LoadOptions.PreserveWhitespace);
            var anchors = xElement.XPathSelectElements(".//a");

            foreach (var anchor in anchors)
            {
                var text = anchor.Value;
                var href = anchor.Attribute("href")?.Value;
                var title = anchor.Attribute("title")?.Value;

                if (string.IsNullOrWhiteSpace(title))
                {
                    title = text;
                }
                
                anchor.ReplaceWith($"[{text}]({href} \"{title}\")");
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
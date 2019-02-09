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
            var xElement = XElement.Parse(html);
            var pres = xElement.XPathSelectElements("//pre");
            foreach (var pre in pres)
            {
                var anchors = pre.XPathSelectElements("//a");
                foreach (var anchor in anchors)
                {
                    anchor.ReplaceWith(anchor.Value);
                }
                var breaks = pre.XPathSelectElements("//br");
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

        private static string TabsToSpaces(string tag)
        {
            return tag.Replace("\t", "    ");
        }
        
        public static string ReplaceImg(string html)
        {
            var xElement = XElement.Parse(html);
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
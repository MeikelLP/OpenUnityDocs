using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace UnityDocsToMarkdown.Core
{
    public static class HtmlConverter
    {
        private static readonly string[] ContentNodes =
        {
            "p", "ul", "ol", "h*", "b", "i", "img", "pre", "code", "a", "td", "th", "li"
        };

        private static readonly string[] All =
        {
            "p", "ul", "ol", "h*", "b", "i", "img", "pre", "code", "table", "tr", "tbody", "thead", "th", "a", "div",
            "span", "body", "html", "head", "header", "section", "footer"
        };

        private static readonly string[] InlineContent =
        {
            "b", "i", "img", "code", "a"
        };

        public static string ToMarkDown(string html)
        {
            html = Regex.Replace(html, @$"\s*{Environment.NewLine}\s*", "", RegexOptions.Multiline);
            html = Regex.Replace(html, @"\s{2,}", " ", RegexOptions.Multiline);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var sb = new StringBuilder();
            var rootNode = doc.DocumentNode.Element("html") ?? doc.DocumentNode;

            foreach (var childNode in rootNode.ChildNodes)
            {
                sb.Append(Convert(childNode));
            }

            return sb.ToString();
        }

        public static string Convert(HtmlNode node, params string[] except)
        {
            if (node is HtmlTextNode textNode)
            {
                if (ContentNodes.Contains(textNode.ParentNode.Name) ||
                    ContentNodes.Contains("h*") && CustomMarkdown.HeaderRegex.IsMatch(textNode.ParentNode.Name))
                {
                    return textNode.Text.Replace("\n", "").Replace("\r", "");
                }

                return "";
            }

            if (string.IsNullOrWhiteSpace(node.Name))
            {
                throw new ArgumentNullException(nameof(node), "Node name must not be null");
            }

            if (!except.Contains(node.Name))
            {
                switch (node.Name)
                {
                    case "a":
                        return ConvertAnchor(node);
                    case "br":
                        return ConvertBreak(node);
                    case "img":
                        return ConvertImage(node);
                    case "b":
                        return ConvertBold(node);
                    case "i":
                        return ConvertItalic(node);
                    case "code":
                        return ConvertCode(node);
                    case "ol":
                        return ConvertOrderedList(node) + Environment.NewLine + Environment.NewLine;
                    case "ul":
                        return ConvertUnorderedList(node) + Environment.NewLine + Environment.NewLine;
                    case "p":
                        return ConvertParagraph(node) + Environment.NewLine + Environment.NewLine;
                    case "table":
                        return ConvertTable(node) + Environment.NewLine + Environment.NewLine;
                    case "pre":
                        return ConvertPre(node) + Environment.NewLine + Environment.NewLine;
                    case "div":
                    case "body":
                    case "html":
                        var content = ConvertGeneric(node);
                        if (!string.IsNullOrWhiteSpace(content.Trim()) && !content.EndsWith(Environment.NewLine))
                        {
                            content += Environment.NewLine + Environment.NewLine;
                        }

                        return content;
                    case "span":
                        return ConvertGeneric(node);
                }
            }

            if (!except.Contains("h*") && CustomMarkdown.HeaderRegex.IsMatch(node.Name))
            {
                return ConvertHeader(node) + Environment.NewLine + Environment.NewLine;
            }

            return ""; // everything else is ignored
        }

        #region Elements

        private static string ConvertGeneric(HtmlNode node)
        {
            var sb = new StringBuilder();
            foreach (var child in node.ChildNodes)
            {
                sb.Append(Convert(child));
            }

            return sb.ToString();
        }

        private static string ConvertAnchor(HtmlNode node)
        {
            if (node.Name != "a") throw new ArgumentException("Node must be of type anchor");

            string text;

            if (node.ChildNodes.Count > 0)
            {
                var sb = new StringBuilder();
                foreach (var childNode in node.ChildNodes)
                {
                    sb.Append(Convert(childNode, All.Except(InlineContent).ToArray()));
                }

                text = sb.ToString();
            }
            else
            {
                text = node.InnerText.Trim();
            }


            if (!string.IsNullOrWhiteSpace(text))
            {
                var href = node.GetAttributeValue("href", "#").Replace(".html", ".md");
                var title = node.GetAttributeValue("title", node.InnerText.Trim());

                return $"[{text}]({href} \"{title}\")";
            }

            return "";
        }

        private static string ConvertTable(HtmlNode node)
        {
            if (node.Name != "table") throw new ArgumentException("Node must be of type table");
            if (!node.Descendants("tr").Any() || !node.Descendants("td").Any()) return "";

            var sb = new StringBuilder();

            var headers = (node.Element("thead") ?? node).Descendants("th").Select(ConvertTableHeader).ToArray();
            if (headers.Length == 0)
            {
                headers = node.Descendants("tr").First().Elements("td").Select(x => "").ToArray();
            }

            var headerRow = string.Join(" | ", headers);
            var dividerRow = string.Join(" | ", headers.Select(x => "---"));

            sb.AppendLine($"| {headerRow} |");
            sb.AppendLine($"| {dividerRow} |");


            var rows = (node.Element("tbody") ?? node).Descendants("tr");

            if (rows != null)
            {
                var newRows = rows
                    .Select(row => row.Descendants("td").ToArray())
                    .Select(tableData => tableData.Select(ConvertTableData))
                    .Select(rowData => "| " + string.Join(" | ", rowData) + " |")
                    .ToArray();

                sb.Append(string.Join(Environment.NewLine, newRows));
                // no new line at the end
            }

            return sb.ToString();
        }

        private static string ConvertTableData(HtmlNode node)
        {
            if (node.Name != "td") throw new ArgumentException("Node must be of type table data");

            var sb = new StringBuilder();
            foreach (var childNode in node.ChildNodes)
            {
                sb.Append(Convert(childNode, "br", "p", "h*"));
            }

            return sb.ToString();
        }

        public static string ConvertTableHeader(HtmlNode node)
        {
            if (node.Name != "th") throw new ArgumentException("Node must be of type table header");

            var sb = new StringBuilder();
            foreach (var childNode in node.ChildNodes)
            {
                sb.Append(Convert(childNode, "img", "p", "br", "pre", "h*"));
            }

            return sb.ToString();
        }

        private static string ConvertOrderedList(HtmlNode node, int indention = 0)
        {
            if (node.Name != "ol") throw new ArgumentException("Node must be of type ordered list");

            var sb = new StringBuilder();
            var listItems = node.Elements("li").ToArray();
            var lines = listItems
                .Select((li, i) => ConvertListItem(li, i, labelIndex => $"{labelIndex}.", indention))
                .ToArray();
            sb.Append(string.Join(Environment.NewLine, lines));

            return sb.ToString();
        }

        private static string ConvertUnorderedList(HtmlNode node, int indention = 0)
        {
            if (node.Name != "ul") throw new ArgumentException("Node must be of type unordered list");

            var sb = new StringBuilder();
            var listItems = node.Elements("li").ToArray();
            var lines = listItems
                .Select((li, i) => ConvertListItem(li, i, _ => "*", indention))
                .ToArray();

            sb.Append(string.Join(Environment.NewLine, lines));

            return sb.ToString();
        }

        private static string ConvertListItem(HtmlNode node, int itemIndex, Func<int, string> prefix, int indention)
        {
            if (node.Name != "li") throw new ArgumentException("Node must be of type list item");

            var sb = new StringBuilder();
            foreach (var childNode in node.ChildNodes)
            {
                if (childNode.Name == "ul")
                {
                    var text = ConvertUnorderedList(childNode, indention + 2);
                    sb.Append(text);
                }
                else if (childNode.Name == "ol")
                {
                    var text = ConvertOrderedList(childNode, indention + 2);
                    sb.Append(text);
                }
                else
                {
                    var text = Convert(childNode, All.Except(InlineContent).ToArray());
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        var indent = new string(Enumerable.Range(0, indention).Select(x => ' ').ToArray());
                        var content = $"{indent}{prefix.Invoke(itemIndex + 1)} {text}";
                        sb.Append(content);
                    }
                }
            }

            return sb.ToString();
        }

        private static string ConvertCode(HtmlNode node)
        {
            if (node.Name != "code") throw new ArgumentException("Node must be of type code");

            return $"`{node.InnerText}`";
        }

        private static string ConvertItalic(HtmlNode node)
        {
            if (node.Name != "i") throw new ArgumentException("Node must be of type italic");

            return $"_{node.InnerText}_";
        }

        private static string ConvertBold(HtmlNode node)
        {
            if (node.Name != "b") throw new ArgumentException("Node must be of type bold");

            return $"**{node.InnerText}**";
        }

        private static string ConvertBreak(HtmlNode node)
        {
            if (node.Name != "br") throw new ArgumentException("Node must be of type break");

            return Environment.NewLine + Environment.NewLine;
        }

        private static string ConvertPre(HtmlNode node, string lang = "csharp")
        {
            if (node.Name != "pre") throw new ArgumentException("Node must be of type anchor");

            node.InnerHtml = CustomMarkdown.BreakReplacer.Invoke(node.InnerHtml);
            var txt = node.InnerText.Trim();

            var nl = Environment.NewLine;
            return $"```{lang}{nl}{txt.Replace("\t", "     ")}{nl}```";
        }

        private static string ConvertHeader(HtmlNode node)
        {
            var match = CustomMarkdown.HeaderRegex.Match(node.Name);
            if (!match.Success) throw new ArgumentException("Node must be of type header (h1, ...)");

            var level = int.Parse(match.Groups[1].Value);
            var hashes = Enumerable.Range(0, level).Select(x => '#').ToArray();

            var sb = new StringBuilder($"{new string(hashes)} ");
            foreach (var childNode in node.ChildNodes)
            {
                sb.Append(Convert(childNode, "img", "pre", "br"));
            }

            return sb.ToString();
        }

        private static string ConvertImage(HtmlNode node)
        {
            if (node.Name != "img") throw new ArgumentException("Node must be of type image");

            var src = node.GetAttributeValue("src", "");
            var alt = node.GetAttributeValue("alt", Path.GetFileNameWithoutExtension(src));
            var title = node.GetAttributeValue("title", alt);

            // TODO reference path must be valid (copy file)

            var titleStr = title?.Length > 0 ? $" \"{title}\"" : "";
            return $"![{alt}]({src}{titleStr})";
        }

        private static string ConvertParagraph(HtmlNode node)
        {
            if (node.Name != "p") throw new ArgumentException("Node must be of type paragraph");

            var sb = new StringBuilder();
            foreach (var childNode in node.ChildNodes)
            {
                sb.Append(Convert(childNode, All.Except(InlineContent).ToArray()));
            }

            return sb.ToString();
        }

        #endregion
    }
}
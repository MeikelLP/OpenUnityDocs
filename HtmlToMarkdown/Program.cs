using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using CommandLine;

namespace HtmlToMarkdown
{
    internal static class Program
    {
        private static readonly Regex TooManyEmptyLinesRegex =
            new Regex("(\r?\n){2,}", RegexOptions.Compiled);

        private static readonly Regex TableRegex = new Regex("<table[ \\w\\d=\"-.]+>", RegexOptions.Compiled);
        private static readonly Regex HtmlToMarkDownLinksRegex = new Regex("", RegexOptions.Compiled);

        private static readonly Regex TrimLinesRegex =
            new Regex("[ \\t]+\r?$", RegexOptions.Multiline | RegexOptions.Compiled);

        private static readonly Regex UselessDivsRegex =
            new Regex("(\\r?\\n?[ \\t]*<div[ \\w\\d=\\\"]+>\\r?\\n?)|(<\\/?div>)", RegexOptions.Compiled);


        private static async Task Main(string[] args)
        {
            var options = Parser.Default.ParseArguments<CommandLineArgs>(args).MapResult<CommandLineArgs, CommandLineArgs>(x => x, errors =>
            {
                foreach (var error in errors)
                {
                    Console.Error.Write(error);
                }

                return null;
            });

            if (options == null)
            {
                Environment.Exit(1);
            }
            
            options.ValidateOrExit();
            
#if !DEBUG
            var files = new DirectoryInfo(options.SourcePath).GetFiles().Where(x => x.Name != "30_search.html").ToArray();
#else
            var files = dir.GetFiles().Where(x => x.Name == "Accessibility.VisionUtility.GetColorBlindSafePalette.html")
                .ToArray();
            files = files.Take(1).ToArray();
#endif
            var parallelFiles = files.AsParallel();

            if (!Directory.Exists(options.OutPath))
            {
                Directory.CreateDirectory(options.OutPath);
            }

            var startTime = DateTime.Now;
            var failed = new List<string>();

#if !DEBUG
            var tasks = parallelFiles.Select(fileInfo =>
            {
                return Task.Run(async () =>
                {
#else
            foreach (var fileInfo in files)
            {
#endif
                try
                {
                    var str = await Html2Markdown(fileInfo.FullName);
                    var outPath = Path.Combine(options.OutPath, fileInfo.Name.Replace(".html", ".md"));
                    await File.WriteAllTextAsync(outPath, str);
                }
                catch (Exception e)
                {
                    failed.Add(fileInfo.FullName);
                }
#if !DEBUG
                });
            
            });

            await Task.WhenAll(tasks);
#else
            }
#endif


            var time = DateTime.Now - startTime;

            Console.WriteLine(
                $"Converted {files.Length:N0}x in {time:g}s. Successful: {files.Length - failed.Count:N0} | Failed {failed.Count:N0}x");
        }

        private static async Task<string> Html2Markdown(string fileName)
        {
            using (var sr = new StreamReader(fileName))
            {
                var xDoc = await XDocument.LoadAsync(sr, LoadOptions.PreserveWhitespace, CancellationToken.None);

                var relevantHtml = CleanupDocument(xDoc);

                var parent = XElement.Parse("<div></div>", LoadOptions.PreserveWhitespace);
                var section = XElement.Parse(relevantHtml, LoadOptions.PreserveWhitespace);
                parent.Add(section);

                parent.Add(section.Nodes());
                section.Remove();

                var subSections = parent.XPathSelectElements(".//div[./@class = 'subsection']").ToArray();
                parent.Add(subSections.Nodes());
                subSections.Remove();

                var str = parent.ToString();

                str = CustomMarkdown.StrongReplacerStart(str);
                str = CustomMarkdown.StrongReplacerEnd(str);
                str = CustomMarkdown.EmReplacerStart(str);
                str = CustomMarkdown.EmReplacerEnd(str);

                str = HtmlParser.ReplaceParagraph(str);

                str = HtmlParser.ReplacePre(str);
                str = HtmlParser.ReplaceImg(str);
                str = HtmlParser.ReplaceAnchors(str);

                str = CustomMarkdown.Header1Replacer(str);
                str = CustomMarkdown.Header2Replacer(str);
                str = CustomMarkdown.Header3Replacer(str);
                str = CustomMarkdown.Header4Replacer(str);
                str = CustomMarkdown.Header5Replacer(str);
                str = CustomMarkdown.Header6Replacer(str);
                str = CustomMarkdown.HeaderEndingReplacer(str);

                str = CustomMarkdown.BreakReplacer(str);

                str = ConvertTables(str);
                str = RemoveUselessDivs(str);
                str = FixTooManyEmptyLines(str);
                str = FixEmptyStartLines(str);

                str = HtmlParser.ReplaceEscapedHtml(str);
                str = FixEndLine(str);

                return str;
            }
        }

        private static string FixEndLine(string str)
        {
            return str.TrimEnd('\r', '\n') + Environment.NewLine;
        }

        private static string CleanupDocument(XNode xDoc)
        {
            xDoc.XPathSelectElement(
                    ".//*[contains(concat(' ', normalize-space(./@class), ' '), ' header-wrapper ')]")
                .Remove(); // remove header
            xDoc.XPathSelectElement(".//div[./@id = 'sidebar']").Remove(); // remove sidebar

            var sections =
                xDoc.XPathSelectElements(".//*[contains(concat(' ', normalize-space(./@class), ' '), ' section ')]")
                    .ToArray();

            // remove footer
            sections.Last().Remove();
            xDoc.XPathSelectElement(
                ".//*[contains(concat(' ', normalize-space(./@class), ' '), ' footer-wrapper ')]");

            // remove feedback buttons
            xDoc.XPathSelectElement(
                ".//*[contains(concat(' ', normalize-space(./@class), ' '), ' otherversionswrapper ')]")?.Remove();
            xDoc.XPathSelectElement(
                ".//*[contains(concat(' ', normalize-space(./@class), ' '), ' scrollToFeedback ')]")?.Remove();

            // remove bad objects
            xDoc.XPathSelectElements(".//*[contains(concat(' ', normalize-space(./@class), ' '), ' subsection ')]")
                .FirstOrDefault()?.Remove();
            xDoc.XPathSelectElement(".//div[contains(concat(' ', normalize-space(./@class), ' '), ' suggest ')]")?
                .Remove();

            xDoc.XPathSelectElements(".//div[./@class = 'clear']").Remove();
            xDoc.XPathSelectElements(".//a[./@href = '']").Remove();

            return sections.First().ToString();
        }

        private static string FixEmptyStartLines(string outString)
        {
            return outString.TrimStart(Environment.NewLine.ToCharArray());
        }


        private static string FixTooManyEmptyLines(string outString)
        {
            outString = TrimLinesRegex.Replace(outString, ""); 
            return TooManyEmptyLinesRegex.Replace(outString, Environment.NewLine + Environment.NewLine);
        }

        private static string RemoveUselessDivs(string str)
        {
            return UselessDivsRegex.Replace(str, "");
        }

        private static string FixUrls(string str)
        {
            return HtmlToMarkDownLinksRegex.Replace(str, "$1.md");
        }

        private static string ConvertTables(string outString)
        {
            var count = TableRegex.Matches(outString).Count;

            var sb = new StringBuilder();

            var lastIndex = 0;
            for (var i = 0; i < count; i++)
            {
                var startIndex = outString.IndexOf("<table", lastIndex, StringComparison.InvariantCulture);
                var closeIndex = outString.IndexOf("</table>", startIndex, StringComparison.InvariantCulture);
                var length = closeIndex - startIndex + "</table>".Length;


                var table = new string(outString.Skip(startIndex).Take(length).ToArray());

                var xElement = XElement.Parse(table);
                var rows = xElement.Elements("tr").ToArray();
                var strBefore = new string(outString.Skip(lastIndex).Take(startIndex).ToArray());

                if (rows.Length > 0)
                {
                    sb.Append(strBefore.TrimEnd(' ')); // append everything before the table

                    // table header
                    sb.Append("| ");
                    sb.Append(string.Join(" | ", Enumerable.Range(0, rows.Length).Select(x => "")));
                    sb.AppendLine(" |");
                    sb.Append("| ");
                    sb.Append(string.Join(" | ", Enumerable.Range(0, rows.Length).Select(x => "---")));
                    sb.AppendLine(" |");

                    foreach (var element in rows)
                    {
                        var cols = element.Elements("td");
                        sb.Append("| ");
                        sb.Append(string.Join(" | ", cols.Select(x => x.Value.Trim())));
                        sb.AppendLine(" |");
                    }
                }
                else
                {
                    sb.Append(strBefore);
                }

                lastIndex = length + startIndex;
            }

            sb.AppendLine(new string(outString.Skip(lastIndex).ToArray()));

            return sb.ToString();
        }
    }
}
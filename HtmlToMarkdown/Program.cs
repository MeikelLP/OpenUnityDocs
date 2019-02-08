using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Html2Markdown;

namespace HtmlToMarkdown
{
    class Program
    {
        private static readonly Regex TooManyEmptyLinesRegex = new Regex("[\n\r]{3,}", RegexOptions.Compiled);
        private static readonly Regex TableRegex = new Regex("<table[ \\w\\d=\"-.]+>", RegexOptions.Compiled);
        private static readonly Regex UselessDivsRegex = new Regex("(\\r?\\n?[ \\t]*<div[ \\w\\d=\"]+>\\r?\\n?)|(<\\/div>)", RegexOptions.Compiled);
        private static readonly string OutDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "UnityDocs");

        static async Task Main(string[] args)
        {
            var dir = new DirectoryInfo(
                @"C:\Program Files\Unity\Hub\Editor\2019.1.0b1\Editor\Data\Documentation\en\ScriptReference");
            var files = dir.GetFiles().Where(x => x.Name != "30_search.html").Take(1).ToArray();
            var parallelFiles = files.AsParallel();

            if (!Directory.Exists(OutDir))
            {
                Directory.CreateDirectory(OutDir);
            }
            
            var startTime = DateTime.Now;
            var failed = 0;
            
            var tasks = parallelFiles.Select(fileInfo =>
            {
                return Task.Run(async () =>
                {
                    try
                    {
                        var str = await Html2Markdown(fileInfo.FullName);
                        var outPath = Path.Combine(OutDir, fileInfo.Name.Replace(".html", ".md"));
                        await File.WriteAllTextAsync(outPath, str);
                    }
                    catch (Exception e)
                    {
                        failed++;
                    }
                });
            });

            await Task.WhenAll(tasks);

            var time = DateTime.Now - startTime;

            Console.WriteLine($"Converted {files.Length:D}x in {time:g}s. Successful: {files.Length - failed} | Failed {failed}");
        }

        public static async Task<string> Html2Markdown(string fileName)
        {
            using (var sr = new StreamReader(fileName))
            {
                var xDoc = await XDocument.LoadAsync(sr, LoadOptions.None, CancellationToken.None);

                var converter = new Converter();

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
                    ".//*[contains(concat(' ', normalize-space(./@class), ' '), ' otherversionswrapper ')]").Remove();
                xDoc.XPathSelectElement(
                    ".//*[contains(concat(' ', normalize-space(./@class), ' '), ' scrollToFeedback ')]").Remove();

                // remove bad objects
                xDoc.XPathSelectElements(".//*[contains(concat(' ', normalize-space(./@class), ' '), ' subsection ')]")
                    .First().Remove();
                xDoc.XPathSelectElement(".//div[contains(concat(' ', normalize-space(./@class), ' '), ' suggest ')]")
                    .Remove();

                xDoc.XPathSelectElements(".//div[./@class = 'clear']").Remove();

                var relevantHtml = sections.First().ToString();


                var outString = converter.Convert(relevantHtml);

                outString = ConvertTables(outString);
                outString = RemoveUselessDivs(outString);
                outString = FixTooManyEmptyLines(outString);
                outString = FixEmptyStartLines(outString);

                return outString;
            }
        }

        private static string FixEmptyStartLines(string outString)
        {
            return outString.TrimStart(Environment.NewLine.ToCharArray());
        }


        private static string FixTooManyEmptyLines(string outString)
        {
            return TooManyEmptyLinesRegex.Replace(outString, Environment.NewLine + Environment.NewLine);
        }

        private static string RemoveUselessDivs(string str)
        {
            return UselessDivsRegex.Replace(str, "");
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
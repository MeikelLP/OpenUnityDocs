using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using UnityDocsToMarkdown.Core;

namespace HtmlToMarkdown
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            var options = Parser.Default.ParseArguments<CommandLineArgs>(args).MapResult(x => x, errors =>
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

            var files = new DirectoryInfo(options.SourcePath)
                .GetFiles()
                .Where(x => x.Name != "30_search.html")
                .ToArray();
            var parallelFiles = files.AsParallel();

            if (!Directory.Exists(options.OutPath))
            {
                Directory.CreateDirectory(options.OutPath);
            }

            var startTime = DateTime.Now;
            var failed = new List<string>();


            Console.WriteLine($"Converting {files.Length:N0}x files in \"{options.SourcePath}\" to \"{options.OutPath}\"");
            var tasks = parallelFiles.Select(async fileInfo =>
            {
                try
                {
                    var html = await File.ReadAllTextAsync(fileInfo.FullName);
                    html = UnityCleaner.CleanupDocument(html);
                    var str = HtmlConverter.ToMarkDown(html);
                    var outPath = Path.Combine(options.OutPath, fileInfo.Name.Replace(".html", ".md"));
                    await File.WriteAllTextAsync(outPath, str);
                }
                catch (Exception)
                {
                    failed.Add(fileInfo.FullName);
                }
            });

            await Task.WhenAll(tasks);

            var time = DateTime.Now - startTime;

            Console.WriteLine(
                $"Converted {files.Length:N0}x in {time.TotalSeconds:F}s. Successful: {files.Length - failed.Count:N0} | Failed {failed.Count:N0}x");
        }
    }
}
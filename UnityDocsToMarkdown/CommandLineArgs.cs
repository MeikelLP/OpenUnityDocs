using System;
using System.IO;
using CommandLine;

namespace HtmlToMarkdown
{
    public class CommandLineArgs
    {
        private const string DefaultSourceDirectory =
            @"C:\Program Files\Unity\Hub\Editor\2019.3.0f3\Editor\Data\Documentation\en\ScriptReference";

        private static readonly string DefaultOutDirectory =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "UnityDocs");

        private const int SourcePathMissingExitCode = 2;

        [Option('p', "path", HelpText = "Source where to find html files.")]
        public string SourcePath { get; set; }

        [Option('o', "out-path", HelpText = "Where the output files shall be saved to.")]
        public string OutPath { get; set; }

        public void ValidateOrExit()
        {
            SourcePath = !string.IsNullOrWhiteSpace(SourcePath) ? SourcePath : DefaultSourceDirectory;

            var srcDir = new DirectoryInfo(SourcePath);

            if (!srcDir.Exists)
            {
                Console.Error.Write($"Path \"{srcDir.FullName}\" does not exist.");
                Environment.Exit(SourcePathMissingExitCode);
            }

            OutPath = !string.IsNullOrWhiteSpace(OutPath) ? OutPath : DefaultOutDirectory;
        }
    }
}
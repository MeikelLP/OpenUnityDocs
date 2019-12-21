using System;
using System.Text.RegularExpressions;

namespace UnityDocsToMarkdown.Core
{
    public static class CustomMarkdown
    {
        public static readonly Func<string, string> StrongReplacerStart = html => 
            StrongStartRegex.Replace(html, " **");
        
        public static readonly Func<string, string> StrongReplacerEnd = html => 
            StrongEndRegex.Replace(html, "** ");
        
        public static readonly Func<string, string> HeaderEndingReplacer = html => 
            HeaderEndingRegex.Replace(html, Environment.NewLine + Environment.NewLine);
        
        public static readonly Func<string, string> Header1Replacer = html => 
            Header1Regex.Replace(html, "# ");
        
        public static readonly Func<string, string> Header2Replacer = html => 
            Header2Regex.Replace(html, Environment.NewLine + Environment.NewLine + "## ");

        public static readonly Func<string, string> Header3Replacer = html => 
            Header3Regex.Replace(html, Environment.NewLine + Environment.NewLine + "### ");

        public static readonly Func<string, string> Header4Replacer = html => 
            Header4Regex.Replace(html, Environment.NewLine + Environment.NewLine + "#### ");

        public static readonly Func<string, string> Header5Replacer = html => 
            Header5Regex.Replace(html, Environment.NewLine + Environment.NewLine + "##### ");

        public static readonly Func<string, string> Header6Replacer = html => 
            Header6Regex.Replace(html, Environment.NewLine + Environment.NewLine + "###### ");

        public static readonly Func<string, string> EmReplacerStart = html => 
            EmStartRegex.Replace(html, " _");
        
        public static readonly Func<string, string> EmReplacerEnd = html => 
            EmEndRegex.Replace(html, "_ ");
        
        public static readonly Func<string, string> BreakReplacer = html => 
            BreakRegex.Replace(html, Environment.NewLine);

        private static readonly Regex StrongStartRegex = new Regex("[\\s]*<(strong|b)>", RegexOptions.Compiled);
        private static readonly Regex StrongEndRegex = new Regex("</(strong|b)>[\\s]*", RegexOptions.Compiled);
        private static readonly Regex HeaderEndingRegex = new Regex("[\\s]*</h[1-6]>[\\s]*", RegexOptions.Compiled);
        private static readonly Regex Header1Regex = new Regex("[\\s]*<h1[^>]*>[\\s]*", RegexOptions.Compiled);
        private static readonly Regex Header2Regex = new Regex("[\\s]*<h2[^>]*>[\\s]*", RegexOptions.Compiled);
        private static readonly Regex Header3Regex = new Regex("[\\s]*<h3[^>]*>[\\s]*", RegexOptions.Compiled);
        private static readonly Regex Header4Regex = new Regex("[\\s]*<h4[^>]*>[\\s]*", RegexOptions.Compiled);
        private static readonly Regex Header5Regex = new Regex("[\\s]*<h5[^>]*>[\\s]*", RegexOptions.Compiled);
        private static readonly Regex Header6Regex = new Regex("[\\s]*<h6[^>]*>[\\s]*", RegexOptions.Compiled);
        private static readonly Regex EmStartRegex = new Regex("[\\s]*<(em|i)>", RegexOptions.Compiled);
        private static readonly Regex EmEndRegex = new Regex("</(em|i)>[\\s]*", RegexOptions.Compiled);
        private static readonly Regex BreakRegex = new Regex("[\\s]*<br[^>]*>", RegexOptions.Compiled);
        public static readonly Regex AnchorRegex = new Regex("(.+)\\.html$");
        public static readonly Regex HeaderRegex = new Regex("h([\\d])", RegexOptions.Compiled);
        
        public static readonly Regex TooManyEmptyLinesRegex =
            new Regex("(\r?\n){2,}", RegexOptions.Compiled);

        public static readonly Regex TableRegex = new Regex("<table[ \\w\\d=\"-.]+>", RegexOptions.Compiled);
        public static readonly Regex HtmlToMarkDownLinksRegex = new Regex("", RegexOptions.Compiled);

        public static readonly Regex TrimLinesRegex =
            new Regex("[ \\t]+\r?$", RegexOptions.Multiline | RegexOptions.Compiled);

        public static readonly Regex UselessDivsRegex =
            new Regex("(\\r?\\n?[ \\t]*<div[ \\w\\d=\\\"]+>\\r?\\n?)|(<\\/?div>)", RegexOptions.Compiled);


    }
}
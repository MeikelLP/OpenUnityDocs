using HtmlAgilityPack;
using NUnit.Framework;
using UnityDocsToMarkdown.Core;

namespace UnityDocsToMarkdown.Tests
{
    [Parallelizable(ParallelScope.All)]
    public class TestConvertersAdvanced
    {
        [Test]
        public void Header1WithAnchor()
        {
            const string html = "<h6>AccelerationEvent <a href=\"between.html\">between</a></h1>";
            const string expected = @"###### AccelerationEvent [between](between.md ""between"")

";

            var node = HtmlNode.CreateNode(html);
            var markdown = HtmlConverter.Convert(node);

            Assert.AreEqual(expected, markdown);
        }

        [Test]
        public void TableWithAnchors()
        {
            const string html = @"<table>
    <thead>
        <tr>
            <th><a href=""google.com"">Abc</a></th>
            <th>Def</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td><a href=""google.com"">Ghi</a></td>
            <td>Jkl</td>
        </tr>
        <tr>
            <td>Mno</td>
            <td>Pqr</td>
        </tr>
    </tbody>
</table>";
            const string expected = @"| [Abc](google.com ""Abc"") | Def |
| --- | --- |
| [Ghi](google.com ""Ghi"") | Jkl |
| Mno | Pqr |

";

            var node = HtmlNode.CreateNode(html);
            var markdown = HtmlConverter.Convert(node);

            Assert.AreEqual(expected, markdown);
        }

        [Test]
        public void ParagraphWithAnchor()
        {
            const string html = "<p>Trace a line <a href=\"between.html\">between</a> two points on the NavMesh.</p>";
            const string expected = @"Trace a line [between](between.md ""between"") two points on the NavMesh.

";

            var node = HtmlNode.CreateNode(html);
            var markdown = HtmlConverter.Convert(node);

            Assert.AreEqual(expected, markdown);
        }

        [Test]
        public void ConvertUnorderedListWithUnorderedList()
        {
            const string html = @"<ul>
    <li>Abc</li>
    <li>
        <ul>
            <li>Def</li>
            <li>Ghi</li>
        </ul>
    </li>
</ul>";
            const string expected = @"* Abc
  * Def
  * Ghi

";

            var node = HtmlNode.CreateNode(html);
            var markdown = HtmlConverter.Convert(node);

            Assert.AreEqual(expected, markdown);
        }

        [Test]
        public void ConvertUnorderedListWithAnchor()
        {
            const string html = @"<ul>
    <li>Abc</li>
    <li><a href=""google.com"">Def</a></li>>
</ul>";
            const string expected = @"* Abc
* [Def](google.com ""Def"")

";

            var node = HtmlNode.CreateNode(html);
            var markdown = HtmlConverter.Convert(node);

            Assert.AreEqual(expected, markdown);
        }

        [Test]
        public void ConvertAnchorWithFormatterElement()
        {
            const string html = @"<a href=""abc.html""><b>Google</b></a>";
            const string expected = @"[**Google**](abc.md ""Google"")";

            var node = HtmlNode.CreateNode(html);
            var markdown = HtmlConverter.Convert(node);

            Assert.AreEqual(expected, markdown);
        }

        [Test]
        public void ConvertAnchorWithImage()
        {
            const string html = @"<a href=""abc.html""><img src=""google.com"" alt=""Google"" /></a>";
            const string expected = @"[![Google](google.com ""Google"")](abc.md """")";

            var node = HtmlNode.CreateNode(html);
            var markdown = HtmlConverter.Convert(node);

            Assert.AreEqual(expected, markdown);
        }

        [Test]
        public void ConvertNestedDivs()
        {
            const string html = @"
  <div class=""header-wrapper"">
    <div id=""header"" class=""header"">
      <div class=""content"">
        <div class=""spacer"">
          <div class=""menu"">
            <div id=""nav-open"" for=""nav-input""><span></span></div>
            <div class=""logo""><a href=""""></a></div>
            <div class=""search-form"">
              <form action=""30_search.html"" method=""get"" class=""apisearch""><input type=""text"" name=""q""
                  placeholder=""Search scripting..."" autosave=""Unity Reference"" results=""5"" class=""sbox field""
                  id=""q""></input><input type=""submit"" class=""submit""></input></form>
            </div>
            <ul>
              <li><a href=""../Manual/index.html"">Manual</a></li>
              <li><a href=""../ScriptReference/index.html"" class=""selected"">Scripting API</a></li>
            </ul>
          </div>
        </div>
      </div>
    </div>
  </div>";
            const string expected = @"* [Manual](../Manual/index.md ""Manual"")
* [Scripting API](../ScriptReference/index.md ""Scripting API"")

";
            
            var markdown = HtmlConverter.ToMarkDown(html);

            Assert.AreEqual(expected, markdown);
        }
    }
}
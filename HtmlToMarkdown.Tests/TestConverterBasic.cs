using System;
using HtmlAgilityPack;
using NUnit.Framework;
using UnityDocsToMarkdown.Core;

namespace UnityDocsToMarkdown.Tests
{
    [Parallelizable(ParallelScope.All)]
    public class TestConvertersBasic
    {
        [Test]
        public void AnchorConvert()
        {
            const string html = "<a href=\"UnityEngine.InputLegacyModule.html\">UnityEngine.InputLegacyModule</a>";
            const string expected =
                "[UnityEngine.InputLegacyModule](UnityEngine.InputLegacyModule.md \"UnityEngine.InputLegacyModule\")";

            var node = HtmlNode.CreateNode(html);
            var markdown = HtmlConverter.Convert(node);

            Assert.AreEqual(expected, markdown);
        }

        [Test]
        public void PreConvert()
        {
            const string html = @"<pre class=""codeExampleCS"">
// TargetReachable
using UnityEngine;
using UnityEngine.AI;<br /><br />public class TargetReachable : <a href=""MonoBehaviour.html"">MonoBehaviour</a>
{
    public <a href=""Transform.html"">Transform</a> target;
    private <a href=""AI.NavMeshHit.html"">NavMeshHit</a> hit;
    private bool blocked = false;<br /><br />    void <a href=""PlayerLoop.Update.html"">Update</a>()
    {
        blocked = <a href=""AI.NavMesh.Raycast.html"">NavMesh.Raycast</a>(transform.position, target.position, out hit, <a href=""AI.NavMesh.AllAreas.html"">NavMesh.AllAreas</a>);
        <a href=""Debug.DrawLine.html"">Debug.DrawLine</a>(transform.position, target.position, blocked ? <a href=""Color-red.html"">Color.red</a> : <a href=""Color-green.html"">Color.green</a>);<br /><br />        if (blocked)
            <a href=""Debug.DrawRay.html"">Debug.DrawRay</a>(hit.position, <a href=""Vector3-up.html"">Vector3.up</a>, <a href=""Color-red.html"">Color.red</a>);
    }
}
</pre>";
            const string expected = @"```csharp
// TargetReachable
using UnityEngine;
using UnityEngine.AI;

public class TargetReachable : MonoBehaviour
{
    public Transform target;
    private NavMeshHit hit;
    private bool blocked = false;

    void Update()
    {
        blocked = NavMesh.Raycast(transform.position, target.position, out hit, NavMesh.AllAreas);
        Debug.DrawLine(transform.position, target.position, blocked ? Color.red : Color.green);

        if (blocked)
            Debug.DrawRay(hit.position, Vector3.up, Color.red);
    }
}
```

";
            var node = HtmlNode.CreateNode(html);
            var markdown = HtmlConverter.Convert(node);

            Assert.AreEqual(expected, markdown);
        }

        [Test]
        public void ConvertHeader1()
        {
            const string html = "<h1>AccelerationEvent</h1>";
            const string expected = @"# AccelerationEvent

";

            var node = HtmlNode.CreateNode(html);
            var markdown = HtmlConverter.Convert(node);

            Assert.AreEqual(expected, markdown);
        }

        [Test]
        public void ConvertHeader6()
        {
            const string html = "<h6>AccelerationEvent</h1>";
            const string expected = @"###### AccelerationEvent

";

            var node = HtmlNode.CreateNode(html);
            var markdown = HtmlConverter.Convert(node);

            Assert.AreEqual(expected, markdown);
        }

        [Test]
        public void ConvertImage()
        {
            const string html = "<img src=\"logo.png\" alt=\"text\" title=\"Title\"/>";
            const string expected = "![text](logo.png \"Title\")";

            var node = HtmlNode.CreateNode(html);
            var markdown = HtmlConverter.Convert(node);

            Assert.AreEqual(expected, markdown);
        }

        [Test]
        public void ConvertParagraph()
        {
            const string html = "<p>Trace a line between two points on the NavMesh.</p>";
            const string expected = @"Trace a line between two points on the NavMesh.

";

            var node = HtmlNode.CreateNode(html);
            var markdown = HtmlConverter.Convert(node);

            Assert.AreEqual(expected, markdown);
        }

        [Test]
        public void ConvertBreak()
        {
            const string html = "<br />";
            var expected = Environment.NewLine + Environment.NewLine;

            var node = HtmlNode.CreateNode(html);
            var markdown = HtmlConverter.Convert(node);

            Assert.AreEqual(expected, markdown);
        }

        [Test]
        public void ConvertCode()
        {
            const string html = "<code>abc</code>";
            const string expected = "`abc`";

            var node = HtmlNode.CreateNode(html);
            var markdown = HtmlConverter.Convert(node);

            Assert.AreEqual(expected, markdown);
        }

        [Test]
        public void ConvertBold()
        {
            const string html = "<b>abc</b>";
            const string expected = "**abc**";

            var node = HtmlNode.CreateNode(html);
            var markdown = HtmlConverter.Convert(node);

            Assert.AreEqual(expected, markdown);
        }

        [Test]
        public void ConvertItalic()
        {
            const string html = "<i>abc</i>";
            const string expected = "_abc_";

            var node = HtmlNode.CreateNode(html);
            var markdown = HtmlConverter.Convert(node);

            Assert.AreEqual(expected, markdown);
        }

        [Test]
        public void ConvertTable()
        {
            const string html = @"<table>
    <thead>
        <tr>
            <th>Abc</th>
            <th>Def</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>Ghi</td>
            <td>Jkl</td>
        </tr>
        <tr>
            <td>Mno</td>
            <td>Pqr</td>
        </tr>
    </tbody>
</table>";
            const string expected = @"| Abc | Def |
| --- | --- |
| Ghi | Jkl |
| Mno | Pqr |

";

            var node = HtmlNode.CreateNode(html);
            var markdown = HtmlConverter.Convert(node);

            Assert.AreEqual(expected, markdown);
        }

        [Test]
        public void ConvertOrderedList()
        {
            const string html = @"<ol>
    <li>Abc</li>
    <li>Def</li>
</ol>";
            const string expected = @"1. Abc
2. Def

";

            var node = HtmlNode.CreateNode(html);
            var markdown = HtmlConverter.Convert(node);

            Assert.AreEqual(expected, markdown);
        }

        [Test]
        public void ConvertUnorderedList()
        {
            const string html = @"<ul>
    <li>Abc</li>
    <li>Def</li>
</ul>";
            const string expected = @"* Abc
* Def

";

            var node = HtmlNode.CreateNode(html);
            var markdown = HtmlConverter.Convert(node);

            Assert.AreEqual(expected, markdown);
        }
    }
}
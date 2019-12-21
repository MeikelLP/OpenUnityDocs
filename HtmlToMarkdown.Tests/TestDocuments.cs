using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityDocsToMarkdown.Core;

namespace UnityDocsToMarkdown.Tests
{
    public class TestDocuments
    {
        [Test]
        public void TestAccelerationEvent()
        {
            var html = File.ReadAllText("fixtures/AccelerationEvent.html");

            var markDown = HtmlConverter.ToMarkDown(html);
            TestContext.Out.WriteLine(markDown);
            Assert.NotNull(markDown);
        }
    }
}
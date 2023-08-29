using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Svg.Css;
using NUnit.Framework;
using ExCSS;
using Fizzler;

namespace Svg.UnitTests
{
    /// <summary>
    ///This is a test class for CssQueryTest and is intended
    ///to contain all CssQueryTest Unit Tests
    ///</summary>
    [TestFixture]
    public class CssQueryTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        private void TestSelectorSpecificity(string selector, int specificity)
        {
            var stylesheetParser = new StylesheetParser(true, true);
            var stylesheet = stylesheetParser.Parse(selector + " {color:black}");
            Assert.AreEqual(specificity, CssQuery.GetSpecificity(stylesheet.StyleRules.First().Selector));
        }

        /// <summary>
        ///A test for GetSpecificity
        ///</summary>
        ///<remarks>Lifted from http://www.smashingmagazine.com/2007/07/27/css-specificity-things-you-should-know/, and http://css-tricks.com/specifics-on-css-specificity/ </remarks>
        [Test]
        [TestCase("*", 0x0)]
        [TestCase("li", 0x10)]
        [TestCase("li:first-line", 0x20)]
        [TestCase("ul li", 0x20)]
        [TestCase("ul ol+li", 0x30)]
        [TestCase("h1 + *[rel=up]", 0x110)]
        [TestCase("ul ol li.red", 0x130)]
        [TestCase("li.red.level", 0x210)]
        [TestCase("p", 0x010)]
        [TestCase("div p", 0x020)]
        [TestCase(".sith", 0x100)]
        [TestCase("div p.sith", 0x120)]
        [TestCase("#sith", 0x1000)]
        [TestCase("body #darkside .sith p", 0x1120)]
        [TestCase("body #content .data img:hover", 0x1220)]
        [TestCase("a#a-02", 0x1010)]
        [TestCase("a[id=\"a-02\"]", 0x0110)]
        [TestCase("ul#nav li.active a", 0x1130)]
        [TestCase("body.ie7 .col_3 h2 ~ h2", 0x0230)]
        [TestCase("#footer *:not(nav) li", 0x1110)]
        [TestCase("ul > li ul li ol li:first-letter", 0x0070)]
        public void RunSpecificityTests(string selector, int specifity)
        {
            TestSelectorSpecificity(selector, specifity);
        }

        [Test]
        [TestCase("font-size:13px;", "font-size:13px;")]
        [TestCase("font-size:13px;font-style:normal;", "font-size:13px;font-style:normal;")]
        [TestCase("font-size:13px;font-style:normal;font-weight:bold;", "font-size:13px;font-style:normal;font-weight:bold;")]
        [TestCase("font-family:Nimbus Sans L,'Arial Narrow',sans-serif;Sans L',sans-serif;", "font-family:Nimbus Sans L, \"Arial Narrow\", sans-serif;")]
        public void TestStyleDeclarations(string style, string expected)
        {
            var actual = new StringBuilder();

            var stylesheetParser = new StylesheetParser(true, true);
            var stylesheet = stylesheetParser.Parse("#a{" + style + "}");
            foreach (var rule in stylesheet.StyleRules)
                foreach (var declaration in rule.Style)
                    actual.Append(declaration.Name).Append(":").Append(declaration.Value).Append(";");

            Assert.AreEqual(expected, actual.ToString());
        }

        [Test]
        [TestCase("#testId.test1", "struct-use-11-f")]
        [TestCase("*.test2", "struct-use-11-f")]
        [TestCase("circle.test3", "struct-use-11-f")]
        [TestCase(".descendant circle.test4", "struct-use-11-f")]
        [TestCase(".child", "struct-use-11-f")]
        [TestCase("circle.test5", "struct-use-11-f")]
        [TestCase(".child > circle.test5", "struct-use-11-f")]
        [TestCase(".test6:first-child", "struct-use-11-f")]
        [TestCase(".sibling + circle.test7", "struct-use-11-f")]
        [TestCase("circle[cx].test8", "struct-use-11-f")]
        [TestCase("circle[cx=\"50\"].test9", "struct-use-11-f")]
        [TestCase("circle[foo~=\"warning1\"].test10", "struct-use-11-f")]
        [TestCase("circle[lang|=\"en\"].test11", "struct-use-11-f")]
        [TestCase(".test12", "struct-use-11-f")]
        [TestCase(".twochildren:first-child", "struct-use-11-f")]
        public void RunSelectorTests(string selector, string baseName)
        {
            var elementFactory = new SvgElementFactory();
            var testSuite = Path.Combine(ImageTestDataSource.SuiteTestsFolder, "W3CTestSuite");
            string basePath = testSuite;
            var svgPath = Path.Combine(basePath, "svg", baseName + ".svg");
            var styles = new List<ISvgNode>();
            using (var xmlFragment = File.Open(svgPath, FileMode.Open))
            {
                using (var xmlTextReader = new XmlTextReader(xmlFragment))
                {
                    var svgDocument = SvgDocument.Create<SvgDocument>(xmlTextReader, elementFactory, styles);

                    var rootNode = new NonSvgElement();
                    rootNode.Children.Add(svgDocument);

                    SvgElementOpsFunc.NodeDebug = SvgElementOps.NodeDebug = nameof(SvgElementOpsFunc.Type);

                    Debug.WriteLine(Environment.NewLine);
                    Debug.WriteLine("Fizzler:\r\n");
                    var fizzlerElements = QuerySelectorFizzlerAll(rootNode, selector, elementFactory).ToList();
                    Debug.WriteLine(Environment.NewLine);
                    Debug.WriteLine("ExCss:\r\n");
                    var exCssElements = QuerySelectorExCssAll(rootNode, selector, elementFactory).ToList();
                    Debug.WriteLine(Environment.NewLine);

                    var areEqual = fizzlerElements.SequenceEqual(exCssElements);
                    if (!areEqual)
                    {
                        Assert.IsTrue(areEqual, "should select the same elements");
                    }
                    else
                    {
                        Assert.IsTrue(areEqual, "should select the same elements");
                    }
                }
            }
        }

        private IEnumerable<SvgElement> QuerySelectorExCssAll(NonSvgElement elem, string selector, SvgElementFactory elementFactory)
        {
            var stylesheetParser = new StylesheetParser(true, true);
            var stylesheet = stylesheetParser.Parse(selector + " {color:black}");
            var exCssSelector = stylesheet.StyleRules.First().Selector;
            return elem.QuerySelectorAll(exCssSelector, elementFactory);
        }

        private IEnumerable<SvgElement> QuerySelectorFizzlerAll(NonSvgElement elem, string selector, SvgElementFactory elementFactory)
        {
            var generator = new SelectorGenerator<SvgElement>(new SvgElementOps(elementFactory));
            Fizzler.Parser.Parse(selector, generator);
            return generator.Selector(Enumerable.Repeat(elem, 1));
        }
    }
}

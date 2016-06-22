using System;
using System.Linq;
using Lury.Compiling.Lexer;
using NUnit.Framework;

namespace UnitTest
{
    [TestFixture]
    public class LexerTest
    {
        [Test]
        public void SourceNameTest()
        {
            const string sourceName = "TestSource";
            var lexer = new Lexer(sourceName, string.Empty);
            Assert.AreEqual(sourceName, lexer.SourceName);
        }

        [Test]
        public void TokenOutputError()
        {
            var lexer = new Lexer(string.Empty, string.Empty);

            // ReSharper disable once UnusedVariable
            Assert.Throws<InvalidOperationException>(() => { var foo = lexer.TokenOutput; });
        }

        [Test]
        public void TokenizeTest()
        {
            var lexer = new Lexer(string.Empty, string.Empty);
            Assert.IsTrue(lexer.Tokenize());

            CollectionAssert.AreEqual(new[] { "EndOfFile" }, lexer.TokenOutput.Select(t => t.Entry.Name).ToArray());
        }

        [Test]
        public void TokenizeError()
        {
            var lexer = new Lexer(string.Empty, string.Empty);
            lexer.Tokenize();
            Assert.Throws<InvalidOperationException>(() => lexer.Tokenize());
        }
    }
}

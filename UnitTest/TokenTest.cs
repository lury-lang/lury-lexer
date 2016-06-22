using System;
using Lury.Compiling.Lexer;
using Lury.Compiling.Utils;
using NUnit.Framework;

namespace UnitTest
{
    [TestFixture]
    public class TokenTest
    {
        const string Name = "dummyName";
        const string NameShort = "n";
        const string Regex = "dummyRegex";
        const string Source = "dummyText";
        const int Index = 2;
        const int Length = 3;
        static CharPosition position;
        static RegexTokenEntry entry, entryShort;

        [OneTimeSetUp]
        public static void ClassInitialize(TestContext context)
        {
            position = new CharPosition(1, 3);
            entry = new RegexTokenEntry(Name, Regex);
            entryShort = new RegexTokenEntry(NameShort, Regex);
        }

        [Test]
        public void LengthTest()
        {
            Token token = new Token(entry, string.Empty, Source, Index, Length);
            Assert.AreEqual(Length, token.Length);
        }

        [Test]
        public void EntryTest()
        {
            Token token = new Token(entry, string.Empty, Source, Index, Length);
            Assert.AreEqual(entry, token.Entry);
        }

        [Test]
        [TestCase("mmy")]
        [TestCase("")]
        public void TextTest(string tokenText)
        {
            Token token = new Token(entry, tokenText, Source, Index, Length);
            Assert.AreEqual(tokenText, token.Text);
        }

        [Test]
        public void IndexTest()
        {
            Token token = new Token(entry, string.Empty, Source, Index, Length);
            Assert.AreEqual(Index, token.Index);
        }

        [Test]
        [TestCase(Source, 2, 3, 1, 3)]
        [TestCase(Source, 9, 0, 1, 10)]
        [TestCase("", 0, 0, 1, 1)]
        public void PositionTest(string sourceCode, int index, int length, int line, int column)
        {
            Token token = new Token(entry, string.Empty, sourceCode, index, length);
            Assert.AreEqual(new CharPosition(line, column), token.CodePosition.CharPosition);
        }

        [Test]
        public void ToStringTest()
        {
            Token token = new Token(entry, string.Empty, Source, Index, Length);
            string tokenString = token.ToString();

            Assert.IsTrue(tokenString.Contains(position.Line.ToString()));
            Assert.IsTrue(tokenString.Contains(position.Column.ToString()));
            Assert.IsTrue(tokenString.Contains(entry.Name));
            Assert.IsTrue(tokenString.Contains("mmy"));
        }

        [Test]
        public void ToStringTestShort()
        {
            Token token = new Token(entryShort, string.Empty, Source, Index, Length);
            string tokenString = token.ToString();

            Assert.IsTrue(tokenString.Contains(position.Line.ToString()));
            Assert.IsTrue(tokenString.Contains(position.Column.ToString()));
            Assert.IsTrue(tokenString.Contains(entryShort.Name));
        }

        private static readonly TestCaseData[] ConstructorErrorTestCases =
        {
            new TestCaseData(entry, string.Empty, Source, Source.Length, 1),
            new TestCaseData(null, string.Empty, Source, 0, 1),
            new TestCaseData(entry, null, Source, 0, 1),
            new TestCaseData(entry, string.Empty, null, 0, 1),
            new TestCaseData(entry, string.Empty, Source, -1, 1),
            new TestCaseData(entry, string.Empty, Source, Source.Length + 1, -1),
            new TestCaseData(entry, string.Empty, Source, 0, -1)
        };

        [Test]
        [TestCaseSource(nameof(ConstructorErrorTestCases))]
        public void ConstructorError(TokenEntry entry, string sourceName, string sourceCode, int index, int length)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Token(entry, sourceName, sourceCode, index, length));
        }
    }
}

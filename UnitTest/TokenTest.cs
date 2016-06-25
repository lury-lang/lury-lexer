using System;
using Lury.Compiling.Lexer;
using Lury.Compiling.Utils;
using NUnit.Framework;

namespace UnitTest
{
    [TestFixture]
    public class TokenTest
    {
        private const string Name = "dummyName";
        private const string NameShort = "n";
        private const string Regex = "dummyRegex";
        private const string Source = "dummyText";
        private const int Index = 2;
        private const int Length = 3;
        private static CharPosition _position;
        private static RegexTokenEntry _entry;
        private static RegexTokenEntry _entryShort;

        [OneTimeSetUp]
        public static void OneTimeSetUp()
        {
            _position = new CharPosition(1, 3);
            _entry = new RegexTokenEntry(Name, Regex);
            _entryShort = new RegexTokenEntry(NameShort, Regex);
        }

        [Test]
        public void LengthTest()
        {
            var token = new Token(_entry, string.Empty, Source, Index, Length);
            Assert.AreEqual(Length, token.Length);
        }

        [Test]
        public void EntryTest()
        {
            var token = new Token(_entry, string.Empty, Source, Index, Length);
            Assert.AreEqual(_entry, token.Entry);
        }

        [Test]
        [TestCase("mmy")]
        [TestCase("")]
        public void TextTest(string tokenText)
        {
            var token = new Token(_entry, tokenText, Source, Index, Length);
            Assert.AreEqual(tokenText, token.Text);
        }

        [Test]
        public void IndexTest()
        {
            var token = new Token(_entry, string.Empty, Source, Index, Length);
            Assert.AreEqual(Index, token.Index);
        }

        [Test]
        [TestCase(Source, 2, 3, 1, 3)]
        [TestCase(Source, 9, 0, 1, 10)]
        [TestCase("", 0, 0, 1, 1)]
        public void PositionTest(string sourceCode, int index, int length, int line, int column)
        {
            var token = new Token(_entry, string.Empty, sourceCode, index, length);
            Assert.AreEqual(new CharPosition(line, column), token.CodePosition.CharPosition);
        }

        [Test]
        public void ToStringTest()
        {
            var token = new Token(_entry, string.Empty, Source, Index, Length);
            var tokenString = token.ToString();

            Assert.IsTrue(tokenString.Contains(_position.Line.ToString()));
            Assert.IsTrue(tokenString.Contains(_position.Column.ToString()));
            Assert.IsTrue(tokenString.Contains(_entry.Name));
            Assert.IsTrue(tokenString.Contains("mmy"));
        }

        [Test]
        public void ToStringTestShort()
        {
            var token = new Token(_entryShort, string.Empty, Source, Index, Length);
            var tokenString = token.ToString();

            Assert.IsTrue(tokenString.Contains(_position.Line.ToString()));
            Assert.IsTrue(tokenString.Contains(_position.Column.ToString()));
            Assert.IsTrue(tokenString.Contains(_entryShort.Name));
        }

        private static readonly TestCaseData[] ConstructorErrorTestCases =
        {
            new TestCaseData(_entry, string.Empty, Source, Source.Length, 1),
            new TestCaseData(null, string.Empty, Source, 0, 1),
            new TestCaseData(_entry, null, Source, 0, 1),
            new TestCaseData(_entry, string.Empty, null, 0, 1),
            new TestCaseData(_entry, string.Empty, Source, -1, 1),
            new TestCaseData(_entry, string.Empty, Source, Source.Length + 1, -1),
            new TestCaseData(_entry, string.Empty, Source, 0, -1)
        };

        [Test]
        [TestCaseSource(nameof(ConstructorErrorTestCases))]
        public void ConstructorError(TokenEntry entry, string sourceName, string sourceCode, int index, int length)
        {
            // ReSharper disable once ObjectCreationAsStatement
            Assert.Throws<ArgumentOutOfRangeException>(() => new Token(entry, sourceName, sourceCode, index, length));
        }
    }
}

using System;
using System.Collections.Generic;
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
        private static readonly CharPosition Position;
        private static readonly RegexTokenEntry Entry;
        private static readonly RegexTokenEntry EntryShort;
        
        static TokenTest()
        {
            Position = new CharPosition(1, 3);
            Entry = new RegexTokenEntry(Name, Regex);
            EntryShort = new RegexTokenEntry(NameShort, Regex);
        }

        [Test]
        public void LengthTest()
        {
            var token = new Token(Entry, string.Empty, Source, Index, Length);
            Assert.AreEqual(Length, token.Length);
        }

        [Test]
        public void EntryTest()
        {
            var token = new Token(Entry, string.Empty, Source, Index, Length);
            Assert.AreEqual(Entry, token.Entry);
        }

        [Test]
        [TestCase("mmy", Index, Length)]
        [TestCase("", 0, 0)]
        public void TextTest(string tokenText, int index, int length)
        {
            var token = new Token(Entry, string.Empty, Source, index, length);
            Assert.AreEqual(tokenText, token.Text);
        }

        [Test]
        public void IndexTest()
        {
            var token = new Token(Entry, string.Empty, Source, Index, Length);
            Assert.AreEqual(Index, token.Index);
        }

        [Test]
        [TestCase(Source, 2, 3, 1, 3)]
        [TestCase(Source, 9, 0, 1, 10)]
        [TestCase("", 0, 0, 1, 1)]
        public void PositionTest(string sourceCode, int index, int length, int line, int column)
        {
            var token = new Token(Entry, string.Empty, sourceCode, index, length);
            Assert.AreEqual(new CharPosition(line, column), token.CodePosition.CharPosition);
        }

        [Test]
        public void ToStringTest()
        {
            var token = new Token(Entry, string.Empty, Source, Index, Length);
            var tokenString = token.ToString();

            Assert.IsTrue(tokenString.Contains(Position.Line.ToString()));
            Assert.IsTrue(tokenString.Contains(Position.Column.ToString()));
            Assert.IsTrue(tokenString.Contains(Entry.Name));
            Assert.IsTrue(tokenString.Contains("mmy"));
        }

        [Test]
        public void ToStringTestShort()
        {
            var token = new Token(EntryShort, string.Empty, Source, Index, Length);
            var tokenString = token.ToString();

            Assert.IsTrue(tokenString.Contains(Position.Line.ToString()));
            Assert.IsTrue(tokenString.Contains(Position.Column.ToString()));
            Assert.IsTrue(tokenString.Contains(EntryShort.Name));
        }

        private static IEnumerable<TestCaseData> ConstructorErrorTestCases1
        {
            get
            {
                yield return new TestCaseData(Entry, string.Empty, Source, Source.Length, 1);
                yield return new TestCaseData(Entry, string.Empty, Source, -1, 1);
                yield return new TestCaseData(Entry, string.Empty, Source, Source.Length + 1, -1);
                yield return new TestCaseData(Entry, string.Empty, Source, 0, -1);
            }
        }

        private static IEnumerable<TestCaseData> ConstructorErrorTestCases2
        {
            get
            {
                yield return new TestCaseData(null, string.Empty, Source, 0, 1);
                yield return new TestCaseData(Entry, null, Source, 0, 1);
                yield return new TestCaseData(Entry, string.Empty, null, 0, 1);
            }
        }

        [Test]
        [TestCaseSource(nameof(ConstructorErrorTestCases1))]
        public void ConstructorError1(TokenEntry entry, string sourceName, string sourceCode, int index, int length)
        {
            // ReSharper disable once ObjectCreationAsStatement
            Assert.Throws<ArgumentOutOfRangeException>(() => new Token(entry, sourceName, sourceCode, index, length));
        }

        [Test]
        [TestCaseSource(nameof(ConstructorErrorTestCases2))]
        public void ConstructorError2(TokenEntry entry, string sourceName, string sourceCode, int index, int length)
        {
            // ReSharper disable once ObjectCreationAsStatement
            Assert.Throws<ArgumentNullException>(() => new Token(entry, sourceName, sourceCode, index, length));
        }
    }
}

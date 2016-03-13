using System;
using Lury.Compiling.Lexer;
using Lury.Compiling.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    [TestClass]
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

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            position = new CharPosition(1, 3);
            entry = new RegexTokenEntry(Name, Regex);
            entryShort = new RegexTokenEntry(NameShort, Regex);
        }

        [TestMethod]
        public void LengthTest()
        {
            Token token = new Token(entry, string.Empty, Source, Index, Length);
            Assert.AreEqual(Length, token.Length);
        }

        [TestMethod]
        public void EntryTest()
        {
            Token token = new Token(entry, string.Empty, Source, Index, Length);
            Assert.AreEqual(entry, token.Entry);
        }

        [TestMethod]
        public void TextTest()
        {
            Token token = new Token(entry, string.Empty, Source, Index, Length);
            Assert.AreEqual("mmy", token.Text);
        }

        [TestMethod]
        public void TextTest2()
        {
            Token token = new Token(entry, string.Empty, Source, Index, 0);
            Assert.AreEqual(string.Empty, token.Text);
        }

        [TestMethod]
        public void IndexTest()
        {
            Token token = new Token(entry, string.Empty, Source, Index, Length);
            Assert.AreEqual(Index, token.Index);
        }

        [TestMethod]
        public void PositionTest()
        {
            Token token = new Token(entry, string.Empty, Source, Index, Length);
            Assert.AreEqual(position, token.CodePosition.CharPosition);
        }

        [TestMethod]
        public void PositionTest2()
        {
            Token token = new Token(entry, string.Empty, Source, Source.Length, 0);
            Assert.AreEqual(new CharPosition(1, 10), token.CodePosition.CharPosition);
        }

        [TestMethod]
        public void PositionTest3()
        {
            Token token = new Token(entry, string.Empty, string.Empty, 0, 0);
            Assert.AreEqual(CharPosition.BasePosition, token.CodePosition.CharPosition);
        }

        [TestMethod]
        public void ToStringTest()
        {
            Token token = new Token(entry, string.Empty, Source, Index, Length);
            string tokenString = token.ToString();

            Assert.IsTrue(tokenString.Contains(position.Line.ToString()));
            Assert.IsTrue(tokenString.Contains(position.Column.ToString()));
            Assert.IsTrue(tokenString.Contains(entry.Name));
            Assert.IsTrue(tokenString.Contains("mmy"));
        }

        [TestMethod]
        public void ToStringTestShort()
        {
            Token token = new Token(entryShort, string.Empty, Source, Index, Length);
            string tokenString = token.ToString();

            Assert.IsTrue(tokenString.Contains(position.Line.ToString()));
            Assert.IsTrue(tokenString.Contains(position.Column.ToString()));
            Assert.IsTrue(tokenString.Contains(entryShort.Name));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ConstructorError1()
        {
            Token token = new Token(entry, string.Empty, Source, Source.Length, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorError2()
        {
            Token token = new Token(null, string.Empty, Source, 0, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorError3()
        {
            Token token = new Token(entry, null, Source, 0, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorError4()
        {
            Token token = new Token(entry, string.Empty, null, 0, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ConstructorError5()
        {
            Token token = new Token(entry, string.Empty, Source, -1, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ConstructorError6()
        {
            Token token = new Token(entry, string.Empty, Source, Source.Length + 1, -1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ConstructorError7()
        {
            Token token = new Token(entry, string.Empty, Source, 0, -1);
        }
    }
}

using System;
using Lury.Compiling.Lexer;
using Lury.Compiling.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    [TestClass]
    public class TokenTest
    {
        const string name = "dummyName";
        const string nameShort = "n";
        const string regex = "dummyRegex";
        const string text = "dummyText";
        const int index = 42;
        static CharPosition position;
        static TokenEntry entry, entryShort;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            position = new CharPosition(4, 2);
            entry = new TokenEntry(name, regex);
            entryShort = new TokenEntry(nameShort, regex);
        }

        [TestMethod]
        public void EntryTest()
        {
            Token token = new Token(entry, text, index, position);
            Assert.AreEqual(entry, token.Entry);
        }

        [TestMethod]
        public void TextTest()
        {
            Token token = new Token(entry, text, index, position);
            Assert.AreEqual(text, token.Text);
        }

        [TestMethod]
        public void IndexTest()
        {
            Token token = new Token(entry, text, index, position);
            Assert.AreEqual(index, token.Index);
        }

        [TestMethod]
        public void PositionTest()
        {
            Token token = new Token(entry, text, index, position);
            Assert.AreEqual(position, token.Position);
        }

        [TestMethod]
        public void ToStringTest()
        {
            Token token = new Token(entry, text, index, position);
            string tokenString = token.ToString();

            Assert.IsTrue(tokenString.Contains(position.Line.ToString()));
            Assert.IsTrue(tokenString.Contains(position.Column.ToString()));
            Assert.IsTrue(tokenString.Contains(entry.Name));
            Assert.IsTrue(tokenString.Contains(text));
        }

        [TestMethod]
        public void ToStringTestShort()
        {
            Token token = new Token(entryShort, text, index, position);
            string tokenString = token.ToString();

            Assert.IsTrue(tokenString.Contains(position.Line.ToString()));
            Assert.IsTrue(tokenString.Contains(position.Column.ToString()));
            Assert.IsTrue(tokenString.Contains(entryShort.Name));
        }
    }
}

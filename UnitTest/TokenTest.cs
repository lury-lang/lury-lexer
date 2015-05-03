﻿using System;
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
        const string source = "dummyText";
        const int index = 2;
        const int length = 3;
        static CharPosition position;
        static RegexTokenEntry entry, entryShort;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            position = new CharPosition(1, 3);
            entry = new RegexTokenEntry(name, regex);
            entryShort = new RegexTokenEntry(nameShort, regex);
        }

        [TestMethod]
        public void EntryTest()
        {
            Token token = new Token(entry, source, index, length);
            Assert.AreEqual(entry, token.Entry);
        }

        [TestMethod]
        public void TextTest()
        {
            Token token = new Token(entry, source, index, length);
            Assert.AreEqual("mmy", token.Text);
        }

        [TestMethod]
        public void IndexTest()
        {
            Token token = new Token(entry, source, index, length);
            Assert.AreEqual(index, token.Index);
        }

        [TestMethod]
        public void PositionTest()
        {
            Token token = new Token(entry, source, index, length);
            Assert.AreEqual(position, token.Position);
        }

        [TestMethod]
        public void ToStringTest()
        {
            Token token = new Token(entry, source, index, length);
            string tokenString = token.ToString();

            Assert.IsTrue(tokenString.Contains(position.Line.ToString()));
            Assert.IsTrue(tokenString.Contains(position.Column.ToString()));
            Assert.IsTrue(tokenString.Contains(entry.Name));
            Assert.IsTrue(tokenString.Contains("mmy"));
        }

        [TestMethod]
        public void ToStringTestShort()
        {
            Token token = new Token(entryShort, source, index, length);
            string tokenString = token.ToString();

            Assert.IsTrue(tokenString.Contains(position.Line.ToString()));
            Assert.IsTrue(tokenString.Contains(position.Column.ToString()));
            Assert.IsTrue(tokenString.Contains(entryShort.Name));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ConstructorErrorTest()
        {
            Token token = new Token(entry, source, source.Length, 1);
        }
    }
}

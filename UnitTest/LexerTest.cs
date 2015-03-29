using System;
using System.Linq;
using Lury.Compiling.Lexer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    [TestClass]
    public class LexerTest
    {
        [TestMethod]
        public void TokenizeTest1()
        {
            var lexer = new Lexer("");
            lexer.Tokenize();

            Assert.IsTrue(lexer.TokenOutput.Select(t => t.Entry.Name).SequenceEqual(new string[] { "EndOfFile" }));
        }
    }
}

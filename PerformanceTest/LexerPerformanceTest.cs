using System;
using Lury.Compiling.Lexer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PerformanceTest
{
    [TestClass]
    public class LexerPerformanceTest
    {
        private static string inputSouceCode;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            LexerPerformanceTest.inputSouceCode = System.IO.File.ReadAllText("Input.lr");
        }

        [TestMethod]
        public void TokenizeTest()
        {
            const int count = 100;

            for (int i = 0; i < count; i++)
            {
                Lexer lexer = new Lexer(inputSouceCode);
                lexer.Tokenize();
            }
        }
    }
}

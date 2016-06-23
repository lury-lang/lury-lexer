using System;
using System.IO;
using Lury.Compiling.Lexer;
using NUnit.Framework;

namespace PerformanceTest
{
    [TestFixture]
    public class LexerPerformanceTest
    {
        private static string _inputSouceCode;

        [OneTimeSetUp]
        public static void OneTimeSetUp()
        {
            _inputSouceCode = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Input.lr"));
        }

        [Test]
        public void TokenizeTest()
        {
            const int count = 100;

            for (var i = 0; i < count; i++)
            {
                var lexer = new Lexer(string.Empty, _inputSouceCode);
                lexer.Tokenize();
            }
        }
    }
}

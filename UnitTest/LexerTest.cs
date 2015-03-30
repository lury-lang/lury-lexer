using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lury.Compiling.Lexer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    [TestClass]
    public class LexerTest
    {
        private static IEnumerable<AnswerFile> answerFiles;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            answerFiles = Directory.GetFiles("input", "*.answer").Select(s => new AnswerFile(s));
        }

        [TestMethod]
        public void TokenizeTest1()
        {
            var lexer = new Lexer("");
            lexer.Tokenize();

            Assert.IsTrue(lexer.TokenOutput.Select(t => t.Entry.Name).SequenceEqual(new string[] { "EndOfFile" }));
        }

        [TestMethod]
        public void TokenizeTestExternal()
        {
            foreach (var file in answerFiles)
            {
                var lexer = new Lexer(File.ReadAllText(file.TestFilePath));
                lexer.Tokenize();
                int index = 0;

                foreach (var token in lexer.TokenOutput)
                {
                    Assert.AreEqual(file.Answers[index].TokenName, token.Entry.Name);

                    if (file.Answers[index].TokenValue != null)
                        Assert.AreEqual(file.Answers[index].TokenValue, token.Text);

                    index++;
                }
            }
        }
    }
}

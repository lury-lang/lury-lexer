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

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            LexerTest.answerFiles = Directory.GetFiles("input", "*.answer").Select(s => new AnswerFile(s));
        }

        [TestMethod]
        public void TokenizeTest1()
        {
            var lexer = new Lexer("");
            lexer.Tokenize();

            CollectionAssert.AreEqual(new string[] { "EndOfFile" }, lexer.TokenOutput.Select(t => t.Entry.Name).ToArray());
        }

        [TestMethod]
        public void TokenizeTestExternal()
        {
            foreach (var file in answerFiles)
            {
                var lexer = new Lexer(File.ReadAllText(file.TestFilePath));
                lexer.Tokenize();
                int index = 0;

                this.TestContext.WriteLine("検証中: {0}", Path.GetFileName(file.AnswerFilePath));

                foreach (var token in lexer.TokenOutput)
                {
                    string message = string.Format("ファイル {0} の {1} 番目のトークン {2} は {3} と一致しません。(値は {4} および {5})",
                                        Path.GetFileName(file.AnswerFilePath),
                                        index + 1,
                                        file.Answers[index].TokenName,
                                        token.Entry.Name,
                                        file.Answers[index].TokenValue ?? "(null)",
                                        token.Text);
                    
                    Assert.AreEqual(file.Answers[index].TokenName, token.Entry.Name, message);

                    if (file.Answers[index].TokenValue != null)
                        Assert.AreEqual(file.Answers[index].TokenValue, token.Text, message);

                    index++;
                }
            }
        }
    }
}

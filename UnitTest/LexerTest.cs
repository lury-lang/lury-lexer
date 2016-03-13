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
        private static IEnumerable<AnswerFile> errorFiles;

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            answerFiles = Directory.GetFiles("Input", "*.answer").Select(s => new AnswerFile(s));
            errorFiles = Directory.GetFiles("Error", "*.answer").Select(s => new AnswerFile(s));
        }

        [TestMethod]
        public void SourceNameTest()
        {
            const string SourceName = "TestSource";
            var lexer = new Lexer(SourceName, string.Empty);
            Assert.AreEqual(SourceName, lexer.SourceName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TokenOutputError()
        {
            var lexer = new Lexer(string.Empty, string.Empty);
            var output = lexer.TokenOutput;
        }

        [TestMethod]
        public void TokenizeTest1()
        {
            var lexer = new Lexer(string.Empty, string.Empty);
            Assert.IsTrue(lexer.Tokenize());

            CollectionAssert.AreEqual(new[] { "EndOfFile" }, lexer.TokenOutput.Select(t => t.Entry.Name).ToArray());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TokenizeError()
        {
            var lexer = new Lexer(string.Empty, string.Empty);
            lexer.Tokenize();
            lexer.Tokenize();
        }

        [TestMethod]
        public void TokenizeTestExternal()
        {
            foreach (var file in answerFiles)
            {
                var lexer = new Lexer(string.Empty, File.ReadAllText(file.TestFilePath));
                Assert.IsTrue(lexer.Tokenize());
                int index = 0;

                this.TestContext.WriteLine("検証中: {0}", Path.GetFileName(file.AnswerFilePath));

                Assert.AreEqual(file.Answers.Count, lexer.TokenOutput.Count());

                foreach (var token in lexer.TokenOutput)
                {
                    if (file.Answers[index].TokenName != token.Entry.Name ||
                       (file.Answers[index].TokenValue != null && 
                        file.Answers[index].TokenValue != token.Text))
                        Assert.Fail("ファイル {0} の {1} 番目のトークン {2} は {3} と一致しません。(値は {4} および {5})",
                                    Path.GetFileName(file.AnswerFilePath),
                                    index + 1,
                                    file.Answers[index].TokenName,
                                    token.Entry.Name,
                                    file.Answers[index].TokenValue ?? "(null)",
                                    token.Text);

                    index++;
                }
            }
        }

        [TestMethod]
        public void TokenizeTestExternalError()
        {
            foreach (var file in errorFiles)
            {
                var lexer = new Lexer(string.Empty, File.ReadAllText(file.TestFilePath));
                Assert.IsFalse(lexer.Tokenize());
                int index = 0;

                this.TestContext.WriteLine("検証中: {0}", Path.GetFileName(file.AnswerFilePath));

                Assert.AreEqual(file.Answers.Where(a => a.TokenName != "!Error").Count(), lexer.TokenOutput.Count());

                foreach (var token in lexer.TokenOutput)
                {
                    if (file.Answers[index].TokenName != token.Entry.Name ||
                       (file.Answers[index].TokenValue != null &&
                        file.Answers[index].TokenValue != token.Text))
                        Assert.Fail("ファイル {0} の {1} 番目のトークン {2} は {3} と一致しません。(値は {4} および {5})",
                                    Path.GetFileName(file.AnswerFilePath),
                                    index + 1,
                                    file.Answers[index].TokenName,
                                    token.Entry.Name,
                                    file.Answers[index].TokenValue ?? "(null)",
                                    token.Text);
                    index++;
                }

                var errorAnswer = file.Answers.Where(a => a.TokenName == "!Error").ToList();

                foreach (var error in lexer.Logger.ErrorOutputs)
                {
                    var answer = errorAnswer.Find(a => a.TokenValue == error.OutputNumber.ToString());

                    if (answer != null)
                        errorAnswer.Remove(answer);
                    else
                        Assert.Fail("予期しないエラー (#{0}) が発生しました。{3} 位置: {1}、テキスト `{2}'",
                                    error.OutputNumber,
                                    error.CodePosition.CharPosition,
                                    error.Code,
                                    error.Message);
                }

                foreach (var unexpectedError in errorAnswer)
                {
                    Assert.Fail("予期されたエラー (#{0}) は発生しませんでした。", unexpectedError.TokenValue);
                }
            }
        }
    }
}

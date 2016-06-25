using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lury.Compiling.Lexer;
using NUnit.Framework;

namespace IntegrationTest
{
    [TestFixture]
    public class Test
    {
        private static IEnumerable<AnswerFile> TokenizeTestExternalSource => Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Input"), "*.answer").Select(s => new AnswerFile(s));

        private static IEnumerable<AnswerFile> TokenizeTestExternalErrorSource => Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Error"), "*.answer").Select(s => new AnswerFile(s));

        [Test]
        [TestCaseSource(nameof(TokenizeTestExternalSource))]
        public void TokenizeTestExternal(AnswerFile file)
        {
            var lexer = new Lexer(Path.GetFileName(file.TestFilePath), File.ReadAllText(file.TestFilePath));
            Assert.IsTrue(lexer.Tokenize());

            var index = 0;
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

        [Test]
        [TestCaseSource(nameof(TokenizeTestExternalErrorSource))]
        public void TokenizeTestExternalError(AnswerFile file)
        {
            var lexer = new Lexer(Path.GetFileName(file.TestFilePath), File.ReadAllText(file.TestFilePath));
            Assert.IsFalse(lexer.Tokenize());

            var index = 0;
            Assert.AreEqual(file.Answers.Count(a => a.TokenName != "!Error"), lexer.TokenOutput.Count());

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

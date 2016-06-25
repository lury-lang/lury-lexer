using System.Collections.Generic;
using System.IO;

namespace IntegrationTest
{
    public class Answer
    {
        public string TokenName { get; private set; }

        public string TokenValue { get; private set; }

        public Answer(string tokenName, string tokenValue)
        {
            this.TokenName = tokenName;
            this.TokenValue = tokenValue;
        }

        public static IReadOnlyList<Answer> FromFile(string filePath)
        {
            List<Answer> answers = new List<Answer>();

            using (FileStream stream = new FileStream(filePath, FileMode.Open))
            using (StreamReader sr = new StreamReader(stream))
            {
                string line;

                while ((line = sr.ReadLine()) != null)
                {
                    int sep;

                    if ((sep = line.IndexOf('\t')) == -1)
                        answers.Add(new Answer(line, null));
                    else
                        answers.Add(new Answer(line.Substring(0, sep), ReplaceEscapeChars(line.Substring(sep + 1))));
                }
            }

            return answers;
        }

        private static string ReplaceEscapeChars(string input)
        {
            return input.Replace("\\r", "\r").Replace("\\n", "\n").Replace("\\t", "\t");
        }
    }
}

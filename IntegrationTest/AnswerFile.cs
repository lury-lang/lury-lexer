using System;
using System.Collections.Generic;
using System.IO;

namespace IntegrationTest
{
    public class AnswerFile
    {
        public IReadOnlyList<Answer> Answers { get; private set; }

        public string AnswerFilePath { get; }
        
        // ReSharper disable once AssignNullToNotNullAttribute
        public string TestFilePath => Path.Combine(Path.GetDirectoryName(AnswerFilePath), Path.GetFileNameWithoutExtension(AnswerFilePath) + ".lr");

        public AnswerFile(string answerFilePath)
        {
            if (answerFilePath == null)
                throw  new ArgumentNullException(nameof(answerFilePath));

            AnswerFilePath = answerFilePath;
            Answers = Answer.FromFile(answerFilePath);
        }
    }
}

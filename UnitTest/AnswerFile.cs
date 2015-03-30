using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest
{
    class AnswerFile
    {
        public IReadOnlyList<Answer> Answers { get; private set; }

        public string AnswerFilePath { get; private set; }

        public string TestFilePath
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(this.AnswerFilePath), Path.GetFileNameWithoutExtension(this.AnswerFilePath) + ".lr");
            }
        }

        public AnswerFile(string answerFilePath)
        {
            this.AnswerFilePath = answerFilePath;
            this.Answers = Answer.FromFile(answerFilePath);
        }
    }
}

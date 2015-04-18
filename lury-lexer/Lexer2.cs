using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lury.Compiling.Logger;
using Lury.Compiling.Utils;

namespace Lury.Compiling.Lexer
{
    public partial class Lexer2
    {
        #region -- Private Fields --

        private readonly List<Token> output;
        private readonly Stack<int> indentStack;
        private int index;
        private readonly int length;
        private CharPosition position;
        private readonly string sourceCode;
        private bool commaDetected;

        #endregion

        #region -- Public Properties --

        public string SourceCode { get { return this.sourceCode; } }

        public IEnumerable<Token> TokenOutput { get { return this.output; } }

        public OutputLogger Logger { get; private set; }

        #endregion

        #region -- Constructors --

        public Lexer2(string sourceCode)
        {
            this.sourceCode = sourceCode;
            this.index = 0;
            this.length = sourceCode.Length;
            this.position = CharPosition.BasePosition;
            this.Logger = new OutputLogger();
            this.output = new List<Token>();
            this.indentStack = new Stack<int>();
        }

        #endregion

        #region -- Public Methods --

        public bool Tokenize()
        {
            for (; this.index < this.length; )
            {
                // 0 : Comments
                if (this.JudgeEqual("\\", "#") >= 0)
                {
                    if (!this.SkipComment()) return false;
                    continue;
                }

                this.index++;
            }

            return true;
        }

        #endregion

        #region -- Private Methods --

        #region Comment

        private bool SkipComment()
        {
            int elementIndex;

            if (this.JudgeEqual('\\'))
            {
                // LineCancel
                string[] lineCancel = new string[] { "\\\u000d\u000a", "\\\u000a", "\\\u000d", "\\\u2028", "\\\u2029" };

                if ((elementIndex = this.JudgeEqual(lineCancel)) == -1)
                {
                    // !Error: NewLine is expected after `\'
                    return false;
                }

                this.index += lineCancel[elementIndex].Length;
            }
            else if (this.JudgeEqual('#'))
            {
                if (this.JudgeEqual("###"))
                {
                    this.index += 3;

                    // BlockComment
                    if (this.Skip("###") == -1)
                    {
                        // !Error: BlockComment is not closed!
                        return false;
                    }

                    this.index += 3;
                }
                else
                {
                    // LineComment
                    string[] newLine = new string[] { "\u000d\u000a", "\u000a", "\u000d", "\u2028", "\u2029", "\u0000", "\u001a" };
                    this.Skip(newLine);

                    if ((elementIndex = this.JudgeEqual(newLine)) == -1)
                        // Reached the end of file
                        this.index = this.length;
                    else
                        this.index += newLine[elementIndex].Length;
                }
            }

            return true;
        }

        #endregion

        private int JudgeEqual(params string[] chars)
        {
            for (int i = 0, count = chars.Length; i < count; i++)
                if (this.sourceCode.IndexOf(chars[i], this.index, chars[i].Length, StringComparison.Ordinal) == this.index)
                    return i;

            return -1;
        }

        private bool JudgeEqual(string chars)
        {
            return (this.sourceCode.IndexOf(chars, this.index, chars.Length, StringComparison.Ordinal) == this.index);
        }

        private bool JudgeEqual(char character)
        {
            return (this.sourceCode[this.index] == character);
        }

        private int Skip(params string[] chars)
        {
            var keys = chars.Select(c => c[0]).Distinct().ToArray();
            int i = 0;

            for (; i < this.length; i++, this.index++)
                for (int j = 0; j < keys.Length; j++)
                    if (this.JudgeEqual(keys[j]) &&
                        this.JudgeEqual(chars) >= 0)
                        return i;

            this.index -= i;
            return -1;
        }

        private int Skip(string chars)
        {
            int i = 0;

            for (; i < this.length; i++, this.index++)
                if (this.JudgeEqual(chars[0]) &&
                    this.JudgeEqual(chars))
                    return i;

            this.index -= i;
            return -1;
        }

        private int Skip(char character)
        {
            int i = 0;

            for (; i < this.length; i++, this.index++)
                if (this.JudgeEqual(character))
                    return i;

            this.index -= i;
            return -1;
        }

        #endregion


    }
}

//
// Lexer.cs
//
// Author:
//       Tomona Nanase <nanase@users.noreply.github.com>
//
// The MIT License (MIT)
//
// Copyright (c) 2015 Tomona Nanase
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Lury.Compiling.Logger;
using Lury.Compiling.Utils;

namespace Lury.Compiling.Lexer
{
    public partial class Lexer
    {
        #region -- Private Fields --

        private readonly List<Token> output;
        private readonly Stack<int> indentStack;
        private int index;
        private readonly int length;
        private CharPosition position;
        private readonly string sourceCode;
        private bool commaDetected;
        private int indentIndex;

        #endregion

        #region -- Public Properties --

        public string SourceCode { get { return this.sourceCode; } }

        public IEnumerable<Token> TokenOutput { get { return this.output; } }

        public OutputLogger Logger { get; private set; }

        public bool IsFinished { get; private set; }

        #endregion

        #region -- Constructors --

        public Lexer(string sourceCode)
        {
            this.sourceCode = sourceCode;
            this.index = 0;
            this.length = sourceCode.Length;
            this.position = CharPosition.BasePosition;
            this.Logger = new OutputLogger();
            this.output = new List<Token>();
            this.indentStack = new Stack<int>();
            this.indentStack.Push(0);
        }

        #endregion

        #region -- Public Methods --

        public bool Tokenize()
        {
            if (this.IsFinished)
                throw new InvalidOperationException("Lexical analysis is already finished.");

            this.IsFinished = true;

            bool lineBreak = true;
            int lineBreakIndex = 0;
            int lineBreakLength = 0;
            this.indentIndex = -1;
            int indentLength = 0;
            bool zeroWidthIndent = false;
            int elementIndex;
            bool passedFirstLine = false;

            for (; this.index < this.length; )
            {
                #region 1 : WhiteSpace and Comment
                // 1.1 : NewLine
                if ((elementIndex = this.SkipOver(StringConstants.NewLine)) >= 0)
                {
                    if (!lineBreak)
                    {
                        lineBreakLength = StringConstants.NewLine[elementIndex].Length;
                        lineBreakIndex = this.index - lineBreakLength;
                        lineBreak = true;
                    }

                    this.indentIndex = -1;
                    indentLength = 0;
                    zeroWidthIndent = true;
                    continue;
                }

                // 1.2 : WhiteSpace
                if (this.JudgeEqual(StringConstants.Space) >= 0)
                {
                    if (lineBreak && this.indentIndex < 0)
                        this.indentIndex = this.index;

                    int spaceLength = this.SkipWhile(StringConstants.Space);

                    if (lineBreak)
                        indentLength += spaceLength;

                    zeroWidthIndent = false;
                    continue;
                }

                // 1.3 : Comments
                if (this.JudgeEqual("\\", "#") >= 0)
                {
                    if (!this.SkipComment()) return false;
                    continue;
                }

                // 1.4 : EndOfFile
                if (this.JudgeEqual(StringConstants.EndOfFile) >= 0)
                    break;

                // 1.5 : NewLine and Indent/Dedent
                if (lineBreak)
                {
                    if (passedFirstLine)
                        this.AddToken(Lexer.newline, lineBreakIndex, lineBreakLength);

                    if (!this.commaDetected)
                    {
                        if (zeroWidthIndent)
                            this.StackIndent(this.index, 0);
                        else if (!passedFirstLine &&
                                 indentLength > 0)
                        {
                            this.Logger.ReportError(LexerError.InvalidIndentFirstLine,
                                                    null,
                                                    this.sourceCode,
                                                    this.sourceCode.GetPositionByIndex(this.indentIndex));
                            return false;
                        }
                        else if (this.indentIndex >= 0 &&
                                 !this.StackIndent(this.indentIndex, indentLength))
                            return false;
                    }

                    zeroWidthIndent = false;
                    lineBreak = false;
                    this.indentIndex = -1;
                    indentLength = 0;
                }

                this.commaDetected = false;
                passedFirstLine = true;
                #endregion

                #region 2 : Number

                if (this.JudgeEqual(StringConstants.DigitAndDot) >= 0)
                {
                    this.SkipNumber();
                    continue;
                }

                #endregion

                #region 3 : String

                if (this.JudgeEqual(StringConstants.StringLiteral) >= 0)
                {
                    if (!this.SkipString()) return false;
                    continue;
                }

                #endregion

                #region 4 : Operator and Delimiter

                if (this.JudgeEqual(StringConstants.OperatorAndDelimiter) >= 0)
                {
                    this.SkipOperatorAndDelimiter();
                    continue;
                }

                #endregion

                #region 5 : Identifier

                if (this.SkipIdentifier())
                    continue;
                else
                {
                    // !Error: Unknown Character!
                    this.Logger.ReportError(LexerError.InvalidCharacter,
                                            this.sourceCode[this.index].ToString(),
                                            this.sourceCode,
                                            this.sourceCode.GetPositionByIndex(this.index));
                    return false;
                }

                #endregion
            }

            if (this.length > 0)
                this.StackIndent(this.length - 1, 0);

            this.AddToken(Lexer.endoffile, 0);
            return true;
        }

        #endregion

        #region -- Private Methods --

        #region Comment

        /// <summary>
        /// コメントを読み飛ばし、インデクスを前進させます。
        /// </summary>
        /// <returns>成功した時 true、処理に失敗し続行できないとき false。</returns>
        private bool SkipComment()
        {
            if (this.JudgeEqual('\\'))
            {
                // LineCancel
                if (this.SkipOver(StringConstants.LineCancel) == -1)
                {
                    // !Error: NewLine is expected after `\'
                    this.Logger.ReportError(LexerError.UnexpectedCharacterAfterBackslash,
                                            this.sourceCode[this.index].ToString(),
                                            this.sourceCode,
                                            this.sourceCode.GetPositionByIndex(this.index));
                    return false;
                }
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
                        this.Logger.ReportError(LexerError.UnclosedBlockComment,
                                                this.sourceCode[this.index].ToString(),
                                                this.sourceCode,
                                                this.sourceCode.GetPositionByIndex(this.index));
                        return false;
                    }

                    this.index += 3;
                }
                else
                {
                    // LineComment

                    // For empty line with line comment
                    this.indentIndex = -1;
                    if (this.Skip(StringConstants.LineBreak) == -1)
                        // Reached the end of file
                        this.index = this.length;
                }
            }

            return true;
        }

        #endregion

        #region Indent

        private bool StackIndent(int indentIndex, int level)
        {
            int peek = this.indentStack.Peek();

            if (peek == level)
                return true;
            else if (peek < level)
            {
                indentStack.Push(level);
                this.AddToken(Lexer.indent, indentIndex, 0);
            }
            else // peek > level
            {
                int dedentCount = 0;
                do
                {
                    indentStack.Pop();
                    dedentCount++;

                    if (indentStack.Count == 0 || indentStack.Peek() == level)
                        break;
                }
                while (indentStack.Count > 0);

                if (indentStack.Count == 0)
                {
                    this.Logger.ReportError(LexerError.InvalidIndent, null, this.SourceCode);
                    return false;
                }

                for (int i = 0; i < dedentCount; i++)
                    this.AddToken(Lexer.dedent, indentIndex, 0);
            }

            return true;
        }

        #endregion

        #region Number

        private bool SkipNumber()
        {
            #region Labels
            const int START = 0,
                      INT_ZERO = 1,
                      INT_ONE = 2,
                      INT_UNDER = 3,
                      HEX_PREFIX = 4,
                      HEX_DIGIT = 5,
                      HEX_UNDER = 6,
                      BIN_PREFIX = 7,
                      BIN_DIGIT = 8,
                      BIN_UNDER = 9,
                      OCT_PREFIX = 10,
                      OCT_DIGIT = 11,
                      OCT_UNDER = 12,
                      POINT_END = 13,
                      RANGE = 14,
                      POINT_START = 15,
                      FLT_DECIMAL = 16,
                      FLT_UNDER = 17,
                      EXP_PREFIX_INTEGER = 18,
                      EXP_PREFIX_FLOAT = 19,
                      EXP_SIGN_INTEGER = 20,
                      EXP_SIGN_FLOAT = 21,
                      EXP_DIGIT = 22,
                      EXP_UNDER = 23,
                      INTEGER = 24,
                      INTEGER_BACK = 25,
                      FLOATING = 26,
                      FLOATING_BACK = 27,
                      IMAGINARY = 28;
            #endregion

            int index_old = this.index;
                       
            switch (START)
            {
                case START:
                    if (this.JudgeEqual('0')) goto case INT_ZERO;
                    if (this.JudgeEqual('.')) goto case POINT_START;
                    goto case INT_ONE;

                #region Integer
                #region Decimal
                case INT_ZERO:
                    this.index++;
                    if (this.JudgeEqual('x') || this.JudgeEqual('X')) goto case HEX_PREFIX;
                    if (this.JudgeEqual('o') || this.JudgeEqual('O')) goto case OCT_PREFIX;
                    if (this.JudgeEqual('b') || this.JudgeEqual('B')) goto case BIN_PREFIX;
                    if (this.JudgeEqual('0')) goto case INT_ZERO;
                    if (this.JudgeEqual('_')) goto case INT_UNDER;
                    if (this.JudgeEqual('.')) goto case POINT_END;
                    if (this.JudgeEqual('e') || this.JudgeEqual('E')) goto case EXP_PREFIX_INTEGER;
                    if (this.JudgeEqual(StringConstants.DigitWithoutZero) >= 0) goto case INT_ONE;
                    goto case INTEGER;

                case INT_UNDER:
                    this.index++;
                    if (this.JudgeEqual('0')) goto case INT_ZERO;
                    if (this.JudgeEqual(StringConstants.DigitWithoutZero) >= 0) goto case INT_ONE;
                    goto case INTEGER_BACK;

                case INT_ONE:
                    this.index++;
                    if (this.JudgeEqual('0')) goto case INT_ZERO;
                    if (this.JudgeEqual('_')) goto case INT_UNDER;
                    if (this.JudgeEqual('.')) goto case POINT_END;
                    if (this.JudgeEqual('e') || this.JudgeEqual('E')) goto case EXP_PREFIX_INTEGER;
                    if (this.JudgeEqual(StringConstants.DigitWithoutZero) >= 0) goto case INT_ONE;
                    goto case INTEGER;
                #endregion

                #region Hexadecimal
                case HEX_PREFIX:
                    this.index++;
                    if (this.JudgeEqual(StringConstants.Hexadecimal) >= 0) goto case HEX_DIGIT;
                    goto case INTEGER_BACK;

                case HEX_DIGIT:
                    this.index++;
                    if (this.JudgeEqual('_')) goto case HEX_UNDER;
                    if (this.JudgeEqual(StringConstants.Hexadecimal) >= 0) goto case HEX_DIGIT;
                    goto case INTEGER;

                case HEX_UNDER:
                    this.index++;
                    if (this.JudgeEqual(StringConstants.Hexadecimal) >= 0) goto case HEX_DIGIT;
                    goto case INTEGER_BACK;
                #endregion

                #region Octal
                case OCT_PREFIX:
                    this.index++;
                    if (this.JudgeEqual(StringConstants.Octal) >= 0) goto case OCT_DIGIT;
                    goto case INTEGER_BACK;

                case OCT_DIGIT:
                    this.index++;
                    if (this.JudgeEqual('_')) goto case OCT_UNDER;
                    if (this.JudgeEqual(StringConstants.Octal) >= 0) goto case OCT_DIGIT;
                    goto case INTEGER;

                case OCT_UNDER:
                    this.index++;
                    if (this.JudgeEqual(StringConstants.Octal) >= 0) goto case OCT_DIGIT;
                    goto case INTEGER_BACK;
                #endregion

                #region Binary
                case BIN_PREFIX:
                    this.index++;
                    if (this.JudgeEqual('0') || this.JudgeEqual('1')) goto case BIN_DIGIT;
                    goto case INTEGER_BACK;

                case BIN_DIGIT:
                    this.index++;
                    if (this.JudgeEqual('_')) goto case BIN_UNDER;
                    if (this.JudgeEqual('0') || this.JudgeEqual('1')) goto case BIN_DIGIT;
                    goto case INTEGER;

                case BIN_UNDER:
                    this.index++;
                    if (this.JudgeEqual('0') || this.JudgeEqual('1')) goto case BIN_DIGIT;
                    goto case INTEGER_BACK;
                #endregion
                #endregion

                case POINT_END:
                    this.index++;
                    if (this.JudgeEqual('.')) goto case RANGE;
                    if (this.JudgeEqual(StringConstants.Digit) >= 0) goto case FLT_DECIMAL;
                    goto case FLOATING;

                case RANGE:
                    goto case INTEGER_BACK;

                case POINT_START:
                    this.index++;
                    goto case FLT_DECIMAL;

                #region FloatingPoint
                case FLT_DECIMAL:
                    this.index++;
                    if (this.JudgeEqual('_')) goto case FLT_UNDER;
                    if (this.JudgeEqual('e') || this.JudgeEqual('E')) goto case EXP_PREFIX_FLOAT;
                    if (this.JudgeEqual(StringConstants.Digit) >= 0) goto case FLT_DECIMAL;
                    goto case FLOATING;

                case FLT_UNDER:
                    this.index++;
                    if (this.JudgeEqual(StringConstants.Digit) >= 0) goto case FLT_DECIMAL;
                    goto case FLOATING_BACK;

                case EXP_PREFIX_INTEGER:
                    this.index++;
                    if (this.JudgeEqual('+') || this.JudgeEqual('-')) goto case EXP_SIGN_INTEGER;
                    if (this.JudgeEqual(StringConstants.Digit) >= 0) goto case EXP_DIGIT;
                    goto case INTEGER_BACK;

                case EXP_PREFIX_FLOAT:
                    this.index++;
                    if (this.JudgeEqual('+') || this.JudgeEqual('-')) goto case EXP_SIGN_FLOAT;
                    if (this.JudgeEqual(StringConstants.Digit) >= 0) goto case EXP_DIGIT;
                    goto case FLOATING_BACK;

                case EXP_SIGN_INTEGER:
                    this.index++;
                    if (this.JudgeEqual(StringConstants.Digit) >= 0) goto case EXP_DIGIT;
                    this.index -= 2;
                    goto case INTEGER;

                case EXP_SIGN_FLOAT:
                    this.index++;
                    if (this.JudgeEqual(StringConstants.Digit) >= 0) goto case EXP_DIGIT;
                    this.index -= 2;
                    goto case FLOATING;

                case EXP_DIGIT:
                    this.index++;
                    if (this.JudgeEqual('_')) goto case EXP_UNDER;
                    if (this.JudgeEqual(StringConstants.Digit) >= 0) goto case EXP_DIGIT;
                    goto case FLOATING;

                case EXP_UNDER:
                    this.index++;
                    if (this.JudgeEqual(StringConstants.Digit) >= 0) goto case EXP_DIGIT;
                    goto case FLOATING_BACK;
                #endregion

                #region Output
                case INTEGER_BACK:
                    this.index--;
                    goto case INTEGER;

                case INTEGER:
                    if (this.JudgeEqual('i')) goto case IMAGINARY;
                    this.AddToken(Lexer.Integer, index_old, this.index - index_old);
                    break;

                case FLOATING_BACK:
                    this.index--;
                    goto case FLOATING;

                case FLOATING:
                    if (this.JudgeEqual('i')) goto case IMAGINARY;
                    this.AddToken(Lexer.FloatNumber, index_old, this.index - index_old);
                    break;

                case IMAGINARY:
                    this.index++;
                    this.AddToken(Lexer.ImaginaryNumber, index_old, this.index - index_old);
                    break;
                #endregion
            }

            return true;
        }

        #endregion

        #region String

        private bool SkipString()
        {
            Match match;

            if (this.JudgeEqual('\''))
            {
                if ((match = Lexer.StringLiteral.Regex.Match(this.sourceCode, this.index)).Success &&
                    match.Index == this.index)
                    this.AddToken(Lexer.StringLiteral, match.Length);
            }
            else if (this.JudgeEqual('"'))
            {
                if ((match = Lexer.EmbedStringLiteral.Regex.Match(this.sourceCode, this.index)).Success &&
                    match.Index == this.index)
                    this.AddToken(Lexer.EmbedStringLiteral, match.Length);
            }
            else
            {
                if ((match = Lexer.WysiwygStringLiteral.Regex.Match(this.sourceCode, this.index)).Success &&
                    match.Index == this.index)
                    this.AddToken(Lexer.WysiwygStringLiteral, match.Length);
            }

            if (!match.Success)
            {
                // !Error: String literal is not closed!
                this.Logger.ReportError(LexerError.UnclosedStringLiteral,
                                        this.sourceCode[this.index].ToString(),
                                        this.sourceCode,
                                        this.sourceCode.GetPositionByIndex(this.index));
                return false;
            }

            this.index += match.Length;
            return true;
        }

        #endregion

        #region Operator and Delimiter

        private bool SkipOperatorAndDelimiter()
        {
            var token = Lexer.staticAndOperators.First(e => this.JudgeEqual(e.TokenValue));
            this.AddToken(token, token.TokenValue.Length);

            if (token == comma)
                this.commaDetected = true;

            this.index += token.TokenValue.Length;
            return true;
        }

        #endregion

        #region Identifier

        private bool SkipIdentifier()
        {
            Match match = Lexer.identifier.Regex.Match(this.sourceCode, this.index);

            if (!match.Success || match.Index != this.index)
                return false;

            var keyword = Lexer.identifiers.FirstOrDefault(e => e.TokenValue == match.Value);

            if (keyword == null)
                this.AddToken(Lexer.identifier, match.Length);
            else
                this.AddToken(keyword, match.Length);

            this.index += match.Length;
            return true;
        }

        #endregion

        private void AddToken(TokenEntry tokenEntry, int length)
        {
            this.AddToken(tokenEntry, this.index, length);
        }

        private void AddToken(TokenEntry tokenEntry, int index, int length)
        {
            this.output.Add(new Token(tokenEntry, this.sourceCode, index, length));
        }

        /// <summary>
        /// 指定された文字列の配列のうち、いずれかが現在のインデクスから一致しているかを判定します。
        /// </summary>
        /// <param name="chars">一致を判定する文字列の配列。</param>
        /// <returns>
        /// 引数として指定された文字列の配列のうち、最初に一致と判定された要素のインデクス。
        /// いずれの文字列にも一致しない場合は -1。
        /// </returns>
        private int JudgeEqual(params string[] chars)
        {
            for (int i = 0, count = chars.Length; i < count; i++)
                if (this.length >= this.index + chars[i].Length &&
                    this.sourceCode.IndexOf(chars[i], this.index, chars[i].Length, StringComparison.Ordinal) == this.index)
                    return i;

            return -1;
        }

        /// <summary>
        /// 指定された文字列が、現在のインデクスから一致しているかを判定します。
        /// </summary>
        /// <param name="chars">一致を判定する文字列。</param>
        /// <returns>一致するとき true、しないとき false。</returns>
        private bool JudgeEqual(string chars)
        {
            return (this.length >= this.index + chars.Length &&
                    this.sourceCode.IndexOf(chars, this.index, chars.Length, StringComparison.Ordinal) == this.index);
        }

        private int JudgeEqual(params char[] chars)
        {
            if (this.length < this.index + 1)
                return -1;

            char current = this.sourceCode[this.index];
            for (int i = 0, count = chars.Length; i < count; i++)
                if (current == chars[i])
                    return i;

            return -1;
        }

        /// <summary>
        /// 指定された文字が、現在のインデクスが指し示す文字と一致するかを判定します。
        /// </summary>
        /// <param name="character">一致を判定する文字。</param>
        /// <returns>一致するとき true、しないとき false。</returns>
        private bool JudgeEqual(char character)
        {
            return (this.length >= this.index + 1 &&
                    this.sourceCode[this.index] == character);
        }

        /// <summary>
        /// 指定された文字列の配列のうち、いずれかの文字列が出現するまでインデクスを前進させます。
        /// 指定された文字列は前進に含めません。
        /// </summary>
        /// <param name="chars">一致を判定する文字列の配列。</param>
        /// <returns>
        /// 現在のインデクスから前進したインデクス数。
        /// 文字列終端まで達し、前進できなかった場合は -1。
        /// </returns>
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

        /// <summary>
        /// 指定された文字列が出現するまでインデクスを前進させます。
        /// 指定された文字列は前進に含めません。
        /// </summary>
        /// <param name="chars">一致を判定する文字列。</param>
        /// <returns>
        /// 現在のインデクスから前進したインデクス数。
        /// 文字列終端まで達し、前進できなかった場合は -1。
        /// </returns>
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

        /// <summary>
        /// 指定された文字列の配列のうち、一致するいずれかの文字列の長さだけインデクスを前進させます。
        /// </summary>
        /// <param name="chars">読み飛ばす文字列の配列。</param>
        /// <returns>
        /// 読み飛ばした文字列の配列のインデクス。
        /// いずれの文字列にも一致しなかったとき、-1。
        /// </returns>
        private int SkipOver(params string[] chars)
        {
            int elementIndex;
            if ((elementIndex = this.JudgeEqual(chars)) == -1)
                return -1;

            this.index += chars[elementIndex].Length;
            return elementIndex;
        }

        private int SkipWhile(params char[] chars)
        {
            int length = 0;

            for (; this.index < this.length; this.index++, length++)
                if (this.JudgeEqual(chars) < 0)
                    return length;

            return length;
        }

        #endregion
    }
}

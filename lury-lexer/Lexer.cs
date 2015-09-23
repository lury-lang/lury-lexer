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
    /// <summary>
    /// 与えられた単一のソースコードをトークン列に変換するための字句解析器です。
    /// </summary>
    public partial class Lexer
    {
        #region -- Private Fields --

        private readonly List<Token> output;
        private readonly Stack<int> indentStack;
        private readonly int sourceLength;
        private readonly string sourceCode;
        private readonly string sourceName;

        private int lookIndex;
        private bool commaDetected;
        private int indentIndex;
        private char? indentChar;

        #endregion

        #region -- Public Properties --

        /// <summary>
        /// ソースコードを識別するための名前を取得します。
        /// </summary>
        public string SourceName { get { return this.sourceName; } }

        /// <summary>
        /// ソースコードの文字列を取得します。
        /// </summary>
        public string SourceCode { get { return this.sourceCode; } }

        /// <summary>
        /// 字句解析の結果、出力されたトークン列のリストを取得します。
        /// </summary>
        public IReadOnlyList<Token> TokenOutput
        {
            get
            {
                if (!this.IsFinished)
                    throw new InvalidOperationException("先に Tokenize メソッドを実行してください。");

                return this.output;
            }
        }

        /// <summary>
        /// トークン出力で報告されたメッセージを格納した
        /// <see cref="Lury.Compiling.Logger.OutputLogger"/> オブジェクトを指定します。
        /// </summary>
        public OutputLogger Logger { get; private set; }

        /// <summary>
        /// 字句解析が終了したかの真偽値を取得します。
        /// </summary>
        public bool IsFinished { get; private set; }

        #endregion

        #region -- Constructors --

        /// <summary>
        /// ソースコードとその名前を指定して、
        /// 新しい <see cref="Lury.Compiling.Lexer.Lexer"/> クラスのインスタンスを初期化します。
        /// </summary>
        /// <param name="sourceName">ソースコードを識別するための null でない名前。</param>
        /// <param name="sourceCode">ソースコードの文字列。</param>
        public Lexer(string sourceName, string sourceCode)
        {
            this.sourceName = sourceName;
            this.sourceCode = sourceCode;
            this.lookIndex = 0;
            this.sourceLength = sourceCode.Length;
            this.Logger = new OutputLogger();
            this.output = new List<Token>();
            this.indentStack = new Stack<int>();
            this.indentStack.Push(0);
        }

        #endregion

        #region -- Public Methods --

        /// <summary>
        /// ソースコードを字句解析し、結果を TokenOutput プロパティに格納します。
        /// </summary>
        /// <returns>字句解析に成功したとき true、それ以外のとき false。</returns>
        public bool Tokenize()
        {
            if (this.IsFinished)
                throw new InvalidOperationException("Lexical analysis is already finished.");

            this.IsFinished = true;
            this.indentIndex = -1;

            bool lineBreak = true;
            int lineBreakIndex = 0;
            int lineBreakLength = 0;
            int indentLength = 0;
            bool zeroWidthIndent = false;
            bool passedFirstLine = false;

            while (this.lookIndex < this.sourceLength)
            {
                #region 1 : WhiteSpace and Comment
                int elementIndex;

                // 1.1 : NewLine
                if ((elementIndex = this.SkipOver(StringConstants.NewLine)) >= 0)
                {
                    if (!lineBreak)
                    {
                        lineBreakLength = StringConstants.NewLine[elementIndex].Length;
                        lineBreakIndex = this.lookIndex - lineBreakLength;
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
                        this.indentIndex = this.lookIndex;

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
                            this.StackIndent(this.lookIndex, 0);
                        else if (!passedFirstLine &&
                                 indentLength > 0)
                        {
                            this.ReportErrorZeroWidth(LexerError.InvalidIndentFirstLine, this.indentIndex);
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
                    this.ReportErrorHere(LexerError.InvalidCharacter);
                    return false;
                }

                #endregion
            }

            // Dedent All
            if (this.sourceLength > 0)
                this.StackIndent(this.sourceLength - 1, 0, atEndOfFile: true);

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
                    this.ReportErrorHere(LexerError.UnexpectedCharacterAfterBackslash);
                    return false;
                }
            }
            else if (this.JudgeEqual('#'))
            {
                if (this.JudgeEqual("###"))
                {
                    this.lookIndex += 3;

                    // BlockComment
                    if (this.Skip("###") == -1)
                    {
                        // !Error: BlockComment is not closed!
                        this.ReportErrorHere(LexerError.UnclosedBlockComment);
                        return false;
                    }

                    this.lookIndex += 3;
                }
                else
                {
                    // LineComment

                    // For empty line with line comment
                    this.indentIndex = -1;
                    if (this.Skip(StringConstants.LineBreak) == -1)
                        // Reached the end of file
                        this.lookIndex = this.sourceLength;
                }
            }

            return true;
        }

        #endregion

        #region Indent

        private bool StackIndent(int indentIndex, int level, bool atEndOfFile = false)
        {
            int peek = this.indentStack.Peek();

            if (peek == level)
                return true;
            else if (peek < level)
            {
                // issue #2: 混在したインデント文字に対するエラー
                // https://github.com/lury-lang/lury-lexer/issues/2
                if (!this.CheckIndentChar(indentIndex, level))
                {
                    this.ReportErrorZeroWidth(LexerError.IndentCharacterConfusion, indentIndex);
                    return false;
                }

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

                if (atEndOfFile)
                    this.AddToken(Lexer.newline, indentIndex, 0);

                for (int i = 0; i < dedentCount; i++)
                    this.AddToken(Lexer.dedent, indentIndex, 0);
            }

            return true;
        }

        private bool CheckIndentChar(int indentIndex, int length)
        {
            if (!this.indentChar.HasValue)
                this.indentChar = this.sourceCode[indentIndex];

            char c = this.indentChar.Value;

            for (int i = indentIndex, l = indentIndex + length; i < l; i++)
                if (this.sourceCode[i] != c)
                    return false;

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

            int index_old = this.lookIndex;

            switch (START)
            {
                case START:
                    if (this.JudgeEqual('0')) goto case INT_ZERO;
                    if (this.JudgeEqual('.')) goto case POINT_START;
                    goto case INT_ONE;

                #region Integer
                #region Decimal
                case INT_ZERO:
                    this.lookIndex++;
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
                    this.lookIndex++;
                    if (this.JudgeEqual('0')) goto case INT_ZERO;
                    if (this.JudgeEqual(StringConstants.DigitWithoutZero) >= 0) goto case INT_ONE;
                    goto case INTEGER_BACK;

                case INT_ONE:
                    this.lookIndex++;
                    if (this.JudgeEqual('0')) goto case INT_ZERO;
                    if (this.JudgeEqual('_')) goto case INT_UNDER;
                    if (this.JudgeEqual('.')) goto case POINT_END;
                    if (this.JudgeEqual('e') || this.JudgeEqual('E')) goto case EXP_PREFIX_INTEGER;
                    if (this.JudgeEqual(StringConstants.DigitWithoutZero) >= 0) goto case INT_ONE;
                    goto case INTEGER;
                #endregion

                #region Hexadecimal
                case HEX_PREFIX:
                    this.lookIndex++;
                    if (this.JudgeEqual(StringConstants.Hexadecimal) >= 0) goto case HEX_DIGIT;
                    goto case INTEGER_BACK;

                case HEX_DIGIT:
                    this.lookIndex++;
                    if (this.JudgeEqual('_')) goto case HEX_UNDER;
                    if (this.JudgeEqual(StringConstants.Hexadecimal) >= 0) goto case HEX_DIGIT;
                    goto case INTEGER;

                case HEX_UNDER:
                    this.lookIndex++;
                    if (this.JudgeEqual(StringConstants.Hexadecimal) >= 0) goto case HEX_DIGIT;
                    goto case INTEGER_BACK;
                #endregion

                #region Octal
                case OCT_PREFIX:
                    this.lookIndex++;
                    if (this.JudgeEqual(StringConstants.Octal) >= 0) goto case OCT_DIGIT;
                    goto case INTEGER_BACK;

                case OCT_DIGIT:
                    this.lookIndex++;
                    if (this.JudgeEqual('_')) goto case OCT_UNDER;
                    if (this.JudgeEqual(StringConstants.Octal) >= 0) goto case OCT_DIGIT;
                    goto case INTEGER;

                case OCT_UNDER:
                    this.lookIndex++;
                    if (this.JudgeEqual(StringConstants.Octal) >= 0) goto case OCT_DIGIT;
                    goto case INTEGER_BACK;
                #endregion

                #region Binary
                case BIN_PREFIX:
                    this.lookIndex++;
                    if (this.JudgeEqual('0') || this.JudgeEqual('1')) goto case BIN_DIGIT;
                    goto case INTEGER_BACK;

                case BIN_DIGIT:
                    this.lookIndex++;
                    if (this.JudgeEqual('_')) goto case BIN_UNDER;
                    if (this.JudgeEqual('0') || this.JudgeEqual('1')) goto case BIN_DIGIT;
                    goto case INTEGER;

                case BIN_UNDER:
                    this.lookIndex++;
                    if (this.JudgeEqual('0') || this.JudgeEqual('1')) goto case BIN_DIGIT;
                    goto case INTEGER_BACK;
                #endregion
                #endregion

                case POINT_END:
                    this.lookIndex++;
                    if (this.JudgeEqual('.')) goto case RANGE;
                    if (this.JudgeEqual(StringConstants.Digit) >= 0) goto case FLT_DECIMAL;
                    goto case FLOATING;

                case RANGE:
                    goto case INTEGER_BACK;

                case POINT_START:
                    this.lookIndex++;
                    goto case FLT_DECIMAL;

                #region FloatingPoint
                case FLT_DECIMAL:
                    this.lookIndex++;
                    if (this.JudgeEqual('_')) goto case FLT_UNDER;
                    if (this.JudgeEqual('e') || this.JudgeEqual('E')) goto case EXP_PREFIX_FLOAT;
                    if (this.JudgeEqual(StringConstants.Digit) >= 0) goto case FLT_DECIMAL;
                    goto case FLOATING;

                case FLT_UNDER:
                    this.lookIndex++;
                    if (this.JudgeEqual(StringConstants.Digit) >= 0) goto case FLT_DECIMAL;
                    goto case FLOATING_BACK;

                case EXP_PREFIX_INTEGER:
                    this.lookIndex++;
                    if (this.JudgeEqual('+') || this.JudgeEqual('-')) goto case EXP_SIGN_INTEGER;
                    if (this.JudgeEqual(StringConstants.Digit) >= 0) goto case EXP_DIGIT;
                    goto case INTEGER_BACK;

                case EXP_PREFIX_FLOAT:
                    this.lookIndex++;
                    if (this.JudgeEqual('+') || this.JudgeEqual('-')) goto case EXP_SIGN_FLOAT;
                    if (this.JudgeEqual(StringConstants.Digit) >= 0) goto case EXP_DIGIT;
                    goto case FLOATING_BACK;

                case EXP_SIGN_INTEGER:
                    this.lookIndex++;
                    if (this.JudgeEqual(StringConstants.Digit) >= 0) goto case EXP_DIGIT;
                    this.lookIndex -= 2;
                    goto case INTEGER;

                case EXP_SIGN_FLOAT:
                    this.lookIndex++;
                    if (this.JudgeEqual(StringConstants.Digit) >= 0) goto case EXP_DIGIT;
                    this.lookIndex -= 2;
                    goto case FLOATING;

                case EXP_DIGIT:
                    this.lookIndex++;
                    if (this.JudgeEqual('_')) goto case EXP_UNDER;
                    if (this.JudgeEqual(StringConstants.Digit) >= 0) goto case EXP_DIGIT;
                    goto case FLOATING;

                case EXP_UNDER:
                    this.lookIndex++;
                    if (this.JudgeEqual(StringConstants.Digit) >= 0) goto case EXP_DIGIT;
                    goto case FLOATING_BACK;
                #endregion

                #region Output
                case INTEGER_BACK:
                    this.lookIndex--;
                    goto case INTEGER;

                case INTEGER:
                    if (this.JudgeEqual('i')) goto case IMAGINARY;
                    this.AddToken(Lexer.Integer, index_old, this.lookIndex - index_old);
                    break;

                case FLOATING_BACK:
                    this.lookIndex--;
                    goto case FLOATING;

                case FLOATING:
                    if (this.JudgeEqual('i')) goto case IMAGINARY;
                    this.AddToken(Lexer.FloatNumber, index_old, this.lookIndex - index_old);
                    break;

                case IMAGINARY:
                    this.lookIndex++;
                    this.AddToken(Lexer.ImaginaryNumber, index_old, this.lookIndex - index_old);
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
                if ((match = Lexer.StringLiteral.Regex.Match(this.sourceCode, this.lookIndex)).Success &&
                    match.Index == this.lookIndex)
                    this.AddToken(Lexer.StringLiteral, match.Length);
            }
            else if (this.JudgeEqual('"'))
            {
                if ((match = Lexer.EmbedStringLiteral.Regex.Match(this.sourceCode, this.lookIndex)).Success &&
                    match.Index == this.lookIndex)
                    this.AddToken(Lexer.EmbedStringLiteral, match.Length);
            }
            else
            {
                if ((match = Lexer.WysiwygStringLiteral.Regex.Match(this.sourceCode, this.lookIndex)).Success &&
                    match.Index == this.lookIndex)
                    this.AddToken(Lexer.WysiwygStringLiteral, match.Length);
            }

            if (!match.Success)
            {
                // !Error: String literal is not closed!
                this.ReportErrorHere(LexerError.UnclosedStringLiteral);
                return false;
            }

            this.lookIndex += match.Length;
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

            this.lookIndex += token.TokenValue.Length;
            return true;
        }

        #endregion

        #region Identifier

        private bool SkipIdentifier()
        {
            Match match = Lexer.identifier.Regex.Match(this.sourceCode, this.lookIndex);

            if (!match.Success || match.Index != this.lookIndex)
                return false;

            var keyword = Lexer.identifiers.FirstOrDefault(e => e.TokenValue == match.Value);

            if (keyword == null)
                this.AddToken(Lexer.identifier, match.Length);
            else
                this.AddToken(keyword, match.Length);

            this.lookIndex += match.Length;
            return true;
        }

        #endregion

        private void AddToken(TokenEntry tokenEntry, int length)
        {
            this.AddToken(tokenEntry, this.lookIndex, length);
        }

        private void AddToken(TokenEntry tokenEntry, int index, int length)
        {
            this.output.Add(new Token(tokenEntry, this.sourceName, this.sourceCode, index, length));
        }

        private void ReportErrorHere(LexerError error)
        {
            this.Logger.ReportError(
                error,
                this.sourceCode[this.lookIndex].ToString(),
                this.sourceCode,
                new CodePosition(this.sourceName, this.sourceCode.GetPositionByIndex(this.lookIndex)));
        }

        private void ReportErrorZeroWidth(LexerError error, int index)
        {
            this.Logger.ReportError(
                error,
                null,
                this.sourceCode,
                new CodePosition(this.sourceName, this.sourceCode.GetPositionByIndex(index)));
        }

        #region JudgeEqual

        // [JudgeEqualメソッド群] lookIndex以降、一致しているかを判定
        // パラメータが配列 ... インデクスを返却。全不一致なら -1
        // パラメータが単一 ... 真偽値を返却。不一致なら false

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
                if (this.sourceLength >= this.lookIndex + chars[i].Length &&
                    this.sourceCode.IndexOf(chars[i], this.lookIndex, chars[i].Length, StringComparison.Ordinal) == this.lookIndex)
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
            return (this.sourceLength >= this.lookIndex + chars.Length &&
                    this.sourceCode.IndexOf(chars, this.lookIndex, chars.Length, StringComparison.Ordinal) == this.lookIndex);
        }

        /// <summary>
        /// 指定された文字列の配列のうち、いずれかが現在のインデクスから一致しているかを判定します。
        /// </summary>
        /// <param name="chars">一致を判定する文字列の配列。</param>
        /// <returns>
        /// 引数として指定された文字列の配列のうち、最初に一致と判定された要素のインデクス。
        /// いずれの文字列にも一致しない場合は -1。
        /// </returns>
        private int JudgeEqual(params char[] chars)
        {
            if (this.sourceLength < this.lookIndex + 1)
                return -1;

            char current = this.sourceCode[this.lookIndex];
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
            return (this.sourceLength >= this.lookIndex + 1 &&
                    this.sourceCode[this.lookIndex] == character);
        }

        #endregion

        #region Skip Methods

        // [Skipメソッド群] 一定の条件の下でlookIndex前進
        // Skip      ... 指定された文字(列)まで前進。文字(列)のインデクスを返却
        // SkipOver  ... 指定された文字列分だけ前進。文字列のインデクスを返却
        // SkipWhile ... 指定された文字が続くまで前進。前進文字数を返却

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

            for (; i < this.sourceLength; i++, this.lookIndex++)
                for (int j = 0; j < keys.Length; j++)
                    if (this.JudgeEqual(keys[j]) &&
                        this.JudgeEqual(chars) >= 0)
                        return i;

            this.lookIndex -= i;
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

            for (; i < this.sourceLength; i++, this.lookIndex++)
                if (this.JudgeEqual(chars[0]) &&
                    this.JudgeEqual(chars))
                    return i;

            this.lookIndex -= i;
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

            this.lookIndex += chars[elementIndex].Length;
            return elementIndex;
        }

        /// <summary>
        /// 指定された文字の配列のうち、いずれかが一致した場合のみのインデクスを前進させます。
        /// </summary>
        /// <param name="chars">読み飛ばす文字の配列。</param>
        /// <returns>読み飛ばした文字数。</returns>
        private int SkipWhile(params char[] chars)
        {
            int length = 0;

            for (; this.lookIndex < this.sourceLength; this.lookIndex++, length++)
                if (this.JudgeEqual(chars) < 0)
                    return length;

            return length;
        }

        #endregion
        #endregion
    }
}

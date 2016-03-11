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

        private int lookIndex;
        private bool commaDetected;
        private int indentIndex;
        private char? indentChar;

        #endregion

        #region -- Public Properties --

        /// <summary>
        /// ソースコードを識別するための名前を取得します。
        /// </summary>
        public string SourceName { get; }

        /// <summary>
        /// ソースコードの文字列を取得します。
        /// </summary>
        public string SourceCode { get; }

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
        public OutputLogger Logger { get; }

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
            this.SourceName = sourceName;
            this.SourceCode = sourceCode;
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
                        this.AddToken(newline, lineBreakIndex, lineBreakLength);

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
                // !Error: Unknown Character!
                this.ReportErrorHere(LexerError.InvalidCharacter);
                return false;

                #endregion
            }

            // Dedent All
            if (this.sourceLength > 0)
                this.StackIndent(this.sourceLength - 1, 0, true);

            this.AddToken(endoffile, 0);
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

        private bool StackIndent(int targetIndentIndex, int level, bool atEndOfFile = false)
        {
            int peek = this.indentStack.Peek();

            if (peek == level)
                return true;
            if (peek < level)
            {
                // issue #2: 混在したインデント文字に対するエラー
                // https://github.com/lury-lang/lury-lexer/issues/2
                if (!this.CheckIndentChar(targetIndentIndex, level))
                {
                    this.ReportErrorZeroWidth(LexerError.IndentCharacterConfusion, targetIndentIndex);
                    return false;
                }

                this.indentStack.Push(level);
                this.AddToken(indent, targetIndentIndex, 0);
            }
            else // peek > level
            {
                int dedentCount = 0;
                do
                {
                    this.indentStack.Pop();
                    dedentCount++;

                    if (this.indentStack.Count == 0 || this.indentStack.Peek() == level)
                        break;
                }
                while (indentStack.Count > 0);

                if (indentStack.Count == 0)
                {
                    this.Logger.ReportError(LexerError.InvalidIndent, null, this.SourceCode);
                    return false;
                }

                if (atEndOfFile)
                    this.AddToken(newline, targetIndentIndex, 0);

                for (int i = 0; i < dedentCount; i++)
                    this.AddToken(dedent, targetIndentIndex, 0);
            }

            return true;
        }

        private bool CheckIndentChar(int targetIndentIndex, int length)
        {
            if (!this.indentChar.HasValue)
                this.indentChar = this.SourceCode[targetIndentIndex];

            char c = this.indentChar.Value;

            for (int i = targetIndentIndex, l = targetIndentIndex + length; i < l; i++)
                if (this.SourceCode[i] != c)
                    return false;

            return true;
        }

        #endregion

        #region Number

        private bool SkipNumber()
        {
            #region Labels
            const int Start = 0,
                      IntZero = 1,
                      IntOne = 2,
                      IntUnder = 3,
                      HexPrefix = 4,
                      HexDigit = 5,
                      HexUnder = 6,
                      BinPrefix = 7,
                      BinDigit = 8,
                      BinUnder = 9,
                      OctPrefix = 10,
                      OctDigit = 11,
                      OctUnder = 12,
                      PointEnd = 13,
                      Range = 14,
                      PointStart = 15,
                      FltDecimal = 16,
                      FltUnder = 17,
                      ExpPrefixInteger = 18,
                      ExpPrefixFloat = 19,
                      ExpSignInteger = 20,
                      ExpSignFloat = 21,
                      ExpDigit = 22,
                      ExpUnder = 23,
                      Integer = 24,
                      IntegerBack = 25,
                      Floating = 26,
                      FloatingBack = 27,
                      Imaginary = 28;
            #endregion

            int indexOld = this.lookIndex;

            switch (Start)
            {
                case Start:
                    if (this.JudgeEqual('0')) goto case IntZero;
                    if (this.JudgeEqual('.')) goto case PointStart;
                    goto case IntOne;

                #region Integer
                #region Decimal
                case IntZero:
                    this.lookIndex++;
                    if (this.JudgeEqual('x') || this.JudgeEqual('X')) goto case HexPrefix;
                    if (this.JudgeEqual('o') || this.JudgeEqual('O')) goto case OctPrefix;
                    if (this.JudgeEqual('b') || this.JudgeEqual('B')) goto case BinPrefix;
                    if (this.JudgeEqual('0')) goto case IntZero;
                    if (this.JudgeEqual('_')) goto case IntUnder;
                    if (this.JudgeEqual('.')) goto case PointEnd;
                    if (this.JudgeEqual('e') || this.JudgeEqual('E')) goto case ExpPrefixInteger;
                    if (this.JudgeEqual(StringConstants.DigitWithoutZero) >= 0) goto case IntOne;
                    goto case Integer;

                case IntUnder:
                    this.lookIndex++;
                    if (this.JudgeEqual('0')) goto case IntZero;
                    if (this.JudgeEqual(StringConstants.DigitWithoutZero) >= 0) goto case IntOne;
                    goto case IntegerBack;

                case IntOne:
                    this.lookIndex++;
                    if (this.JudgeEqual('0')) goto case IntZero;
                    if (this.JudgeEqual('_')) goto case IntUnder;
                    if (this.JudgeEqual('.')) goto case PointEnd;
                    if (this.JudgeEqual('e') || this.JudgeEqual('E')) goto case ExpPrefixInteger;
                    if (this.JudgeEqual(StringConstants.DigitWithoutZero) >= 0) goto case IntOne;
                    goto case Integer;
                #endregion

                #region Hexadecimal
                case HexPrefix:
                    this.lookIndex++;
                    if (this.JudgeEqual(StringConstants.Hexadecimal) >= 0) goto case HexDigit;
                    goto case IntegerBack;

                case HexDigit:
                    this.lookIndex++;
                    if (this.JudgeEqual('_')) goto case HexUnder;
                    if (this.JudgeEqual(StringConstants.Hexadecimal) >= 0) goto case HexDigit;
                    goto case Integer;

                case HexUnder:
                    this.lookIndex++;
                    if (this.JudgeEqual(StringConstants.Hexadecimal) >= 0) goto case HexDigit;
                    goto case IntegerBack;
                #endregion

                #region Octal
                case OctPrefix:
                    this.lookIndex++;
                    if (this.JudgeEqual(StringConstants.Octal) >= 0) goto case OctDigit;
                    goto case IntegerBack;

                case OctDigit:
                    this.lookIndex++;
                    if (this.JudgeEqual('_')) goto case OctUnder;
                    if (this.JudgeEqual(StringConstants.Octal) >= 0) goto case OctDigit;
                    goto case Integer;

                case OctUnder:
                    this.lookIndex++;
                    if (this.JudgeEqual(StringConstants.Octal) >= 0) goto case OctDigit;
                    goto case IntegerBack;
                #endregion

                #region Binary
                case BinPrefix:
                    this.lookIndex++;
                    if (this.JudgeEqual('0') || this.JudgeEqual('1')) goto case BinDigit;
                    goto case IntegerBack;

                case BinDigit:
                    this.lookIndex++;
                    if (this.JudgeEqual('_')) goto case BinUnder;
                    if (this.JudgeEqual('0') || this.JudgeEqual('1')) goto case BinDigit;
                    goto case Integer;

                case BinUnder:
                    this.lookIndex++;
                    if (this.JudgeEqual('0') || this.JudgeEqual('1')) goto case BinDigit;
                    goto case IntegerBack;
                #endregion
                #endregion

                case PointEnd:
                    this.lookIndex++;
                    if (this.JudgeEqual('.')) goto case Range;
                    if (this.JudgeEqual(StringConstants.Digit) >= 0) goto case FltDecimal;
                    goto case Floating;

                case Range:
                    goto case IntegerBack;

                case PointStart:
                    this.lookIndex++;
                    goto case FltDecimal;

                #region FloatingPoint
                case FltDecimal:
                    this.lookIndex++;
                    if (this.JudgeEqual('_')) goto case FltUnder;
                    if (this.JudgeEqual('e') || this.JudgeEqual('E')) goto case ExpPrefixFloat;
                    if (this.JudgeEqual(StringConstants.Digit) >= 0) goto case FltDecimal;
                    goto case Floating;

                case FltUnder:
                    this.lookIndex++;
                    if (this.JudgeEqual(StringConstants.Digit) >= 0) goto case FltDecimal;
                    goto case FloatingBack;

                case ExpPrefixInteger:
                    this.lookIndex++;
                    if (this.JudgeEqual('+') || this.JudgeEqual('-')) goto case ExpSignInteger;
                    if (this.JudgeEqual(StringConstants.Digit) >= 0) goto case ExpDigit;
                    goto case IntegerBack;

                case ExpPrefixFloat:
                    this.lookIndex++;
                    if (this.JudgeEqual('+') || this.JudgeEqual('-')) goto case ExpSignFloat;
                    if (this.JudgeEqual(StringConstants.Digit) >= 0) goto case ExpDigit;
                    goto case FloatingBack;

                case ExpSignInteger:
                    this.lookIndex++;
                    if (this.JudgeEqual(StringConstants.Digit) >= 0) goto case ExpDigit;
                    this.lookIndex -= 2;
                    goto case Integer;

                case ExpSignFloat:
                    this.lookIndex++;
                    if (this.JudgeEqual(StringConstants.Digit) >= 0) goto case ExpDigit;
                    this.lookIndex -= 2;
                    goto case Floating;

                case ExpDigit:
                    this.lookIndex++;
                    if (this.JudgeEqual('_')) goto case ExpUnder;
                    if (this.JudgeEqual(StringConstants.Digit) >= 0) goto case ExpDigit;
                    goto case Floating;

                case ExpUnder:
                    this.lookIndex++;
                    if (this.JudgeEqual(StringConstants.Digit) >= 0) goto case ExpDigit;
                    goto case FloatingBack;
                #endregion

                #region Output
                case IntegerBack:
                    this.lookIndex--;
                    goto case Integer;

                case Integer:
                    if (this.JudgeEqual('i')) goto case Imaginary;
                    this.AddToken(integer, indexOld, this.lookIndex - indexOld);
                    break;

                case FloatingBack:
                    this.lookIndex--;
                    goto case Floating;

                case Floating:
                    if (this.JudgeEqual('i')) goto case Imaginary;
                    this.AddToken(floatNumber, indexOld, this.lookIndex - indexOld);
                    break;

                case Imaginary:
                    this.lookIndex++;
                    this.AddToken(imaginaryNumber, indexOld, this.lookIndex - indexOld);
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
                if ((match = stringLiteral.Regex.Match(this.SourceCode, this.lookIndex)).Success &&
                    match.Index == this.lookIndex)
                    this.AddToken(stringLiteral, match.Length);
            }
            else if (this.JudgeEqual('"'))
            {
                if ((match = embedStringLiteral.Regex.Match(this.SourceCode, this.lookIndex)).Success &&
                    match.Index == this.lookIndex)
                    this.AddToken(embedStringLiteral, match.Length);
            }
            else
            {
                if ((match = wysiwygStringLiteral.Regex.Match(this.SourceCode, this.lookIndex)).Success &&
                    match.Index == this.lookIndex)
                    this.AddToken(wysiwygStringLiteral, match.Length);
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
            var token = staticAndOperators.First(e => this.JudgeEqual(e.TokenValue));
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
            Match match = identifier.Regex.Match(this.SourceCode, this.lookIndex);

            if (!match.Success || match.Index != this.lookIndex)
                return false;

            var keyword = identifiers.FirstOrDefault(e => e.TokenValue == match.Value);

            if (keyword == null)
                this.AddToken(identifier, match.Length);
            else
                this.AddToken(keyword, match.Length);

            this.lookIndex += match.Length;
            return true;
        }

        #endregion

        private void AddToken(TokenEntry tokenEntry, int length)
            => this.AddToken(tokenEntry, this.lookIndex, length);

        private void AddToken(TokenEntry tokenEntry, int index, int length)
            => this.output.Add(new Token(tokenEntry, this.SourceName, this.SourceCode, index, length));
        
        private void ReportErrorHere(LexerError error)
            => this.Logger.ReportError(
                error,
                this.SourceCode[this.lookIndex].ToString(),
                this.SourceCode,
                new CodePosition(this.SourceName, this.SourceCode.GetPositionByIndex(this.lookIndex)));
        
        private void ReportErrorZeroWidth(LexerError error, int index)
            => this.Logger.ReportError(
                error,
                null,
                this.SourceCode,
                new CodePosition(this.SourceName, this.SourceCode.GetPositionByIndex(index)));

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
                    this.SourceCode.IndexOf(chars[i], this.lookIndex, chars[i].Length, StringComparison.Ordinal) == this.lookIndex)
                    return i;

            return -1;
        }

        /// <summary>
        /// 指定された文字列が、現在のインデクスから一致しているかを判定します。
        /// </summary>
        /// <param name="chars">一致を判定する文字列。</param>
        /// <returns>一致するとき true、しないとき false。</returns>
        private bool JudgeEqual(string chars)
            => this.sourceLength >= this.lookIndex + chars.Length &&
               this.SourceCode.IndexOf(chars, this.lookIndex, chars.Length, StringComparison.Ordinal) == this.lookIndex;
     
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

            char current = this.SourceCode[this.lookIndex];
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
            => this.sourceLength >= this.lookIndex + 1 &&
                    this.SourceCode[this.lookIndex] == character;

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
                if (keys.Any(t => this.JudgeEqual(t) &&
                                  this.JudgeEqual(chars) >= 0))
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

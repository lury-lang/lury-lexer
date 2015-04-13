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
        private bool commaDetected;

        #endregion

        #region -- Public Properties --

        public string SourceCode { get; private set; }

        public IEnumerable<Token> TokenOutput { get { return this.output; } }

        public OutputLogger Logger { get; private set; }

        #endregion

        #region -- Constructors --

        public Lexer(string sourceCode)
        {
            this.SourceCode = sourceCode;
            this.index = 0;
            this.Logger = new OutputLogger();
            this.output = new List<Token>();
            this.indentStack = new Stack<int>();
        }

        #endregion

        #region -- Public Methods --

        public bool Tokenize()
        {
            bool checkIndentFirstLine = false;
            bool lineBreak = false;
            bool zeroWidthIndent = false;
            bool reachEndOfFile = false;
            Match indentSpace = null;
            Match newlineMatch = null;

            this.output.Clear();
            this.commaDetected = false;
            this.indentStack.Push(0);

            for (this.index = 0; this.index < this.SourceCode.Length && !reachEndOfFile; )
            {
                Match m;

                if (IsMatch(space, this.SourceCode, this.index, out m))
                {
                    if (!checkIndentFirstLine)
                    {
                        this.Logger.ReportError(LexerError.InvalidIndentFirstLine,
                                                m.Value,
                                                this.SourceCode,
                                                this.SourceCode.GetPositionByIndex(m.Index));
                        return false;
                    }

                    if (lineBreak)
                        indentSpace = m;

                    zeroWidthIndent = false;
                }
                else if (IsMatch(endoffile, this.SourceCode, this.index, out m))
                    reachEndOfFile = true;
                else if (IsMatch(newline, this.SourceCode, this.index, out m))
                {
                    if (!lineBreak)
                        newlineMatch = m;

                    lineBreak = true;
                    zeroWidthIndent = true;
                    indentSpace = null;
                }
                else if (!this.MatchComment(out m))
                {
                    TokenEntry entry;

                    if (lineBreak)
                        this.output.Add(new Token(newline, newlineMatch.Value, newlineMatch.Index, this.SourceCode.GetPositionByIndex(newlineMatch.Index)));

                    if (zeroWidthIndent || lineBreak && indentSpace != null)
                    {
                        if (!commaDetected && !this.StackIndent(indentSpace))
                            return false;

                        indentSpace = null;
                    }

                    commaDetected = false;
                    lineBreak = false;
                    zeroWidthIndent = false;

                    if (this.MatchOtherTokens(out m, out entry))
                        this.output.Add(new Token(entry, m.Value, this.index, this.SourceCode.GetPositionByIndex(this.index)));
                    else
                    {
                        string unrecognizableChar = this.SourceCode[this.index].ToString();
                        this.Logger.ReportError(LexerError.InvalidCharacter,
                                                unrecognizableChar,
                                                this.SourceCode,
                                                this.SourceCode.GetPositionByIndex(this.index),
                                                string.Format("Character `{0}'", unrecognizableChar.ConvertControlChars()));
                        return false;
                    }
                }

                this.MoveForward(m);
                checkIndentFirstLine = true;
            }

            // End of SourceCode
            this.index = this.SourceCode.Length - 1;

            this.StackIndent(null);

            this.output.Add(new Token(endoffile, "", this.SourceCode.Length == 0 ? 0 : this.index, this.SourceCode.Length == 0 ? CharPosition.BasePosition : this.SourceCode.GetPositionByIndex(this.index)));
            return true;
        }

        #endregion

        #region -- Private Methods --

        private bool MatchOtherTokens(out Match m, out TokenEntry tokenEntry)
        {
            foreach (var entry in Lexer.tokenEntry)
            {
                if (IsMatch(entry, this.SourceCode, this.index, out m))
                {
                    Match tempMatch = m;

                    if (entry == Lexer.identifier &&
                        MatchTokenEntries(Lexer.identifiers, tempMatch.Value, 0, out m, out tokenEntry, perfect: true))
                        return true;                    // Identifier

                    if (entry == Lexer.numberAndRange)
                    {
                        MatchTokenEntries(Lexer.number, tempMatch.Groups["num"].Value, 0, out m, out tokenEntry);
                        this.output.Add(new Token(tokenEntry, tempMatch.Groups["num"].Value, this.index, this.SourceCode.GetPositionByIndex(this.index)));
                        this.MoveForward(m);

                        MatchTokenEntries(Lexer.tokenEntry, tempMatch.Groups["op"].Value, 0, out m, out tokenEntry);
                        return true;
                    }
                    else if (entry == Lexer.dot)
                    {
                        if (MatchTokenEntries(Lexer.number, this.SourceCode, this.index, out m, out tokenEntry))
                            return true;                        // Number
                        else
                        {
                            m = tempMatch;
                            tokenEntry = entry;
                            return true;                        // Dot
                        }
                    }
                    else
                    {
                        if (entry == Lexer.comma)
                            this.commaDetected = true;

                        m = tempMatch;
                        tokenEntry = entry;
                        return true;                        // Other Token
                    }
                }
            }

            if (MatchTokenEntries(Lexer.number, this.SourceCode, this.index, out m, out tokenEntry))
                return true;

            m = null;
            tokenEntry = null;

            return false;                                   // Error
        }

        private bool MatchComment(out Match m)
        {
            foreach (var comment in Lexer.comment)
            {
                if (IsMatch(comment, this.SourceCode, this.index, out m))
                    return true;
            }

            m = null;

            return false;
        }

        private bool StackIndent(Match match)
        {
            int level = (match == null) ? 0 : match.Length;
            int peek = this.indentStack.Peek();

            if (peek == level)
                return true;
            //else if (level == 0)    // Empty Line
            else if (peek < level)
            {
                indentStack.Push(level);
                this.output.Add(new Token(Lexer.indent, "", this.index, this.SourceCode.GetPositionByIndex(this.index)));
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
                    this.Logger.ReportError(LexerError.InvalidIndent, null, this.SourceCode, this.SourceCode.GetPositionByIndex(this.index));
                    return false;
                }

                this.output.AddRange(Enumerable.Repeat(new Token(Lexer.dedent,
                                                                 "",
                                                                 this.index,
                                                                 this.SourceCode.GetPositionByIndex(this.index)),
                                                                 dedentCount));
            }

            return true;
        }

        private void MoveForward(Match match)
        {
            this.index += match.Length;
        }

        #endregion

        #region -- Private Static Methods --

        private static bool MatchTokenEntries(IEnumerable<TokenEntry> entries, string code, int index, out Match m, out TokenEntry entry, bool perfect = false)
        {
            foreach (var targetEntry in entries)
            {
                if (IsMatch(targetEntry, code, index, out m, perfect))
                {
                    entry = targetEntry;
                    return true;
                }
            }

            m = null;
            entry = null;
            return false;
        }

        private static bool IsMatch(TokenEntry entry, string source, int index, out Match match, bool perfect = false)
        {
            match = entry.Regex.Match(source, index);
            return match.Success && match.Index == index && (!perfect || match.Length == source.Length);
        }

        #endregion
    }
}

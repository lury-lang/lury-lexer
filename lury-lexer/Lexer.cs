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
using System.Text.RegularExpressions;
using Lury.Compiling.Utils;

namespace Lury.Compiling.Lexer
{
    public class Lexer
    {
        #region Token Entry
        private static readonly TokenEntry identifier = new TokenEntry("Identifier", @"([_\p{Lu}\p{Ll}\p{Lt}\p{Lm}\p{Lo}\p{Nl}][_\p{Lu}\p{Ll}\p{Lt}\p{Lm}\p{Lo}\p{Nl}\p{Mn}\p{Mc}\p{Nd}\p{Pc}]*)");
        private static readonly TokenEntry numberAndRange = new TokenEntry("NumberAndRange", @"(?<num>0([xX][0-9a-fA-F](_?[0-9a-fA-F])*|[oO][0-7](_?[0-7])*|[bB][01](_?[01])*)|[0-9](_?[0-9])*([eE][\+\-]?[0-9](_?[0-9])*)?i?)(?<op>\.{2,3})");
        private static readonly TokenEntry comma = new TokenEntry(",", @",");
        private static readonly TokenEntry dot = new TokenEntry(".", @"\.");

        private static readonly IReadOnlyCollection<TokenEntry> tokenEntry = new[]{ 
            Lexer.identifier,
            new TokenEntry("$", @"\$"),

            new TokenEntry("StringLiteral", @"'(\\'|\\(\n|(\r\n)|\r|\u2028|\u2029)|.)*?'"),
            new TokenEntry("EmbedStringLiteral", @"""(\\""|\\(\n|(\r\n)|\r|\u2028|\u2029)|.)*?"""),
            new TokenEntry("WysiwygStringLiteral", new Regex(@"`(``|[^`])*`", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.ExplicitCapture)),

            numberAndRange,
                        
            new TokenEntry("RangeOpen", @"\.{3}"),
            new TokenEntry("RangeClose", @"\.{2}"),
            dot,
            new TokenEntry("Increment", @"\+{2}"),
            new TokenEntry("AssignmentAdd", @"\+="),
            new TokenEntry("+", @"\+"),
            new TokenEntry("Decrement", @"\-{2}"),
            new TokenEntry("AssignmentSub", @"\-="),
            new TokenEntry("AnnotationReturn", @"\-\>"),
            new TokenEntry("-", @"\-"),
            new TokenEntry("AssignmentConcat", @"~="),
            new TokenEntry("~", @"~"),
            new TokenEntry("AssignmentPower", @"\*{2}="),
            new TokenEntry("Power", @"\*{2}"),
            new TokenEntry("AssignmentMultiply", @"\*="),
            new TokenEntry("*", @"\*"),
            new TokenEntry("AssignmentIntDivide", @"//="),
            new TokenEntry("IntDivide", @"//"),
            new TokenEntry("AssignmentDivide", @"/="),
            new TokenEntry("/", @"/"),
            new TokenEntry("AssignmentModulo", @"%="),
            new TokenEntry("%", @"%"),
            new TokenEntry("AssignmentLeftShift", @"<<="),
            new TokenEntry("LeftShift", @"<<"),
            new TokenEntry("LessThan", @"<="),
            new TokenEntry("<", @"<"),
            new TokenEntry("AssignmentRightShift", @">>="),
            new TokenEntry("RightShift", @">>"),
            new TokenEntry("MoreThan", @">="),
            new TokenEntry(">", @">"),
            new TokenEntry("Equal", @"={2}"),
            new TokenEntry("Lambda", @"=>"),
            new TokenEntry("=", @"="),
            new TokenEntry("NotIn", @"!in"),
            new TokenEntry("NotIs", @"!is"),
            new TokenEntry("NotEqual", @"!="),
            new TokenEntry("!", @"!"),
            new TokenEntry("AndShort", @"&&"),
            new TokenEntry("AssginmentAnd", @"&="),
            new TokenEntry("&", @"&"),
            new TokenEntry("AssignmentXor", @"\^="),
            new TokenEntry("^", @"\^"),
            new TokenEntry("OrShort", @"\|{2}"),
            new TokenEntry("AssignmentOr", @"\|="),
            new TokenEntry("|", @"\|"),
            new TokenEntry("NilCoalesce", @"\?{2}"),
            new TokenEntry("?", @"\?"),
            new TokenEntry(":", @":"),
            comma,
            new TokenEntry("(", @"\("),
            new TokenEntry(")", @"\)"),
            new TokenEntry("[", @"\["),
            new TokenEntry("]", @"\]"),
            new TokenEntry("{", @"\{"),
            new TokenEntry("}", @"\}"),
            new TokenEntry(";", @";"),
            new TokenEntry("@", @"@"),
        };
        #endregion

        #region Number
        private static readonly IReadOnlyCollection<TokenEntry> number = new[]{ 
            new TokenEntry("ImaginaryNumber", @"(([0-9](_?[0-9])*)?\.[0-9](_?[0-9])*|[0-9](_?[0-9])*\.?)([eE][\+\-]?[0-9](_?[0-9])*)?i"),
            // Base Model:
            // (([0-9](_?[0-9])*|(([0-9](_?[0-9])*)?\.[0-9](_?[0-9])*|[0-9](_?[0-9])*\.))[eE][\+\-]?[0-9](_?[0-9])*|(([0-9](_?[0-9])*)?\.[0-9](_?[0-9])*|[0-9](_?[0-9])*\.))
            new TokenEntry("FloatNumber", @"(([0-9](_?[0-9])*|(([0-9](_?[0-9])*)?\.[0-9](_?[0-9])*|[0-9](_?[0-9])*\.))[eE][\+\-]?[0-9](_?[0-9])*|(([0-9](_?[0-9])*)?\.[0-9](_?[0-9])*|[0-9](_?[0-9])*\.))"),
            new TokenEntry("Integer", @"(0([xX][0-9a-fA-F](_?[0-9a-fA-F])*|[oO][0-7](_?[0-7])*|[bB][01](_?[01])*)|[0-9](_?[0-9])*)"),
        };
        #endregion

        #region Identifier
        private static readonly IReadOnlyCollection<TokenEntry> identifiers = new[]{
            new TokenEntry("IdentifierGet", @"get"),
            new TokenEntry("IdentifierSet", @"set"),
            new TokenEntry("IdentifierFile", @"file"),
            new TokenEntry("IdentifierLine", @"line"),
            new TokenEntry("IdentifierExit", @"exit"),
            new TokenEntry("IdentifierSuccess", @"success"),
            new TokenEntry("IdentifierFailure", @"failure"),

            new TokenEntry("KeywordAbstract", @"abstract"),
            new TokenEntry("KeywordAnd", @"and"),
            new TokenEntry("KeywordBreak", @"break"),
            new TokenEntry("KeywordCase", @"case"),
            new TokenEntry("KeywordCatch", @"catch"),
            new TokenEntry("KeywordClass", @"class"),
            new TokenEntry("KeywordContinue", @"continue"),
            new TokenEntry("KeywordDef", @"def"),
            new TokenEntry("KeywordDefault", @"default"),
            new TokenEntry("KeywordDelete", @"delete"),
            new TokenEntry("KeywordElif", @"elif"),
            new TokenEntry("KeywordElse", @"else"),
            new TokenEntry("KeywordEnum", @"enum"),
            new TokenEntry("KeywordExtended", @"extended"),
            new TokenEntry("KeywordFalse", @"false"),
            new TokenEntry("KeywordFinally", @"finally"),
            new TokenEntry("KeywordFor", @"for"),
            new TokenEntry("KeywordIf", @"if"),
            new TokenEntry("KeywordImport", @"import"),
            new TokenEntry("KeywordIn", @"in"),
            new TokenEntry("KeywordInterface", @"interface"),
            new TokenEntry("KeywordInvariant", @"invariant"),
            new TokenEntry("KeywordIs", @"is"),
            new TokenEntry("KeywordLazy", @"lazy"),
            new TokenEntry("KeywordNameof", @"nameof"),
            new TokenEntry("KeywordNew", @"new"),
            new TokenEntry("KeywordNil", @"nil"),
            new TokenEntry("KeywordNot", @"not"),
            new TokenEntry("KeywordOr", @"or"),
            new TokenEntry("KeywordOut", @"out"),
            new TokenEntry("KeywordOverride", @"override"),
            new TokenEntry("KeywordPass", @"pass"),
            new TokenEntry("KeywordPrivate", @"private"),
            new TokenEntry("KeywordProperty", @"property"),
            new TokenEntry("KeywordProtected", @"protected"),
            new TokenEntry("KeywordPublic", @"public"),
            new TokenEntry("KeywordRef", @"ref"),
            new TokenEntry("KeywordReflect", @"reflect"),
            new TokenEntry("KeywordReturn", @"return"),
            new TokenEntry("KeywordScope", @"scope"),
            new TokenEntry("KeywordSealed", @"sealed"),
            new TokenEntry("KeywordStatic", @"static"),
            new TokenEntry("KeywordSuper", @"super"),
            new TokenEntry("KeywordSwitch", @"switch"),
            new TokenEntry("KeywordThis", @"this"),
            new TokenEntry("KeywordThrow", @"throw"),
            new TokenEntry("KeywordTrue", @"true"),
            new TokenEntry("KeywordTry", @"try"),
            new TokenEntry("KeywordUnittest", @"unittest"),
            new TokenEntry("KeywordUnless", @"unless"),
            new TokenEntry("KeywordUntil", @"until"),
            new TokenEntry("KeywordVar", @"var"),
            new TokenEntry("KeywordWhile", @"while"),
            new TokenEntry("KeywordWith", @"with"),
            new TokenEntry("KeywordYield", @"yield"),
        };
        #endregion

        #region Space
        private static readonly TokenEntry space = new TokenEntry("Space", @"[\u0020\u0009\u000b\u000c]+");
        private static readonly TokenEntry endoffile = new TokenEntry("EndOfFile", @"[\u0000\u001a]");
        private static readonly TokenEntry newline = new TokenEntry("NewLine", @"(\n|(\r\n)|\r|\u2028|\u2029)");

        private static readonly IReadOnlyCollection<TokenEntry> whitespace = new[]{ 
            Lexer.space,
            Lexer.endoffile,
            Lexer.newline,
            //new TokenEntry("Indent"),
            //new TokenEntry("Dedent"),
        };
        #endregion

        #region Comment
        private static readonly IReadOnlyCollection<TokenEntry> comment = new[]{ 
            new TokenEntry("BlockComment", new Regex(@"###.*?###", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.ExplicitCapture)),
            new TokenEntry("LineComment", @"#[^\n\r\u2028\u2029]*"),
            new TokenEntry("LineCancel", @"\\(\n|(\r\n)|\r|\u2028|\u2029)"),
        };
        #endregion

        private int index;
        private bool commaDetected;
        private readonly List<Token> output;
        private readonly Stack<int> indentStack;

        public string SourceCode { get; private set; }

        public IEnumerable<Token> TokenOutput { get { return this.output; } }

        public Lexer(string sourceCode)
        {
            this.SourceCode = sourceCode;
            this.index = 0;
            this.output = new List<Token>();
            this.indentStack = new Stack<int>();
        }

        public void Tokenize()
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
                        throw new Exception("LexerError: 最初の行はインデントできません.");

                    if (lineBreak)
                        indentSpace = m;

                    zeroWidthIndent = false;
                }
                else if (IsMatch(endoffile, this.SourceCode, this.index, out m))
                    reachEndOfFile = true;
                else if (IsMatch(newline, this.SourceCode, this.index, out m))
                {
                    if(!lineBreak)
                        newlineMatch = m;

                    lineBreak = true;
                    zeroWidthIndent = true;
                }
                else if (!this.MatchComment(out m))
                {
                    TokenEntry entry;

                    if (lineBreak)
                        this.output.Add(new Token(newline, newlineMatch.Value, newlineMatch.Index, this.SourceCode.GetPositionByIndex(newlineMatch.Index)));

                    if (zeroWidthIndent || lineBreak && indentSpace != null)
                    {
                        if (!commaDetected)
                            this.StackIndent(indentSpace);

                        indentSpace = null;
                    }

                    commaDetected = false;
                    lineBreak = false;
                    zeroWidthIndent = false;

                    if (this.MatchOtherTokens(out m, out entry))
                        this.output.Add(new Token(entry, m.Value, this.index, this.SourceCode.GetPositionByIndex(this.index)));
                    else
                        throw new Exception("LexerError: 認識できない文字が検出されました. " + this.SourceCode[this.index]);
                }

                this.MoveForward(m);
                checkIndentFirstLine = true;
            }

            // End of SourceCode
            this.index = this.SourceCode.Length - 1;

            if (indentStack.Count > 1)
            {
                this.StackIndent(null);
                lineBreak = false;
            }

            this.output.Add(new Token(endoffile, "", this.SourceCode.Length == 0 ? 0 : this.index,  this.SourceCode.Length == 0 ? CharPosition.BasePosition : this.SourceCode.GetPositionByIndex(this.index)));
        }

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
                        if (MatchTokenEntries(Lexer.number, tempMatch.Groups["num"].Value, 0, out m, out tokenEntry))
                            this.output.Add(new Token(tokenEntry, tempMatch.Groups["num"].Value, this.index,  this.SourceCode.GetPositionByIndex(this.index)));
                        else
                            throw new Exception("不明なエラー.");

                        this.MoveForward(m);

                        if (!MatchTokenEntries(Lexer.tokenEntry, tempMatch.Groups["op"].Value, 0, out m, out tokenEntry))
                            throw new Exception("不明なエラー.");

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

        private void StackIndent(Match match)
        {
            int level = (match == null) ? 0 : match.Length;
            int peek = this.indentStack.Peek();

            if (peek == level)
                return;
            //else if (level == 0)    // Empty Line
            //    yield break;
            else if (peek < level)
            {
                indentStack.Push(level);
                this.output.Add(new Token(new TokenEntry("Indent"), "", this.index, this.SourceCode.GetPositionByIndex(this.index)));
            }
            else // peek > level
            {
                do
                {
                    indentStack.Pop();
                    this.output.Add(new Token(new TokenEntry("Dedent"), "", this.index, this.SourceCode.GetPositionByIndex(this.index)));

                    if (indentStack.Count == 0 || indentStack.Peek() == level)
                        break;
                }
                while (indentStack.Count > 0);

                if (indentStack.Count == 0)
                    throw new Exception("LexerError: インデントが不正です.");
            }
        }

        private static bool IsMatch(TokenEntry entry, string source, int index, out Match match, bool perfect = false)
        {
            match = entry.Regex.Match(source, index);
            return match.Success && match.Index == index && (!perfect || match.Length == source.Length);
        }

        private void MoveForward(Match match)
        {
            this.index += match.Length;
        }
    }
}

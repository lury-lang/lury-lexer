//
// TokenEntries.cs
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

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Lury.Compiling.Lexer
{
    public partial class Lexer
    {
        #region Token Entry
        // Unicode 6.0 Emoji Code Blocks
        //     U+ : Unicode
        //     \u : UTF-16 and Regex format
        //
        // Geometric Shapes (U+25A0 - U+25FF)
        //     [\u25a0-\u25ff]
        // Miscellaneous Symbols (U+2600 - U+26FF)
        //     [\u2600-\u26ff]
        // Dingbats (U+2700 - U+27BF)
        //     [\u2700-\u27bf]
        // Miscellaneous Symbols And Pictographs (U+1F300 - U+1F5FF)
        //     \ud83c[\udf00-\udff7]|\ud83d[\udc00-\uddff]
        // Emoticons (U+1F600 - U+1F64F)
        //     \ud83d[\ude00-\ude4f]
        // Transport and Map Symbols (U+1F680 - U+1F6FF)
        //     \ud83d[\ude80-\udef3]
        //
        // Combined: ([\u25a0-\u27bf]|\ud83c[\udf00-\udff7]|\ud83d[\udc00-\ude4f\ude80-\udef3])
        //
        private static readonly RegexTokenEntry identifier = new RegexTokenEntry("Identifier", @"(\uD83C[\uDF00-\uDFF7]|\uD83D[\uDC00-\uDE4F\uDE80-\uDEF3]|[_\p{Lu}\p{Ll}\p{Lt}\p{Lm}\p{Lo}\p{Nl}\u25A0-\u27BF])(\uD83C[\uDF00-\uDFF7]|\uD83D[\uDC00-\uDE4F\uDE80-\uDEF3]|[_\p{Lu}\p{Ll}\p{Lt}\p{Lm}\p{Lo}\p{Nl}\p{Mn}\p{Mc}\p{Nd}\p{Pc}\u25A0-\u27BF])*");

        private static readonly RegexTokenEntry
            StringLiteral = new RegexTokenEntry("StringLiteral", @"'(\\'|\\(\n|(\r\n)|\r|\u2028|\u2029)|.)*?'"),
            EmbedStringLiteral = new RegexTokenEntry("EmbedStringLiteral", @"""(\\""|\\(\n|(\r\n)|\r|\u2028|\u2029)|.)*?"""),
            WysiwygStringLiteral = new RegexTokenEntry("WysiwygStringLiteral", new Regex(@"`(``|[^`])*`", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.ExplicitCapture));

        #region Static Characters

        private static readonly StaticTokenEntry comma = new StaticTokenEntry(",");
        private static readonly StaticTokenEntry dot = new StaticTokenEntry(".");

        private static readonly IReadOnlyCollection<StaticTokenEntry> staticAndOperators = new[]{ 
            new StaticTokenEntry("$"),
            new StaticTokenEntry("RangeOpen", "..."),
            new StaticTokenEntry("RangeClose", ".."),
            dot,
            new StaticTokenEntry("Increment", "++"),
            new StaticTokenEntry("AssignmentAdd", "+="),
            new StaticTokenEntry("+"),
            new StaticTokenEntry("Decrement", "--"),
            new StaticTokenEntry("AssignmentSub", "-="),
            new StaticTokenEntry("AnnotationReturn", "->"),
            new StaticTokenEntry("-"),
            new StaticTokenEntry("AssignmentConcat", "~="),
            new StaticTokenEntry("~"),
            new StaticTokenEntry("AssignmentPower", "**="),
            new StaticTokenEntry("Power", "**"),
            new StaticTokenEntry("AssignmentMultiply", "*="),
            new StaticTokenEntry("*"),
            new StaticTokenEntry("AssignmentIntDivide", "//="),
            new StaticTokenEntry("IntDivide", "//"),
            new StaticTokenEntry("AssignmentDivide", "/="),
            new StaticTokenEntry("/"),
            new StaticTokenEntry("AssignmentModulo", "%="),
            new StaticTokenEntry("%"),
            new StaticTokenEntry("AssignmentLeftShift", "<<="),
            new StaticTokenEntry("LeftShift", "<<"),
            new StaticTokenEntry("LessThan", "<="),
            new StaticTokenEntry("<"),
            new StaticTokenEntry("AssignmentRightShift", ">>="),
            new StaticTokenEntry("RightShift", ">>"),
            new StaticTokenEntry("MoreThan", ">="),
            new StaticTokenEntry(">"),
            new StaticTokenEntry("Equal", "=="),
            new StaticTokenEntry("Lambda", "=>"),
            new StaticTokenEntry("="),
            new StaticTokenEntry("NotEqual", "!="),
            new StaticTokenEntry("!"),
            new StaticTokenEntry("AndShort", "&&"),
            new StaticTokenEntry("AssginmentAnd", "&="),
            new StaticTokenEntry("&"),
            new StaticTokenEntry("AssignmentXor", "^="),
            new StaticTokenEntry("^"),
            new StaticTokenEntry("OrShort", "||"),
            new StaticTokenEntry("AssignmentOr", "|="),
            new StaticTokenEntry("|"),
            new StaticTokenEntry("NilCoalesce", "??"),
            new StaticTokenEntry("?"),
            new StaticTokenEntry(":"),
            comma,
            new StaticTokenEntry("("),
            new StaticTokenEntry(")"),
            new StaticTokenEntry("["),
            new StaticTokenEntry("]"),
            new StaticTokenEntry("{"),
            new StaticTokenEntry("}"),
            new StaticTokenEntry(";"),
            new StaticTokenEntry("@"),
        };

        #endregion
        #endregion

        #region Number
        private static readonly RegexTokenEntry
            ImaginaryNumber = new RegexTokenEntry("ImaginaryNumber", @"(([0-9](_?[0-9])*)?\.[0-9](_?[0-9])*|[0-9](_?[0-9])*\.?)([eE][\+\-]?[0-9](_?[0-9])*)?i?"),
            FloatNumber = new RegexTokenEntry("FloatNumber", @"(([0-9](_?[0-9])*|(([0-9](_?[0-9])*)?\.[0-9](_?[0-9])*|[0-9](_?[0-9])*\.))[eE][\+\-]?[0-9](_?[0-9])*|(([0-9](_?[0-9])*)?\.[0-9](_?[0-9])*|[0-9](_?[0-9])*\.))"),
            Integer = new RegexTokenEntry("Integer", @"(0([xX][0-9a-fA-F](_?[0-9a-fA-F])*|[oO][0-7](_?[0-7])*|[bB][01](_?[01])*)|[0-9](_?[0-9])*)");
        #endregion

        #region Identifier
        private static readonly IReadOnlyCollection<StaticTokenEntry> identifiers = new[]{
            new StaticTokenEntry("IdentifierGet", @"get"),
            new StaticTokenEntry("IdentifierSet", @"set"),
            new StaticTokenEntry("IdentifierFile", @"file"),
            new StaticTokenEntry("IdentifierLine", @"line"),
            new StaticTokenEntry("IdentifierExit", @"exit"),
            new StaticTokenEntry("IdentifierSuccess", @"success"),
            new StaticTokenEntry("IdentifierFailure", @"failure"),

            new StaticTokenEntry("KeywordAbstract", @"abstract"),
            new StaticTokenEntry("KeywordAnd", @"and"),
            new StaticTokenEntry("KeywordBreak", @"break"),
            new StaticTokenEntry("KeywordCase", @"case"),
            new StaticTokenEntry("KeywordCatch", @"catch"),
            new StaticTokenEntry("KeywordClass", @"class"),
            new StaticTokenEntry("KeywordContinue", @"continue"),
            new StaticTokenEntry("KeywordDef", @"def"),
            new StaticTokenEntry("KeywordDefault", @"default"),
            new StaticTokenEntry("KeywordDelete", @"delete"),
            new StaticTokenEntry("KeywordElif", @"elif"),
            new StaticTokenEntry("KeywordElse", @"else"),
            new StaticTokenEntry("KeywordEnum", @"enum"),
            new StaticTokenEntry("KeywordExtended", @"extended"),
            new StaticTokenEntry("KeywordFalse", @"false"),
            new StaticTokenEntry("KeywordFinally", @"finally"),
            new StaticTokenEntry("KeywordFor", @"for"),
            new StaticTokenEntry("KeywordIf", @"if"),
            new StaticTokenEntry("KeywordImport", @"import"),
            new StaticTokenEntry("KeywordIn", @"in"),
            new StaticTokenEntry("KeywordInterface", @"interface"),
            new StaticTokenEntry("KeywordInvariant", @"invariant"),
            new StaticTokenEntry("KeywordIs", @"is"),
            new StaticTokenEntry("KeywordLazy", @"lazy"),
            new StaticTokenEntry("KeywordNameof", @"nameof"),
            new StaticTokenEntry("KeywordNew", @"new"),
            new StaticTokenEntry("KeywordNil", @"nil"),
            new StaticTokenEntry("KeywordNot", @"not"),
            new StaticTokenEntry("KeywordOr", @"or"),
            new StaticTokenEntry("KeywordOut", @"out"),
            new StaticTokenEntry("KeywordOverride", @"override"),
            new StaticTokenEntry("KeywordPass", @"pass"),
            new StaticTokenEntry("KeywordPrivate", @"private"),
            new StaticTokenEntry("KeywordProperty", @"property"),
            new StaticTokenEntry("KeywordProtected", @"protected"),
            new StaticTokenEntry("KeywordPublic", @"public"),
            new StaticTokenEntry("KeywordRef", @"ref"),
            new StaticTokenEntry("KeywordReflect", @"reflect"),
            new StaticTokenEntry("KeywordReturn", @"return"),
            new StaticTokenEntry("KeywordScope", @"scope"),
            new StaticTokenEntry("KeywordSealed", @"sealed"),
            new StaticTokenEntry("KeywordStatic", @"static"),
            new StaticTokenEntry("KeywordSuper", @"super"),
            new StaticTokenEntry("KeywordSwitch", @"switch"),
            new StaticTokenEntry("KeywordThis", @"this"),
            new StaticTokenEntry("KeywordThrow", @"throw"),
            new StaticTokenEntry("KeywordTrue", @"true"),
            new StaticTokenEntry("KeywordTry", @"try"),
            new StaticTokenEntry("KeywordUnittest", @"unittest"),
            new StaticTokenEntry("KeywordUnless", @"unless"),
            new StaticTokenEntry("KeywordUntil", @"until"),
            new StaticTokenEntry("KeywordVar", @"var"),
            new StaticTokenEntry("KeywordWhile", @"while"),
            new StaticTokenEntry("KeywordWith", @"with"),
            new StaticTokenEntry("KeywordYield", @"yield"),
        };
        #endregion

        #region Space
        private static readonly StaticTokenEntry endoffile = new StaticTokenEntry("EndOfFile");
        private static readonly StaticTokenEntry newline = new StaticTokenEntry("NewLine");

        private static readonly StaticTokenEntry indent = new StaticTokenEntry("Indent");
        private static readonly StaticTokenEntry dedent = new StaticTokenEntry("Dedent");
        #endregion
    }
}

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
        private static readonly TokenEntry identifier = new TokenEntry("Identifier", @"(\uD83C[\uDF00-\uDFF7]|\uD83D[\uDC00-\uDE4F\uDE80-\uDEF3]|[_\p{Lu}\p{Ll}\p{Lt}\p{Lm}\p{Lo}\p{Nl}\u25A0-\u27BF])(\uD83C[\uDF00-\uDFF7]|\uD83D[\uDC00-\uDE4F\uDE80-\uDEF3]|[_\p{Lu}\p{Ll}\p{Lt}\p{Lm}\p{Lo}\p{Nl}\p{Mn}\p{Mc}\p{Nd}\p{Pc}\u25A0-\u27BF])*");
        
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

        private static readonly TokenEntry indent = new TokenEntry("Indent");
        private static readonly TokenEntry dedent = new TokenEntry("Dedent");

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
    }
}

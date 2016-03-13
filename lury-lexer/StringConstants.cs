//
// StringConstants.cs
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

namespace Lury.Compiling.Lexer
{
    /// <summary>
    /// 字句解析で用いられる文字列定数を格納します。
    /// </summary>
    internal static class StringConstants
    {
        #region -- Public Static Fields --

        public static readonly string[]
           
            LineCancel = {
                "\\\u000d\u000a",
                "\\\u000a",
                "\\\u000d",
                "\\\u2028",
                "\\\u2029"
            },

            NewLine = {
                "\u000d\u000a",
                "\u000a",
                "\u000d",
                "\u2028",
                "\u2029"
            },

            LineBreak = {
                "\u000d\u000a",
                "\u000a",
                "\u000d",
                "\u2028",
                "\u2029",
                "\u0000",
                "\u001a"
            },
            
            DigitAndDot = {
                "0",
                "1",
                "2",
                "3",
                "4",
                "5",
                "6",
                "7",
                "8",
                "9",
                ".0",
                ".1",
                ".2",
                ".3",
                ".4",
                ".5",
                ".6",
                ".7",
                ".8",
                ".9"
            };

        public static readonly char[]

            Space = { 
                '\u0020',
                '\u0009',
                '\u000b',
                '\u000c'
            },

            EndOfFile = {
                '\u0000',
                '\u001a'
            },

            StringLiteral = {
                '\'',
                '"',
                '`'
            },

            Digit = {
                '0',
                '1',
                '2',
                '3',
                '4',
                '5',
                '6',
                '7',
                '8',
                '9'
            },

            DigitWithoutZero = {
                '1',
                '2',
                '3',
                '4',
                '5',
                '6',
                '7',
                '8',
                '9'
            },

            Hexadecimal = {
                '0',
                '1',
                '2',
                '3',
                '4',
                '5',
                '6',
                '7',
                '8',
                '9',
                'a',
                'b',
                'c',
                'd',
                'e',
                'f',
                'A',
                'B',
                'C',
                'D',
                'E',
                'F'
            },

            Octal = {
                '0',
                '1',
                '2',
                '3',
                '4',
                '5',
                '6',
                '7'
            },

            OperatorAndDelimiter = {
                '.',
                '+',
                '-',
                '~',
                '*',
                '/',
                '%',
                '<',
                '>',
                '=',
                '!',
                '&',
                '^',
                '|',
                '?',
                ':',
                ',',
                '(',
                ')',
                '[',
                ']',
                '{',
                '}',
                ';',
                '@',
                '$'
            };

        #endregion
    }
}

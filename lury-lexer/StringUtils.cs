//
// StringUtils.cs
//
// Author:
//       Tomona Nanase <nanase@users.noreply.github.com>
//
// The MIT License (MIT)
//
// Copyright (c) 2014-2015 Tomona Nanase
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
using System.Text.RegularExpressions;

namespace Lury.Compiling.Lexer
{
    /// <summary>
    /// 文字列に対する拡張メソッドを提供します。
    /// </summary>
    static class StringUtils
    {
        #region -- Private Static Fields --

        private static readonly Regex NewLine = new Regex(@"(?:\n|(?:\r\n)|\r|\u2028|\u2029)", RegexOptions.Compiled | RegexOptions.Singleline);

        #endregion

        #region -- Public Static Methods --

        /// <summary>
        /// 文字列の行数を取得します。
        /// </summary>
        /// <returns>指定された文字列の行数。</returns>
        /// <param name="text">文字列。null の場合は常に 0 が返されます。</param>
        public static int GetNumberOfLine(this string text)
        {
            return (text == null) ? 0 : NewLine.Matches(text).Count + 1;
        }

        /// <summary>
        /// 文字列の行と列の位置をインデクスから求めます。
        /// </summary>
        /// <returns>インデクスに対応する行と列の位置。</returns>
        /// <param name="text">文字列。</param>
        /// <param name="index">文字列の位置を指し示すインデクス。</param>
        public static CharPosition GetPositionByIndex(this string text, int index)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            if (index == 0 && text.Length == 0)
                return CharPosition.BasePosition;

            if (index < 0 || index >= text.Length)
                throw new ArgumentOutOfRangeException("index");

            CharPosition 　position = CharPosition.BasePosition;
            Match prevMatch = null;

            foreach (Match match in NewLine.Matches(text))
            {
                if (match.Index + match.Length - 1 >= index)
                    break;

                prevMatch = match; 
                position.Line++;
            }

            position.Column = (prevMatch == null) ? index + 1 :
                     index - prevMatch.Index - prevMatch.Length + 1;

            return position;
        }

        #endregion
    }
}


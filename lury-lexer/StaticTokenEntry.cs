//
// TokenEnrty.cs
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

using System.Text.RegularExpressions;

namespace Lury.Compiling.Lexer
{
    /// <summary>
    /// トークンの値が変化しないトークンエントリを表します。
    /// </summary>
    public class StaticTokenEntry : TokenEntry
    {
        #region -- Public Properties --

        /// <summary>
        /// トークンの値を表す文字列を取得します。
        /// </summary>
        public string TokenValue { get; private set; }

        #endregion

        #region -- Constructors --

        /// <summary>
        /// トークン名を指定して新しい StaticTokenEntry クラスのインスタンスを初期化します。
        /// トークン値はトークン名と同一となります。
        /// </summary>
        /// <param name="name">トークン名およびトークン値。</param>
        public StaticTokenEntry(string name)
            : this(name, name)
        {
        }

        /// <summary>
        /// トークン名とトークン値を指定して新しい StaticTokenEntry クラスのインスタンスを初期化します。
        /// </summary>
        /// <param name="name">トークン名。</param>
        /// <param name="tokenValue">トークン値。</param>
        public StaticTokenEntry(string name, string tokenValue)
            : base(name)
        {
            this.TokenValue = tokenValue;
        }

        #endregion
    }
}

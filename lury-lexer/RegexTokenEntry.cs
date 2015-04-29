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
    /// 正規表現を使ったマッチングが必要なトークンエントリを表します。
    /// </summary>
    public class RegexTokenEntry : TokenEntry
    {
        #region -- Public Properties --

        /// <summary>
        /// マッチングに使用する正規表現オブジェクトを取得します。
        /// </summary>
        public Regex Regex { get; private set; }

        #endregion

        #region -- Constructors --

        /// <summary>
        /// トークン名と正規表現オブジェクトを指定して新しい RegexTokenEntry クラスのインスタンスを初期化します。
        /// </summary>
        /// <param name="name">トークン名。</param>
        /// <param name="regex">正規表現オブジェクト。</param>
        public RegexTokenEntry(string name, Regex regex)
            : base(name)
        {
            this.Regex = regex;
        }

        /// <summary>
        /// トークン名と正規表現パターンを指定して新しい RegexTokenEntry クラスのインスタンスを初期化します。
        /// </summary>
        /// <param name="name">トークン名。</param>
        /// <param name="regex">
        /// 正規表現パターン。
        /// 既定で RegexOptions.Compiled および RegexOptions.ExplicitCapture 列挙体が指定されます。
        /// </param>
        public RegexTokenEntry(string name, string regex)
            : this(name, new Regex(regex, RegexOptions.Compiled | RegexOptions.ExplicitCapture))
        {
        }

        #endregion
    }
}

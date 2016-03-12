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

namespace Lury.Compiling.Lexer
{
    /// <summary>
    /// 出力されるトークンを区別するためのクラスです。
    /// </summary>
    public class TokenEntry
    {
        #region -- Public Properties --

        /// <summary>
        /// トークン名を表す文字列を取得します。
        /// </summary>
        public string Name { get; }

        #endregion

        #region -- Constructors --

        /// <summary>
        /// トークン名を指定して新しい TokenEntry クラスのインスタンスを初期化します。
        /// </summary>
        /// <param name="name">トークン名。</param>
        public TokenEntry(string name)
        {
            this.Name = name;
        }

        #endregion
    }
}

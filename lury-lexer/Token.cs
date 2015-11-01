//
// Token.cs
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
using Lury.Compiling.Utils;

namespace Lury.Compiling.Lexer
{
    /// <summary>
    /// 字句解析で生成されたトークンを表します。
    /// </summary>
    public class Token
    {
        #region -- Public Properties --

        /// <summary>
        /// トークンエントリを取得します。
        /// </summary>
        public TokenEntry Entry { get; private set; }

        /// <summary>
        /// トークンの文字列を取得します。
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// トークンの長さを取得します。
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// トークンの SourceCode での出現位置を取得します。
        /// </summary>
        public CodePosition Position
        {
            get
            {
                var pos = this.SourceCode.GetPositionByIndex(this.Index);
                return new CodePosition(this.SourceName, pos);
            }
        }

        /// <summary>
        /// トークンの SourceCode でのインデクスを取得します。
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// 字句解析されたソースコードを表す文字列を取得します。
        /// </summary>
        public string SourceCode { get; private set; }

        /// <summary>
        /// 字句解析されたソースコードを識別するための名前を取得します。
        /// </summary>
        /// <value>ソースコードの名前。</value>
        public string SourceName { get; private set; }

        #endregion

        #region -- Constructors --

        /// <summary>
        /// パラメータを指定して新しい Token クラスのインスタンスを初期化します。
        /// </summary>
        /// <param name="entry">トークンエントリ。</param>
        /// <param name="sourceName">字句解析された元のソースコードの名前。</param>
        /// <param name="sourceCode">字句解析された元のソースコード。</param>
        /// <param name="index">トークンが出現したインデクス。</param>
        /// <param name="length">トークンの長さ。</param>
        public Token(TokenEntry entry, string sourceName, string sourceCode, int index, int length)
        {
            if (entry == null)
                throw new ArgumentNullException("entry");

            if (sourceName == null)
                throw new ArgumentNullException("sourceName");

            if (sourceCode == null)
                throw new ArgumentNullException("sourceCode");

            if (sourceCode.Length < index + length)
                throw new ArgumentOutOfRangeException("length");

            // index == sourceCode.Length when entry == EndOfFile!
            if (index < 0 || index > sourceCode.Length)
                throw new ArgumentOutOfRangeException("index");

            if (length < 0 || length > sourceCode.Length)
                throw new ArgumentOutOfRangeException("length");

            this.Entry = entry;
            this.SourceName = sourceName;
            this.SourceCode = sourceCode;
            this.Index = index;
            this.Length = length;

            this.Text = (length == 0 && index == sourceCode.Length) ?
                string.Empty :
                this.SourceCode.Substring(index, length);
        }

        #endregion

        #region -- Public Methods --

        /// <summary>
        /// このトークンオブジェクトの現在の状態を表す文字列を取得します。
        /// </summary>
        /// <returns>トークンエントリの名前、出現位置そしてトークン文字列を含む文字列。</returns>
        public override string ToString()
        {
            return string.Format(
                "{0} {1}{2}",
                this.Position.Position,
                this.Entry.Name,
                this.Entry.Name.Length > 1 ? " - " + this.Text : "");
        }

        #endregion
    }
}

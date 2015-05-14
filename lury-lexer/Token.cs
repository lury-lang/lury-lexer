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
    public class Token
    {
        #region -- Public Properties --

        public TokenEntry Entry { get; private set; }

        public string Text
        {
            get
            {
                if (this.Length == 0)
                    return "";
                else
                    return this.SourceCode.Substring(this.Index, this.Length);
            }
        }

        public int Length { get; private set; }

        public CharPosition Position
        {
            get
            {
                return this.SourceCode.GetPositionByIndex(this.Index);
            }
        }

        public int Index { get; private set; }

        public string SourceCode { get; private set; }

        #endregion

        #region -- Constructors --

        public Token(TokenEntry entry, string sourceCode, int index, int length)
        {
            if (sourceCode.Length < index + length)
                throw new ArgumentOutOfRangeException("length");

            this.Entry = entry;
            this.SourceCode = sourceCode;
            this.Index = index;
            this.Length = length;
        }

        #endregion

        #region -- Public Methods --

        public override string ToString()
        {
            return string.Format("{0} {1}{2}", this.Position, this.Entry.Name, this.Entry.Name.Length > 1 ? " - " + this.Text : "");
        }

        #endregion
    }
}

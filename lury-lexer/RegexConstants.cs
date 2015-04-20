//
// RegexConstants.cs
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
    internal class RegexConstants
    {
        #region -- Public Static Fields --

        public static readonly Regex

            IntegerAndRange = new Regex(@"(?<num>0([xX][0-9a-fA-F](_?[0-9a-fA-F])*|[oO][0-7](_?[0-7])*|[bB][01](_?[01])*)|[0-9](_?[0-9])*([eE][\+\-]?[0-9](_?[0-9])*)?i?)(\.{2,3})", RegexOptions.Compiled | RegexOptions.ExplicitCapture),
            Integer = new Regex(@"(0([xX][0-9a-fA-F](_?[0-9a-fA-F])*|[oO][0-7](_?[0-7])*|[bB][01](_?[01])*)|[0-9](_?[0-9])*([eE][\+\-]?[0-9](_?[0-9])*)?i?)", RegexOptions.Compiled | RegexOptions.ExplicitCapture),
            FloatAndImaginary = new Regex(@"(([0-9](_?[0-9])*|(([0-9](_?[0-9])*)?\.[0-9](_?[0-9])*|[0-9](_?[0-9])*\.))[eE][\+\-]?[0-9](_?[0-9])*|(([0-9](_?[0-9])*)?\.[0-9](_?[0-9])*|[0-9](_?[0-9])*\.))i?", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        #endregion
    }
}

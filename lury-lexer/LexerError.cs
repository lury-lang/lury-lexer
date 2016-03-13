//
// LexerError.cs
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
    // Lexer's Number Range: 0x10_0000 - 0x1f_ffff

    /// <summary>
    /// エラーを識別するための列挙体です。
    /// </summary>
    public enum LexerError
    {
        // Number Range: 0x10_0000 - 0x10_ffff

        /// <summary>
        /// 不明なエラーです。
        /// </summary>
        Unknown = 0x100000,

        /// <summary>
        /// 論理行の一行目に無効なインデントが存在します。
        /// </summary>
        InvalidIndentFirstLine = 0x100001,

        /// <summary>
        /// 認識できない文字が検出されました。
        /// </summary>
        InvalidCharacter = 0x100002,

        /// <summary>
        /// 無効なインデントが検出されました。
        /// </summary>
        InvalidIndent = 0x100003,

        /// <summary>
        /// バックスラッシュの後に予期しない文字が存在します。
        /// </summary>
        UnexpectedCharacterAfterBackslash = 0x100004,

        /// <summary>
        /// ブロックコメントが閉じられていません。
        /// </summary>
        UnclosedBlockComment = 0x100005,

        /// <summary>
        /// 文字列リテラルが閉じられていません。
        /// </summary>
        UnclosedStringLiteral = 0x100006,

        /// <summary>
        /// インデントの文字種が混在しています。
        /// </summary>
        IndentCharacterConfusion = 0x100007
    }

    /// <summary>
    /// 警告を識別するための列挙体です。
    /// </summary>
    public enum LexerWarning
    {
        // Number Range: 0x11_0000 - 0x11_ffff
        
        /// <summary>
        /// 不明な警告です。
        /// </summary>
        Unknown = 0x110000
    }

    /// <summary>
    /// 情報を識別するための列挙体です。
    /// </summary>
    public enum LexerInfo
    {
        // Number Range: 0x12_0000 - 0x12_ffff

        /// <summary>
        /// 不明な情報です。
        /// </summary>
        Unknown = 0x120000
    }
}

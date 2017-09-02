﻿// MIT License
// 
// Copyright(c) 2017 Luciano (Xeeynamo) Ciccariello
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using RSDK;
using System;
using System.Collections.Generic;
using System.IO;

namespace RSDK5
{
    public class Animation
    {
        const int MagicCode = 0x00525053;

        public List<string> SpriteSheets { get; }
        
        public Animation(BinaryReader reader)
        {
            int r;
            if ((r = reader.ReadInt32()) != MagicCode)
                throw new InvalidProgramException($"Magic Code {r.ToString("X08")} not recognized.");

            reader.ReadInt32(); // ???

            int spriteSheetsCount = reader.ReadByte();
            SpriteSheets = new List<string>(spriteSheetsCount);
            while (spriteSheetsCount-- > 0)
                SpriteSheets.Add(StringEncoding.GetString(reader));


        }
    }
}

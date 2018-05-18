// MIT License
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

using System.IO;
using RSDK;

namespace RSDK1
{
    public class Frame : IFrame
    {
        public int SpriteSheet { get; set; }

        public int CollisionBox { get; set; }

        public int Id { get => 0; set { } }

        public bool flag1 { get => false; set { } }

        public bool flag2 { get => false; set { } }

        public int Duration
        {
            get => 256;
            set { }
        }

        public int X { get; set; }

        public int Y { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public int CenterX { get; set; }

        public int CenterY { get; set; }

        public IHitbox GetHitbox(int index)
        {
            return new Hitbox();
        }

        public void SaveChanges(BinaryWriter writer)
        {
            Write(writer);
        }

        public void Read(BinaryReader reader)
        {
                // byte 1 - Number of image the frame is located in
                // byte 2 - A flag mask of some kind
                //		In Sonic 1 and Sonic 2, it has value 3 when crouching down, value 1 when jumping
                //		In Sonic CD, it has value 1 for 3D Ramp 7, and value 2 for Size change,
                //		and value 1 when jumping.
                //		The bitmask most likely is composed by two bits.			
                // byte 3 - X position in image of the frame
                // byte 4 - Y position in image of the frame
                // byte 5 - Width of frame
                // byte 6 - Height of frame
                // byte 7 - Hot spot horizontal displacement (signed)
                // byte 8 - Hot spot vertical displacement (signed)				
                SpriteSheet = reader.ReadByte();
                int buf = reader.ReadByte();
                flag1 = (buf & 1) > 0;
                flag2 = (buf & 2) > 0;
                X = reader.ReadByte();
                Y = reader.ReadByte();
                Width = reader.ReadByte();
                Height = reader.ReadByte();
                CenterX = reader.ReadSByte();
                CenterY = reader.ReadSByte();
                Id = 0;

            /*SpriteSheet = reader.ReadByte();
    CollisionBox = reader.ReadByte();
    Id = 0;
    X = reader.ReadByte();
    Y = reader.ReadByte();
    Width = reader.ReadByte();
    Height = reader.ReadByte();
    CenterX = reader.ReadSByte();
    CenterY = reader.ReadSByte();*/
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write((byte)SpriteSheet);
            writer.Write((byte)CollisionBox);
            writer.Write((byte)X);
            writer.Write((byte)Y);
            writer.Write((byte)Width);
            writer.Write((byte)Height);
            writer.Write((byte)CenterX);
            writer.Write((byte)CenterY);
        }

        public object Clone()
        {
            return new Frame()
            {
                SpriteSheet = SpriteSheet,
                CollisionBox = CollisionBox,
                X = X,
                Y = Y,
                Width = Width,
                Height = Height,
                CenterX = CenterX,
                CenterY = CenterY
            };
        }
    }
}

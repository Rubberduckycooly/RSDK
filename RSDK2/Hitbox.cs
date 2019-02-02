using RSDK;
using System.IO;

namespace RSDK2
{
    public class Hitbox : IHitbox
    {
        public int Left { get; set; }

        public int Top { get; set; }

        public int Right { get; set; }

        public int Bottom { get; set; }

        public Hitbox() { }

        public Hitbox(BinaryReader reader,bool BitFlipped = false)
        {
            Read(reader,BitFlipped);
        }

        public void SaveChanges(BinaryWriter writer)
        {
            Write(writer);
        }
        public void Read(BinaryReader reader)
        {
            Left = reader.ReadSByte();
            Top = reader.ReadSByte();
            Right = reader.ReadSByte();
            Bottom = reader.ReadSByte();
        }

        public void Read(BinaryReader reader,bool BitFlipped = false)
        {
            Left = reader.ReadSByte();
            Top = reader.ReadSByte();
            Right = reader.ReadSByte();
            Bottom = reader.ReadSByte();
            if (BitFlipped)
            {
                byte l = (byte)Left;
                byte r = (byte)Right;
                byte b = (byte)Bottom;
                byte t = (byte)Top;
                l ^= 255;
                t ^= 255;
                r ^= 255;
                b ^= 255;
                Left = (sbyte)l;
                Right = (sbyte)r;
                Bottom = (sbyte)b;
                Top = (sbyte)t;
            }
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write((sbyte)Left);
            writer.Write((sbyte)Top);
            writer.Write((sbyte)Right);
            writer.Write((sbyte)Bottom);
        }

        public object Clone()
        {
            return new Hitbox()
            {
                Left = Left,
                Top = Top,
                Right = Right,
                Bottom = Bottom
            };
        }
    }
}

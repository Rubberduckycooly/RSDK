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
using System.Linq;

namespace RSDK2
{
    public class Animation : IAnimation
    {
        public int Version => 2;

        public List<string> SpriteSheets { get; }

        public List<AnimationEntry> Animations { get; }

        public List<HitboxEntry> Hitboxes { get; }

        public IEnumerable<string> HitboxTypes => null;

        public byte[] UnusedBytes;

        public Animation(BinaryReader reader)
        {
            UnusedBytes = reader.ReadBytes(5); //skip these bytes, as they seem to be useless/unused...

            int spriteSheetsCount = 3; //always 3

            SpriteSheets = new List<string>(spriteSheetsCount);

            byte[] byteBuf = null;

            for (int i = 0; i < spriteSheetsCount; i++)
            {
                int sLen = reader.ReadByte();
                byteBuf = new byte[sLen];

                byteBuf = reader.ReadBytes(sLen);

                string result = System.Text.Encoding.UTF8.GetString(byteBuf);

                SpriteSheets.Add(result);
                Console.WriteLine(result);
            }
            byteBuf = null;

            byte EndTexFlag = reader.ReadByte(); //Seems to tell the RSDK's reader when to stop reading textures???
            
            // Read number of animations
            var animationsCount = reader.ReadByte();

            Animations = new List<AnimationEntry>(animationsCount);

            for (int i = 0; i < animationsCount; i++)
            {
                // Read number of frames
                int frameCount = reader.ReadByte();
                // Read speed
                int animationSpeed = reader.ReadByte();
                // Read loop start
                int loopFrom = reader.ReadByte();

                Animations.Add(new AnimationEntry(("Sonic-Nexus Animation #" + (i + 1)), frameCount, animationSpeed,
                    loopFrom, 0, reader));
            }

            var collisionBoxesCount = reader.ReadByte();
            Hitboxes = new List<HitboxEntry>(collisionBoxesCount);
            while (collisionBoxesCount-- > 0)
            { Hitboxes.Add(new HitboxEntry(reader)); }
        }
        public void Factory(out IAnimationEntry o) { o = new AnimationEntry(); }
        public void Factory(out IFrame o) { o = new Frame(); }
        public void Factory(out IHitboxEntry o) { o = new HitboxEntry(); }

        public IEnumerable<IAnimationEntry> GetAnimations()
        {
            return Animations.Select(x => (IAnimationEntry)x);
        }

        public void SetAnimations(IEnumerable<IAnimationEntry> animations)
        {
            Animations.Clear();
            Animations.AddRange(animations
                .Select(x => x as AnimationEntry)
                .Where(x => x != null));
        }

        public IEnumerable<IHitboxEntry> GetHitboxes()
        {
            return Hitboxes.Select(x => (IHitboxEntry)x);
        }

        public void SetHitboxes(IEnumerable<IHitboxEntry> hitboxes)
        {
            Hitboxes.Clear();
            Hitboxes.AddRange(hitboxes
                .Select(x => x as HitboxEntry)
                .Where(x => x != null));
        }
        public void SetHitboxTypes(IEnumerable<string> hitboxTypes)
        { }


        public void SaveChanges(BinaryWriter writer)
        {
            writer.Write(UnusedBytes);

            var spriteSheetsCount = (byte)Math.Min(SpriteSheets.Count, byte.MaxValue);

            for (int i = 0; i < spriteSheetsCount; i++)
            {
                var item = SpriteSheets[i];
                writer.Write(StringEncoding.GetBytes(item));
            }

            writer.Write((byte)0); //Write the "EndTexFlag" byte

            var animationsCount = (byte)Math.Min(Animations.Count, byte.MaxValue);
            writer.Write(animationsCount);

            for (int i = 0; i < animationsCount; i++)
            {
                Animations[i].SaveChanges(writer);
            }

            var collisionBoxesCount = (byte)Math.Min(Hitboxes.Count, byte.MaxValue);
            writer.Write(collisionBoxesCount);
            for (int i = 0; i < collisionBoxesCount; i++)
            {
                Hitboxes[i].SaveChanges(writer);
            }
        }
    }
}

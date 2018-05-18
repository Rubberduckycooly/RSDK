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

using RSDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RSDK1
{
    public class Animation : IAnimation
    {
        public int Version => 1;

        public List<string> SpriteSheets { get; }

        public List<AnimationEntry> Animations { get; }

        public List<HitboxEntry> Hitboxes { get; }

        public IEnumerable<string> HitboxTypes => null;

        public Animation(BinaryReader reader)
        {
            // Read number of image paths		

            reader.ReadByte(); //skip this byte, as i'm unsure what its for...
            int spriteSheetsCount = 3; reader.ReadByte();
            var animationsCount = reader.ReadByte();

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
            //while (spriteSheetsCount-- > 0)
            //SpriteSheets.Add(StringEncoding.GetString(reader));

            // Read number of animations		
            Animations = new List<AnimationEntry>(animationsCount);

            for (int i = 0; i < animationsCount; i++)
            {// read frame count	
                int frameCount = reader.ReadByte();
                Console.WriteLine(frameCount);
                		
                int animationSpeed = reader.ReadByte();
                animationSpeed = animationSpeed * 4;
                int loopFrom = reader.ReadByte();
                loopFrom = loopFrom + 1;

                int buf = reader.ReadByte();

                bool flag1 = (buf & 1) > 0;
                bool flag2 = (buf & 2) > 0;

                // Length of animation data - 4 bytes + (8 bytes * number_of_frames)

                // In the 4 bytes:
                // byte 1 - Number of frames
                // byte 2 - Animation speed
                // byte 3 - Frame to start looping from, when looping
                // byte 4 - A flag of some kind
                //		In Sonic 1, Sonic 2 and Sonic CD, it has value 3 for walking & running animations
                //		Coincidentally, for those animations, the frames for the first half of the 
                //		animation have the  normal graphics, while the second half 
                //		has the rotated sprites
                //		(that are displayed when going up a loop or a slope)
                //		In Sonic 2, for Twirl H it has value 2

                Animations.Add(new AnimationEntry(("Retro-Sonic Animation #" + (i+1)), frameCount, animationSpeed,
                    loopFrom, flag1, flag2, reader));


                }
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
            var spriteSheetsCount = (byte)Math.Min(SpriteSheets.Count, byte.MaxValue);
            writer.Write(spriteSheetsCount);
            for (int i = 0; i < spriteSheetsCount; i++)
            {
                var item = SpriteSheets[i];
                writer.Write(StringEncoding.GetBytes(item));
            }

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

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
        public string[] AnimNames = new string[]
        {
            "Stopped",
            "Waiting",
            "Looking Up",
            "Looking Down",
            "Walking",
            "Running",
            "Skidding",
            "Super Peel Out",
            "Spin Dash",
            "Jumping",
            "Bouncing",
            "Hurt",
            "Dying",
            "Life Icon",
            "Drowning",
            "Fan Rotate",
            "Breathing",
            "Pushing",
            "Flailing Left",
            "Flailing Right",
            "Sliding",
            "Hanging",
            "Dropping",
            "Finish Pose",
            "CorkScrew",
            "Retro Sonic Animation #26",
            "Retro Sonic Animation #27",
            "Retro Sonic Animation #28",
            "Retro Sonic Animation #29",
            "Retro Sonic Animation #30",
            "Bonus Spin",
            "Special Stop",
            "Special Walk",
            "Special Jump",
        };

        public int Version => 1;

        public List<string> SpriteSheets { get; }

        public int PlayerType { get; set; }

        public List<AnimationEntry> Animations { get; }

        public List<HitboxEntry> Hitboxes { get; }

        public IEnumerable<string> HitboxTypes { get; }

        public bool dcVer = false;

        public Animation() { SpriteSheets = new List<string>(); Animations = new List<AnimationEntry>(); Hitboxes = new List<HitboxEntry>(); HitboxTypes = new List<string>(); }

        public Animation(BinaryReader reader, bool dcVer)
        {
            this.dcVer = dcVer;
            reader.ReadByte(); //skip this byte, as it seems unused
            PlayerType = reader.ReadByte();
            int spriteSheetsCount = dcVer ? 2 : 3;
            int animationsCount = reader.ReadByte();

            SpriteSheets = new List<string>();
            for (int i = 0; i < spriteSheetsCount; i++)
            {
                string sheet = System.Text.Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadByte()));

                if (!string.IsNullOrWhiteSpace(sheet))
                    SpriteSheets.Add(sheet);
            }

            // Read number of animations		
            Animations = new List<AnimationEntry>(animationsCount);

            for (int i = 0; i < animationsCount; i++)
            {
                // In the 3 bytes:
                // byte 1 - Number of frames
                // byte 2 - Animation speed
                // byte 3 - Frame to start looping from, when looping

                // read frame count	
                int frameCount = reader.ReadByte();
                //read Animation Speed
                int animationSpeed = reader.ReadByte() * 4;
                //read Loop Index
                int loopFrom = reader.ReadByte();

                //The Retro Sonic Animation Files Don't Have Names, so let's give them "ID's" instead
                string name = "Retro Sonic Animation #" + (i + 1);
                try
                {
                    name = AnimNames[i];
                }
                catch
                {
                    name = "Retro Sonic Animation #" + (i + 1);
                }
                Animations.Add(new AnimationEntry(name, frameCount, animationSpeed,
                    loopFrom, false, false, reader));
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
            //return Hitboxes.Select(x => (IHitboxEntry)x);
            return new List<HitboxEntry>();
        }

        public void SetHitboxes(IEnumerable<IHitboxEntry> hitboxes)
        {
            /*Hitboxes.Clear();
            Hitboxes.AddRange(hitboxes
                .Select(x => x as HitboxEntry)
                .Where(x => x != null));*/
        }
        public void SetHitboxTypes(IEnumerable<string> hitboxTypes)
        { }

        public void SaveChanges(BinaryWriter writer)
        {
            writer.Write((byte)0);
            writer.Write((byte)PlayerType);
            var animationsCount = (byte)Math.Min(Animations.Count, byte.MaxValue);
            writer.Write(animationsCount);

            var spriteSheetsCount = (byte)Math.Min(SpriteSheets.Count, dcVer ? 2 : 3);
            int s = 0;
            for (; s < spriteSheetsCount; s++)
            {
                writer.Write(StringEncoding.GetBytes(SpriteSheets[s]));
            }
            for (; s < (dcVer ? 2 : 3); s++)
            {
                writer.Write(StringEncoding.GetBytes(""));
            }

            for (int i = 0; i < animationsCount; i++)
            {
                Animations[i].SaveChanges(writer);
            }
        }
    }
}

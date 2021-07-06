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

namespace RSDK5
{

    //Dummy Class, done for convenience elsewhere
    public class HitboxEntry : IHitboxEntry
    {
        public List<Hitbox> Hitboxes { get; set; }

        public int Count => Hitboxes.Count;

        public HitboxEntry()
        {
            Hitboxes = new List<Hitbox>();
            for (int i = 0; i < 8; i++)
                Hitboxes.Add(new Hitbox());
        }

        public IHitbox GetHitbox(int index)
        {
            return Hitboxes[index];
        }

        public IEnumerable<IHitbox> GetHitboxes()
        {
            return Hitboxes.Select(x => (IHitbox)x);
        }

        public void SetHitboxes(IEnumerable<IHitbox> hitboxes)
        {
            Hitboxes.Clear();
            Hitboxes.AddRange(hitboxes
                .Select(x => x as Hitbox)
                .Where(x => x != null));
        }
    }

    public class Animation : IAnimation
    {
        const int Signature = 0x00525053; // SPR\0
        
        public int Version => 5;

        public int PlayerType { get; set; }

        public int TotalFramesCount;

        public List<string> SpriteSheets { get; }

        public List<string> CollisionBoxes { get; }

        public List<AnimationEntry> Animations { get; }

        public IEnumerable<string> HitboxTypes => CollisionBoxes;
        public List<HitboxEntry> Hitboxes { get; } //Dummy, used for simplicity

        public Animation() { SpriteSheets = new List<string>(); CollisionBoxes = new List<string>(); Animations = new List<AnimationEntry>(); Hitboxes = new List<HitboxEntry>(); }

        public Animation(BinaryReader reader)
        {
            int signature;
            if ((signature = reader.ReadInt32()) != Signature)
                throw new InvalidProgramException($"Signature {signature.ToString("X08")} does not match the expected signature \"SPR\".");

            TotalFramesCount = reader.ReadInt32();

            int spriteSheetsCount = reader.ReadByte();
            SpriteSheets = new List<string>(spriteSheetsCount);
            while (spriteSheetsCount-- > 0)
                SpriteSheets.Add(StringEncoding.GetString(reader));

            int collisionBoxesCount = reader.ReadByte();
            CollisionBoxes = new List<string>(collisionBoxesCount);
            while (collisionBoxesCount-- > 0)
                CollisionBoxes.Add(StringEncoding.GetString(reader));

            int animationsCount = reader.ReadInt16();
            Animations = new List<AnimationEntry>(animationsCount);
            while (animationsCount-- > 0)
                Animations.Add(new AnimationEntry(reader, CollisionBoxes.Count));
        }

        public void Factory(out IAnimationEntry o) { o = new AnimationEntry(CollisionBoxes.Count); }
        public void Factory(out IFrame o) { o = new Frame(CollisionBoxes.Count); }
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

        public IEnumerable<IHitboxEntry> GetHitboxes() { return new List<IHitboxEntry>(); }

        public void SetHitboxes(IEnumerable<IHitboxEntry> hitboxes) { }
        public void SetHitboxTypes(IEnumerable<string> hitboxTypes)
        {
            CollisionBoxes.Clear();
            CollisionBoxes.AddRange(hitboxTypes);
        }

        public void SaveChanges(BinaryWriter writer)
        {
            writer.Write(Signature);

            var animationsCount = (ushort)Math.Min(Animations.Count, ushort.MaxValue);
            TotalFramesCount = Animations.Take(animationsCount).Sum(x => x.Frames.Count);
            writer.Write(TotalFramesCount);

            var spriteSheetsCount = (byte)Math.Min(SpriteSheets.Count, byte.MaxValue);
            writer.Write(spriteSheetsCount);
            for (int i = 0; i < spriteSheetsCount; i++)
                writer.Write(StringEncoding.GetBytes(SpriteSheets[i]));

            var collisionBoxesCount = (byte)Math.Min(CollisionBoxes.Count, byte.MaxValue);
            writer.Write(collisionBoxesCount);
            for (int i = 0; i < collisionBoxesCount; i++)
                writer.Write(StringEncoding.GetBytes(CollisionBoxes[i]));

            writer.Write(animationsCount);
            for (int i = 0; i < animationsCount; i++)
                Animations[i].SaveChanges(writer);
        }
    }
}

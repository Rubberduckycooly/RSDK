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
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace RSDK3
{
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

        public HitboxEntry(BinaryReader reader)
        {
            Hitboxes = new List<Hitbox>();
            for (int i = 0; i < 8; i++)
                Hitboxes.Add(new Hitbox(reader));
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

        public void SaveChanges(BinaryWriter writer)
        {
            int i = 0;
            for (; i < Hitboxes.Count; i++)
                Hitboxes[i].SaveChanges(writer);
            for (; i < 8; i++)
            {
                Hitbox hitbox = new Hitbox();
                hitbox.SaveChanges(writer);
            }
        }
    }
}

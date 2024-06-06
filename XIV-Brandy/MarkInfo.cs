using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Brandy
{
    public class MarkInfo
    {
        public string Name    = "";
        public Vector2 Size   = new Vector2(128, 128);
        public uint IconId    = 60545;
        public uint Tint      = 0xFFFFFFFF;
        public int VertOffset = 0;

        public Vector2 HalfSize => Size / 2;
    }
}

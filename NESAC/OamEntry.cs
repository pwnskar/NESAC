using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NESAC
{
    public class OamEntry
    {
        public byte Y;
        public byte Char;
        public byte Attribute;
        public byte X;

        public int GetPaletteIndex()
        {
            return ((int)this.Attribute) & 3;
        }

        public bool VerticalIsFlipped()
        {
            return (this.Attribute & 128) == 128;
        }

        public bool HorizontalIsFlipped()
        {
            return (this.Attribute & 64) == 64;
        }
    }
}

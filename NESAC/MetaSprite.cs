using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NESAC
{
    public class MetaSpriteList : List<MetaSprite>
    {
        public MetaSpriteList()
        {
            for (int i = 0; i < 256; i++)
            {
                this.Add(new MetaSprite(i, "metasprite_" + i.ToString()));
            }
        }
    }

    public class MetaSprite : List<OamEntry>
    {
        public string Label = "";

        public int Index = 0;

        public MetaSprite(int index, string label)
        {
            this.Index = index;
            this.Label = label;
        }

        public Bitmap GetBitmap(NESPalette nesPalette, PaletteSelection paletteSelection, ChrTable chrTable)
        {
            Bitmap bm = new Bitmap(128, 128);
            Graphics g = Graphics.FromImage(bm);
            g.Clear(nesPalette[paletteSelection[0][0]]);

            for (int i = this.Count - 1; i >= 0; i--)
            {
                OamEntry o = this[i];

                Bitmap b = chrTable.GetBitmap(o.Char, paletteSelection[o.GetPaletteIndex()], nesPalette);
                if (o.VerticalIsFlipped())
                {
                    b.RotateFlip(RotateFlipType.RotateNoneFlipY);
                }
                if (o.HorizontalIsFlipped())
                {
                    b.RotateFlip(RotateFlipType.RotateNoneFlipX);
                }

                g.DrawImage(b, o.X, o.Y);

            }

            return bm;
        }
    }
}

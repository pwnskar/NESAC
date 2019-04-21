using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NESAC
{
    public class NESPalette : List<Color>
    {
        public NESPalette()
        {
            for (int i = 0; i < 62; i++)
            {
                this.Add(Color.FromArgb(255, 0, 0, 0));
            }

            this[0] = Color.FromArgb(255, 102, 103, 101);
            this[1] = Color.FromArgb(255, 0, 31, 157);
            this[2] = Color.FromArgb(255, 33, 13, 173);
            this[3] = Color.FromArgb(255, 69, 4, 156);
            this[4] = Color.FromArgb(255, 107, 3, 110);
            this[5] = Color.FromArgb(255, 114, 3, 30);
            this[6] = Color.FromArgb(255, 101, 17, 0);
            this[7] = Color.FromArgb(255, 69, 31, 0);
            this[8] = Color.FromArgb(255, 35, 46, 0);
            this[9] = Color.FromArgb(255, 0, 57, 0);
            this[10] = Color.FromArgb(255, 0, 61, 0);
            this[11] = Color.FromArgb(255, 0, 56, 33);
            this[12] = Color.FromArgb(255, 0, 50, 102);
            this[13] = Color.FromArgb(255, 0, 0, 0);

            this[14] = Color.FromArgb(255, 0, 0, 0);
            this[15] = Color.FromArgb(255, 0, 0, 0);

            this[16] = Color.FromArgb(255, 177, 177, 175);
            this[17] = Color.FromArgb(255, 8, 85, 234);
            this[18] = Color.FromArgb(255, 71, 61, 255);
            this[19] = Color.FromArgb(255, 119, 48, 254);
            this[20] = Color.FromArgb(255, 173, 44, 206);
            this[21] = Color.FromArgb(255, 189, 42, 100);
            this[22] = Color.FromArgb(255, 181, 58, 0);
            this[23] = Color.FromArgb(255, 143, 76, 0);
            this[24] = Color.FromArgb(255, 99, 96, 0);
            this[25] = Color.FromArgb(255, 27, 112, 0);
            this[26] = Color.FromArgb(255, 0, 119, 0);
            this[27] = Color.FromArgb(255, 0, 116, 60);
            this[28] = Color.FromArgb(255, 0, 109, 153);
            this[29] = Color.FromArgb(255, 0, 0, 0);

            this[30] = Color.FromArgb(255, 0, 0, 0);
            this[31] = Color.FromArgb(255, 0, 0, 0);

            this[32] = Color.FromArgb(255, 255, 255, 255);
            this[33] = Color.FromArgb(255, 77, 173, 255);
            this[34] = Color.FromArgb(255, 135, 149, 255);
            this[35] = Color.FromArgb(255, 185, 134, 255);
            this[36] = Color.FromArgb(255, 241, 128, 255);
            this[37] = Color.FromArgb(255, 255, 122, 211);
            this[38] = Color.FromArgb(255, 255, 135, 95);
            this[39] = Color.FromArgb(255, 239, 152, 18);
            this[40] = Color.FromArgb(255, 201, 171, 0);
            this[41] = Color.FromArgb(255, 127, 190, 0);
            this[42] = Color.FromArgb(255, 71, 200, 32);
            this[43] = Color.FromArgb(255, 44, 200, 112);
            this[44] = Color.FromArgb(255, 47, 196, 204);
            this[45] = Color.FromArgb(255, 81, 81, 79);

            this[46] = Color.FromArgb(255, 0, 0, 0);
            this[47] = Color.FromArgb(255, 0, 0, 0);

            this[48] = Color.FromArgb(255, 255, 255, 255);
            this[49] = Color.FromArgb(255, 186, 229, 255);
            this[50] = Color.FromArgb(255, 209, 219, 255);
            this[51] = Color.FromArgb(255, 230, 213, 255);
            this[52] = Color.FromArgb(255, 253, 210, 255);
            this[53] = Color.FromArgb(255, 255, 207, 245);
            this[54] = Color.FromArgb(255, 255, 213, 197);
            this[55] = Color.FromArgb(255, 255, 219, 163);
            this[56] = Color.FromArgb(255, 239, 227, 145);
            this[57] = Color.FromArgb(255, 208, 236, 143);
            this[58] = Color.FromArgb(255, 185, 240, 166);
            this[59] = Color.FromArgb(255, 174, 240, 199);
            this[60] = Color.FromArgb(255, 175, 238, 238);
            this[61] = Color.FromArgb(255, 187, 188, 186);
        }


        public Bitmap GetColorAsBitmap(int index)
        {
            return this.GetColorAsBitmap(index, 8, 8);
        }

        public Bitmap GetColorAsBitmap(int index, int width, int height)
        {
            Bitmap bm = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bm);
            g.Clear(this[index]);

            return bm;
        }

        public Bitmap GetFullPaletteAsBitmap()
        {
            Bitmap bm = new Bitmap(16*16, 16*4);
            Graphics g = Graphics.FromImage(bm);

            for (int i = 0; i < this.Count; i++)
            {
                Bitmap b = this.GetColorAsBitmap(i, 16, 16);

                // first, get the row
                int row = i / 16;   // 16 entries/row

                int y = row * 16;   // 16 pixel height for each row
                int x = (i - (row * 16)) * 16;

                g.DrawImage(b, x, y);

            }

            return bm;
        }

        public Bitmap GetPaletteSelectionAsBitmap(PaletteSelection paletteSelection, int paletteSelectionIndex)
        {
            Bitmap bm = new Bitmap(16 * 4, 16);
            Graphics g = Graphics.FromImage(bm);

            for (int i = 0; i < paletteSelection[paletteSelectionIndex].Length; i++)
            {
                Bitmap b = this.GetColorAsBitmap(paletteSelection[paletteSelectionIndex][i], 16, 16);

                int x = i * 16;

                g.DrawImage(b, x, 0);
            }

            return bm;
        }

    }

    public class PaletteSelection : List<int[]>
    {
        public PaletteSelection()
        {
            this.Add(new int[] { 13, 0, 16, 48 });
            this.Add(new int[] { 13, 1, 33, 49 });
            this.Add(new int[] { 13, 6, 22, 38 });
            this.Add(new int[] { 13, 9, 25, 41 });
        }
    }
}

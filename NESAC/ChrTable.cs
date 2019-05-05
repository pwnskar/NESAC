using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NESAC
{
    public class ChrTable : List<byte[]>
    {
        public ChrTable()
        {
            for(int i = 0; i < 512; i++)
            {
                //this.Add(new byte[16], new Bitmap(8, 8));
                this.Add(new byte[16]);
            }
        }

        //public ChrTable(byte[] chrBytes, int offset)
        /*public ChrTable(byte[] chrBytes)
        {
            
            for (int i = 0; i < chrBytes.Length;)//; i+=16)
            {
                byte[] tile = new byte[16];

                try
                {

                    for (int y = 0; y < 16; y++)
                    {
                        tile[y] = chrBytes[i];

                        i++;
                    }

                    this.Add(tile);

                }
                catch
                {

                }
            }

        }*/

        internal void LoadA(byte[] chrBytes)
        {
            this.Load(chrBytes, 0);
        }

        internal void LoadB(byte[] chrBytes)
        {
            this.Load(chrBytes, 256);
        }

        private void Load(byte[] chrBytes, int offset)
        {
            offset = offset * 16 - 1;
            // TODO: check if offset + chrBytes.Length exceeds the size of our tables.
            if (offset + chrBytes.Length > 8192)
            {
                // we've eceeded the size of our tables.
            }

            int maxLength = chrBytes.Length;
            if (maxLength > 4096)
                maxLength = 4096;

            for (int i = 0; i < maxLength;)
            {
                byte[] tile = new byte[16];

                try
                {

                    for (int y = 0; y < 16; y++)
                    {
                        tile[y] = chrBytes[i];

                        i++;
                    }

                    this[(i + offset)/16] = tile;

                }
                catch
                {
                    return;
                }
            }

        }

        public Bitmap GetBitmap(int chrIndex, int[] currPalette, NESPalette nesPalette, bool chr8x16)
        {
            int sizeX = 8;
            int sizeY = 8;

            if (chr8x16)
            {
                // TODO: find out if chrIndex is even or odd. That will determine which pattern table will be used.
                int temp_int = chrIndex / 2 * 2;
                if(chrIndex > temp_int)
                {
                    // odd number. switch pattern table
                    // how?
                    chrIndex += 256;
                }
                chrIndex = chrIndex / 2 * 2;
                sizeY = 16;
            }

            Bitmap tileBitmap = new Bitmap(sizeX, sizeY);

            for (int y = 0; y < 8; y++)  // loop through 8 rows
            {
                // read bits of tiles[i][y] and tiles[i][y+8]

                for (int x = 0; x < 8; x++)
                {
                    // read bit x of tiles[i][y] and tiles[i][y+8]
                    //System.Collections.BitArray bitsA = new System.Collections.BitArray()

                    System.Collections.BitArray bitsA = new System.Collections.BitArray(this[chrIndex][y]);
                    System.Collections.BitArray bitsB = new System.Collections.BitArray(this[chrIndex][y + 8]);

                    int endResult = 0;



                    var bit0 = (this[chrIndex][y] & (1 << ((x * -1) + 8) - 1)) != 0;
                    var bit1 = (this[chrIndex][y + 8] & (1 << ((x * -1) + 8) - 1)) != 0;


                    if (bit0 == true)
                    {
                        endResult += 1;
                    }
                    
                    if (bit1 == true)
                    {
                        endResult += 2;
                    }

                    Color c = nesPalette[currPalette[endResult]];
                    if (endResult == 0)
                    {
                        c = Color.FromArgb(0, c);
                    }


                    tileBitmap.SetPixel(x, y, c);

                }
            }

            if (chr8x16)
            {
                chrIndex++;

                for (int y = 0; y < 8; y++)  // loop through 8 rows
                {
                    // read bits of tiles[i][y] and tiles[i][y+8]

                    for (int x = 0; x < 8; x++)
                    {
                        // read bit x of tiles[i][y] and tiles[i][y+8]
                        //System.Collections.BitArray bitsA = new System.Collections.BitArray()

                        System.Collections.BitArray bitsA = new System.Collections.BitArray(this[chrIndex][y]);
                        System.Collections.BitArray bitsB = new System.Collections.BitArray(this[chrIndex][y + 8]);

                        int endResult = 0;



                        var bit0 = (this[chrIndex][y] & (1 << ((x * -1) + 8) - 1)) != 0;
                        var bit1 = (this[chrIndex][y + 8] & (1 << ((x * -1) + 8) - 1)) != 0;


                        if (bit0 == true)
                        {
                            endResult += 1;
                        }

                        if (bit1 == true)
                        {
                            endResult += 2;
                        }

                        Color c = nesPalette[currPalette[endResult]];
                        if (endResult == 0)
                        {
                            c = Color.FromArgb(0, c);
                        }


                        tileBitmap.SetPixel(x, y+8, c);

                    }
                }

            }

            return tileBitmap;
        }
    }
}

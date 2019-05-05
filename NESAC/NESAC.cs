using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NESAC
{
    public partial class NESAC : Form
    {
        private NESACSession Session = new NESACSession();

        private bool chr8x16Mode = false;

        public bool animNameTextKeyIsDown = false;

        private int chrBitmapAX = 256;
        private int chrBitmapAY = 256;

        private int metaSpriteBitmapX = 256;
        private int metaSpriteBitmapY = 256;

        public ChrTable chrTables = new ChrTable();
        public NESPalette nesPalette = new NESPalette();
        public PaletteSelection paletteSelection = new PaletteSelection();
        private int chrTablePaletteSelection = 0;
        //private int paletteSelectionIndex = 0;

        public MetaSpriteList metaSprites = new MetaSpriteList();
        public int metaSprite_offset_X = 0;
        public int metaSprite_offset_Y = 0;

        private int selMetaSpriteIndex = 0;
        public int SelMetaSpriteIndex
        {
            get
            {
                return this.selMetaSpriteIndex;
            }
            set
            {
                if (value < 0)
                    value += 256;
                if (value > 255)
                    value -= 256;

                this.selMetaSpriteIndex = value;
            }

        }

        public List<AnimationSequence> animations = new List<AnimationSequence>();
        private int selAnimation;
        public int SelAnimation
        {
            get
            {
                if (selAnimation > animations.Count - 1)
                {
                    selAnimation = animations.Count - 1;
                }
                if (selAnimation < 0)
                {
                    selAnimation = 0;
                }

                return selAnimation;
            }
            set
            {
                selAnimation = value;
                selAnimation = SelAnimation;
            }
        }

        private int selAnimationCel = 0;
        public int SelAnimationCel
        {
            get
            {
                if (selAnimationCel > animations[SelAnimation].Count - 1)
                {
                    selAnimationCel = animations[SelAnimation].Count - 1;
                }
                if (selAnimationCel < 0)
                {
                    selAnimationCel = 0;
                }

                return selAnimationCel;
            }
            set
            {
                selAnimationCel = value;
                selAnimationCel = SelAnimationCel;
            }
        }

        private void renderPaletteSelection()
        {
            pctPalette0Box.Image = nesPalette.GetPaletteSelectionAsBitmap(paletteSelection, 0);
            pctPalette1Box.Image = nesPalette.GetPaletteSelectionAsBitmap(paletteSelection, 1);
            pctPalette2Box.Image = nesPalette.GetPaletteSelectionAsBitmap(paletteSelection, 2);
            pctPalette3Box.Image = nesPalette.GetPaletteSelectionAsBitmap(paletteSelection, 3);
        }

        public NESAC()
        {

            string path = Application.StartupPath;

            InitializeComponent();

            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;

            pctFullPaletteBox.Image = nesPalette.GetFullPaletteAsBitmap();

            renderPaletteSelection();

            updateRenders();
        }

        private void NESAC_Load(object sender, EventArgs e)
        {

        }

        #region GUI_events

        private void pctChrTableBox_Click(object sender, EventArgs e)
        {

        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openAnimFileDialog.ShowDialog();
        }

        private void openChrFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            openChrFile(openChrFileDialog.FileName);
        }

        private void btnLoadChr_Click(object sender, EventArgs e)
        {
            openChrFileDialog.ShowDialog();
        }

        private void btnLoadPal_Click(object sender, EventArgs e)
        {
            openPalFileDialog.ShowDialog();
        }

        private void btnLoadMsb_Click(object sender, EventArgs e)
        {
            openMsbFileDialog.ShowDialog();
        }

        private void openPalFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            openPalFile(openPalFileDialog.FileName);
        }

        private void openMsbFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            string fileName = openMsbFileDialog.FileName;
            openMsbFile(fileName);

            // TODO: attempt to load label files
            string labelFileName = Path.GetFileNameWithoutExtension(fileName);
            labelFileName += ".msl";

            openMsbLabels(labelFileName);
        }

        private void btnPrevCel_Click(object sender, EventArgs e)
        {
            SelMetaSpriteIndex--;
            renderSelectedMetasprite();
        }

        private void btnNextCel_Click(object sender, EventArgs e)
        {
            SelMetaSpriteIndex++;
            renderSelectedMetasprite();
        }


        #endregion



        private void updateRenders()
        {
            renderChrTable();
            renderSelectedMetasprite();
            renderSelectedAnimation();
        }


        private void openChrFile(string fileName)
        {
            if (chkChrTableA.Checked)
                openChrFileA(fileName);
            else
                openChrFileB(fileName);

            return;
        }

        private void openChrFileA(string fileName)
        {
            if (!File.Exists(fileName))
                return;

            byte[] chrBytes = File.ReadAllBytes(fileName);

            chrTables.LoadA(chrBytes);

            this.Session.ChrTableFilenameA = fileName;

            updateRenders();
        }

        private void openChrFileB(string fileName)
        {
            if (!File.Exists(fileName))
                return;

            byte[] chrBytes = File.ReadAllBytes(fileName);

            chrTables.LoadB(chrBytes);

            this.Session.ChrTableFilenameB = fileName;

            updateRenders();
        }



        private void openPalFile(string fileName)
        {
            if (!File.Exists(fileName))
                return;

            byte[] palBytes = File.ReadAllBytes(fileName);

            if (palBytes.Length < 16)
            {
                return;
            }

            for (int i = 0; i < 16;)
            {
                for (int y = 0; y < 4; y++)
                {
                    for (int x = 0; x < 4; x++)
                    {
                        paletteSelection[y][x] = palBytes[i];

                        i++;
                    }
                }
            }

            renderPaletteSelection();
            updateRenders();

            this.Session.PalFilename = fileName;
        }



        private void openMsbFile(string fileName)
        {
            if (!File.Exists(fileName))
                return;

            /*metaSprites = new List<MetaSprite>();
            for(int i = 0; i < 256; i++)
            {
                metaSprites.Add(new MetaSprite(i));
            }*/

            string label_default = Path.GetFileNameWithoutExtension(fileName) + "_";

            byte[] metaspriteBytes = File.ReadAllBytes(fileName);

            metaSprite_offset_X = (int)metaspriteBytes[0];
            metaSprite_offset_Y = (int)metaspriteBytes[1];

            for (int i = 0; i < 256; i++)
            {
                int file_index = (i * 256) + 2;

                //MetaSprite metaSprite = new MetaSprite(i, "metasprite_" + i.ToString());
                MetaSprite metaSprite = new MetaSprite(i, label_default + i.ToString());

                if ((int)metaspriteBytes[file_index] != 255)
                {
                    for (int y = 0; y < 62; y++)
                    {
                        byte tile_y = metaspriteBytes[file_index];
                        file_index++;

                        if ((int)tile_y == 255)
                        {
                            break;
                        }

                        OamEntry oamEntry = new OamEntry();
                        oamEntry.Y = tile_y;

                        oamEntry.Char = metaspriteBytes[file_index];
                        file_index++;
                        oamEntry.Attribute = metaspriteBytes[file_index];
                        file_index++;
                        oamEntry.X = metaspriteBytes[file_index];
                        file_index++;

                        metaSprite.Add(oamEntry);
                    }

                    
                }

                metaSprites[i] = metaSprite;

            }

            //renderMetasprite(metaSprites[0]);
            updateRenders();

            this.Session.MsbFilename = fileName;
        }




        private void renderChrTable()
        {
            renderChrTable(chrTablePaletteSelection);
        }

        private void renderChrTable(int paletteSelectionIndex)
        {
            if (chrTables.Count < 256)
                return;

            Bitmap chrBitmapA = new Bitmap(128, 128);
            Graphics g = Graphics.FromImage(chrBitmapA);
            g.Clear(nesPalette[paletteSelection[0][0]]);

            int x_pos = 0;

            int chrIndex = 0;
            if (!chkChrTableA.Checked)
                chrIndex = 256;

            for (int i = 0; i < 256; i++)
            {

                int y_pos = i / 16 * 8;
                
                if (x_pos > 8 * 15)
                {
                    x_pos = 0;
                }


                Bitmap b = chrTables.GetBitmap(chrIndex, paletteSelection[paletteSelectionIndex], nesPalette, false);
                chrIndex++;

                g.DrawImage(b, x_pos, y_pos);


                x_pos += 8;

            }


            chrBitmapA = ResizeBitmap(chrBitmapA, chrBitmapAX, chrBitmapAY);
            g.DrawImage(chrBitmapA, 0, 0);

            pctChrTableBox.Image = chrBitmapA;

        }



        private void renderSelectedMetasprite()
        {
            if (SelMetaSpriteIndex < 0 || SelMetaSpriteIndex > metaSprites.Count)
            {
                SelMetaSpriteIndex = 0;
            }

            if (metaSprites.Count > 0)
            {
                renderMetasprite();
            }

            lblMetaSpriteIndex.Text = "Metasprite " + SelMetaSpriteIndex.ToString();
            txtMetaspriteNameBox.Text = metaSprites[SelMetaSpriteIndex].Label;
        }

        private void renderMetasprite()
        {
            MetaSprite metaSprite = metaSprites[SelMetaSpriteIndex];

            Bitmap metaSpriteBitmap = new Bitmap(128, 128);
            Graphics g = Graphics.FromImage(metaSpriteBitmap);
            g.Clear(nesPalette[paletteSelection[0][0]]);

            for (int i = metaSprite.Count - 1; i >= 0; i--)
            {
                OamEntry o = metaSprite[i];

                Bitmap b = chrTables.GetBitmap(o.Char, paletteSelection[o.GetPaletteIndex()], nesPalette, chr8x16Mode);
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


            metaSpriteBitmap = ResizeBitmap(metaSpriteBitmap, metaSpriteBitmapX, metaSpriteBitmapY);
            g.DrawImage(metaSpriteBitmap, 0, 0);

            pctMetaspriteBox.Image = metaSpriteBitmap;
        }

        private void updateAnimationListBox()
        {
            lstAnimations.BeginUpdate();

            lstAnimations.Items.Clear();

            if (animations.Count > 0)
            {
                for (int i = 0; i < animations.Count; i++)
                {
                    lstAnimations.Items.Add(animations[i].Label);
                }

                lstAnimations.SelectedIndex = SelAnimation;

                exportAnimationsAsasmToolStripMenuItem.Enabled = true;
            }
            else
            {
                exportAnimationsAsasmToolStripMenuItem.Enabled = false;
            }

            lstAnimations.EndUpdate();
        }

        private void btnPrev10Cel_Click(object sender, EventArgs e)
        {
            SelMetaSpriteIndex -= 10;
            renderSelectedMetasprite();
        }

        private void btnNext10Cel_Click(object sender, EventArgs e)
        {
            SelMetaSpriteIndex += 10;
            renderSelectedMetasprite();
        }

        private void btnInsertMetasprite_Click(object sender, EventArgs e)
        {
            int timeDelay = (int)nudCelDefaultTimeDelay.Value;
            animations[SelAnimation].Add(new AnimationCel(SelMetaSpriteIndex, timeDelay));
            SelAnimationCel = animations[SelAnimation].Count - 1;
            updateCelListBox();

            // TODO> select newest frame.

            //lstAnimationCels.SelectedIndex = animations[SelAnimation]

            renderSelectedAnimation();

            updateAnimationControls();
        }

        private void btnAddAnimation_Click(object sender, EventArgs e)
        {
            string name = "animation_" + animations.Count.ToString();
            AnimationSequence a = new AnimationSequence(name);
            animations.Add(a);
            txtAnimationNameBox.Text = name;

            SelAnimation = animations.Count - 1;
            updateAnimationListBox();

            updateAnimationControls();
        }

        private void btnDeleteAnimation_Click(object sender, EventArgs e)
        {
            if (animations.Count > 0)
            {
                SelAnimation = lstAnimations.SelectedIndex;     // redundant?
                animations.RemoveAt(SelAnimation);

                if (animations.Count > 0)
                {
                    lstAnimations.SelectedIndex = SelAnimation;

                    /*if (animations[SelAnimation].Count < (selAnimationCel + 1))
                    {
                        selAnimationCel = animations[SelAnimation].Count - 1;
                        if (selAnimationCel < 0)
                        {
                            selAnimationCel = 0;
                        }
                    }*/
                }
            }

            updateAnimationListBox();
            updateCelListBox();
            updateAnimationControls();
            renderSelectedAnimation();
        }

        private void lstAnimations_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelAnimation = lstAnimations.SelectedIndex;

            renderSelectedAnimation();

            updateCelListBox();

            updateAnimationControls();

            if (animations.Count > 0)
            {
                SelAnimationCel = 0;
                txtAnimationNameBox.Text = animations[SelAnimation].Label;
            }
        }

        private void lstAnimationCels_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelAnimationCel = lstAnimationCels.SelectedIndex;
            renderSelectedAnimation();
            
            if (animations[SelAnimation].Count > 0)
            {
                nudCelTimeDelay.Value = animations[SelAnimation][SelAnimationCel].TimeDelay;

                int loop_max = animations[SelAnimation].Count - 1;
                if (loop_max < 0)
                    loop_max = 0;

                int loop_to_frame = animations[SelAnimation].LoopToFrame;

                nudLoopToFrame.Maximum = loop_max;

                nudLoopToFrame.Value = loop_to_frame;
            }
        }

        private void btnDelCel_Click(object sender, EventArgs e)
        {
            SelAnimationCel = lstAnimationCels.SelectedIndex;     // redundant?
            animations[SelAnimation].RemoveAt(SelAnimationCel);
            updateCelListBox();

            if (animations[SelAnimation].Count > 0)
            {
                lstAnimationCels.SelectedIndex = SelAnimationCel;
            }

            renderSelectedAnimation();

            updateAnimationControls();
        }

        private void btnAnimationPlay_Click(object sender, EventArgs e)
        {
            //tmrAnimationTimer.Enabled = !tmrAnimationTimer.Enabled;

            if (tmrAnimationTimer.Enabled)
            {
                tmrAnimationTimer.Enabled = false;

                btnAnimationPlay.Text = ">";
            }
            else
            {
                tmrAnimationTimer.Enabled = true;

                btnAnimationPlay.Text = "| |";
            }
        }

        private void btnAnimationPause_Click(object sender, EventArgs e)
        {

            tmrAnimationTimer.Enabled = !tmrAnimationTimer.Enabled;
        }

        private void tmrAnimationTimer_Tick(object sender, EventArgs e)
        {
            if (animations.Count < 1)
            {
                tmrAnimationTimer.Enabled = false;
                return;
            }

            AnimationSequence a = animations[SelAnimation];

            if (a.Count > 0)
            {
                AnimationCel ac = a[SelAnimationCel];

                if (ac.CountDown == 0)
                {
                    if (animations[SelAnimation].Count - 1 == SelAnimationCel)
                    {
                        SelAnimationCel = animations[SelAnimation].LoopToFrame;
                    }
                    else
                    {
                        SelAnimationCel++;
                    }

                    ac.CountDown = ac.TimeDelay;
                }
                else
                {
                    ac.CountDown--;
                }




                renderSelectedAnimation();
                //updateCelListBox();

                lstAnimationCels.SelectedIndex = SelAnimationCel;
            }
        }

        private void btnAnimationsSave_Click(object sender, EventArgs e)
        {
            saveAnimFileDialog.ShowDialog();
        }

        private void saveAnimFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            saveAnimFile(saveAnimFileDialog.FileName);
        }

        private void btnAnimationsLoad_Click(object sender, EventArgs e)
        {
            openAnimFileDialog.ShowDialog();
        }

        private void openAnimFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            openAnimFile(openAnimFileDialog.FileName);
        }

        private void txtAnimationNameBox_TextChanged(object sender, EventArgs e)
        {
            if (animNameTextKeyIsDown)
            {
                animNameTextKeyIsDown = false;

                if (animations.Count > 0)
                {

                    if (txtAnimationNameBox.Text.Length == 0)
                    {
                        txtAnimationNameBox.Text = animations[SelAnimation].Label;
                    }

                    //animations[SelAnimation].Name = txtAnimationNameBox.Text;


                    if (animations[SelAnimation].Label != txtAnimationNameBox.Text)
                    {
                        animations[SelAnimation].Label = txtAnimationNameBox.Text;
                        updateAnimationListBox();
                    }


                }
            }
        }

        private void txtAnimationNameBox_KeyDown(object sender, KeyEventArgs e)
        {
            animNameTextKeyIsDown = true;

            /*if (animations.Count > 0)
            {
                //if (animations[SelAnimation].Name != txtAnimationNameBox.Text)
                //{
                    animations[SelAnimation].Name = txtAnimationNameBox.Text;
                    updateAnimationListBox();
                //}
            }*/
        }

        private void nudCelTimeDelay_ValueChanged(object sender, EventArgs e)
        {
            if (animations.Count > 0)
            {
                if (animations[SelAnimation].Count > 0)
                {
                    int timeDelay = (int)nudCelTimeDelay.Value;

                    animations[SelAnimation][SelAnimationCel].TimeDelay = timeDelay;
                    updateCelListBox();
                }
            }
        }

        private void nudLoopToFrame_ValueChanged(object sender, EventArgs e)
        {
            if (animations.Count > 0)
            {
                if (animations[SelAnimation].Count > 0)
                {
                    int loopToFrame = (int)nudLoopToFrame.Value;

                    if (loopToFrame > animations[SelAnimation].Count - 1)
                    {
                        loopToFrame = animations[SelAnimation].Count - 1;
                    }
                    if (loopToFrame < 0)
                    {
                        loopToFrame = 0;
                    }

                    animations[SelAnimation].LoopToFrame = loopToFrame;
                    updateCelListBox();
                }
            }
        }

        private void nudCelDefaultTimeDelay_ValueChanged(object sender, EventArgs e)
        {

        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveAnimFileDialog.ShowDialog();
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void NESAC_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Space)
            {
                btnAnimationPlay.PerformClick();
            }

            if (e.Control)
            {
                if (e.KeyCode == Keys.OemOpenBrackets)
                {
                    btnPrev10Cel.PerformClick();
                }
                if (e.KeyCode == Keys.OemCloseBrackets)
                {
                    btnNext10Cel.PerformClick();
                }
            }
            else
            {
                if (e.KeyCode == Keys.OemOpenBrackets)
                {
                    btnPrevCel.PerformClick();
                }
                if (e.KeyCode == Keys.OemCloseBrackets)
                {
                    btnNextCel.PerformClick();
                }
            }


            //if (lstAnimationCels.Focused)
            //{
                if (e.Control)
                {
                    if (e.KeyCode == Keys.Left)
                    {
                        lstAnimations.Focus();

                        e.Handled = true;
                    }
                    else if (e.KeyCode == Keys.Right)
                    {
                    lstAnimationCels.Focus();

                        e.Handled = true;
                    }
                }
            //}

        }

        private void pctMetaspriteBox_Click(object sender, EventArgs e)
        {

        }








        private void updateCelListBox()
        {
            lstAnimationCels.BeginUpdate();

            lstAnimationCels.Items.Clear();

            if (animations.Count > 0)
            {
                AnimationSequence animation = animations[SelAnimation];
                for (int i = 0; i < animation.Count; i++)
                {
                    //lstAnimationCels.Items.Add(metaSprites[animation[i].MetaSpriteIndex].Label + ", delay: " + animation[i].TimeDelay.ToString());

                    lstAnimationCels.Items.Add("T: " + animation[i].TimeDelay.ToString() + ",  F: " + metaSprites[animation[i].MetaSpriteIndex].Label);
                }

                if (animations[SelAnimation].Count > 0)
                {
                    lstAnimationCels.SelectedIndex = SelAnimationCel;
                }

                int loop_to_frame = animations[SelAnimation].Count - 1;
                if (loop_to_frame < 0)
                    loop_to_frame = 0;

                // DANGEROUS
                nudLoopToFrame.Maximum = loop_to_frame;
            }

            lstAnimationCels.EndUpdate();
        }

        private void renderSelectedAnimation()
        {
            Bitmap celBitmap = new Bitmap(128, 128);
            Graphics g = Graphics.FromImage(celBitmap);

            if (animations.Count > 0)
            {
                AnimationSequence animation = animations[SelAnimation];

                if (animation.Count > 0)
                {
                    AnimationCel ac = animation[SelAnimationCel];
                    celBitmap = metaSprites[ac.MetaSpriteIndex].GetBitmap(nesPalette, paletteSelection, chrTables, chr8x16Mode);
                }
                else
                {
                    g.Clear(SystemColors.ControlDarkDark);
                }
            }
            else
            {
                g.Clear(SystemColors.ControlDarkDark);
            }

            celBitmap = ResizeBitmap(celBitmap, 256, 256);
            g.DrawImage(celBitmap, 0, 0);

            pctAnimationBox.Image = celBitmap;
        }

        public Bitmap ResizeBitmap(Bitmap bmp, int width, int height)
        {
            Bitmap result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.DrawImage(bmp, 0, 0, width, height);
            }

            return result;
        }



        private void updateAnimationControls()
        {
            if (animations.Count > 0)
            {
                lstAnimations.Enabled = true;
                btnInsertMetasprite.Enabled = true;

                if (animations[SelAnimation].Count > 0)
                {
                    btnDelCel.Enabled = true;
                    btnEditMetasprite.Enabled = true;
                    lstAnimationCels.Enabled = true;
                    btnAnimationPlay.Enabled = true;
                }
                else
                {
                    btnDelCel.Enabled = false;
                    btnEditMetasprite.Enabled = false;
                    lstAnimationCels.Enabled = false;
                    btnAnimationPlay.Enabled = false;
                }
            }
            else
            {
                lstAnimations.Enabled = false;
                btnInsertMetasprite.Enabled = false;
                btnEditMetasprite.Enabled = false;
                btnDelCel.Enabled = false;
                lstAnimationCels.Enabled = false;
                btnAnimationPlay.Enabled = false;
            }
        }

        private void saveAnimFile(string fileName)
        {

            List<byte> byteList = new List<byte>();

            for (int i = 0; i < metaSprites.Count; i++)
            {
                int maxLength = 64;

                int length = metaSprites[i].Label.Length;
                if (length > maxLength)
                    length = maxLength;

                byte[] labelBytes = Encoding.ASCII.GetBytes(metaSprites[i].Label);
                byte[] labelBytesFull = new byte[maxLength];

                for (int y = 0; y < maxLength; y++)
                {
                    if (y < length)
                    {
                        labelBytesFull[y] = labelBytes[y];
                        byteList.Add(labelBytes[y]);
                    }
                    else
                    {
                        labelBytesFull[y] = (byte)0;
                        byteList.Add((byte)0);
                    }
                }
            }






            //List<byte> byteList = new List<byte>();
            //byteList.Add((byte)animations.Count);

            //int file_offset = 0;
            for (int i = 0; i < animations.Count; i++)
            {
                int offset_counter = 0;

                // append animation sequence data at a 256 byte offset
                AnimationSequence ac = animations[i];

                byte[] nameBytes = Encoding.ASCII.GetBytes(ac.Label);

                foreach (byte b in nameBytes)
                {
                    byteList.Add(b);
                }

                for (int x = 0; x < 32 - nameBytes.Length; x++)
                {
                    byteList.Add((byte)0);
                }

                offset_counter += 32;


                byteList.Add((byte)ac.LoopToFrame);
                offset_counter++;

                for (int y = 0; y < ac.Count; y++)
                {
                    byteList.Add((byte)ac[y].TimeDelay);
                    byteList.Add((byte)ac[y].MetaSpriteIndex);

                    offset_counter += 2;
                }

                while (offset_counter < 256)
                {
                    byteList.Add((byte)0);

                    offset_counter++;
                }

                //file_offset += 256;
            }

            byte[] bytes = new byte[byteList.Count];
            for (int i = 0; i < byteList.Count; i++)
            {
                bytes[i] = byteList[i];
            }

            //byte[] palBytes = File.ReadAllBytes(fileName);
            File.WriteAllBytes(fileName, bytes);

        }

        private void openAnimFile(string fileName)
        {
            if (!File.Exists(fileName))
                return;

            byte[] fileBytes = File.ReadAllBytes(fileName);

            int animationsStartIndex = 64 * 256;
            //int animationsStartIndex = 0;

            int animation_count = (fileBytes.Length - animationsStartIndex) / 256;

            animations.Clear();

            if (fileBytes.Length > 0)
            {
                for (int i = 0; i < animation_count; i++)
                {
                    int file_offset = (i * 256) + animationsStartIndex;

                    string name = "";


                    for (int y = 0; y < 32; y++)
                    {
                        byte b = fileBytes[file_offset + y];
                        if ((int)b == 0)
                            break;

                        name += System.Text.Encoding.ASCII.GetString(new[] { b });
                    }

                    file_offset += 32;

                    int loopToFrame = (int)fileBytes[file_offset];

                    file_offset++;

                    AnimationSequence a = new AnimationSequence(name);
                    a.LoopToFrame = loopToFrame;

                    while (file_offset < animationsStartIndex + (256 * (i + 1)))
                    {
                        int timeDelay = (int)fileBytes[file_offset];
                        if (timeDelay == 0)
                        {
                            break;
                        }
                        file_offset++;
                        int metaSpriteIndex = (int)fileBytes[file_offset];
                        file_offset++;

                        a.Add(new AnimationCel(metaSpriteIndex, timeDelay));

                    }

                    animations.Add(a);
                }
            }


            updateAnimationListBox();
            updateCelListBox();
            updateAnimationControls();

            updateRenders();
            //renderSelectedAnimation();

            this.Session.AnimFilename = fileName;
        }

        private void openCHRToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openChrFileDialog.ShowDialog();
        }

        private void openPaletteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openPalFileDialog.ShowDialog();
        }

        private void openMSBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openMsbFileDialog.ShowDialog();
        }

        private void txtMetaspriteNameBox_TextChanged(object sender, EventArgs e)
        {
            if (txtMetaspriteNameBox.Text.Length == 0)
            {
                txtMetaspriteNameBox.Text = metaSprites[SelMetaSpriteIndex].Label;
            }

            metaSprites[SelMetaSpriteIndex].Label = txtMetaspriteNameBox.Text;
        }

        private void openMsbLabelsDialog_FileOk(object sender, CancelEventArgs e)
        {
            openMsbLabels(openMsbLabelsDialog.FileName);
        }

        private void openMetaspriteLabelsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openMsbLabelsDialog.ShowDialog();
        }

        private void openMsbLabels(string fileName)
        {
            if (!File.Exists(fileName))
                return;

            byte[] fileBytes = File.ReadAllBytes(fileName);

            if (fileBytes.Length > 0)
            {
                int file_offset = 0;

                for (int i = 0; i < metaSprites.Count; i++)
                {





                    string name = "";


                    while (file_offset < fileBytes.Length)
                    {
                        byte b = fileBytes[file_offset];

                        if ((int)b == 13)
                        {


                            metaSprites[i].Label = name;

                            file_offset++;


                            if ((int)fileBytes[file_offset] == 10)
                            {
                                file_offset++;  // in case the CR is followed by a space
                            }

                            break;      // time for the next metasprite
                        }


                        name += System.Text.Encoding.ASCII.GetString(new[] { b });

                        file_offset++;
                    }

                    if (file_offset == fileBytes.Length)
                    {
                        // this only happens at EOF.
                        metaSprites[i].Label = name;

                        break;
                    }


                }

                renderSelectedMetasprite();
                updateCelListBox();

                this.Session.MsbLabelsFilename = fileName;
            }
        }

        private void saveMetaspriteLabelsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveMsbLabelsDialog.ShowDialog();
        }

        private void saveMsbLabelsDialog_FileOk(object sender, CancelEventArgs e)
        {
            saveMsbLabels(saveMsbLabelsDialog.FileName);
        }

        private void saveMsbLabels(string fileName)
        {
            List<byte> byteList = new List<byte>();

            for (int i = 0; i < metaSprites.Count; i++)
            {

                MetaSprite ms = metaSprites[i];

                byte[] nameBytes = Encoding.ASCII.GetBytes(ms.Label);

                foreach (byte b in nameBytes)
                {
                    byteList.Add(b);
                }

                byteList.Add((byte)13);
                byteList.Add((byte)10);
            }

            byte[] bytes = new byte[byteList.Count];
            for (int i = 0; i < byteList.Count; i++)
            {
                bytes[i] = byteList[i];
            }

            //byte[] palBytes = File.ReadAllBytes(fileName);
            File.WriteAllBytes(fileName, bytes);
        }

        private void openSessionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openSessionDialog.ShowDialog();
        }

        private void saveSessionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveSessionDialog.ShowDialog();
        }

        private void saveSessionDialog_FileOk(object sender, CancelEventArgs e)
        {
            string fileName = saveSessionDialog.FileName;

            string outputSb = "";

            outputSb += Session.ChrTableFilenameA + "\n";
            outputSb += Session.PalFilename + "\n";
            outputSb += Session.MsbFilename + "\n";
            outputSb += Session.MsbLabelsFilename + "\n";
            outputSb += Session.AnimFilename + "\n";
            outputSb += Session.ChrTableFilenameB + "\n";

            byte[] bytes = Encoding.ASCII.GetBytes(outputSb);

            File.WriteAllBytes(fileName, bytes);
        }

        private void openSessionDialog_FileOk(object sender, CancelEventArgs e)
        {
            string fileName = openSessionDialog.FileName;
            openSession(fileName);
        }

        private void openSession(string fileName)
        {
            if (!File.Exists(fileName))
                return;

            byte[] fileBytes = File.ReadAllBytes(fileName);

            if (fileBytes.Length > 0)
            {
                string converted = Encoding.UTF8.GetString(fileBytes, 0, fileBytes.Length);


                string[] result = converted.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                Session.ChrTableFilenameA = result[0];
                Session.PalFilename = result[1];
                Session.MsbFilename = result[2];
                Session.MsbLabelsFilename = result[3];
                Session.AnimFilename = result[4];
                Session.ChrTableFilenameB = result[5];

                openChrFileA(Session.ChrTableFilenameA);
                openChrFileB(Session.ChrTableFilenameB);
                openPalFile(Session.PalFilename);
                openMsbFile(Session.MsbFilename);
                openMsbLabels(Session.MsbLabelsFilename);
                openAnimFile(Session.AnimFilename);

            }
        }

        private void exportAnimationsAsasmToolStripMenuItem_Click(object sender, EventArgs e)
        {
            exportAnimationToAsmDialog.ShowDialog();
        }

        private void exportAnimationsAsJSONToolStripMenuItem_Click(object sender, EventArgs e)
        {
            exportAnimationToJsonDialog.ShowDialog();
        }

        private void exportAnimationsAsCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            exportAnimationToCDialog.ShowDialog();
        }

        private void exportAnimationToAsmDialog_FileOk(object sender, CancelEventArgs e)
        {

            string fileName = exportAnimationToAsmDialog.FileName;

            string animation_label = Path.GetFileNameWithoutExtension(fileName);

            string output_header = animation_label + ":\n";
            output_header += "\t@last_animation_index:\t.db " + (animations.Count - 1).ToString() + "\n\n";

            string output = "\n";

            if (animations.Count > 0)
            {

                for (int i = 0; i < animations.Count; i++)
                {
                    AnimationSequence a = animations[i];

                    int length = a.Count * 3;
                    int loopToFrame = a.LoopToFrame * 3;

                    output_header += "\t.dw " + a.Label + "\n";

                    output += "\t" + a.Label + ":\n";

                    if (i == 0 && a.Count > 1)
                    {
                        output += "\t\t.db " + length.ToString() + "\t; index of last frame starting from this byte.\n";
                        output += "\t\t.db " + loopToFrame.ToString() + "\t; index of frame to loop to * 3. If " + metaSprites[a[0].MetaSpriteIndex].Label + " is 0 and " + metaSprites[a[1].MetaSpriteIndex].Label + " is 1, then those would be 0 and 3." + "\n\n";
                    }
                    else
                    {
                        output += "\t\t.db " + length.ToString() + "\n";
                        output += "\t\t.db " + loopToFrame.ToString() + "\n\n";
                    }

                    for (int y = 0; y < a.Count; y++)
                    {
                        int timeDelay = a[y].TimeDelay;
                        string label = metaSprites[a[y].MetaSpriteIndex].Label;

                        if(i == 0)
                        {
                            output += "\t\t.db " + timeDelay.ToString() + "\t; time delay for this frame.\n";
                            output += "\t\t.dw " + label + "\t; metasprite data pointer for this frame.\n\n";
                        }
                        else
                        {
                            output += "\t\t.db " + timeDelay.ToString() + "\n";
                            output += "\t\t.dw " + label + "\n\n";
                        }
                    }

                    output += "\n";

                }
            }

            output = output_header + output;

            byte[] bytes = Encoding.ASCII.GetBytes(output);

            File.WriteAllBytes(fileName, bytes);
        }

        private void exportAnimationToJsonDialog_FileOk(object sender, CancelEventArgs e)
        {
            string fileName = exportAnimationToJsonDialog.FileName;

            string animation_label = Path.GetFileNameWithoutExtension(fileName);


            string output = "{\n";

            if (metaSprites.Count > 0)
            {
                output += "\t\"metasprite_offsets\": [" + metaSprite_offset_X.ToString() + ", " + metaSprite_offset_Y.ToString() + "],\n";

                output += "\t\"metasprites\": [\n";

                foreach (MetaSprite ms in metaSprites)
                {

                    if (ms.Count < 1)
                        continue;

                    if (metaSprites.IndexOf(ms) > 0)
                        output += ",\n";

                    output += "\t\t{\n";
                    output += "\t\t\t\"label\": \"" + ms.Label + "\",\n";
                    output += "\t\t\t\"oam_entries\": [\n";

                    foreach (OamEntry oam in ms)
                    {
                        output += "\t\t\t\t" + oam.X.ToString() + ", " + oam.Y.ToString() + ", " + oam.Char.ToString() + ", " + oam.Attribute.ToString();

                        if (ms.IndexOf(oam) < ms.Count - 1)
                            output += ",\n";
                    }

                    output += "\n";
                    output += "\t\t\t]\n";

                    output += "\t\t}";

                }

                output += "\n";
                output += "\t]";
            }



            if (animations.Count < 1)
            {
                output += "\n";
            }
            else
            {
                output += ",\n";

                output += "\t\"animations\": [\n";

                foreach (AnimationSequence a in animations)
                {
                    output += "\t\t{\n";
                    output += "\t\t\t\"label\": \"" + a.Label + "\",\n";
                    output += "\t\t\t\"loop_frame\": " + a.LoopToFrame.ToString() + ",\n";
                    output += "\t\t\t\"frames\": [\n";
                    
                    foreach (AnimationCel c in a)
                    {
                        output += "\t\t\t\t{\n";
                        output += "\t\t\t\t\t\"delay\": " + c.TimeDelay.ToString() + ",\n";
                        output += "\t\t\t\t\t\"metasprite\": \"" + metaSprites[c.MetaSpriteIndex].Label + "\"\n";

                        if (a.IndexOf(c) == a.Count - 1)
                            output += "\t\t\t\t}\n";
                        else
                            output += "\t\t\t\t},\n";
                    }

                    output += "\t\t\t]\n";

                    if (animations.IndexOf(a) == animations.Count - 1)
                        output += "\t\t}\n";
                    else
                        output += "\t\t},\n";
                }

                output += "\t]\n";

            }

            output += "}";

            byte[] bytes = Encoding.ASCII.GetBytes(output);

            File.WriteAllBytes(fileName, bytes);
        }

        private void exportAnimationToCDialog_FileOk(object sender, CancelEventArgs e)
        {
            string fileName = exportAnimationToCDialog.FileName;

            string animation_label = Path.GetFileNameWithoutExtension(fileName);

            string output = "#ifndef NESAC_MACROS\n";
            output += "#define NESAC_MACROS\n";
            output += "#define NESAC_LOBYTE(p) (((unsigned int) (p)) & 0xff)\n";
            output += "#define NESAC_HIBYTE(p) (((unsigned int) (p)) >> 8)\n";
            output += "#endif\n\n";

            if (metaSprites.Count > 0)
            {
                output += "const unsigned char " + animation_label + "_metasprite_offset[]={\n";
                output += "\t" + metaSprite_offset_X.ToString() + ", " + metaSprite_offset_Y.ToString() + "\n}\n\n";

                string ms_list = "const unsigned char* const " + animation_label + "_metasprite_list[]={\n";

                foreach (MetaSprite ms in metaSprites)
                {

                    if (ms.Count < 1)
                        continue;

                    if (metaSprites.IndexOf(ms) > 0)
                        ms_list += ",\n";

                    output += "const unsigned char " + metaSprites[ms.Index].Label + "[]={\n";

                    foreach (OamEntry oam in ms)
                    {
                        output += "\t";
                        output += oam.X.ToString() + ", ";
                        output += oam.Y.ToString() + ", ";
                        output += String.Format("0x{0:x2}", oam.Char) + ", ";
                        output += oam.Attribute.ToString() + ", ";
                        output += "\n";
                    }

                    output += "\t128\n";
                    output += "};\n\n";

                    ms_list += "\t" + metaSprites[ms.Index].Label;
                }

                ms_list += "\n};\n\n";
                
                output += ms_list;
                //output += "const unsigned char " + animation_label + "_metasprite_offset[]={\n";
                //output += "\t" + metaSprite_offset_X.ToString() + ", " + metaSprite_offset_Y.ToString() + "\n}\n\n";
            }


            if (animations.Count > 0)
            {

                string a_list = "const unsigned char* const " + animation_label + "[]={\n";

                foreach (AnimationSequence a in animations)
                {
                    output += "const unsigned char " + a.Label + "[]={\n";
                    output += "\t" + a.LoopToFrame.ToString() + ",\t// index of frame to loop to.\n";

                    foreach (AnimationCel c in a)
                    {
                        output += "\t" + c.TimeDelay.ToString() + ",\t// time delay for this frame.\n";
                        output += "\tNESAC_LOBYTE(&" + metaSprites[c.MetaSpriteIndex].Label + "),\n";
                        output += "\tNESAC_HIBYTE(&" + metaSprites[c.MetaSpriteIndex].Label + ")";

                        if (a.IndexOf(c) < a.Count - 1)
                            output += ",\n";
                        else
                            output += "\n";
                    }

                    output += "};\n\n";

                    a_list += "\tNESAC_LOBYTE(&" + a.Label + "),\n";
                    a_list += "\tNESAC_HIBYTE(&" + a.Label + ")";

                    if (animations.IndexOf(a) < animations.Count - 1)
                        a_list += ",\n";
                    else
                        a_list += "\n";
                }

                a_list += "};\n\n";
                output += a_list;
            }

            byte[] bytes = Encoding.ASCII.GetBytes(output);

            File.WriteAllBytes(fileName, bytes);

        }

        private void pctPalette0Box_MouseDown(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                chrTablePaletteSelection = 0;
                renderChrTable();
            }
        }

        private void pctPalette1Box_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                chrTablePaletteSelection = 1;
                renderChrTable();
            }
        }

        private void pctPalette2Box_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                chrTablePaletteSelection = 2;
                renderChrTable();
            }
        }

        private void pctPalette3Box_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                chrTablePaletteSelection = 3;
                renderChrTable();
            }
        }

        private void btnEditMetasprite_Click(object sender, EventArgs e)
        {

            if (animations.Count < 1)
                return;


            animations[SelAnimation][SelAnimationCel].MetaSpriteIndex = SelMetaSpriteIndex;

            updateCelListBox();
            renderSelectedAnimation();

            //updateAnimationControls();
        }

        private void lstAnimationCels_Enter(object sender, EventArgs e)
        {
            lstAnimationCels.BackColor = SystemColors.Window;
        }

        private void lstAnimationCels_Leave(object sender, EventArgs e)
        {
            lstAnimationCels.BackColor = SystemColors.AppWorkspace;
        }

        private void lstAnimations_Enter(object sender, EventArgs e)
        {
            lstAnimations.BackColor = SystemColors.Window;
        }

        private void lstAnimations_Leave(object sender, EventArgs e)
        {
            lstAnimations.BackColor = SystemColors.AppWorkspace;
        }

        private void lstAnimationCels_DrawItem(object sender, DrawItemEventArgs e)
        {
            drawListBoxItem(sender, e, lstAnimationCels);
        }

        private void lstAnimations_DrawItem(object sender, DrawItemEventArgs e)
        {
            drawListBoxItem(sender, e, lstAnimations);
        }

        private void drawListBoxItem(object sender, DrawItemEventArgs e, ListBox listBox)
        {
            if (e.Index < 0) return;

            Color selected_color = SystemColors.ControlDarkDark;
            Brush selected_font_color = Brushes.Black;

            if (listBox.Focused)
            {
                selected_color = SystemColors.MenuHighlight;
            }

            //if the item state is selected them change the back color 
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                e = new DrawItemEventArgs(e.Graphics,
                                          e.Font,
                                          e.Bounds,
                                          e.Index,
                                          e.State ^ DrawItemState.Selected,
                                          e.ForeColor,
                                          selected_color);//Choose the color

                selected_font_color = Brushes.White;
            }

            // Draw the background of the ListBox control for each item.
            e.DrawBackground();
            // Draw the current item text
            e.Graphics.DrawString(listBox.Items[e.Index].ToString(), e.Font, selected_font_color, e.Bounds, StringFormat.GenericDefault);
            // If the ListBox has focus, draw a focus rectangle around the selected item.
            e.DrawFocusRectangle();
        }

        private void chk8x16Mode_CheckedChanged(object sender, EventArgs e)
        {
            chr8x16Mode = chk8x16Mode.Checked;

            renderMetasprite();
            renderSelectedAnimation();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            tmrAnimationTimer.Interval = (int)numericUpDown1.Value;
        }

        private void chkChrTableA_Click(object sender, EventArgs e)
        {
            chkChrTableB.Enabled = true;
            chkChrTableB.Focus();
            chkChrTableB.Checked = false;
            chkChrTableA.Enabled = false;

            renderChrTable();
        }

        private void chkChrTableB_Click(object sender, EventArgs e)
        {
            chkChrTableA.Enabled = true;
            chkChrTableA.Focus();
            chkChrTableA.Checked = false;
            chkChrTableB.Enabled = false;
            
            renderChrTable();
        }
    }
}
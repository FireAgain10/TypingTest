﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TypingTest
{
    public class TransparentPanel : Panel
    {
        private bool allReload = false;
        private bool init = true;
        private int charIndex = 0;
        private int lastCharIndex = 0;
        private bool colorFlag = false;
        private fetchColor setColor = fetchColor.Original;
        private List<int> redChar = new List<int>();
        private Font myFont;
        private float lastCharWidth = 0;
        private float charHeight = 5;
        private int penWidth = 2;
        private int caretOffset = 1;
        private int charSpaceReduce = 4;
        private List<boundStr> bounds = new List<boundStr>();
        private int KeyPress = 0; 
        private Mutex mutex = new Mutex();
        private enum fetchColor
        {
            Red = 1,
            Original = 2
        }
       // private TransparentTextBox tb1;
        public TransparentPanel()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;

            //tb1 = new TransparentTextBox();
            //tb1.Dock = DockStyle.Fill;
            //tb1.Multiline = true;
            //tb1.WordWrap = true;
            //tb1.BorderStyle= BorderStyle.None;
            //tb1.ReadOnly = true;
            myFont = new Font("Arial", 20, FontStyle.Bold);
            //tb1.Font = myFont;

            this.Text = "Hello, this is some text! including the Mozilla Firefox web browser. Always refer to the specific version of the license provided with the software for precise terms and conditions.";

           // this.Controls.Add(tb1);
            redChar = new List<int>();
            using (Graphics g = CreateGraphics())
            {
                charHeight = g.MeasureString(this.Text[0].ToString(), myFont).Height;
                // nextCharWidth = g.MeasureString(this.Text[0].ToString(), myFont).Width;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (SolidBrush brush = new SolidBrush(BackColor))
            {
                e.Graphics.FillRectangle(brush, ClientRectangle);
            }
            //  Console.WriteLine("Changed {0}", charIndex);

            if (allReload)
            {
                updateAll(e);
            }
            else
            {
                if (init)
                {
                    updateAll(e);
                   init = !init;
                }
                else
                {
                    KeyPress--;
                    updateOne(e);
                }
            }
           // mutex.ReleaseMutex();
        }

        public void updateAll(PaintEventArgs e)
        {
            string text = this.Text;
            int fontLen = 5;
            using (Graphics g = CreateGraphics())
            {
                for (int i = 0; i < text.Length; i++)
                {
                    boundStr tmp = new boundStr();
                    if (i == charIndex)
                    {
                        drawCaret(e, Color.DarkOrange, fontLen + caretOffset, (int)charHeight);
                    }
                    if (redChar.Contains(i))
                    {
                        drawText(e, text[i].ToString(), Color.Red, fontLen);
                    }
                    else
                    {
                        if (i < charIndex)
                        {
                            drawText(e, text[i].ToString(), Color.Gray, fontLen);
                        }
                        else
                        {
                            drawText(e, text[i].ToString(), Color.Black, fontLen);
                        }
                    }

                    tmp.index = i;
                    tmp.front = fontLen;
                    tmp.prevCaret = fontLen + caretOffset;

                    if (text[i].ToString() == " ")
                    {
                        lastCharWidth = g.MeasureString(text[i].ToString(), myFont).Width;
                        fontLen += (int)Math.Floor(lastCharWidth);
                    }
                    else
                    {
                        lastCharWidth = g.MeasureString(text[i].ToString(), myFont).Width - charSpaceReduce;
                        fontLen += (int)Math.Floor(lastCharWidth);
                    }

                    tmp.nextCaret = fontLen + caretOffset;
                    tmp.back = fontLen + caretOffset + penWidth;
                    Console.WriteLine("bounds: {0}: {1},{2},{3},{4} ", tmp.index.ToString(), tmp.front.ToString(), tmp.prevCaret.ToString(), tmp.nextCaret.ToString(), tmp.back.ToString());
                    bounds.Add(tmp);
                }
            }
        }

        public void updateOne(PaintEventArgs e)
        {
            string text = this.Text;
            using (Graphics g = CreateGraphics())
            {
                if (colorFlag)
                {
                    drawCaret(e, Color.DarkOrange, bounds[lastCharIndex].prevCaret, (int)charHeight);
                }
                else
                {
                    drawCaret(e, Color.DarkOrange, bounds[lastCharIndex].nextCaret, (int)charHeight);
                }

                if (redChar.Contains(lastCharIndex))
                {
                    drawText(e, text[lastCharIndex].ToString(), Color.Red, bounds[lastCharIndex].front);
                }
                else
                {
                    if (colorFlag)
                    {
                        drawText(e, text[lastCharIndex].ToString(), Color.Black, bounds[lastCharIndex].front);
                    }
                    else
                    {
                        drawText(e, text[lastCharIndex].ToString(), Color.Gray, bounds[lastCharIndex].front);
                    }
                }
            }
        }

        private float getCharWidth(int idx)
        {
            using (Graphics g = CreateGraphics())
            {
                float width = g.MeasureString(this.Text[idx].ToString(), myFont).Width;
                if (this.Text[idx].ToString() != " ") { width -= charSpaceReduce; }
                return width;
            }
        }
        private void drawText(PaintEventArgs e, string txt, Color clr, int loc)
        {
            using (Brush brush = new SolidBrush(clr))
            {
                e.Graphics.DrawString(txt, myFont, brush, loc, 0);
            }

            Console.WriteLine("drawText: {0} ", loc.ToString());
        }
        private void drawCaret(PaintEventArgs e, Color clr, int loc, int height)
        {
            int offset = (height / 2);
            Point p1 = new Point(loc, -offset);
            Point p2 = new Point(loc, height);

            using (Pen pen = new Pen(clr, penWidth))
            {
                e.Graphics.DrawLine(pen, p1, p2);
                Console.WriteLine("drawCaret {0} : ", p1.ToString());
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            this.Invalidate(); // Trigger repaint when text changes
        }

        public void OnKeyPressEvent(char ch)
        {
            //  base.OnKeyPress(e);
            KeyPress++;

            //char ch = e.KeyChar;
            int offset = (((int)charHeight) / 2);
            Rectangle regionToInvalidate;
            int x, y, len, height;

            if (isValidChar(ch) && charIndex != this.Text.Length)
            {
                colorFlag = false;
                if (this.Text[charIndex].ToString() != ch.ToString())
                {
                    //setColor = (int)fetchColor.Red;
                    redChar.Add(charIndex);
                }
                if (!allReload)
                {
                    x = bounds[charIndex].front;
                    y = -offset;
                    len = bounds[charIndex].back - bounds[charIndex].front;
                    height = 3 * offset;
                    regionToInvalidate = new Rectangle(x, y, len, height);
                    Console.WriteLine(Environment.NewLine + "Invalidate From: {0} To: {1}  Index {2}", x, (x + len).ToString(), charIndex.ToString());
                    this.Invalidate(regionToInvalidate);
                }
                else
                {
                    this.Invalidate();
                }
                lastCharIndex = charIndex;
                charIndex++;
                //  setCurrentColor(e.KeyChar);
            }
            else if ((short)Keys.Back == ((short)ch) && charIndex != 0) //backspace
            {
                charIndex--;
                lastCharIndex = charIndex;
                colorFlag = true;
                if (redChar.Contains(charIndex))
                {
                    redChar.Remove(charIndex);
                }
                if (!allReload)
                {
                    //nextCharWidth = getCharWidth(charIndex);
                    //Console.WriteLine("nextCharWidth: {0}", (int)nextCharWidth);
                    ////lastFontLen[0]= lastFontLen[0] - ((int)nextCharWidth); 
                    //regionToInvalidate = new Rectangle(lastFontLen[0], -offset, ((int)nextCharWidth) + caretOffset + penWidth, 3 * offset);
                    //Console.WriteLine(Environment.NewLine + "Invalidate From: {0} To: {1}", (lastFontLen[0]).ToString(), (lastFontLen[0] + (int)nextCharWidth + caretOffset + penWidth).ToString());
                    //this.Invalidate(regionToInvalidate);
                    x = bounds[charIndex].front;
                    y = -offset;
                    len = bounds[charIndex].back - bounds[charIndex].front;
                    height = 3 * offset;
                    regionToInvalidate = new Rectangle(x, y, len, height);
                    Console.WriteLine(Environment.NewLine + "Invalidate From: {0} To: {1}  Index {2}", x, (x + len).ToString(), charIndex.ToString());
                    this.Invalidate(regionToInvalidate);
                }
                else
                {
                    this.Invalidate();
                }
                //   setColor = (int)fetchColor.Original;
            }
            else
            {
                Console.WriteLine(ch);
                Debug.Print(((short)ch).ToString());
            }
            //mutex.ReleaseMutex();
        }

        private bool isValidChar(char ch)
        {
            if (char.IsLetterOrDigit(ch) || char.IsSymbol(ch) || char.IsPunctuation(ch) || char.IsSeparator(ch) || char.IsWhiteSpace(ch))
            {
                return true;
            }
            return false;
        }
        private void setCurrentColor(char current)
        {
            if (this.Text[charIndex].ToString() != current.ToString())
            {
                setColor = fetchColor.Red;
            }
        }
        private Color GetRandomColor()
        {
            Random random = new Random();
            Color clr = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
            //Color clr = Color.Red;
            Console.WriteLine(clr.Name);
            return clr;
        }
        private Color SetDefinedColor()
        {
            if (setColor == fetchColor.Red)
            {
                return Color.Red;
            }
            else
            {
                return Color.Black;
            }
        }
    }


    public class boundStr
    {
        public int index;
        public int front;
        public int back;
        public int prevCaret;
        public int nextCaret;
    }
}

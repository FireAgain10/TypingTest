using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace TypingTest
{
    public class TransparentPanel : Panel
    {
        private bool allReload = false;
        private bool init = true;
        private int charIndex = 0;
        private int lastCharIndex = 0;
        private bool backFlag = false;
        private fetchColor setColor = fetchColor.Original;
        private List<int> redChar = new List<int>();
        private Font myFont;
        private float lastCharWidth = 0;
        private float charHeight = 5;
        private int penWidth = 2;
        private int caretOffset = 2;
        private int charSpaceReduce = 6;
        private int spaceWidthInc = 6;
        private List<boundStr> bounds = new List<boundStr>();
        private int KeyPressCount = 0;
        private Mutex mutex = new Mutex();
        private enum fetchColor
        {
            Red = 1,
            Original = 2
        }
        private Func<int, bool> setMainFormHeight;
        private int myWidth;
        private List<int> nextLine;
        private int endMargin = 5;

        public TransparentPanel(Func<int, bool> callback, int formWidth)
        {
            //Console.WriteLine($"TransparentPanel Thread: {Thread.CurrentThread.ManagedThreadId.ToString()}");
            setMainFormHeight = callback;
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.LightBlue;
            myWidth = formWidth;
            nextLine = new List<int>();
            // this.Text = "HHHHH HHHHH HHHHH HHHHH HHHHH HHHHH";
            var textToShow = "Hello, this is some text! including the Mozilla Firefox web browser. Always refer to the specific version of the license provided with the software for precise terms and conditions.";
            updateText(textToShow);
        }
        private void updateText(string text)
        {
            myFont = new Font("Arial", 20, FontStyle.Regular);
            redChar = new List<int>();
            using (Graphics g = CreateGraphics())
            {
                charHeight = g.MeasureString(text, myFont).Height;
                int height = ((int)charHeight) * 4;
                setMainFormHeight(height);
                // this.Height = height; //dock fill
            }
            //string textToCheck = this.Text;
            setMultilineText(text);
        }

        private bool setMultilineText(string text)
        {
            int posToAdd, idx, finalIdx, lastIdxCheck = 0;
            finalIdx = text.Length;
            string tempString, textToCheck = text;
            idx = GetWhitespaceIndex(text);
            while (true) //implement quick sort
            {
                if (toNextLine(textToCheck.Substring(0, idx)))
                {
                    nextLine.Add(lastIdxCheck);
                    this.Text = textToCheck.Substring(0, lastIdxCheck);
                    Program.printCheckpoint($"{this.Text}", true);
                    return true;
                }
                else
                {
                    if (idx >= finalIdx)
                    {
                        this.Text = textToCheck.Substring(0, lastIdxCheck);
                        Program.printCheckpoint($"{this.Text}", true);
                        return true;
                    }
                    lastIdxCheck = idx;
                    idx++;
                    tempString = textToCheck.Substring(idx);
                    posToAdd = GetWhitespaceIndex(tempString);
                    if (posToAdd == -1)
                    {
                        idx += tempString.Length;
                    }
                    else
                    {
                        idx += posToAdd;
                    }
                }
            }
        }

        private int GetWhitespaceIndex(string input)
        {
            //  Console.WriteLine(input);
            int index = input.IndexOf(' ');
            return index;
        }
        private bool toNextLine(string input)
        {
            using (Graphics g = CreateGraphics())
            {
                float stringWidth = 0;
                if (Program.charByCharPrint)
                {
                    foreach (char c in input)
                    {
                        stringWidth += g.MeasureString(c.ToString(), myFont).Width;
                    }
                }
                else
                {
                    stringWidth = g.MeasureString(input, myFont).Width;
                }
                Program.printCheckpoint($"{myWidth} : {stringWidth} --- {input}", true);
                if (stringWidth > myWidth-endMargin)
                {
                    return true;
                }
                else
                {
                    return false;
                }
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
                    KeyPressCount--;
                    updateOne(e);
                }
            }
            // mutex.ReleaseMutex();
        }

        public void updateAll(PaintEventArgs e)
        {
            string text = this.Text;
            int fontLen = 0;
            using (Graphics g = CreateGraphics())
            {
                // drawText(e, text.ToString(), Color.Red, fontLen);
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

                    tmp.index = i;                                  //index
                    tmp.front = fontLen;                            //front
                    tmp.prevCaret = fontLen + caretOffset;          // where  caret should be if pressed backspace

                    lastCharWidth = getCharWidth(i, g);
                    fontLen += (int)Math.Floor(lastCharWidth);      //end position of char

                    tmp.nextCaret = fontLen + caretOffset;          // where next caret will be
                    tmp.back = fontLen + caretOffset + penWidth;    // from where paiint should be invalidated if pressed backspace
                    Program.printCheckpoint($"bounds: {tmp.index}: {tmp.front},{tmp.prevCaret},{tmp.nextCaret},{tmp.back} ",false);
                    bounds.Add(tmp);
                }
            }
        }

        public void updateOne(PaintEventArgs e)
        {
            string text = this.Text;
            using (Graphics g = CreateGraphics())
            {
                if (backFlag)
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
                    if (backFlag)
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
                float width;
                if (this.Text[idx].ToString() == " ")
                {
                    width = g.MeasureString(this.Text[idx].ToString(), myFont).Width;
                }
                else
                {
                    width = g.MeasureString(this.Text[idx].ToString().Trim(), myFont).Width;
                    width -= charSpaceReduce;
                }
                return width;
            }
        }

        private float getCharWidth(int idx, Graphics g)
        {
            float width;
            if (this.Text[idx].ToString() == " ")
            {
                width = g.MeasureString(this.Text[idx].ToString(), myFont).Width + spaceWidthInc;
            }
            else
            {
                width = g.MeasureString(this.Text[idx].ToString().Trim(), myFont).Width;
                width -= charSpaceReduce;
            }
            return width;
        }
        private void drawText(PaintEventArgs e, string txt, Color clr, int loc)
        {
            using (Brush brush = new SolidBrush(clr))
            {
                e.Graphics.DrawString(txt, myFont, brush, loc, 0);
            }

            Program.printCheckpoint($"drawText: {loc.ToString()} ", false);
        }
        private void drawCaret(PaintEventArgs e, Color clr, int loc, int height)
        {
            int offset = (height / 2);
            Point p1 = new Point(loc, -offset);
            Point p2 = new Point(loc, height);

            using (Pen pen = new Pen(clr, penWidth))
            {
                e.Graphics.DrawLine(pen, p1, p2);
                Program.printCheckpoint($"drawCaret {p1.ToString()}",false);
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            this.Invalidate(); // Trigger repaint when text changes
        }

        public void OnKeyPressEvent(char ch)
        {
            // Console.WriteLine("here");
            //  base.OnKeyPress(e);
            KeyPressCount++;

            //char ch = e.KeyChar;
            int offset = (((int)charHeight) / 2);
            Rectangle regionToInvalidate;
            int x, y, len, height;

            if (isValidChar(ch) && charIndex != this.Text.Length)
            {
                backFlag = false;
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
                backFlag = true;
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

        //public void OnKeyPressEvent(int chr)
        //{
        //    // Console.WriteLine("here");
        //    //  base.OnKeyPress(e);
        //    Keys keys = (Keys)chr;
        //    char ch = ((char)keys);
        //    var str = keys.ToString();
        //    KeyPressCount++;

        //    if (keys >= Keys.A && keys <= Keys.Z)
        //    {
        //        Console.WriteLine($"Alphabet key pressed: {keys}");
        //    }
        //    else if (keys >= Keys.D0 && keys <= Keys.D9)
        //    {
        //        Console.WriteLine($"Numeric key pressed: {keys}");
        //    }
        //    else if (keys >= Keys.Oem1 && keys <= Keys.OemSemicolon)
        //    {
        //        Console.WriteLine($"Punctuation Symbols key pressed: {keys}");
        //    }
        //    else if (keys == Keys.Space)
        //    {
        //        Console.WriteLine($"Space key pressed: {keys}");
        //    }
        //    else
        //    {
        //        Console.WriteLine($"Special key pressed: {keys}");
        //    }

        //    return;

        //    //char ch = e.KeyChar;
        //    int offset = (((int)charHeight) / 2);
        //    Rectangle regionToInvalidate;
        //    int x, y, len, height;

        //    if (isValidChar(ch) && charIndex != this.Text.Length)
        //    {
        //        backFlag = false;
        //        if (this.Text[charIndex].ToString() != ch.ToString())
        //        {
        //            //setColor = (int)fetchColor.Red;
        //            redChar.Add(charIndex);
        //        }
        //        if (!allReload)
        //        {
        //            x = bounds[charIndex].front;
        //            y = -offset;
        //            len = bounds[charIndex].back - bounds[charIndex].front;
        //            height = 3 * offset;
        //            regionToInvalidate = new Rectangle(x, y, len, height);
        //            Console.WriteLine(Environment.NewLine + "Invalidate From: {0} To: {1}  Index {2}", x, (x + len).ToString(), charIndex.ToString());
        //            this.Invalidate(regionToInvalidate);
        //        }
        //        else
        //        {
        //            this.Invalidate();
        //        }
        //        lastCharIndex = charIndex;
        //        charIndex++;
        //        //  setCurrentColor(e.KeyChar);
        //    }
        //    else if ((short)Keys.Back == ((short)ch) && charIndex != 0) //backspace
        //    {
        //        charIndex--;
        //        lastCharIndex = charIndex;
        //        backFlag = true;
        //        if (redChar.Contains(charIndex))
        //        {
        //            redChar.Remove(charIndex);
        //        }
        //        if (!allReload)
        //        {
        //            //nextCharWidth = getCharWidth(charIndex);
        //            //Console.WriteLine("nextCharWidth: {0}", (int)nextCharWidth);
        //            ////lastFontLen[0]= lastFontLen[0] - ((int)nextCharWidth); 
        //            //regionToInvalidate = new Rectangle(lastFontLen[0], -offset, ((int)nextCharWidth) + caretOffset + penWidth, 3 * offset);
        //            //Console.WriteLine(Environment.NewLine + "Invalidate From: {0} To: {1}", (lastFontLen[0]).ToString(), (lastFontLen[0] + (int)nextCharWidth + caretOffset + penWidth).ToString());
        //            //this.Invalidate(regionToInvalidate);
        //            x = bounds[charIndex].front;
        //            y = -offset;
        //            len = bounds[charIndex].back - bounds[charIndex].front;
        //            height = 3 * offset;
        //            regionToInvalidate = new Rectangle(x, y, len, height);
        //            Console.WriteLine(Environment.NewLine + "Invalidate From: {0} To: {1}  Index {2}", x, (x + len).ToString(), charIndex.ToString());
        //            this.Invalidate(regionToInvalidate);
        //        }
        //        else
        //        {
        //            this.Invalidate();
        //        }
        //        //   setColor = (int)fetchColor.Original;
        //    }
        //    else
        //    {
        //        Console.WriteLine(ch);
        //        Debug.Print(((short)ch).ToString());
        //    }
        //    //mutex.ReleaseMutex();
        //}
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
            // Console.WriteLine(clr.Name);
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

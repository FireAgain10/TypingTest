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
        private int charSpaceReduce = 2;
        private int spaceWidthInc = 10;
        private List<boundStr> bounds = new List<boundStr>();
        private Mutex mutex;
        private static bool printAvailable = true;
        private enum fetchColor
        {
            Red = 1,
            Original = 2
        }
        private Func<int, bool> setMainFormHeight;
        private int myWidth;
        private List<int> nextLine;
        private List<string> textStrings;
        private int endMargin = 9;
        private Color fontBackColor = Color.Transparent;
        private string textToShow;
        private bool nextLineChange;
        private bool xhangeLineRender = false;
        private bool prevLineChange;
        private int toNextLineDisplay = 0;
        public TransparentPanel(Func<int, bool> callback, int formWidth)
        {
            //Console.WriteLine($"TransparentPanel Thread: {Thread.CurrentThread.ManagedThreadId.ToString()}");
            mutex = new Mutex();
            setMainFormHeight = callback;
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.LightBlue;
            myWidth = formWidth;
            nextLine = new List<int>();
            textStrings = new List<string>();
            // this.Text = "HHHHH HHHHH HHHHH HHHHH HHHHH HHHHH";
            textToShow = "Hello, this is some text! including the Mozilla Firefox web browser. " +
                            "Always refer to the specific version of the license provided with the software for precise terms and conditions.";

            if (Program.fontWidth < 8) Program.fontWidth = 8;
            setCharSpaceReduction();
            updateText(textToShow);
        }
        private void updateText(string text)
        {
            Program.fontWidth = (Program.fontWidth > 50) ? 50 : Program.fontWidth;
            myFont = new Font("Arial", Program.fontWidth, FontStyle.Regular);
            redChar = new List<int>();
            using (Graphics g = CreateGraphics())
            {
                charHeight = g.MeasureString(text, myFont).Height;
                int height = (int)(charHeight * 3.3);
                setMainFormHeight(height);
                this.Height = height; //dock fill
            }
            //string textToCheck = this.Text;
            setMultilineText(text);
            setPanelText();
        }

        private void setCharSpaceReduction()
        {
            var p = Program.fontWidth;
            if (p <= 12) { charSpaceReduce = 0; ; caretOffset = 1; penWidth = 1; }
            else if (p > 12 && p <= 24) { charSpaceReduce = 3; caretOffset = 1; penWidth = 2; }
            else if (p > 24 && p <= 32) { charSpaceReduce = 6; caretOffset = 3; penWidth = 3; }
            else if (p > 32 && p <= 36) { charSpaceReduce = 8; caretOffset = 4; penWidth = 4; }
            else if (p > 36 && p <= 40) { charSpaceReduce = 9; caretOffset = 4; penWidth = 4; }
            else if (p > 40 && p <= 44) { charSpaceReduce = 10; caretOffset = 4; penWidth = 4; }
            else if (p > 44 && p <= 50) { charSpaceReduce = 12; caretOffset = 4; penWidth = 4; }
            else { charSpaceReduce = 14; caretOffset = 4; penWidth = 5; }
        }

        private bool setMultilineText(string text)
        {
            int posToAdd, idx, finalIdx, lastIdxCheck = 0;
            finalIdx = text.Length;
            string tempString, textToCheck = text;
            idx = GetWhitespaceIndex(text);
            while (true) //implement quick sort
            {
                if (idx != -1 && toNextLine(textToCheck.Substring(0, idx)))
                {
                    if (nextLine.Count > 0) { nextLine.Add(nextLine[nextLine.Count - 1] + lastIdxCheck); }
                    else { nextLine.Add(lastIdxCheck); }

                    Program.printCheckpoint($"{textToCheck.Substring(0, lastIdxCheck)}", true);
                    textStrings.Add(textToCheck.Substring(0, lastIdxCheck));
                    textToCheck = textToCheck.Substring(lastIdxCheck);
                    lastIdxCheck = 0;
                    finalIdx = textToCheck.Length;
                    idx = GetWhitespaceIndex(textToCheck);
                    //this.Text = textToCheck.Substring(0, lastIdxCheck);
                    //return true;
                    if (idx == -1)
                    {
                        idx = textToCheck.Length;
                    }
                }
                else
                {
                    if (idx >= finalIdx)
                    {
                        if (nextLine.Count > 0) { nextLine.Add(nextLine[nextLine.Count - 1] + finalIdx); }
                        else { nextLine.Add(finalIdx); }
                        //this.Text = textToCheck.Substring(0, lastIdxCheck);
                        Program.printCheckpoint($"{textToCheck.Substring(0, finalIdx)}", true);
                        textStrings.Add(textToCheck.Substring(0, finalIdx));
                        return true;
                    }
                    idx++;
                    lastIdxCheck = idx;
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
                        if (char.IsWhiteSpace(c))
                        {
                            stringWidth += spaceWidthInc;
                        }
                        else
                        {
                            stringWidth -= charSpaceReduce;
                        }
                    }
                }
                else
                {
                    stringWidth = g.MeasureString(input, myFont).Width;
                }
                Program.printCheckpoint($"{myWidth}:{endMargin} : {stringWidth} --- {input}", false);
                if (stringWidth > myWidth - endMargin)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private void setPanelText()
        {
            string thisText = "";
            for (int i = 0; i < nextLine.Count && i < 3; i++)
            {
                thisText = textToShow.Substring(0, nextLine[i]);
            }
            this.Text = thisText;
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
                    updateOne(e);
                }
            }
            printAvailable = true;
            //  mutex.ReleaseMutex();
        }

        public void updateAll(PaintEventArgs e)
        {
            int strIdx = 0;
            int fontPos = 0;
            using (Graphics g = CreateGraphics())
            {
                while (strIdx < textStrings.Count && strIdx-toNextLineDisplay < 3)
                {
                    string text = textStrings[strIdx+toNextLineDisplay];
                    int offset = (((int)charHeight) / 2);
                    // drawText(e, text.ToString(), Color.Red, fontPos); // set it for printCharByChar
                    for (int i = 0; i < text.Length; i++)
                    {
                        boundStr tmp = new boundStr();

                        tmp.index = (strIdx > 0 ? bounds.Count : i);
                        tmp.x1 = fontPos;
                        tmp.yCaret = (strIdx * (2 * offset));
                        tmp.yText = (strIdx * (2 * offset));
                        tmp.prevCaret = fontPos + caretOffset;          // where  caret should be if pressed backspace

                        if (i == charIndex && strIdx == 0)
                        {
                            drawCaret(e, Color.DarkOrange, fontBackColor, tmp.prevCaret, (int)charHeight, tmp.yCaret);
                        }
                        if (redChar.Contains(i))
                        {
                            drawText(e, text[i].ToString(), Color.Red, fontBackColor, tmp.x1, tmp.yCaret);
                        }
                        else
                        {
                            if (i < charIndex)
                            {
                                drawText(e, text[i].ToString(), Color.Gray, fontBackColor, tmp.x1, tmp.yCaret);
                            }
                            else
                            {
                                drawText(e, text[i].ToString(), Color.Black, fontBackColor, tmp.x1, tmp.yCaret);
                            }
                        }


                        lastCharWidth = getCharWidth(text, i, g);
                        fontPos += (int)Math.Floor(lastCharWidth);      //end position of char

                        tmp.nextCaret = fontPos + caretOffset;          // where next caret will be
                        tmp.x2 = fontPos + caretOffset + penWidth;    // from where paiint should be invalidated if pressed backspace
                        Program.printCheckpoint($"bounds: {tmp.index}: {tmp.x1},{tmp.prevCaret},{tmp.nextCaret},{tmp.x2} ", true);
                        bounds.Add(tmp);
                    }

                    strIdx++;
                    fontPos = 0;
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
                    drawCaret(e, Color.DarkOrange, fontBackColor, bounds[lastCharIndex].prevCaret, (int)charHeight, bounds[lastCharIndex].yCaret);
                    if (prevLineChange)
                    {
                        changeLineRender();
                        return;
                    }
                }
                else
                {
                    if (nextLineChange && nextLine.Contains(charIndex))
                    {
                        changeLineRender();
                        drawCaret(e, Color.DarkOrange, fontBackColor, bounds[charIndex].prevCaret, (int)charHeight, bounds[charIndex].yCaret);
                        return;
                    }

                    if (redChar.Contains(lastCharIndex) && Program.highlightMistake)
                    {
                        var bgClr = fontBackColor;// Color.FromArgb(255, Color.WhitrSmoke);
                        drawCaret(e, Color.DarkOrange, bgClr, bounds[lastCharIndex].nextCaret, (int)charHeight, bounds[lastCharIndex].yCaret);
                    }
                    else
                    {
                        drawCaret(e, Color.DarkOrange, fontBackColor, bounds[lastCharIndex].nextCaret, (int)charHeight, bounds[lastCharIndex].yCaret);
                    }
                }

                // if (nextLineChange) return;

                if (redChar.Contains(lastCharIndex))
                {
                    drawText(e, text[lastCharIndex].ToString(), Color.Red, fontBackColor, bounds[lastCharIndex].x1, bounds[lastCharIndex].yCaret);
                }
                else
                {
                    if (backFlag)
                    {
                        drawText(e, text[lastCharIndex].ToString(), Color.Black, fontBackColor, bounds[lastCharIndex].x1, bounds[lastCharIndex].yCaret);
                    }
                    else
                    {
                        drawText(e, text[lastCharIndex].ToString(), Color.Gray, fontBackColor, bounds[lastCharIndex].x1, bounds[lastCharIndex].yCaret);
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

        private float getCharWidth(string text, int idx, Graphics g)
        {
            float width;
            if (text[idx].ToString() == " ")
            {
                width = g.MeasureString(text[idx].ToString(), myFont).Width + spaceWidthInc;
            }
            else
            {
                width = g.MeasureString(text[idx].ToString().Trim(), myFont).Width;
                width -= charSpaceReduce;
            }
            return width;
        }
        private void drawText(PaintEventArgs e, string txt, Color fontColor, Color backColor, int posX, int posY = 0)
        {
            //using (Brush brush = new SolidBrush(backColor))
            //{
            //    e.Graphics.FillRectangle(brush,regionToInvalidate);
            //}
            using (Brush brush = new SolidBrush(fontColor))
            {
                e.Graphics.DrawString(txt, myFont, brush, posX, posY);
            }

            Program.printCheckpoint($"drawText: {posX.ToString()} ", false);
        }
        private void drawCaret(PaintEventArgs e, Color fontColor, Color backColor, int posX, int height, int posY = 0)
        {
            // int offset = (height / 2);
            Point p1 = new Point(posX, posY);
            Point p2 = new Point(posX, posY + height);

            using (Brush brush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(brush, regionToInvalidate);
            }
            using (Pen pen = new Pen(fontColor, penWidth))
            {
                e.Graphics.DrawLine(pen, p1, p2);
                Program.printCheckpoint($"drawCaret {p1.ToString()}", false);
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            this.Invalidate(); // Trigger repaint when text changes
        }

        private Rectangle regionToInvalidate;
        public void OnKeyPressEvent(char ch)
        {
            Program.printCheckpoint($"OnKeyPressEvent {ch}", false);

            if (!printAvailable) { return; }
            printAvailable = false;

            backFlag = ((short)Keys.Back == ((short)ch)) ? true : false;

            int offset = (((int)charHeight) / 2);
            int x, y, len, height, strIdx = 0, charPos;
            prevLineChange = nextLineChange = false;

            charPos = charIndex;
            foreach (int num in nextLine)
            {
                if (charIndex >= num)
                {
                    if (backFlag && charIndex == num)
                    {
                        prevLineChange = true;
                        xhangeLineRender = true;
                        break;
                    }
                     strIdx++;
                    charPos = charIndex - nextLine[strIdx - 1];
                }
                else
                {
                    if (charIndex == num - 1)
                    {
                        //if (strIdx >= 1 && strIdx != textStrings.Count-2)
                        //{
                        //    toNextLineDisplay++;
                        //    init = true;
                        //    this.Invalidate();
                        //}
                        if (strIdx == 2 && !backFlag)
                        {
                            Program.printCheckpoint("End of Text", true);
                            printAvailable = true;
                            return;
                        }   //End of third Line
                        nextLineChange = true;
                        xhangeLineRender = true;
                    }
                    else
                    {
                        nextLineChange = false;
                    }
                    break;
                }
            }

            if (isValidChar(ch) && strIdx <= 2)
            {
                if (textStrings[strIdx][charPos].ToString() != ch.ToString())
                {
                    redChar.Add(charIndex);
                }
                if (!allReload)
                {
                    x = bounds[charIndex].x1;
                    y = bounds[charIndex].yCaret;
                    len = bounds[charIndex].x2 - bounds[charIndex].x1;
                    height = (int)charHeight;

                    regionToInvalidate = new Rectangle(x, y, len, height);
                    Program.printCheckpoint($"Invalidate From: [{x},{y}] To: [{(x + len)},{y + height}]  Index {charIndex}", true);
                    this.Invalidate(regionToInvalidate);
                }
                else
                {
                    this.Invalidate();
                }
                lastCharIndex = charIndex;
                charIndex++;
            }
            else if (backFlag && charIndex != 0) //backspace
            {
                charIndex--;
                lastCharIndex = charIndex;
                if (redChar.Contains(charIndex))
                {
                    redChar.Remove(charIndex);
                }
                if (!allReload)
                {
                    x = bounds[charIndex].x1;
                    y = bounds[charIndex].yCaret;
                    len = bounds[charIndex].x2 - bounds[charIndex].x1;
                    height = (int)charHeight;

                    regionToInvalidate = new Rectangle(x, y, len, height);
                    Program.printCheckpoint($"Invalidate From: [{x},{y}] To: [{(x + len)},{y + height}]  Index {charIndex}", true);
                    this.Invalidate(regionToInvalidate);
                }
                else
                {
                    this.Invalidate();
                }
            }
            else
            {
                Console.WriteLine(ch);
                Debug.Print(((short)ch).ToString());
                printAvailable = true;
            }
            //if (nextLineChange) changeLineRender();
        }

        private void changeLineRender()
        {
            if (!xhangeLineRender) return;
            int x = 0, y = 0, len = 0, height = 0;
            if (backFlag)
            {
                x = bounds[charIndex + 1].x1;
                y = bounds[charIndex + 1].yCaret;
                len = caretOffset + penWidth;
                height = (int)charHeight;
                xhangeLineRender = false;
            }
            else
            {
                x = bounds[charIndex].x1;
                y = bounds[charIndex].yCaret;
                len = caretOffset + penWidth;
                height = (int)charHeight;
                xhangeLineRender = false;
            }
            regionToInvalidate = new Rectangle(x, y, len, height);
            Program.printCheckpoint($"Invalidate for Caret Only From: [{x},{y}] To: [{(x + len)},{y + height}]  Index {charIndex}", true);
            this.Invalidate(regionToInvalidate);
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
        public int x1;          //1
        public int x2;          //3
        public int yCaret;
        public int yText;
        public int prevCaret;   //2
        public int nextCaret;   //4
    }
}

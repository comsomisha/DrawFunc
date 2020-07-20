using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Интепретатор_15;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;

namespace СиШарп5
{
    public partial class Form1 : Form
    {
        TParser parser;
        byte fFunc = 0; // для выбора производной
        int n = 150; // для выбора шага построения
        double size = 1; // для регулирования масштаба

        Graphics g;
        public static Pen MyPen;
        public static Pen MyPen2;
        static int I1 = 0, J1 = 0, I2, J2;

        public static double x1 = -8.1, y1 = -20, x2 = 8.1, y2 = 20;

        bool drawing = false;

        MouseEventArgs e0;

        public delegate int IJ(double x);
        // масштабирование, перевод из координат
        int II(double x)
        {
            return I1 + (int)((x - x1) * (I2 - I1) / (x2 - x1));
        }

        static double XX(int I)
        {
            return x1 + (I - I1) * (x2 - x1) / (I2 - I1);
        }

        int JJ(double y)
        {
            return J2 + (int)((y - y1) * (J1 - J2) / (y2 - y1));
        }

        static double YY(int J)
        {
            return y1 + (J - J2) * (y2 - y1) / (J1 - J2);
        }

        int II2(double x) // чтобы OY была видна
        {
            if ((I1 + (int)((x - x1) * (I2 - I1) / (x2 - x1))) < 15) return 15;
            else if ((I1 + (int)((x - x1) * (I2 - I1) / (x2 - x1))) > 1015) return 985;
            else return I1 + (int)((x - x1) * (I2 - I1) / (x2 - x1));
        }

        int JJ2(double y) // чтобы OX была видна
        {
            if ((J2 + (int)((y - y1) * (J1 - J2) / (y2 - y1)))<15) return 15;
            else if ((J2 + (int)((y - y1) * (J1 - J2) / (y2 - y1)))>950) return 950;
            else return J2 + (int)((y - y1) * (J1 - J2) / (y2 - y1));
        }

        public Form1()
        {
            InitializeComponent();
            I2 = ClientSize.Width;
            J2 = ClientSize.Height;
            MyPen = new Pen(Brushes.Black, 2);
            MyPen2 = new Pen(Brushes.Red, 2);
            g = this.CreateGraphics();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            string s = textBox1.Text;
            parser = new TParser();
            try
            {
                parser.SetOperator(ref s, out parser.topOp);
                MyDraw(II, JJ, g);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private double F(double x)
        {
            TParser.aVar[0].value = x;
            parser.topOp.Run_Formula(); // вычисление переменных
            return TParser.aVar[1].value;
        }

        private bool F2(double x, double x2)
        {
            TParser.aVar[0].value = x;
            bool k1 = Convert.ToBoolean(parser.topOp.top.Value);
            TParser.aVar[0].value = x2;
            bool k2 = Convert.ToBoolean(parser.topOp.top.Value);
            if (k1 == k2) return true;
            else return false;
        }

        double HH(double a1, double a2) // для масштабирвоания
        {
            double Result = 1;
            while (Math.Abs(a2 - a1) / Result < 1)
                Result /= 10.0;
            while (Math.Abs(a2 - a1) / Result >= 10)
                Result *= 10.0;
            if (Math.Abs(a2 - a1) / Result < 2.0)
                Result /= 5.0;
            if (Math.Abs(a2 - a1) / Result < 5.0)
                Result /= 2.0;
            return Result;
        }

        byte GetDigits(double dx) // для масштабирования
        {
            byte Result;
            if (dx >= 5) Result = 0;
            else
                if (dx >= 0.5) Result = 1;
                else
                    if (dx >= 0.05) Result = 2;
                    else
                        if (dx >= 0.005) Result = 3;
                        else
                            if (dx >= 0.0005) Result = 4; else Result = 5;
            return Result;
        }

        Font aFont;

        void OX(IJ II, IJ JJ, Graphics g) // построение оси OX
        {
            g.DrawLine(Pens.LightBlue, II(x1), JJ2(0), II(x2), JJ2(0));
            double h1 = HH(x1, x2);
            int k1 = (int)Math.Round(x1 / h1) - 1;
            int k2 = (int)Math.Round(x2 / h1);
            byte Digits = GetDigits(Math.Abs(x2 - x1));
            aFont = new Font("Arial", 14, FontStyle.Bold);
            for (int i = k1; i <= k2; i++)
            {
                g.DrawLine(MyPen, II(i * h1), JJ2(0) - 7, II(i * h1), JJ2(0) + 7);
                string s = Convert.ToString(Math.Round(h1 * i, Digits));
                g.DrawString(s, aFont, Brushes.Black, II(i * h1), JJ2(0) - 19);
            }
        }

        void OY(IJ II, IJ JJ, Graphics g) // построение оси OY
        {
            g.DrawLine(Pens.LightBlue, II2(0), JJ(y1), II2(0), JJ(y2));
            double h1 = HH(y1, y2); int k1 = (int)Math.Round(y1 / h1) - 1;
            int k2 = (int)Math.Round(y2 / h1);
            int Digits = GetDigits(Math.Abs(y2 - y1));
            aFont = new Font("Arial", 14, FontStyle.Bold);
            for (int i = k1; i <= k2; i++)
            {
                g.DrawLine(MyPen, II2(0) - 7, JJ(i * h1), II2(0) + 7, JJ(i * h1));
                string s = Convert.ToString(Math.Round(h1 * i, Digits));
                g.DrawString(s, aFont, Brushes.Black, II2(0), JJ(i * h1) - 2);
            }
        }

        public void MyDraw(IJ II, IJ JJ, Graphics g)
        {
            if (g != null)
            {
                g.Clear(Color.White);
                aFont = new Font("Arial", 10, FontStyle.Bold);
                OX(II, JJ, g); OY(II, JJ, g);
                aFont.Dispose();
                if (parser.topOp != null)
                {
                    // сам график
                    double h = (x2 - x1) / n;
                    for (int i = 1; i < n; i++)
                        if (Math.Abs(F(x1 + (i - 1) * h) - F(x1 + (i - 0) * h)) < 10)
                        {
                            if ((F2(x1 + (i - 1) * h, x1 + i * h) || (Math.Abs(JJ(F(x1 + i * h)) - JJ(F(x1 + (i - 1) * h))) < 10)))
                                g.DrawLine(MyPen, II(x1 + (i - 1) * h), JJ(F(x1 + (i - 1) * h)),
                                                       II(x1 + i * h), JJ(F(x1 + i * h)));
                        }
                    // производная(центральная разность)
                    if (fFunc == 0)
                    {
                        h = (x2 - x1) / n;
                        double oldx, oldyt;
                        double x = x1;
                        double yt = (F(x + h) - F(x - h)) / (2 * h); // центральная разность
                        for (int i = 2; i <= n; i++)
                        {
                            oldx = x;
                            oldyt = yt;
                            x += h;
                            yt = (F(x + h) - F(x - h)) / (2 * h); // центральная разность
                            if ((F2(x - h, x + h)) || (Math.Abs(JJ(yt) - JJ(oldyt)) < 10))
                            {
                                if (Math.Abs(yt - oldyt) < 10)
                                {
                                    g.DrawLine(MyPen2, II(oldx) - 2, JJ(oldyt) - 2, II(x) - 2, JJ(yt) - 2);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            MyDraw(II, JJ, g);
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (drawing)
            {
                double dx = XX(e.X) - XX(e0.X);
                double dy = YY(e.Y) - YY(e0.Y);
                e0 = e;
                x1 -= dx; y1 -= dy; x2 -= dx; y2 -= dy;
                MyDraw(II, JJ, g);
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            I2 = Size.Width - 17; J2 = Size.Height - 40;
            MyDraw(II, JJ, g);
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            drawing = true;
            e0 = e;
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            drawing = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            MouseWheel += new MouseEventHandler(Form1_MouseWheel);
            string s = textBox1.Text;
            parser = new TParser();
            try
            {
                parser.SetOperator(ref s, out parser.topOp);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void Form1_MouseWheel(object sender, MouseEventArgs e) // для колеса мыши
        {
            double x = XX(e.X);
            double y = YY(e.Y);
            if (e.Delta < 0)
                size = 1.03;
            else
                size = 0.97;
            x1 = x - (x - x1) * size;
            x2 = x + (x2 - x) * size;
            y1 = y - (y - y1) * size;
            y2 = y + (y2 - y) * size;
            MyDraw(II, JJ, g);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            x1 += 0.1;
            x2 += 0.1;
            MyDraw(II, JJ, g);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            y1 -= 0.1;
            y2 -= 0.1;
            MyDraw(II, JJ, g);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            x1 -= 0.1; 
            x2 -= 0.1;
            MyDraw(II, JJ, g);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            y1 += 0.1; 
            y2 += 0.1;
            MyDraw(II, JJ, g);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            x1 += 0.1;
            x2 -= 0.1;
            MyDraw(II, JJ, g);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            y1 += 0.1;
            y2 -= 0.1;
            MyDraw(II, JJ, g);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            x1 -= 0.1;
            x2 += 0.1;
            MyDraw(II, JJ, g);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            y1 -= 0.1;
            y2 += 0.1;
            MyDraw(II, JJ, g);
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            fFunc = 0;
            MyDraw(II, JJ, g);
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            fFunc = 1;
            MyDraw(II, JJ, g);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            n = 150;
            MyDraw(II, JJ, g);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            n = 250;
            MyDraw(II, JJ, g);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            n = 350;
            MyDraw(II, JJ, g);
        }
    }
}

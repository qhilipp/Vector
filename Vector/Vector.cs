using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;
using SkiaSharp;
using Xamarin.Forms;

namespace Vector {
    public class Vector {

        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        private Color color = Manager.InvertColor;
        public Color Color {
            get { return color; }
            set { color = value; }
        }
        public Vector() { }
        public Vector(double x, double y) {
            X = x;
            Y = y;
        }
        public Vector(double x, double y, double z) {
            X = x;
            Y = y;
            Z = z;
        }
        public Vector(Color color) {
            Color = color;
        }
        public Vector(double x, double y, Color color) {
            X = x;
            Y = y;
            Color = color;
        }
        public Vector(double x, double y, double z, Color color) {
            X = x;
            Y = y;
            Z = z;
            Color = color;
        }


        private static int r = 1, g = 2, b = 3;
        int c = 0;
        private static int colorDiff = 63;
        public Vector ToColored() {
            Vector v = new Vector(X, Y, Z);
            v.Color = Color.FromRgb(r * colorDiff, g * colorDiff, b * colorDiff);
            if (c == 0) r++;
            else if (c == 1) g++;
            else if (c == 2) b++;
            else c = 0;
            if (r > 255 / colorDiff) r = 1;
            else if (g > 255 / colorDiff) g = 1;
            else if (b > 255 / colorDiff) b = 1;
            c++;
            return v;
        }

        public void Colorize() {
            Color = ToColored().Color;
        }

        public double[,] ToMatrix() {
            double[,] matriX = new double[1, 3];
            matriX[0, 0] = X;
            matriX[0, 1] = Y;
            matriX[0, 2] = Z;
            return matriX;
        }

        public static Vector FromMatrix(double[,] matriX) {
            Vector v = new Vector(matriX[0, 0], matriX[1, 0], matriX[2, 0]);
            return v;
        }

        public static Vector Add(Vector[] vectors) {
            Vector v = new Vector();
            foreach (Vector v0 in vectors) {
                v.X += v0.X;
                v.Y += v0.Y;
                v.Z += v0.Z;
            }
            return v;
        }

        public void Subtract(Vector a) {
            X -= a.X;
            Y -= a.Y;
            Z -= a.Z;
        }

        public static Vector Multiply(Vector v, double f) {
            return new Vector(v.X * f, v.Y * f, v.Z * f);
        }

        public OnLineFeedback OnLine(Line line) {
            double factorX = (X - line.Origin.X) / line.Direction.X;
            double factorY = (Y - line.Origin.Y) / line.Direction.Y;
            double factorZ = (Z - line.Origin.Z) / line.Direction.Z;
            return new OnLineFeedback(factorX == factorY && factorY == factorZ, factorX);
        }

        public bool OnPlane(Function f) {
            return f.F(X, Z) == Y;
        }

        public override string ToString() {
            return X + " " + Y + " " + Z;
        }

        public SKPoint ToSKPoint() {
            return new SKPoint((float)X, (float)Y);
        }

        public void Move(double x, double y) {
            X += x;
            Y += y;
        }

        public static double Dist(Vector a, Vector b) {
            return Math.Sqrt(Math.Pow(a.X-b.X, 2) + Math.Pow(a.Y - b.Y, 2) + Math.Pow(a.Z - b.Z, 2));
        }

        public static Vector CrossProduct(Vector a, Vector b) {
            return new Vector(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X - b.Z, a.X * b.Y - a.Y * b.X);
        }

        public override bool Equals(object obj) {
            if (obj.GetType() != typeof(Vector)) return false;
            Vector v = (Vector)obj;
            return v.X == X && v.Y == Y && v.Z == Z;
        }

    }

    public class OnLineFeedback {
        public double At;
        public bool OnLine;
        public OnLineFeedback(bool onLine, double at) {
            OnLine = onLine;
            At = at;
        }
    }

}

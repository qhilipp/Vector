using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Vector {
    public class Plane {

        public Vector Position;
        public Vector U, V;
        public Color Color = Color.FromRgba(0, .5, 1, .5);

        public Vector GetPoint(double r, double s) {
            return new Vector((double)Position.X + (double)r * (double)U.X + s * (double)V.X, (double)Position.Y + r * (double)U.Y + s * (double)V.Y, (double)Position.Z + r * (double)U.Z + s * (double)V.Z);
        }

        public string ToTerm() {
            return "x^2";
        }

        public double FromXZ(double x, double z) {
            LES les = new LES();
            Equation e1 = new Equation() { result = x - Position.X, names = { "l", "m" }, vals = { U.X, V.X } };
            Equation e2 = new Equation() { result = z - Position.Z, names = { "l", "m" }, vals = { U.Z, V.Z } };
            les.Solve(new List<Equation>() { e1, e2 });
            return (double)GetPoint((double) les.Results[1], (double) les.Results[0]).Y;
        }

        public double FromXY(double x, double y) {
            LES les = new LES();
            Equation e1 = new Equation() { result = x - Position.X, names = { "l", "m" }, vals = { (double)U.X, (double)V.X } };
            Equation e2 = new Equation() { result = y - Position.Y, names = { "l", "m" }, vals = { (double)U.Y, (double)V.Y } };
            les.Solve(new List<Equation>() { e1, e2 });
            return (double)GetPoint((double)les.Results[1], (double)les.Results[0]).Z;
        }

        public double FromYZ(double y, double z) {
            LES les = new LES();
            Equation e1 = new Equation() { result = y - (double)Position.Y, names = { "l", "m" }, vals = { (double)U.Y, (double)V.Y } };
            Equation e2 = new Equation() { result = z - (double)Position.Z, names = { "l", "m" }, vals = { (double)U.Z, (double)V.Z } };
            les.Solve(new List<Equation>() { e1, e2 });
            return (double)GetPoint((double)les.Results[1], (double)les.Results[0]).X;
        }

    }
}

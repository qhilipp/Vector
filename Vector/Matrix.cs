using System;
using System.Collections.Generic;
using System.Text;

namespace Vector {
    public class Matrix {

        private static double[,] rotationX = new double[3, 3];
        private static double[,] rotationY = new double[3, 3];
        private static double[,] rotationZ = new double[3, 3];

        public static double[,] multiply(double[,] a, double[,] b) {
            if (a.GetLength(0) != b.GetLength(1)) {
                return null;
            }
            double[,] m = new double[a.GetLength(1), b.GetLength(0)];
            for (int y = 0; y < m.GetLength(1); y++) {
                for (int x = 0; x < m.GetLength(0); x++) {
                    double sum = 0;
                    for (int i = 0; i < a.GetLength(0); i++) {
                        sum += a[i, x] * b[y, i];
                    }
                    m[x, y] = sum;
                }
            }
            return m;
        }

        public static double[,] transform(double[,] m) {
            double[,] n = new double[m.GetLength(1), m.GetLength(0)];
            for (int y = 0; y < m.GetLength(1); y++) {
                for (int x = 0; x < m.GetLength(0); x++) {
                    n[y, y] = m[x, y];
                }
            }
            return n;
        }

        public static double[,] rotateX(double[,] matrix, double angle) {
            updateXRotation(angle);
            return multiply(rotationX, matrix);
        }

        public static double[,] rotateY(double[,] matrix, double angle) {
            updateYRotation(angle);
            return multiply(rotationY, matrix);
        }

        public static double[,] rotateZ(double[,] matrix, double angle) {
            updateZRotation(angle);
            return multiply(rotationZ, matrix);
        }

        private static void updateXRotation(double angle) {
            rotationX[0, 0] = 1;
            rotationX[0, 1] = 0;
            rotationX[0, 2] = 0;
            rotationX[1, 0] = 0;
            rotationX[1, 1] = Math.Cos(angle);
            rotationX[1, 2] = -Math.Sin(angle);
            rotationX[2, 0] = 0;
            rotationX[2, 1] = Math.Sin(angle);
            rotationX[2, 2] = Math.Cos(angle);
        }

        private static void updateYRotation(double angle) {
            rotationY[0, 0] = Math.Cos(angle);
            rotationY[0, 1] = 0;
            rotationY[0, 2] = -Math.Sin(angle);
            rotationY[1, 0] = 0;
            rotationY[1, 1] = 1;
            rotationY[1, 2] = 0;
            rotationY[2, 0] = Math.Sin(angle);
            rotationY[2, 1] = 0;
            rotationY[2, 2] = Math.Cos(angle);
        }

        private static void updateZRotation(double angle) {
            rotationZ[0, 0] = Math.Cos(angle);
            rotationZ[0, 1] = Math.Sin(angle);
            rotationZ[0, 2] = 0;
            rotationZ[1, 0] = -Math.Sin(angle);
            rotationZ[1, 1] = Math.Cos(angle);
            rotationZ[1, 2] = 0;
            rotationZ[2, 0] = 0;
            rotationZ[2, 1] = 0;
            rotationZ[2, 2] = 1;
        }

    }
}

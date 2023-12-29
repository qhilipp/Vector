using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;

namespace Vector {
	public partial class Viewer : SKCanvasView {

        public List<Line> grid = new List<Line>();
        public List<Vector> vectors = new List<Vector>();
        public List<Line> lines = new List<Line>();
        public List<Function> functions = new List<Function>();
        public List<Plane> planes = new List<Plane>();
        public List<Sculpture> sculptures = new List<Sculpture>();
        public List<List<Vector>> sculpturePoints = new List<List<Vector>>();
        public List<Poly> polies = new List<Poly>();
        public List<Vector> points = new List<Vector>();

        private List<Part> parts = new List<Part>();

        public double functionscale = .5;

        public double size = 10;
        public double axisScale = 5;

        public double lineDetail = 5;

        private double startX = 0, startY = 0;
        private double oldX = 0, oldY = 0;

        public delegate void OpenDelegate(bool open);
        public OpenDelegate Open;

        public Viewer () {
            InitializeComponent();
            EnableTouchEvents = true;
            Touch += Touched;
            UpdateGrid();
            //polies.Add(new Poly() { Points = new List<Vector>() { new Vector(0,0,0), new Vector(1,2,3), new Vector(5, 0, -3)} });
            Manager.VariableChange += () => {
                vectors.Clear();
                foreach (var v in Manager.variables) {
                    if (v.Value.GetType() == typeof(Vector)) vectors.Add((Vector)v.Value);
                }
                lines.Clear();
                foreach (var v in Manager.variables) {
                    if (v.Value.GetType() == typeof(Line)) lines.Add((Line)v.Value);
                }
                functions.Clear();
                foreach (var v in Manager.variables) {
                    if (v.Value.GetType() == typeof(Function)) functions.Add((Function)v.Value);
                }
                planes.Clear();
                foreach (var v in Manager.variables) {
                    if (v.Value.GetType() == typeof(Plane)) planes.Add((Plane)v.Value);
                }
                polies.Clear();
                foreach (var v in Manager.variables) {
                    if (v.Value.GetType() == typeof(Poly)) polies.Add((Poly)v.Value);
                }
                sculptures.Clear();
                sculpturePoints.Clear();
                foreach (var v in Manager.variables) {
                    if (v.Value.GetType() == typeof(Sculpture)) {
                        sculptures.Add((Sculpture)v.Value);
                        sculptures[sculptures.Count - 1].Calculate();
                        sculpturePoints.Add(sculptures[sculptures.Count - 1].points);
                    }
                }
                if (size != (double)Manager.variables["size"]) {
                    size = (double)Manager.variables["size"];
                    axisScale = size / 2;
                    functionscale = size / 20;
                    UpdateGrid();
                }
            };
            Device.StartTimer(TimeSpan.FromMilliseconds(1), () => {
                InvalidateSurface();
                return true;
            });
        }

        private void UpdateGrid() {
            grid.Clear();
            for (double i = axisScale; i <= size; i += axisScale) {
                grid.Add(new Line(new Vector(0, 0, i, Color.FromHex("#202020")), new Vector(1, 0, 0)));
                grid.Add(new Line(new Vector(i, 0, 0, Color.FromHex("#202020")), new Vector(0, 1, 0)));
                grid.Add(new Line(new Vector(0, i, 0, Color.FromHex("#202020")), new Vector(0, 0, 1)));
                grid.Add(new Line(new Vector(0, i, 0, Color.FromHex("#202020")), new Vector(1, 0, 0)));
                grid.Add(new Line(new Vector(0, 0, i, Color.FromHex("#202020")), new Vector(0, 1, 0)));
                grid.Add(new Line(new Vector(i, 0, 0, Color.FromHex("#202020")), new Vector(0, 0, 1)));

                grid.Add(new Line(new Vector(0, 0, -i, Color.FromHex("#202020")), new Vector(1, 0, 0)));
                grid.Add(new Line(new Vector(-i, 0, 0, Color.FromHex("#202020")), new Vector(0, 1, 0)));
                grid.Add(new Line(new Vector(0, -i, 0, Color.FromHex("#202020")), new Vector(0, 0, 1)));
                grid.Add(new Line(new Vector(0, -i, 0, Color.FromHex("#202020")), new Vector(1, 0, 0)));
                grid.Add(new Line(new Vector(0, 0, -i, Color.FromHex("#202020")), new Vector(0, 1, 0)));
                grid.Add(new Line(new Vector(-i, 0, 0, Color.FromHex("#202020")), new Vector(0, 0, 1)));
            }
            grid.Add(new Line(new Vector(0, 0, 0, Color.Red), new Vector(1, 0, 0)));
            grid.Add(new Line(new Vector(0, 0, 0, Color.Green), new Vector(0, 1, 0)));
            grid.Add(new Line(new Vector(0, 0, 0, Color.Blue), new Vector(0, 0, 1)));
        }

        void Touched(object sender, SKTouchEventArgs e) {
            if (e.ActionType == SKTouchAction.Pressed) {
                for (int i = 0; i < sliders.Count; i++) {
                    double y = Height - (i + 1) * sliderSize * 2;
                    if (e.Location.X > Width - sliderWidth - sliderSize + sliders[i].State * sliderWidth - sliderSize / 2
                        && e.Location.Y > y - sliderSize
                        && e.Location.X < Width - sliderWidth + sliderSize + sliders[i].State * sliderWidth
                        && e.Location.Y < y + sliderSize / 2) sliders[i].Dragging = true;
                }
                startX = e.Location.X;
                startY = e.Location.Y;
            }
            else if (e.ActionType == SKTouchAction.Moved) {
                bool dragging = false;
                foreach(SliderProperty s in sliders) {
                    if(s.Dragging) {
                        double x = e.Location.X;
                        double min = Width - sliderMargin - sliderWidth;
                        double max = Width - sliderMargin;
                        s.State = Math.Max(0, Math.Min((x - min) / (max - min), 1));
                        dragging = true;
                        Manager.variables[s.Name] = s.GetValue();
                        Manager.editor.interpreter.Compile(Manager.editor.Code.Split("\n".ToCharArray()), new CompilerException() { IgnoreNumbers = true });
                        break;
                    }
                }
                if(!dragging) {
                    Manager.variables["rotationX"] = -(e.Location.Y - startY) / 150.0 + oldX;
                    Manager.variables["rotationY"] = (e.Location.X - startX) / 150.0 + oldY;
                    if ((double) Manager.variables["rotationX"] < -Math.PI / 2) Manager.variables["rotationX"] = -Math.PI / 2;
                    if ((double) Manager.variables["rotationX"] > Math.PI / 2) Manager.variables["rotationX"] = Math.PI / 2;
                }
            }
            else if (e.ActionType == SKTouchAction.Released) {
                foreach (SliderProperty s in sliders) s.Dragging = false;
                oldX = (double) Manager.variables["rotationX"];
                oldY = (double) Manager.variables["rotationY"];
            }
            e.Handled = true;
        }

        private List<Vector> To2DVectors(List<Vector> v3d) {
            List<Vector> v2d = new List<Vector>();
            for (int i = 0; i < v3d.Count; i++) {
                Vector v = new Vector(v3d[i].X * (double) Manager.variables["scale"], v3d[i].Y * (double) Manager.variables["scale"], v3d[i].Z * (double) Manager.variables["scale"]);
                v = Vector.FromMatrix(Matrix.rotateY(v.ToMatrix(), (double) Manager.variables["rotationY"]));
                v = Vector.FromMatrix(Matrix.rotateX(v.ToMatrix(), (double) Manager.variables["rotationX"]));
                v = Vector.FromMatrix(Matrix.rotateZ(v.ToMatrix(), (double) Manager.variables["rotationZ"]));
                v.X /= (-v.Z / 10 + size) / 150;
                v.Y /= (-v.Z / 10 + size) / -150;
                v.Color = v3d[i].Color;
                v.Move(Width / 2, Height / 2);
                v2d.Add(v);
            }
            return v2d;
        }

        private void Draw(object sender, SKPaintSurfaceEventArgs e) {
            Manager.variables["rotationX"] = (double)Manager.variables["rotationX"] + (double)Manager.variables["spinX"];
            Manager.variables["rotationY"] = (double)Manager.variables["rotationY"] + (double)Manager.variables["spinY"];
            Manager.variables["rotationZ"] = (double)Manager.variables["rotationZ"] + (double)Manager.variables["spinZ"];
            SKCanvas c = e.Surface.Canvas;
            c.Clear(Manager.BaseColor.ToSKColor());
            points.Clear();
            parts.Clear();
            DrawGrid(c);
            DrawFunctions();
            DrawPlanes();
            DrawLines(lines);
            DrawVectors(c);
            DrawSculptures();
            DrawPolies();
            parts = parts.OrderBy((part) => part.Z).ToList();
            foreach(Part part in parts) {
                c.DrawPath(part.Path, new SKPaint() { Color = part.Color, IsStroke = part.IsStroke, IsAntialias = true });
            }
            DrawPoints(c);
            DrawSliders(c);
        }

        private void AddPart(Color color, double z, bool isStroke, params SKPoint[] points) {
            SKPath path = new SKPath();
            path.AddPoly(points);
            parts.Add(new Part() { Path = path, Color = color.ToSKColor(), Z = z, IsStroke = isStroke });
        }

        private void DrawGrid(SKCanvas c) {
            if (!(bool)Manager.variables["showGrid"]) return;
            for(double i = -size; i <= size; i+=axisScale) {
                List<Vector> p = To2DVectors(new List<Vector>() { new Vector(i, 0, 0), new Vector(0, i, 0), new Vector(0, 0, i) });
                c.DrawText(i + "", p[0].ToSKPoint(), new SKPaint() { TextSize = (float)(10 * (double)Manager.variables["scale"]), Color = Color.Red.ToSKColor() });
                c.DrawText(i + "", p[1].ToSKPoint(), new SKPaint() { TextSize = (float)(10 * (double)Manager.variables["scale"]), Color = Color.Green.ToSKColor() });
                c.DrawText(i + "", p[2].ToSKPoint(), new SKPaint() { TextSize = (float)(10 * (double)Manager.variables["scale"]), Color = Color.Blue.ToSKColor() });
            }
            DrawLines(grid);
        }

        private void DrawPlanes() {
            foreach(Plane p in planes) {
                List<Vector> points = new List<Vector>();
                Point[] combinations = { new Point(size, size), new Point(size, -size), new Point(-size, -size), new Point(-size, size) };
                foreach(Point com in combinations) {
                    double y = p.FromXZ(com.X, com.Y);
                    this.points.Add(new Vector(com.X, y, com.Y));
                }
                //points = To2DVectors(points);
                //SKPath path = new SKPath();
                //SKPoint[] points2D = new SKPoint[points.Count];
                //for(int i = 0; i < points.Count; i++) {
                //    points2D[i] = new SKPoint((float) points[i].X, (float) points[i].Y);
                //}
                //path.AddPoly(points2D, true);
                //Color color = Color.FromRgba(p.Color.R, p.Color.G, p.Color.B, .25);
                //AddPart(color, points[0].Z, false, points2D);
            }
        }

        private void DrawFunctions() {
            foreach (Function plane in functions) {
                List<Vector> pos3D = new List<Vector>();
                for (double x = -size; x <= size; x += functionscale) {
                    for (double z = -size; z <= size; z += functionscale) {
                        pos3D.Add(new Vector(x, plane.F(x, z), z));
                    }
                }
                List<Vector> pos = To2DVectors(pos3D);
                int s = (int)Math.Sqrt(pos.Count);
                for (int i = 0; i < pos.Count - s - 1; i++) {
                    if ((i + 1) % s == 0) continue;
                    if (pos3D[i].Y > size && pos3D[i + 1].Y > size && pos3D[i + s + 1].Y > size && pos3D[i + s].Y > size) continue;
                    if (pos3D[i].Y < -size && pos3D[i + 1].Y < -size && pos3D[i + s + 1].Y < -size && pos3D[i + s].Y < -size) continue;
                    SKPoint[] poly = { pos[i].ToSKPoint(), pos[i + 1].ToSKPoint(), pos[i + s + 1].ToSKPoint(), pos[i + s].ToSKPoint() };
                    Color color = plane.Color;
                    if(plane.Color == Color.Transparent) {
                        double avg = (pos3D[i].Y + pos3D[i + 1].Y + pos3D[i + s + 1].Y + pos3D[i + s].Y) / 4;
                        color = Color.FromHsla((avg + size) / (2 * size), 1, .5);
                    }
                    AddPart(color, pos[i].Z, false, poly);
                }
            }
        }

        private Vector[] GetIntervals(Vector first, Vector second, int amount) {
            Vector[] points = new Vector[amount];
            double x = (first.X - second.X) / (amount - 1);
            double y = (first.Y - second.Y) / (amount - 1);
            double z = (first.Z - second.Z) / (amount - 1);
            for(int i = 0; i < amount; i++) {
                points[i] = new Vector(second.X + x * i, second.Y + y * i, second.Z + z * i);
            }
            return points;
        }

        private void DrawLines(List<Line> lines) {
            List<Vector> linesP1 = new List<Vector>();
            List<Vector> linesP2 = new List<Vector>();
            for (int i = 0; i < lines.Count; i++) {
                double factorXPos = Math.Abs((size - lines[i].Origin.X) / lines[i].Direction.X);
                double factorYPos = Math.Abs((size - lines[i].Origin.Y) / lines[i].Direction.Y);
                double factorZPos = Math.Abs((size - lines[i].Origin.Z) / lines[i].Direction.Z);
                if (double.IsNaN(factorXPos)) factorXPos = double.PositiveInfinity;
                if (double.IsNaN(factorYPos)) factorYPos = double.PositiveInfinity;
                if (double.IsNaN(factorZPos)) factorZPos = double.PositiveInfinity;
                double factorPos = Math.Min(factorXPos, Math.Min(factorYPos, factorZPos));

                double factorXNeg = -Math.Abs((-size - lines[i].Origin.X) / lines[i].Direction.X);
                double factorYNeg = -Math.Abs((-size - lines[i].Origin.Y) / lines[i].Direction.Y);
                double factorZNeg = -Math.Abs((-size - lines[i].Origin.Z) / lines[i].Direction.Z);
                if (double.IsNaN(factorXNeg)) factorXNeg = double.NegativeInfinity;
                if (double.IsNaN(factorYNeg)) factorYNeg = double.NegativeInfinity;
                if (double.IsNaN(factorZNeg)) factorZNeg = double.NegativeInfinity;
                double factorNeg = Math.Max(factorXNeg, Math.Max(factorYNeg, factorZNeg));

                Vector[] p1 = { lines[i].Origin, Vector.Multiply(lines[i].Direction, factorNeg) };
                linesP1.Add(Vector.Add(p1));
                Vector[] p2 = { lines[i].Origin, Vector.Multiply(lines[i].Direction, factorPos) };
                linesP2.Add(Vector.Add(p2));
            }
            linesP1 = To2DVectors(linesP1);
            linesP2 = To2DVectors(linesP2);
            for (int i = 0; i < linesP1.Count; i++) {
                Vector[] points = GetIntervals(linesP1[i], linesP2[i], 20);
                for(int j = 0; j < points.Length - 1; j++) {
                    AddPart(lines[i].Origin.Color, points[j].Z, true, points[j].ToSKPoint(), points[j + 1].ToSKPoint());
                }

            }
            for (int i = 0; i < lines.Count; i++) {
                for(int j = 0; j < lines.Count; j++) {
                    if (i == j) continue;
                    Vector v = lines[i].Intersects(lines[j]);
                    if (v != null) {
                        points.Add(v);
                    }
                }
            }
        }

        private void DrawPolies() {
            foreach(Poly poly in polies) {
                SKPoint[] points = new SKPoint[poly.Points.Count];
                List<Vector> vectors2D = To2DVectors(poly.Points);
                for(int i = 0; i < points.Length; i++) {
                    points[i] = vectors2D[i].ToSKPoint();
                }
                AddPart(Color.Red, 0, false, points);
            }
        }

        private void DrawVectors(SKCanvas c) {
            List<Vector> vectors2D = To2DVectors(vectors);
            for (int i = 0; i < vectors2D.Count; i++) {
                SKPaint paint = new SKPaint(new SKFont(SKTypeface.Default, size: (float)(10 * (double) Manager.variables["scale"]))) { Color = vectors2D[i].Color.ToSKColor(), IsAntialias = true };
                float s = (float) ((double) Manager.variables["scale"]) * 3;
                float l = (float) ((double) Manager.variables["scale"] * 5);
                float w = l / 3f;
                c.DrawText(vectors[i].ToString(), (int)vectors2D[i].X + s, (int)vectors2D[i].Y, paint);
                double slope = (vectors2D[i].Y - Height / 2) / (vectors2D[i].X - Width / 2);
                double coslope = -(vectors2D[i].X - Width / 2) / (vectors2D[i].Y - Height / 2);
                if (vectors2D[i].X - Width / 2 < 0) l = -l;
                Vector p = new Vector(vectors2D[i].X - Math.Cos(Math.Atan(slope)) * l, vectors2D[i].Y - Math.Sin(Math.Atan(slope)) * l);
                SKPoint[] points = {
                    new SKPoint((float)(p.X - Math.Cos(Math.Atan(coslope)) * w), (float)(p.Y - Math.Sin(Math.Atan(coslope)) * w)),
                    new SKPoint((float)(p.X + Math.Cos(Math.Atan(coslope)) * w), (float)(p.Y + Math.Sin(Math.Atan(coslope)) * w)),
                    new SKPoint((float)vectors2D[i].X, (float)vectors2D[i].Y)
                };
                AddPart(vectors[i].Color, vectors[i].Z, false, points);
                List<Vector> lineSegments = To2DVectors(GetIntervals(new Vector(0, 0, 0), vectors[i], (int) size).ToList());
                for(int j = 0; j < lineSegments.Count - 1; j++) {
                    AddPart(vectors[i].Color, lineSegments[j].Z, true, lineSegments[j].ToSKPoint(), lineSegments[j + 1].ToSKPoint());
                }
            }
        }

        private void DrawSculptures() {
            for(int i = 0; i < sculptures.Count; i++) {
                List<Vector> points = To2DVectors(sculpturePoints[i]);
                for(int j = 0; j < points.Count - 1; j++) {
                    Color color = sculptures[i].Color;
                    if (color == Color.Transparent) {
                        color = Color.FromHsla((sculptures[i].points[j].Y + size) / size / 2, 1, 0.5);
                    }
                    AddPart(color, points[j].Z, true, points[j].ToSKPoint(), points[j + 1].ToSKPoint());
                }
            }
        }

        private void DrawPoints(SKCanvas c) {
            List<Vector> points2D = To2DVectors(points);
            float s = (float)(double)Manager.variables["scale"];
            for(int i = 0; i < points.Count; i++) {
                c.DrawOval((float)points2D[i].X, (float)points2D[i].Y, s, s, new SKPaint() { Color = points[i].Color.ToSKColor() });
                c.DrawText(points[i].ToString(), (float)points2D[i].X, (float)points2D[i].Y, new SKPaint() { TextSize = 30, Color = points[i].Color == Color.Transparent ? Color.Red.ToSKColor() : points[i].Color.ToSKColor() });
            }
        }

        private void DrawSliders(SKCanvas c) {
            for (int i = 0; i < sliders.Count; i++) {
                double x = (double) Manager.variables[sliders[i].Name];
                double min = sliders[i].Min;
                double max = sliders[i].Max;
                sliders[i].State = Math.Max(0, Math.Min((x - min) / (max - min), 1));
                double y = Height - (i + 1) * sliderSize * 2;
                c.DrawText(
                    sliders[i].Name,
                    (float)(Width - sliderMargin * 2 - sliderWidth),
                    (float)(y + sliderSize / 2),
                    new SKPaint() { Color = Manager.InvertColor.ToSKColor(), TextSize = 20, TextAlign = SKTextAlign.Right }
                ); ;
                c.DrawLine(
                    (float)(Width - sliderMargin - sliderWidth),
                    (float)y,
                    (float)(Width - sliderMargin),
                    (float)y,
                    new SKPaint() { Color = Manager.BackColor.ToSKColor() }
                );
                c.DrawOval(
                    (float)(Width - sliderWidth - sliderSize + sliders[i].State * sliderWidth),
                    (float)y,
                    (float)(sliderSize / 2),
                    (float)(sliderSize / 2),
                    new SKPaint() { Color = sliders[i].Dragging ? Manager.InvertColor.ToSKColor() : Manager.BackColor.ToSKColor() }
                );
                c.DrawText(Math.Round(sliders[i].GetValue(), 4) + "", (float)(Width - sliderWidth - sliderSize + sliders[i].State * sliderWidth), (float) (y + sliderSize), new SKPaint() { Color = Manager.InvertColor.ToSKColor(), TextSize = (float) (sliderSize / 2), TextAlign = SKTextAlign.Center});
            }
        }

        public List<SliderProperty> sliders = new List<SliderProperty>();
        double sliderWidth = 150;
        double sliderMargin = 20;
        double sliderSize = 20;

        public void AddSlider(string name, double min, double max) {
            for(int i = 0; i < sliders.Count; i++) {
                if (sliders[i].Name == name) return;
            }
            sliders.Add(new SliderProperty() { Name = name, Min = min, Max = max });
        }

        public class SliderProperty {
            public string Name;
            public double State;
            public double Min = 0, Max = 1;
            public bool Dragging = false;
            public double GetValue() {
                return (Max - Min) * State + Min;
            }
        }

        private class Part {
            public bool IsStroke = false;
            public SKColor Color;
            public double Z;
            public SKPath Path;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Vector {
    public class Line {

        public Vector Origin { get; set; }
        public Vector Direction { get; set; }

        public Line() { }

        public Line(Vector Origin, Vector Direction) {
            this.Origin = Origin;
            this.Direction = Direction;
        }

        public LineFeedback GetLineRelation(Line line) {
            if ((double)line.Direction.X / (double)Direction.X == (double)line.Direction.Y / (double)Direction.Y && (double)line.Direction.X / (double)Direction.X == (double)line.Direction.Z / (double)Direction.Z) {
                if((double)line.Origin.X / (double)Origin.X == (double)line.Origin.Y / (double)Origin.Y && (double)line.Origin.X / (double)Origin.X == (double)line.Origin.Z / (double)Origin.Z) {
                    return new LineFeedback(null, LineRelation.ParallelWithIntersection);
                }
                return new LineFeedback(null, LineRelation.Parallel);
            }
            Vector intersection = Intersects(line);
            if (intersection != null) return new LineFeedback(intersection, LineRelation.Intersection);
            return new LineFeedback(null, LineRelation.Crooked);
        }

        public Vector GetVectorAt(double lambda) {
            Vector[] v = { Origin, Vector.Multiply(Direction, lambda) };
            return Vector.Add(v);
        }

        public Vector Intersects(Line line) {
            LES les = new LES();
            Equation e1 = new Equation() { names = { "n", "n2" }, vals = { (double)Direction.X, (double)line.Direction.X}, result = (double)line.Origin.X - (double)Origin.X };
            Equation e2 = new Equation() { names = { "n", "n2" }, vals = { (double)Direction.Y, (double)line.Direction.Y }, result = (double)line.Origin.Y - (double)Origin.Y };
            try {
                les.Solve(new List<Equation> { e1, e2 });
                List<object> results = les.Results;
                return line.GetVectorAt(-(double) results[1]);
            } catch (Exception) {
                return null;
            }
        }

        public Vector IntersectsPlane(Plane plane) {
            LES les = new LES();
            Equation e1 = new Equation() { names = { "n", "n2", "n3" }, vals = { plane.U.X, plane.V.X, -Direction.X }, result = Origin.X + plane.Position.X };
            Equation e2 = new Equation() { names = { "n", "n2", "n3" }, vals = { plane.U.Y, plane.V.Y, -Direction.X }, result = Origin.Y + plane.Position.Y };
            Equation e3 = new Equation() { names = { "n", "n2", "n3" }, vals = { plane.U.Z, plane.V.Z, -Direction.X }, result = Origin.Z + plane.Position.Z };
            try {
                les.Solve(new List<Equation> { e1, e2, e3 });
                return GetVectorAt((double) les.Results[2]);
            } catch (Exception) {
                return null;
            }
        }

        public override string ToString() {
            return Origin.ToString() + " " + Direction.ToString();
        }

    }

    public class LineFeedback {
        public Vector Intersection { get; set; }
        public LineRelation LineRelation { get; set; }
        public LineFeedback(Vector i, LineRelation relation) {
            Intersection = i;
            LineRelation = relation;
        }

    }

    public enum LineRelation {
        Parallel,
        ParallelWithIntersection,
        Crooked,
        Intersection
    }

}

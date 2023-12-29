using System;
using System.Collections.Generic;
using System.Drawing;

namespace Vector {
    class LES {

        Dictionary<string, double> variables = new Dictionary<string, double>();
        public List<object> Results = new List<object>();
        public List<string> Msgs = new List<string>();

        public void Solve(List<Equation> equations) {
            Results.Clear();
            Step(equations, 0);
            bool[] oks = new bool[equations.Count];
            string msg = "";
            for (int i = 0; i < equations.Count; i++) {
                double res = 0;
                for (int j = 0; j < equations[i].vals.Count; j++) {
                    res += equations[i].vals[j] * variables[equations[i].names[j]];
                }
                Results.Add(variables[equations[0].names[i]]);
                Msgs.Add(equations[0].names[i] + " = " + Results[Results.Count - 1]);
                double remain = Math.Abs(res - equations[i].result);
                oks[i] = remain < 0.0000001;
                msg += oks[i] + " ";
            }
            Msgs.Add(msg);
            bool ok = true;
            foreach (bool o in oks)
                if (!o) {
                    ok = false;
                    break;
                }
            if(!ok) Msgs.Add("There is no solution :(");
        }

        void Step(List<Equation> equations, int solve) {
            if (equations.Count == 0) return;
            for(int i = equations.Count - 1; i >= 0; i--) {
                int count = 0;
                int index = 0;
                for(int j = 0; j < equations[i].vals.Count; j++) {
                    if(equations[i].vals[j] != 0) {
                        count++;
                        index = j;
                    }
                }  
                if(count == 1) {
                    variables[equations[i].names[index]] = equations[i].result / equations[i].vals[index];
                    if (equations.Count == 1) return;
                }
                if (count == 0 || count == 1) equations.RemoveAt(i);
            }
            if(equations.Count == 0) {
                throw new Exception("Not solveable!");
            }
            foreach (Equation e in equations) Print(e);
            List<Point> indexes = new List<Point>();
            for(int i = 0; i < equations.Count; i++) {
                for(int j = 0; j < equations.Count; j++) {
                    bool ok = true;
                    foreach(Point p in indexes) {
                        if ((p.X == i || p.Y == i) && (p.X == j || p.Y == j)) {
                            ok = false;
                            break;
                        }
                    }
                    if(i != j && ok) indexes.Add(new Point(j, i));
                }
            }
            if (equations.Count != 1) {
                List<Equation> newEquations = new List<Equation>();
                for (int i = 0; i < indexes.Count; i++) {
                    Equation e = Eliminate(equations[indexes[i].X], equations[indexes[i].Y]);
                    if (e != null) newEquations.Add(e);
                }
                Msgs.Add("-------------------");
                Step(newEquations, solve + 1);
            }
            foreach (Equation e in equations) {
                if (e.vals[solve] == 0) continue;
                Print(e);
                double result = e.result;
                for (int i = solve + 1; i < e.vals.Count; i++) {
                    result -= variables[e.names[i]] * e.vals[i];
                }
                result /= e.vals[solve];
                variables[e.names[solve]] = result;
                return;
            }
            throw new Exception("Not solveable! :(");
        }

        Equation Eliminate(Equation a, Equation b) {
            int index = 0;
            while (index < b.vals.Count && b.vals[index] == 0) index++;
            if (index >= b.vals.Count) return null;
            double factor = a.vals[index] / b.vals[index];
            Equation e = new Equation();
            for (int i = 0; i < a.vals.Count; i++) {
                e.name = a.name + " - " + factor + b.name;
                e.names.Add(a.names[i]);
                e.vals.Add(a.vals[i] - b.vals[i] * factor);
            }
            e.result = a.result - b.result * factor;
            return e;
        }

        void Print(Equation e) {
            Msgs.Add(e.ToString());
        }

    }
}

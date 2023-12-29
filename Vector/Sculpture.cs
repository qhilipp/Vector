using System;
using System.Collections.Generic;
using Xamarin.Forms;
using NCalc;

namespace Vector {
    public class Sculpture {

        public string XTerm { get; set; }
        public string YTerm { get; set; }
        public string ZTerm { get; set; }

        public double Start { get; set; }
        public double End { get; set; }

        public Color Color { get; set; }

        public double Detail { get; set; }

        public List<Vector> points = new List<Vector>();

        public Sculpture() {
            Detail = 0.005;
            Color = Color.Transparent;
        }

        public void Calculate() {
            points.Clear();
            for(double d = Start; d < End; d += Detail) {
                try {
                    Expression xExpression = new Expression(XTerm);
                    xExpression.EvaluateParameter += delegate (string name, ParameterArgs args) {
                        if (name == "t") {
                            args.Result = d; return;
                        }
                        args.Result = Manager.variables[name];
                    };

                    Expression yExpression = new Expression(YTerm);
                    yExpression.EvaluateParameter += delegate (string name, ParameterArgs args) {
                        if (name == "t") {
                            args.Result = d; return;
                        }
                        args.Result = Manager.variables[name];
                    };

                    Expression zExpression = new Expression(ZTerm);
                    zExpression.EvaluateParameter += delegate (string name, ParameterArgs args) {
                        if (name == "t") {
                            args.Result = d; return;
                        }
                        args.Result = Manager.variables[name];
                    };
                    object x = xExpression.Evaluate();
                    object y = yExpression.Evaluate();
                    object z = zExpression.Evaluate();
                    double xD = 0, yD = 0, zD = 0;
                    if (x.GetType() == typeof(int)) xD = (int)x;
                    else xD = (double)x;
                    if (y.GetType() == typeof(int)) yD = (int)y;
                    else yD = (double)y;
                    if (z.GetType() == typeof(int)) zD = (int)z;
                    else zD = (double)z;
                    points.Add(new Vector(xD, yD, zD));
                } catch (Exception e) {
                    Console.WriteLine(e.Message);
                }
            }
        }



    }
}

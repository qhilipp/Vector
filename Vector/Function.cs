using System;
using Xamarin.Forms;
using NCalc;

namespace Vector {
    public class Function {

        public string Term { get; set; }
        public Color Color { get; set; }

        public Function() {
            Color = Color.Transparent;
        }

        public double F(double x, double z) {
            try {
                Expression e = new Expression(Term);
                e.EvaluateParameter += delegate(string name, ParameterArgs args) {
                    args.Result = Manager.variables[name];
                };
                e.Parameters["x"] = x;
                e.Parameters["z"] = z;
                object res = e.Evaluate();
                if (res.GetType() == typeof(int)) return (int)res;
                else return (double)e.Evaluate();
            }catch(Exception) { return 0; }
        }

        public override string ToString() {
            return Term;
        }

    }
}
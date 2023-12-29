using System;
using System.Collections.Generic;

namespace Vector {
    public class Equation {
        public List<string> names = new List<string>();
        public List<double> vals = new List<double>();
        public double result;
        public string name;

        public override string ToString() {
            string msg = "";
            for (int i = 0; i < names.Count; i++) {
                string prtr = i < names.Count - 1 ? vals[i + 1] >= 0 ? " + " : " - " : " ";
                msg += Math.Abs(Math.Round(vals[i], 1)) + names[i] + prtr;
            }
            msg += "= " + Math.Round(result, 1);
            return msg;
        }

    }
}

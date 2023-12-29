using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;
using NCalc;
using Xamarin.Forms;

namespace Vector {
    public class Interpreter {

        private Viewer viewer;
        private ConsolePage console;

        public List<MethodInfo> functions;

        private string currentVariable = "";
        private bool strictMode = false;
        private bool debugMode = true;

        public Interpreter(Viewer viewer, ConsolePage console) {
            this.viewer = viewer;
            this.console = console;
            functions = (from f in GetType().GetMethods() where f.GetCustomAttributes<FunctionAttribute>().Count() > 0 select f).ToList();
        }

        public Feedback Compile(string[] lines, CompilerException exception = null) {          
            for(int i = 0; i < lines.Length; i++) {
                try {
                    Interpret(lines[i], exception);
                } catch (Exception e) {
                    return new Feedback($"Line {i+1}: " + e.Message, Feedback.ERROR);
                }
            }
            Manager.VariableChange();
            return new Feedback("Successfully compiled", Feedback.SUCCESS);
        }

        public void Interpret(string line, CompilerException exception = null) {
            line = line.Replace(" ", "");
            if (line.StartsWith("#")) return;
            bool declaration = false;
            int brackets = 0;
            foreach(char c in line) {
                if (c == '(') brackets++;
                if (c == ')') brackets--;
                if(c == '=' && brackets == 0) {
                    declaration = true;
                    break;
                }
            }
            if (declaration) {
                int firstEqual = line.IndexOf("=");
                if(line[firstEqual - 1] != '!' && line[firstEqual + 1] != '=' && line[firstEqual - 1] != '<' && line[firstEqual - 1] != '>') {
                    string name = line.Substring(0,firstEqual);
                    CheckValidName(name);
                    if(exception != null) {
                        if(Manager.variables.ContainsKey(name) && exception.IgnoreNumbers) {
                            try {
                                double d = (double)Manager.variables[name];
                                return;
                            } catch (Exception) { }
                        }
                        if (exception.Code == CompilerException.Ignore && exception.Name == name) return;
                        if(exception.Code == CompilerException.Remove && exception.Name == name) {
                            Manager.editor.ReplaceLine(line);
                        }
                        if(exception.Code == CompilerException.Replace && exception.Name == name) {
                            Manager.editor.ReplaceLine(line, exception.Args);
                        }
                    }
                    currentVariable = name;
                    string s = line.Substring(firstEqual + 1, line.Length - firstEqual - 1);
                    //Manager.SetVariable(name, GetValue(s));
                    Manager.variables[name] = GetValue(s);
                    currentVariable = null;
                    return;
                }
            } else {
                GetFunctionValue(line);
            }
        }

        private void CheckValidName(string name) {
            string nonValid = "\"()=,*";
            foreach(char c in nonValid) if (name.Contains(c)) throw new Exception($"The character '{c}' in the variable name \"{name}\" is not allowed");
        }

        private object GetFunctionValue(string call) {
            if (call == "") return null;
            List<MethodInfo> methods = new List<MethodInfo>();
            foreach (MethodInfo info in functions) {
                if (info.Name == call.Split('(')[0]) {
                    methods.Add(info);                  
                }
            }
            foreach(MethodInfo info in methods) {
                int first = call.IndexOf('(') + 1;
                int last = call.LastIndexOf(')');
                int brackets = 0;
                int lastIndex = first - 1;
                List<string> args = new List<string>();
                for (int i = first; i < last; i++) {
                    char current = call[i];
                    if (current == '(') brackets++;
                    else if (current == ')') brackets--;
                    if (brackets == 0 && current == ',') {
                        args.Add(call.Substring(lastIndex + 1, i - lastIndex - 1));
                        lastIndex = i;
                    }
                    if (brackets < 0) throw new Exception("Some brackets missing");
                }
                int l = last - lastIndex - 1;
                if (l > 0) args.Add(call.Substring(lastIndex + 1, l));
                object[] a = new object[args.Count];
                for (int i = 0; i < args.Count; i++) {
                    a[i] = GetValue(args[i]);
                    if ((a[i].GetType() == typeof(int) || a[i].GetType() == typeof(double)) && info.GetParameters()[i].ParameterType == typeof(string)) a[i] = a[i] + "";
                }
                if (a.Length > 0) {
                    bool oneParameterType = true;
                    Type t = a[0].GetType();
                    for (int i = 1; i < a.Length; i++) {
                        if (a[i].GetType() != t) {
                            oneParameterType = false;
                            break;
                        }
                    }
                    if (oneParameterType && info.GetParameters().Length == 1) {
                        Type temp = Array.CreateInstance(t, a.Length).GetType();
                        if (info.GetParameters()[0].ParameterType == temp) {
                            var v = Array.CreateInstance(t, a.Length);
                            for (int i = 0; i < a.Length; i++) v.SetValue(a[i], i);
                            a = new object[1];
                            a[0] = v;
                        }
                    }
                }
                try {
                    return info.Invoke(this, a);
                } catch (Exception e) {}
            }
            if (methods.Count > 0) throw new Exception($"No overload of {methods[0].Name} found that match the given arguments");
            if (strictMode && methods.Count == 0) throw new Exception($"No method found named {call.Split('(')[0]}");
            else return null;
        }

        public object GetValue(string arg) {
            if (arg.StartsWith("*") && arg.EndsWith("*")) return arg.Substring(1, arg.Length - 2);
            object temp;
            if (Manager.variables.TryGetValue(arg, out temp)) return Manager.variables[arg];
            bool canBeFunction = false;
            string possibleFunctionName = arg.Split('(')[0];
            foreach(var f in functions) {
                if(f.Name == possibleFunctionName) {
                    canBeFunction = true;
                    break;
                }
            }
            if(canBeFunction) temp = GetFunctionValue(arg);
            if(temp != null) {
                return temp;
            }
            try {
                double r = double.Parse(arg);
                return r;
            } catch (Exception) {}
            try {
                Expression e = new Expression(arg);
                e.EvaluateParameter += delegate (string name, ParameterArgs args) {
                    args.Result = Manager.variables[name];
                };
                object res = e.Evaluate();
                if (res.GetType() == typeof(bool)) return (bool) res;
                if (res.GetType() == typeof(double)) return (double)res;
                if (res.GetType() == typeof(int)) return (double)((int)res + 0.0);
                return arg;
            }catch(Exception) {
                return arg;
            }
        }

        [Function("Creates a color from a hex value")]
        public Color Color(string hex) {
            return Xamarin.Forms.Color.FromHex(hex);
        }

        [Function("Creates a color from a integer rbg values between 0 and 255")]
        public Color Color(int r, int g, int b) {
            return Xamarin.Forms.Color.FromRgb(r / 255.0, g / 255.0, b / 255.0);
        }

        [Function("Creates a color creates from hsl values between 0 and 1")]
        public Color Color(double h, double s, double l) {
            return Xamarin.Forms.Color.FromHsla(h, s, l);
        }

        [Function("Creates a vector at x y z coordinates")]
        public Vector Vector(double x, double y, double z) {
            return new Vector(x, y, z); ;
        }

        [Function("Creates a vector at x y z coordinates with specified color")]
        public Vector Vector(double x, double y, double z, Color color) {
            return new Vector(x, y, z) { Color = color };
        }

        [Function("")]
        public Poly Poly(params Vector[] points) {
            return new Poly() { Points = points.ToList() };
        }

        [Function("Creates a vector at the coordinates of the sum of each vectors coordinates")]
        public Vector Add(Vector[] vectors) {
            double x = 0, y = 0, z = 0;
            foreach(Vector v in vectors) {
                x += v.X;
                y += v.Y;
                z += v.Z;
            }
            return new Vector(x, y, z);
        }

        [Function("Creates a vector from a plane")]
        public Vector FromPlane(Plane p, double lambda, double müh) {
            return p.GetPoint(lambda, müh);
        }

        [Function("Creates a vector from a plane at specified x and z")]
        public Vector FromXZ(Plane p, double x, double z) {
            return new Vector(x, p.FromXZ(x, z), z);
        }

        [Function("Creates a line from origin and direction vector")]
        public Line Line(Vector origin, Vector direction) {
            return new Line(origin, direction);
        }

        [Function("Creates a 3 dimensional function with dynamic color")]
        public Function Function(string term) {
            return new Function() { Term = term };
        }

        [Function("Creates a 3 dimensional function with specified color")]
        public Function Function(string term, Color color) {
            return new Function() { Term = term, Color = color };
        }

        [Function("Creates a plane from an origin and u and v tension vectors")]
        public Plane Plane(Vector origin, Vector u, Vector v) {
            return new Plane() { Position = origin, U = u, V = v };
        }

        [Function("Creates a plane from an origin and u and v tension vectors with specified color")]
        public Plane Plane(Vector a, Vector u, Vector v, Color color) {
            return new Plane() { Position = a, U = u, V = v, Color = color };
        }

        [Function("Creates a decimal number")]
        public double Double(double value) {
            return value;
        }

        [Function("Creates a sculpture")]
        public Sculpture Sculpture(string xTerm, string yTerm, string zTerm, double start, double end) {
            return new Sculpture() { XTerm = xTerm, YTerm = yTerm, ZTerm = zTerm, Start = start, End = end };
        }

        [Function("Prints the given object to the console")]
        public void Print(object s) {
            try {
                foreach(object o in (List<object>) s) {
                    Print(o);
                }
            }catch(Exception) {
                console.Log(new Feedback(s.ToString(), Feedback.MSG));
            }
        }

        [Function("Creates a slider of the given variable in a range between min and max")]
        public void Slider(string s, double min = 0, double max = 1) {
            Manager.viewer.AddSlider(s, min, max);
        }

        [Function("Toggles on or off the strict mode for the compiler")]
        public void StrictMode(bool on) {
            strictMode = on;
        }

        [Function("Returns the wether or not the strict mode for the compiler is activated")]
        public bool StrictMode() {
            return strictMode;
        }

        [Function("Returns the dot product of a and b")]
        public double Dot(Vector a, Vector b) {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        [Function("Returns the magnitude of vector v")]
        public double Magnitude(Vector v) {
            return Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
        }

        [Function("Returns the angle between vector a and b")]
        public double Angle(Vector a, Vector b) {
            return Math.Acos(Dot(a, b) / Magnitude(a) * Magnitude(b));
        }

        [Function("Removes everything and resets initial variables")]
        public void Reset() {
            Manager.ClearAll();
        }

        [Function("Returns a list of values that solve the given equations")]
        public List<object> Solve(string[] equations) {
            List<Equation> e = new List<Equation>();
            for (int i = 0; i < equations.Length; i++) {
                double res = (double)GetValue(equations[i].Split('=')[1]);
                List<double> vals = new List<double>();
                List<string> names = new List<string>();
                List<string> parts = new List<string>();
                string current = "";
                int brackets = 0;
                foreach (char c in equations[i].Split('=')[0]) {
                    if (c == '(') {
                        brackets++;
                    }
                    if (c == ')') {
                        brackets--;
                    }
                    if (brackets == 0 && (c == '+' || c == '-')) {
                        parts.Add(current);
                        current = "";
                        if (c == '-') current += c;
                    } else
                        current += c;
                }
                parts.Add(current);
                foreach (string part in parts) {
                    names.Add(part[part.Length - 1] + "");
                    string number = part.Substring(0, part.Length - 1);
                    double value = (double)GetValue(number);
                    double sign = part[0] == '-' ? -1 : 1;
                    vals.Add(value);
                }
                Equation equation = new Equation();
                equation.name = (char) (i + 97) + "";
                equation.names = names;
                equation.vals = vals;
                equation.result = res;
                e.Add(equation);
            }
            LES les = new LES();
            try {
                les.Solve(e);
                if (debugMode) {
                    foreach (string msg in les.Msgs) Print(msg);
                } else {
                    foreach (double res in les.Results) Print(res);
                }
                return les.Results;
            } catch (Exception ex) {
                foreach (string msg in les.Msgs) Print(msg);
                Print(ex.Message);
                return new List<object>();
            }
        }

    }

    
    public class FunctionAttribute : Attribute {

        public string Description { get; set; }

        public FunctionAttribute(string des) {
            Description = des;
        }

        public FunctionAttribute() { }

    }

    public class CompilerException {
        public string Name { get; set; }
        public string Args { get; set; }
        public bool IgnoreNumbers;
        public int Code;
        public static int Ignore = 0;
        public static int Remove = 1;
        public static int Replace = 2;
    }

}
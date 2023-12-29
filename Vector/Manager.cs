using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Essentials;

namespace Vector {
    public class Manager {

        public delegate void VariableChangeDelegate();
        public static VariableChangeDelegate VariableChange;

        public static Dictionary<string, object> variables = new Dictionary<string, object>();
        public static EditorPage editor;
        public static Viewer viewer;
        public static ConsolePage console;

        public static Color BackColor = AppInfo.RequestedTheme == AppTheme.Dark ? Color.FromHex("#252525") : Color.FromHex("#dedede");
        public static Color BaseColor = AppInfo.RequestedTheme == AppTheme.Dark ? Color.Black : Color.FromHex("#eeeeee");
        public static Color InvertColor = AppInfo.RequestedTheme == AppTheme.Dark ? Color.White : Color.Black;

        private static List<string> initNames = new List<string>();

        public Manager() {
            InitVariables();
        }

        public static void SetVariable(string name, object value) {
            variables[name] = value;
            VariableChange?.Invoke();
        }

        public static void RemoveVariable(string name) {           
            for(int i = 0; i < viewer.sliders.Count; i++) {
                if(viewer.sliders[i].Name == name) { viewer.sliders.RemoveAt(i);break; }
            }
            object value = variables[name];
            variables.Remove(name);
            VariableChange?.Invoke();
        }

        public static string GetName(object value) {
            foreach(var v in variables) {
                if (v.Value == value) return v.Key;
            }
            return null;
        }

        public static void ClearVariables() {
            viewer.sliders.Clear();
            List<string> nonInits = new List<string>();
            foreach (var v in variables) if (!initNames.Contains(v.Key)) nonInits.Add(v.Key);
            foreach (string s in nonInits) variables.Remove(s);
            VariableChange?.Invoke();
        }

        public static void ClearAll() {
            variables.Clear();
            InitVariables();
            VariableChange?.Invoke();
        }

        public static void InitVariables() {
            AddInit("size", 10.0);
            AddInit("scale", 1.5);
            AddInit("showGrid", true);
            AddInit("rotationX", 0.0);
            AddInit("rotationY", 0.0);
            AddInit("rotationZ", 0.0);
            AddInit("spinX", 0.0);
            AddInit("spinY", 0.0);
            AddInit("spinZ", 0.0);
        }

        public static void AddInit(string name, object value) {
            variables[name] = value;
            initNames.Add(name);
        }

        public static Color MixColor(params Color[] colors) {
            double r = 0, g = 0, b = 0, a = 0;
            foreach(Color c in colors) {
                r += c.R;
                g += c.G;
                b += c.B;
                a += c.A;
            }
            r /= colors.Length;
            g /= colors.Length;
            b /= colors.Length;
            a /= colors.Length;
            return Color.FromRgba(r, g, b, a);
        }

    }
}

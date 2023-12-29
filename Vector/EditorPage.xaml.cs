using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Vector {
	public partial class EditorPage : ContentView {

        public string Code {
            get {
                return input.Text;
            }
            set {
                input.Text = value;
                ShowMsg(interpreter.Compile(input.Text.Split("\n".ToCharArray())));
            }
        }

        public Interpreter interpreter;
        ConsolePage console;
        Viewer viewer;

        public EditorPage (Viewer viewer, ConsolePage console) {
			InitializeComponent();
            interpreter = new Interpreter(viewer, console);
            input.BackgroundColor = Manager.BaseColor;
            input.TextColor = Manager.InvertColor;
            this.console = console;
            this.viewer = viewer;
            update.BackgroundColor = Manager.BackColor;
            help.BackgroundColor = Manager.BackColor;
        }

        public void ReplaceLine(string line, string replace = "") {
            List<string> newLines = new List<string>();
            foreach (string l in input.Text.Split("\n".ToArray())) {
                if (l.Replace(" ", "") != line.Replace(" ", "")) newLines.Add(l);
                else if(replace != "") {
                    newLines.Add(replace);
                }
            }
            input.Text = "";
            foreach(string part in newLines) {
                input.Text += part + "\n";
            }
        }

        public void ShowMsg(Feedback feedback) {
            if (feedback == null) return;
            console.Log(feedback);
        }

        public void InputCompleted(Object sender, EventArgs e) {
            console.Clear();
            Manager.ClearVariables();
            ShowMsg(interpreter.Compile(input.Text.Split("\n".ToCharArray())));
        }

        public void AddInput(string input) {
            this.input.Text += "\n" + input;
        }

        private void ShowHelp(object sender, EventArgs e) {
            ContentPage helper = new ContentPage();
            StackLayout layout = new StackLayout();
            Button back = new Button() { Text = "Back" };
            back.HorizontalOptions = LayoutOptions.StartAndExpand;
            back.Clicked += (object s, EventArgs ea) => { Navigation.PopModalAsync(); };
            layout.Children.Add(back);
            layout.Children.Add(new Helper(interpreter));
            helper.Content = layout;
            NavigationPage.SetHasNavigationBar(helper, false);
            Navigation.PushModalAsync(new NavigationPage(helper));
        }

    }
}
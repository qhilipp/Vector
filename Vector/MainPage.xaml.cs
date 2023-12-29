using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Essentials;

namespace Vector {
    public partial class MainPage : ContentPage {

        public MainPage() {
            InitializeComponent();

            side.BackgroundColor = Manager.BackColor;

            Viewer viewer = new Viewer();
            ConsolePage console = new ConsolePage(this);
            EditorPage editor = new EditorPage(viewer, console);

            Manager.editor = editor;
            Manager.console = console;
            Manager.viewer = viewer;
            Manager.InitVariables();

            UIEditor<Vector> vectorEditor = new UIEditor<Vector>();
            vectorEditor.Build += (string name, StackLayout layout) => {
                Vector v = (Vector)Manager.variables[name];
                layout.Children.Add(new Label() { Text = name + ": " + v.ToString(), TextColor = v.Color, FontSize = 25 });
            };

            UIEditor<Line> lineEditor = new UIEditor<Line>();
            lineEditor.Build += (string name, StackLayout layout) => {
                Line l = (Line)Manager.variables[name];
                layout.Children.Add(new Label() { Text = name + ": " + l.ToString(), TextColor = l.Origin.Color, FontSize = 25 });
            };

            UIEditor<Function> functionEditor = new UIEditor<Function>();
            functionEditor.Build += (string name, StackLayout layout) => {
                Function f = (Function)Manager.variables[name];
                layout.Children.Add(new Label() { Text = name + ": " + f.ToString(), TextColor = f.Color == Color.Transparent ? Manager.InvertColor : f.Color, FontSize = 25 });
            };

            UIEditor<Plane> planeEditor = new UIEditor<Plane>();
            planeEditor.Build += (string name, StackLayout layout) => {
                Plane p = (Plane)Manager.variables[name];
                layout.Children.Add(new Label() { Text = name + p.ToTerm(), TextColor = p.Color });
            };

            UIEditor<double> numberEditor = new UIEditor<double>();
            numberEditor.Build += (string name, StackLayout layout) => {
                double d = (double)Manager.variables[name];
                layout.Children.Add(new Label() { Text = name + ": " + d });
            };

            MediaPage media = new MediaPage();

            AddTab("View", viewer);
            AddTab("Editor", editor);
            AddTab("Console", console);
            AddTab("Vector", vectorEditor);
            AddTab("Line", lineEditor);
            AddTab("Function", functionEditor);
            AddTab("Plane", planeEditor);
            AddTab("Number", numberEditor);
            AddTab("Media", media);

            Navigate("View");

            editor.InputCompleted(null, null);

        }

        public void AddTab(string name, View tab) {           
            tab.HorizontalOptions = LayoutOptions.FillAndExpand;
            tab.VerticalOptions = LayoutOptions.FillAndExpand;

            ToggleButton btn = new ToggleButton();
            btn.Text = name;
            btn.Data = tab;
            btn.ToggleChange += (ToggleButton sender) => {
                Navigate(sender.Text);
            };
            tabs.Children.Add(btn);
        }

        public void Navigate(string name) {
            foreach (View v in tabs.Children) {
                ToggleButton btn = ((ToggleButton)v);
                btn.IsToggled = false;
            }
            foreach(ToggleButton btn in tabs.Children) {
                if(btn.Text == name) {
                    btn.IsToggled = true;
                    content.Content = (View) btn.Data;
                    content.Content.IsVisible = true;
                }
            }
            Console.WriteLine("-----------------------------------");
            foreach(ToggleButton btn in tabs.Children) {
                Console.WriteLine(((View)btn.Data).IsVisible);
            }
        }

    }
}

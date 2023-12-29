using System;
using System.Collections.ObjectModel;
using System.Reflection;
using Xamarin.Forms;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Essentials;

namespace Vector {
    public class UICreator : ContentPage {

        public delegate void FinishDelegate(string value);
        public FinishDelegate Finish;

        bool withName;
        object o;

        StackLayout selection;
        ContentView content;

        public UICreator(object o, bool withName) {

            this.withName = withName;
            this.o = o;

            NavigationPage.SetHasBackButton(this, true);

            StackLayout main = new StackLayout() { HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };
            Content = main;

            main.Children.Add(new BoxView() { HeightRequest = 20 });

            selection = new StackLayout() { Orientation = StackOrientation.Horizontal, HorizontalOptions = LayoutOptions.CenterAndExpand };
            main.Children.Add(selection);

            content = new ContentView() { HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };
            main.Children.Add(content);

            var props = from f in typeof(Interpreter).GetMethods()
                        where f.GetCustomAttributes<FunctionAttribute>().Count() > 0 && f.ReturnType == o.GetType() && f.Name == o.GetType().Name
                        select f;
            int i = 1;
            foreach (var info in props) {
                ToggleButton btn = new ToggleButton();
                btn.Data = AddPage(info);
                btn.Text = i++ + "";
                btn.ToggleChange += (ToggleButton sender) => {
                    Navigate(sender.Text);
                };
                selection.Children.Add(btn);
            }

            Navigate("1");

        }

        private void Navigate(string index) {
            foreach (View v in selection.Children) {
                ((ToggleButton)v).IsToggled = false;
            }
            foreach (ToggleButton b in selection.Children) {
                if (b.Text == index) {
                    b.IsToggled = true;
                    content.Content = (View)b.Data;
                }
            }
        }

        private View AddPage(MethodInfo methodInfo) {

            ScrollView scroll = new ScrollView();
            scroll.HorizontalOptions = LayoutOptions.FillAndExpand;
            scroll.VerticalOptions = LayoutOptions.FillAndExpand;

            StackLayout layout = new StackLayout();
            layout.HorizontalOptions = LayoutOptions.FillAndExpand;
            layout.VerticalOptions = LayoutOptions.CenterAndExpand;
            layout.Spacing = 20;
            scroll.Content = layout;

            if (withName) AddOption(null, "name", layout);

            var parameters = methodInfo.GetParameters();
            for (int i = 0; i < parameters.Length; i++) {
                AddOption(parameters[i].ParameterType, parameters[i].Name, layout);
            }
            Button add = new Button() { Text = "Add", WidthRequest = 500, BackgroundColor = Manager.BackColor, HorizontalOptions = LayoutOptions.Center, FontSize = 35, HeightRequest = 40 };
            add.Clicked += (object sender, EventArgs e) => {
                for (int i = 0; i < layout.Children.Count - 1; i++) {
                    string s = ((UIOption)((StackLayout)layout.Children[i]).Children[1]).Value;
                    if (s == null || s == "") {
                        Navigation.PopModalAsync();
                        return;
                    }
                }
                string args = "";
                for (int i = withName ? 1 : 0; i < layout.Children.Count - 1; i++) {
                    string arg = ((UIOption)((StackLayout)layout.Children[i]).Children[1]).Value;
                    if (arg != null && arg != "") args += arg + ", ";
                }
                if (args.Length > 2) args = args.Remove(args.Length - 2);
                if (withName) {
                    string name = ((UIOption)((StackLayout)layout.Children[0]).Children[1]).Value;
                    Manager.editor.AddInput($"{name} = {o.GetType().Name}({args})");
                    Manager.editor.InputCompleted(null, null);
                } else {
                    Finish?.Invoke($"{o.GetType().Name}({args})");
                }
                Navigation.PopModalAsync();
            };
            layout.Children.Add(add);

            return scroll;

        }

        private void AddOption(Type type, string name, StackLayout layout) {
            StackLayout sl = new StackLayout() { Orientation = StackOrientation.Horizontal, WidthRequest = 500};
            sl.HorizontalOptions = LayoutOptions.CenterAndExpand;
            sl.Children.Add(new Label() { Text = name, FontSize = 35 });
            if (type == typeof(string) || type == null) {
                sl.Children.Add(new UIOption("") { WidthRequest = 333 });
            } else {
                sl.Children.Add(new UIOption(Activator.CreateInstance(type, false)) { WidthRequest = 333 });
            }
            sl.WidthRequest = 500;
            layout.Children.Add(sl);
        }

    }
}


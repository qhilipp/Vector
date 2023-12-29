using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Runtime.CompilerServices;

namespace Vector {
    public class UIEditor<T> : AbsoluteLayout {

        public delegate void BuildDelegate(string name, StackLayout layout);
        private BuildDelegate build;
        public BuildDelegate Build {
            get { return build; }
            set {
                build = value;
                foreach (var item in items) {
                    AddElement(item);
                }
            }
        }

        public bool Active = false;

        List<string> items = new List<string>();

        Button add;
        ScrollView scroll;
        StackLayout layout;

        public UIEditor() {

            foreach(var item in Manager.variables) if (item.Value.GetType() == typeof(T)) items.Add(item.Key);
            Manager.VariableChange += () => {
                //if (IsVisible) Update();
            };

            HorizontalOptions = LayoutOptions.FillAndExpand;
            VerticalOptions = LayoutOptions.FillAndExpand;

            scroll = new ScrollView();
            SetLayoutFlags(scroll, AbsoluteLayoutFlags.All);
            SetLayoutBounds(scroll, new Rectangle(0, 0, 1, 1));
            Children.Add(scroll);

            layout = new StackLayout();
            layout.Padding = new Thickness(10, 10, 10, 10);
            layout.HorizontalOptions = LayoutOptions.FillAndExpand;
            layout.VerticalOptions = LayoutOptions.FillAndExpand;
            scroll.Content = layout;

            add = new Button() { Text = "+", FontSize = 20, BackgroundColor = Color.FromHex("#40D040"), TextColor = Color.White };
            SetLayoutFlags(add, AbsoluteLayoutFlags.PositionProportional);
            SetLayoutBounds(add, new Rectangle(.5, .975, 50, 50));
            add.Clicked += (object sender, EventArgs e) => {
                NavigationPage page = new NavigationPage(new UICreator(Activator.CreateInstance(typeof(T)), true));
                Navigation.PushModalAsync(page);
                page.Popped += (object s, NavigationEventArgs ne) => {
                    Update();
                };
            };
            Children.Add(add);

        }

        public void Update() {
            if(layout != null) layout.Children.Clear();
            if(items != null) items.Clear();
            foreach (var item in Manager.variables) {
                if (item.Value.GetType() == typeof(T)) {
                    items.Add(item.Key);
                    AddElement(item.Key);
                }
            }
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            Update();
        }

        public void AddElement(string element) {
            Frame f = new Frame() { CornerRadius = 25, BackgroundColor = Manager.BackColor, HorizontalOptions = LayoutOptions.FillAndExpand, HasShadow=false };
            StackLayout layout = new StackLayout() { Orientation = StackOrientation.Horizontal, Padding = 10 };
            Build?.Invoke(element, layout);
            Button remove = new Button() { FontSize = 15, TextColor = Color.FromHex("#DF5346"), BackgroundColor = Color.Transparent };
            remove.HorizontalOptions = LayoutOptions.EndAndExpand;
            remove.VerticalOptions = LayoutOptions.CenterAndExpand;
            remove.Text = "Remove";
            remove.Clicked += (object sender, EventArgs e) => {
                Manager.editor.interpreter.Compile(Manager.editor.Code.Split("\n".ToCharArray()), new CompilerException() { Code = CompilerException.Remove, Name = element});
                Manager.editor.InputCompleted(null, null);
                Update();
            };
            layout.Children.Add(remove);
            f.Content = layout;
            if(this.layout != null) this.layout.Children.Add(f);
        }

    }
}

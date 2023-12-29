using System;
using Xamarin.Forms;

namespace Vector {
    public class UIOption : ContentView {

        public string Value { get; set; }
        public bool IsPrimitive { get; set; }

        object o;

        public UIOption(object o) {

            this.o = o;

            HorizontalOptions = LayoutOptions.EndAndExpand;

            if (o.GetType() == typeof(double) || o.GetType() == typeof(string) || o.GetType() == typeof(int)) SetPrimitiveOption();
            else SetObjectOption();

        }

        private void SetObjectOption() {
            IsPrimitive = false;
            StackLayout layout = new StackLayout() { Orientation = StackOrientation.Horizontal, HorizontalOptions = LayoutOptions.FillAndExpand };
            Picker picker = new Picker() { Title = $"Choose {o.GetType().Name}", HorizontalOptions = LayoutOptions.FillAndExpand, FontSize = 35 };
            picker.SelectedIndexChanged += (object sender, EventArgs e) => { Value = (string) picker.SelectedItem; };
            foreach(var variable in Manager.variables) {
                if (variable.Value.GetType() == o.GetType()) picker.Items.Add(variable.Key);
            }
            Button add = new Button() { Text = "Add", VerticalOptions = LayoutOptions.FillAndExpand, FontSize = 35 };
            add.Clicked += (object sender, EventArgs e) => {
                UICreator creator = new UICreator(o, false);
                creator.Finish += (string value) => {
                    picker.Items.Add(value);
                    picker.SelectedItem = value;
                    Value = value;
                }; 
                Navigation.PushModalAsync(new NavigationPage(creator));
            };
            layout.Children.Add(picker);
            layout.Children.Add(add);
            Content = layout;
        }

        private void SetPrimitiveOption() {
            IsPrimitive = true;
            Entry primitiveOption = new Entry() { FontSize = 35, HorizontalOptions = LayoutOptions.FillAndExpand };
            primitiveOption.TextChanged += (object sender, TextChangedEventArgs e) => {
                Value = e.NewTextValue;
            };
            Content = primitiveOption;
        }

    }
}

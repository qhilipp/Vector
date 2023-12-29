using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Vector {
	public partial class Helper : ContentView {

        public Helper (Interpreter interpreter) {
			InitializeComponent();
            foreach (MethodInfo info in interpreter.functions) AddItem(info);
        }

        private void AddItem(MethodInfo info) {
            Frame f = new Frame() { CornerRadius = 15, BackgroundColor = Color.FromHex("#101010"), HorizontalOptions = LayoutOptions.FillAndExpand };
            string args = "";
            int i = 0;
            foreach(var arg in info.GetParameters()) {
                args += arg.ParameterType.Name + " " + arg.Name + (i < info.GetParameters().Count() - 1 ? ", " : "");
                i++;
            }
            Label body = new Label() { Text = $"{info.ReturnType.Name} {info.Name}({args})", TextColor = Color.White, FontSize = 25 };
            string description = info.GetCustomAttribute<FunctionAttribute>().Description;
            StackLayout layout = new StackLayout();
            layout.Children.Add(body);
            if(description != null) layout.Children.Add(new Label() { Text = description, TextColor = Color.DarkGray, FontSize = 20 });
            f.Content = layout;
            items.Children.Add(f);
        }

    }
}
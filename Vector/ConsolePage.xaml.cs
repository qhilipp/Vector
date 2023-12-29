using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Vector {
	public partial class ConsolePage : ContentView {

        private MainPage mainPage;

        public ConsolePage (MainPage mainPage) {
            InitializeComponent();
            this.mainPage = mainPage;
        }

        public void Log(Feedback feedback) {
            if (feedback == null) return;
            if (feedback.GetCodeType() == Feedback.ERROR) mainPage.Navigate("Console");
            txt.Children.Add(new Label() { Text = feedback.GetMessage(), TextColor = feedback.GetColor(), FontSize = 20 });
        }

        public void Clear() {
            txt.Children.Clear();
        }

    }
}
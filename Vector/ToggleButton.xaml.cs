using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Vector {
	public partial class ToggleButton : ContentView {

        public delegate void ToggleChangeDelegate(ToggleButton sender);
        public ToggleChangeDelegate ToggleChange;

        public object Data { get; set; }

        private string text;
        public string Text {
            get {
                return text;
            }
            set {
                text = value;
                content.Text = text;
            }
        }
        private bool isToggled = false;
        public bool IsToggled {
            get {
                return isToggled;
            }
            set {
                isToggled = value;
                frame.BackgroundColor = isToggled ? ToggledColor : Color.Transparent;
            }
        }

        private Color ToggledColor = AppInfo.RequestedTheme == AppTheme.Dark ? Color.FromRgba(1,1,1,.2) : Color.FromRgba(0,0,0,.2);

        public ToggleButton () {
			InitializeComponent();

            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += (s, e) => {
                ToggleChange(this);
            };
            GestureRecognizers.Add(tapGestureRecognizer);
        }
	}
}
using System;
using System.IO;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Collections.Generic;

namespace Vector {
    public class MediaPage : ContentView {

        StackLayout main, selection;
        ContentView content;

        string path = Path.Combine(FileSystem.AppDataDirectory, "Saves");
        string extension = ".vector";

        public MediaPage() {

            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            main = new StackLayout() { HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };
            Content = main;

            selection = new StackLayout() { HorizontalOptions = LayoutOptions.CenterAndExpand, Orientation = StackOrientation.Horizontal, Padding = 20 };
            main.Children.Add(selection);

            ToggleButton safe = new ToggleButton() { Text = "Save" };
            safe.ToggleChange += (ToggleButton sender) => { Navigate("save"); };
            selection.Children.Add(safe);

            ToggleButton load = new ToggleButton() { Text = "Load" };
            load.ToggleChange += (ToggleButton sender) => { Navigate("load"); };
            selection.Children.Add(load);

            ToggleButton share = new ToggleButton() { Text = "Share" };
            share.ToggleChange += (ToggleButton sender) => { Navigate("share"); };
            selection.Children.Add(share);

            content = new ContentView() { HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };
            main.Children.Add(content);

            Navigate("save");

        }

        public void Navigate(string name) {
            foreach (View v in selection.Children) ((ToggleButton)v).IsToggled = false;
            if(name == "save") {
                ShowSafePage();
                ((ToggleButton)selection.Children[0]).IsToggled = true;
            }else if(name == "load") {
                ShowLoadPage();
                ((ToggleButton)selection.Children[1]).IsToggled = true;
            } else if(name == "share") {
                ShowSharePage();
                ((ToggleButton)selection.Children[2]).IsToggled = true;
            }
        }

        private void ShowSafePage() {
            StackLayout layout = new StackLayout() { HorizontalOptions = LayoutOptions.CenterAndExpand, VerticalOptions = LayoutOptions.CenterAndExpand };
            Entry name = new Entry() { Placeholder = "File name", WidthRequest = 300, HorizontalOptions = LayoutOptions.CenterAndExpand };
            layout.Children.Add(name);
            Button safe = new Button() { Text = "Save", BackgroundColor = Manager.BackColor };
            safe.Clicked += (object sender, EventArgs e) => {
                string fileName = Path.Combine(path, name.Text + ".vector");
                if(File.Exists(fileName)) {
                    name.BackgroundColor = Color.Red;
                }
                StreamWriter sw = new StreamWriter(fileName);
                sw.WriteLine(Manager.editor.Code);
                sw.Close();
            };
            layout.Children.Add(safe);
            content.Content = layout;
        }

        private void ShowLoadPage() {
            StackLayout layout = new StackLayout() { HorizontalOptions = LayoutOptions.CenterAndExpand, VerticalOptions = LayoutOptions.CenterAndExpand };
            Picker picker = new Picker() { WidthRequest = 300 };
            foreach (string s in GetFiles()) picker.Items.Add(s);
            picker.SelectedIndexChanged += (object sender, EventArgs e) => {
                StreamReader sr = new StreamReader(Path.Combine(path, (string) picker.SelectedItem + extension));
                Manager.editor.Code = sr.ReadToEnd();
                sr.Close();
            };
            layout.Children.Add(picker);
            content.Content = layout;
        }

        private void ShowSharePage() {
            StackLayout layout = new StackLayout() { HorizontalOptions = LayoutOptions.CenterAndExpand, VerticalOptions = LayoutOptions.CenterAndExpand };
            Picker picker = new Picker() { WidthRequest = 300 };
            foreach (string s in GetFiles()) picker.Items.Add(s);
            picker.SelectedIndexChanged += async (object sender, EventArgs e) => {
                await Share.RequestAsync(new ShareFileRequest {
                    Title = "Vector file",
                    File = new ShareFile((string) picker.SelectedItem + extension),
                });
            };
            layout.Children.Add(picker);
            content.Content = layout;
        }

        private List<string> GetFiles() {
            List<string> files = new List<string>();
            foreach(string s in Directory.GetFiles(path)) {
                if (new FileInfo(s).Extension == ".vector") files.Add(Path.GetFileNameWithoutExtension(s));
            }
            return files;
        }

    }
}


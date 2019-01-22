﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace vlabel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public const int fps = 25;

        public int CurrentFrame
        {
            get => (int)(media_element.Position.TotalSeconds * fps);
            set
            {
                media_element.Position = TimeSpan.FromSeconds(((float)value) / fps);
            }
        }
        public string CurrentCat => (string)combo_categories.SelectedValue;

        public VLabeling Main = new VLabeling();

        private bool IsPlaying;

        public MainWindow()
        {
            InitializeComponent();
            Main.Categories = new string[] { "Default" };
            UpdateCategories();
        }

        private void OpenFile(string fn)
        {
            media_element.Source = new Uri(fn);
            media_element.Play();
            media_element.Pause();
            IsPlaying = false;
        }

        private void cmd_Open(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.DefaultExt = "mp4";
            if (openFileDialog.ShowDialog()==true)
                OpenFile(openFileDialog.FileName);
        }

        private void Media_element_MediaOpened(object sender, RoutedEventArgs e)
        {
            slider_video.Maximum = media_element.NaturalDuration.TimeSpan.TotalSeconds * fps;
            Main.VideoFrames = (int)(media_element.NaturalDuration.TimeSpan.TotalSeconds * fps);
        }

        private void Slider_video_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            CurrentFrame = (int)e.NewValue;
            UpdatePosition();
        }

        private void cmd_Prev(object sender, RoutedEventArgs e)
        {
            media_element.Position -= TimeSpan.FromSeconds(1.0 / fps);
            UpdatePosition();
        }

        private void cmd_Next(object sender, RoutedEventArgs e)
        {
            media_element.Position += TimeSpan.FromSeconds(1.0 / fps);
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            foreach(CheckBox cb in panel_categories.Children)
            {
                cb.IsChecked = Main.GetStatus(CurrentFrame, (string)cb.Content);
            }
            label_position.Content = CurrentFrame.ToString();
            slider_video.Value = CurrentFrame;
        }

        private void cmd_Play(object sender, RoutedEventArgs e)
        {
            if (IsPlaying)
            {
                media_element.Pause();
                IsPlaying = false;
                btn_play.Content = "Play";
            }
            else
            {
                media_element.Play();
                IsPlaying = true;
                btn_play.Content = "Pause";
            }
        }

        private void cmd_Categories(object sender, RoutedEventArgs e)
        {
            var dlg = new PromptDialog("Enter categories");
            if (dlg.ShowDialog()==true)
            {
                Main.Categories = dlg.Text.Split(',').Select(x => x.Trim()).ToArray();
                UpdateCategories();
            }
        }

        private void UpdateCategories()
        {
            panel_categories.Children.Clear();
            combo_categories.Items.Clear();
            foreach(var c in Main.Categories)
            {
                combo_categories.Items.Add(c);
                panel_categories.Children.Add(new CheckBox() { Content = c });
            }
        }

        private void cmd_MarkIn(object sender, RoutedEventArgs e)
        {
            if (CurrentCat == null) return;
            Main.MarkIn(CurrentFrame, CurrentCat);
            Redraw_Detail();
        }

        private void cmd_MarkOut(object sender, RoutedEventArgs e)
        {
            if (CurrentCat == null) return;
            Main.MarkOut(CurrentFrame, CurrentCat);
            Redraw_Detail();
        }

        private void Redraw_Detail()
        {
            detail_panel.Children.Clear();
            foreach(var c in Main.Intervals.Keys)
            {
                var sp = new StackPanel();
                sp.Children.Add(new TextBlock() { Text = c });
                var lb = new ListBox();
                foreach(var i in Main.Intervals[c].OrderBy(x => x.StartFrame))
                {
                    lb.Items.Add(new ListBoxItem() { Content = $"{i.StartFrame} -> {i.EndFrame}" });
                }
                lb.SelectionChanged += (s, ea) =>
                {
                    var L = (Main.Intervals[c].OrderBy(x => x.StartFrame).ToArray())[lb.SelectedIndex];
                    CurrentFrame = L.StartFrame;
                    UpdatePosition();
                };
                sp.Children.Add(lb);
                detail_panel.Children.Add(sp);
            }
        }

        private void Lb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void cmd_Del(object sender, RoutedEventArgs e)
        {
            Main.DeleteLabel(CurrentFrame, CurrentCat);
            Redraw_Detail();
        }

        private void cmd_PrevMark(object sender, RoutedEventArgs e)
        {
            CurrentFrame = Main.PrevMark(CurrentFrame, CurrentCat);
            UpdatePosition();
        }
        private void cmd_NextMark(object sender, RoutedEventArgs e)
        {
            CurrentFrame = Main.NextMark(CurrentFrame, CurrentCat);
            UpdatePosition();
        }

    }
}

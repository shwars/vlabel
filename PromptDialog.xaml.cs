﻿using System;
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
using System.Windows.Shapes;

namespace vlabel
{
    /// <summary>
    /// Interaction logic for PromptDialog.xaml
    /// </summary>
    public partial class PromptDialog : Window
    {
        public PromptDialog(string Prompt)
        {
            InitializeComponent();
            label.Content = Prompt;
        }

        public string Text => textbox.Text;

        public string LastCommand { get; set; } = "Unknown";

        public bool NeedClear { get => LastCommand == "Replace"; }

        private void ProcessCmd(object sender, RoutedEventArgs e)
        {
            var t = ((string)((Button)sender).Content);
            LastCommand = t;
            DialogResult =  t == "Add" || t=="Replace";
        }
    }
}

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
using System.Windows.Shapes;

namespace CybersecurityChatbot
{

    public partial class NameInputDialog : Window
    {
        public string UserName { get; private set; }
        
        public NameInputDialog()
        {
            InitializeComponent();
        }
        
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                UserName = NameTextBox.Text;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Please enter your name", "Name Required", 
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
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

namespace MinecraftServerLauncher
{
    /// <summary>
    /// Interaction logic for ImportServerWindow.xaml
    /// </summary>
    public partial class ImportServerWindow : Window
    {
        public ImportServerWindow()
        {
            InitializeComponent();
        }

        private void Cancel_Button_Clicked(object sender, RoutedEventArgs e)
        {
            var win = Window.GetWindow(this);
            win.DialogResult = false;
            win.Close();
        }
    }
}

// Made by Kieran Kelly
// Last changed on 2025-04-28 at 00:26
// Where we're going we don't need roads!

using System.Windows;

namespace MinecraftServerLauncher
{
    /// <summary>
    /// Interaction logic for ChangeLogViewerDialog.xaml
    /// </summary>
    public partial class ChangeLogViewerDialog : Window
    {
        public ChangeLogViewerDialog()
        {
            InitializeComponent();
        }

        private void Back_Button_Clicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

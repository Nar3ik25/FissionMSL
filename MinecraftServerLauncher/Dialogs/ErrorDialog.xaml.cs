// Made by Kieran Kelly
// Last changed on 2025-10-15 at 04:02
// This is hopefully the only code that will never be used

using System.Windows;

namespace MinecraftServerLauncher
{
    /// <summary>
    /// Interaction logic for ErrorDialog.xaml
    /// </summary>
    public partial class ErrorDialog : Window
    {
        private string details;

        public ErrorDialog(string _details)
        {
            details = _details;
            InitializeComponent();
        }

        private void Ok_Button_Clicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Details_Button_Clicked(object sender, RoutedEventArgs e)
        {
            mainStuff.Visibility = Visibility.Hidden;
            detailText.Text = details;
            detailsStuff.Visibility = Visibility.Visible;
        }
    }
}

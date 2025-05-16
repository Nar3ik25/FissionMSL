using System.Windows;

namespace MinecraftServerLauncher
{
    /// <summary>
    /// Interaction logic for ErrorDialog.xaml
    /// </summary>
    public partial class ErrorDialog : Window
    {
        public ErrorDialog()
        {
            InitializeComponent();
        }

        private void Ok_Button_Clicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

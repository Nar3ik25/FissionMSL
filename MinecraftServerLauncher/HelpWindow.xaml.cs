using System.Windows;

namespace MinecraftServerLauncher
{
    /// <summary>
    /// Interaction logic for HelpWindow.xaml
    /// </summary>
    public partial class HelpWindow : Window
    {
        public HelpWindow()
        {
            InitializeComponent();
        }

        private void Close_Button_Clicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

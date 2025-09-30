// Made by Kieran Kelly
// Last changed on 2025-04-27 at 22:37
// Tis' but a flesh wound!

using System.Windows;

namespace MinecraftServerLauncher
{
    /// <summary>
    /// Interaction logic for CommandWindow.xaml
    /// </summary>
    public partial class CommandWindow : Window
    {
        public CommandWindow()
        {
            InitializeComponent();
        }

        private void Back_Button_Clicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AddScript_Button_Clicked(object sender, RoutedEventArgs e)
        {

        }
    }
}

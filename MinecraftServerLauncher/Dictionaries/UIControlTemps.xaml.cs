using System.Windows;

namespace MinecraftServerLauncher.Dictionaries
{
    public partial class UIControlTemps : ResourceDictionary
    {
        private void OnClose_Button_Click(object sender, RoutedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;
            window.Close();
        }

        private void OnMaximize_Button_Click(object sender, RoutedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;
            window.WindowState = (window.WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;
        }

        private void OnMinimize_Button_Click(object sender, RoutedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;
            window.WindowState = WindowState.Minimized;
        }

        private void OnHelp_Button_Click(object sender, RoutedEventArgs e)
        {
            HelpWindow hp = new HelpWindow();
            hp.Owner = MainWindow.Instance;
            hp.Show();
        }
    }
}

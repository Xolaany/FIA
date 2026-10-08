using Avalonia.Controls;
using Avalonia.Interactivity;

namespace frontend.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnShowRegister_Click(object? sender, RoutedEventArgs e)
        {
            LoginPanel.IsVisible = false;
            RegisterPanel.IsVisible = true;
        }

        private void BtnBackToLogin_Click(object? sender, RoutedEventArgs e)
        {
            RegisterPanel.IsVisible = false;
            LoginPanel.IsVisible = true;
        }

        private void BtnLogin_Click(object? sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void BtnSaveRegister_Click(object? sender, RoutedEventArgs e)
        {
            RegisterPanel.IsVisible = false;
            LoginPanel.IsVisible = true;
        }
    }
}
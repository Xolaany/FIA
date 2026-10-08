using Avalonia.Controls;

namespace frontend.Views
{
    public partial class MainWindow : Window
    {
        public static MainWindow? Instance { get; private set; }
        public MainWindow()
        {
            InitializeComponent();

            Instance = this;

            MostrarLogin();
        }

        private void MostrarLogin()
        {
            var loginControl = new LoginWindow();
            
            loginControl.OnLoginSuccess += (sender, args) =>
            {
                NavigateTo(new CaptureView()); 
            };

            MainContent.Content = loginControl;
        }

        public void NavigateTo(UserControl view)
        {
            MainContent.Content = view;
        }
    }
}
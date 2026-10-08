using Avalonia.Controls;

namespace frontend.Views
{  
    public partial class MainWindow : Window
    {
        public static MainWindow? Instance { get; private set;}

        public MainWindow()
        {
            InitializeComponent();
            Instance  = this;

            NavigateTo(new CaptureView());
        }

        public void NavigateTo(UserControl view)
        {
            MainContent.Content = view;
        }
    }
}
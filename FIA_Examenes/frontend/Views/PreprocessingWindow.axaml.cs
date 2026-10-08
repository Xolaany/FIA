using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using frontend.Helpers;

namespace frontend.Views
{
    public partial class PreprocessingView : UserControl
    {
        private readonly string _imagePath;

        public PreprocessingView()
        {
            InitializeComponent();
            _imagePath = string.Empty;
        }

        public PreprocessingView(string imagePath) : this()
        {
            _imagePath = imagePath;
            if (!string.IsNullOrEmpty(_imagePath) && File.Exists(_imagePath))
            {
                ImgOriginal.Source = new Bitmap(_imagePath);
            }
        }

        private async void BtnProcess_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_imagePath) || !File.Exists(_imagePath)) return;

            TxtStatus.Text = "Estado: Enviando a Python...";
            byte[]? resultBytes = await ApiService.ProcessGrayscaleAsync(_imagePath);

            if (resultBytes != null)
            {
                using var stream = new MemoryStream(resultBytes);
                ImgProcessed.Source = new Bitmap(stream);
                TxtStatus.Text = "Estado: Procesamiento completado";
            }
            else
            {
                TxtStatus.Text = "Estado: Error al conectar con la API";
            }
        }

        private void BtnBack_Click(object? sender, RoutedEventArgs e)
        {
            // Retornar a la vista principal
            MainWindow.Instance?.NavigateTo(new CaptureView());
        }
    }
}

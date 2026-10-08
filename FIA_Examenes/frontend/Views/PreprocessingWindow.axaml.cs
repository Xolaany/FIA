using System;
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
        private string _lastAction = "original";
        private double _lastParam = 1.0;
        private int _currentImageId = 0;
        private int _userId = 1; // Asigna aquí el ID del usuario con sesión activa

        public PreprocessingView()
        {
            InitializeComponent();
            _imagePath = string.Empty;
        }

        public PreprocessingView(string imagePath, int imageId = 0) : this()
        {
            _imagePath = imagePath;
            _currentImageId = imageId;

            if (!string.IsNullOrEmpty(_imagePath) && File.Exists(_imagePath))
            {
                ImgOriginal.Source = new Bitmap(_imagePath);
                ImgProcessed.Source = new Bitmap(_imagePath);
            }
        }

        private void BtnSepararCapas_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_imagePath)) return;
            
            var layersWindow = new ColorLayersWindow(_imagePath);
            layersWindow.Show();
        }

        private async void BtnGris_Click(object? sender, RoutedEventArgs e)
        {
            await ProcessImageFilterAsync("gray");
        }

        private async void BtnHsv_Click(object? sender, RoutedEventArgs e)
        {
            await ProcessImageFilterAsync("hsv");
        }

        private async void BtnDestacarRojo_Click(object? sender, RoutedEventArgs e)
        {
            await ProcessImageFilterAsync("highlight_red");
        }

        private async void BtnDestacarVerde_Click(object? sender, RoutedEventArgs e)
        {
            await ProcessImageFilterAsync("highlight_green");
        }

        private async void BtnDestacarAzul_Click(object? sender, RoutedEventArgs e)
        {
            await ProcessImageFilterAsync("highlight_blue");
        }

        private async void BtnNegativa_Click(object? sender, RoutedEventArgs e)
        {
            await ProcessImageFilterAsync("negative");
        }

        private async void SliderGamma_ValueChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (TxtStatus == null) return;
            double gammaVal = e.NewValue;
            await ProcessImageFilterAsync("gamma", gammaVal);
        }

        private async void BtnGuardar_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_imagePath) || !File.Exists(_imagePath)) return;

            TxtStatus.Text = "Estado: Guardando en Base de Datos...";

            // 1. Si no se ha guardado la imagen original previamente, la registramos en la BD para obtener su ID
            if (_currentImageId == 0)
            {
                int newId = await ApiService.SaveOriginalImageAsync(_userId, Path.GetFileName(_imagePath), _imagePath);
                if (newId > 0)
                {
                    _currentImageId = newId;
                }
                else
                {
                    TxtStatus.Text = "Estado: Error al registrar la imagen original en BD";
                    return;
                }
            }

            // Guardar el preprocesamiento usando el ID
            double? paramVal = _lastAction == "gamma" ? _lastParam : null;
            bool success = await ApiService.SavePreprocessedImageAsync(_currentImageId, _lastAction, _imagePath, paramVal);
            
            TxtStatus.Text = success ? "Estado: Preprocesamiento guardado en BD" : "Estado: Error al guardar en BD";
        }

        private async System.Threading.Tasks.Task ProcessImageFilterAsync(string action, double param = 1.0)
        {
            if (string.IsNullOrEmpty(_imagePath) || !File.Exists(_imagePath)) return;

            _lastAction = action;
            _lastParam = param;

            TxtStatus.Text = $"Estado: Aplicando {action}...";
            byte[]? resultBytes = await ApiService.ApplyFilterAsync(_imagePath, action, param);

            if (resultBytes != null)
            {
                using var stream = new MemoryStream(resultBytes);
                ImgProcessed.Source = new Bitmap(stream);
                TxtStatus.Text = "Estado: Procesamiento completado";
            }
            else
            {
                TxtStatus.Text = "Estado: Error al procesar imagen";
            }
        }

        private void BtnBack_Click(object? sender, RoutedEventArgs e)
        {
            MainWindow.Instance?.NavigateTo(new CaptureView());
        }
    }
}

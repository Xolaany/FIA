using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using frontend.Helpers;
using Avalonia.Platform.Storage;

namespace frontend.Views
{
    public partial class CaptureView : UserControl
    {
        private CancellationTokenSource? _cts;
        private string? _selectedFilePath;

        public CaptureView()
        {
            InitializeComponent();
        }

        private async void BtnBrowsePhoto_Click(object? sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Seleccionar Imagen",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Imágenes")
                    {
                        Patterns = new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp" }
                    }
                }
            });

            if (files.Count > 0)
            {
                _selectedFilePath = files[0].Path.LocalPath;
                using var stream = await files[0].OpenReadAsync();
                PictureBox.Source = new Bitmap(stream);
                TxtPlaceholder.IsVisible = false;
                TxtStatus.Text = "Estado: Imagen Cargada";
            }
        }

        private void BtnClear_Click(object? sender, RoutedEventArgs e)
        {
            _selectedFilePath = null;
            PictureBox.Source = null;
            TxtPlaceholder.IsVisible = true;
            TxtStatus.Text = "Estado: Limpio";
        }

        private void BtnStartCamera_Click(object? sender, RoutedEventArgs e)
        {
            BtnStartCamera.IsVisible = false;
            BtnStopCamera.IsVisible = true;

            if(_cts != null) return;

            _cts = new CancellationTokenSource();
            TxtStatus.Text = "Estado: Conectando a transmisión...";
            TxtPlaceholder.IsVisible = false;

            _ = Task.Run(() => ReadVideoStreamAsync(_cts.Token));
        }

        private async Task ReadVideoStreamAsync(CancellationToken token)
        {
            try
            {
                using var stream = await ApiService.GetVideo(token);
                var buffer = new byte[8192];
                var rawData = new List<byte>();

                int bytesRead;

                while (!token.IsCancellationRequested && (bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
                {
                    // Agregar los nuevos bytes leídos a la lista
                    for (int i = 0; i < bytesRead; i++)
                    {
                        rawData.Add(buffer[i]);
                    }

                    byte[] data = rawData.ToArray();

                    // Buscar marcador de inicio (0xFF, 0xD8) y fin (0xFF, 0xD9) de JPEG
                    int start = FindJpegHeader(data, 0);
                    int end = FindJpegFooter(data, start);

                    if (start != -1 && end != -1 && end > start)
                    {
                        int length = (end + 2) - start;
                        byte[] frameBytes = new byte[length];
                        Array.Copy(data, start, frameBytes, 0, length);

                        // Remover los bytes del frame ya procesado de la lista para no saturar la memoria
                        rawData.RemoveRange(0, end + 2);

                        try
                        {
                            using var frameStream = new MemoryStream(frameBytes);
                            var bitmap = new Bitmap(frameStream);

                            await Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                PictureBox.Source = bitmap;
                                TxtStatus.Text = "Estado: Transmitiendo en Vivo";
                            });
                        }
                        catch
                        {
                            // Frame incompleto o corrupto, se omite silenciosamente
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Transmisión cancelada normalmente
            }
            catch (Exception ex)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    TxtStatus.Text = "Estado: Error en la transmisión";
                });
            }
        }

        private int FindJpegHeader(byte[] data, int startIndex)
        {
            for (int i = startIndex; i < data.Length - 1; i++)
            {
                if (data[i] == 0xFF && data[i + 1] == 0xD8) return i;
            }
            return -1;
        }

        private int FindJpegFooter(byte[] data, int startIndex)
        {
            if (startIndex == -1) return -1;
            for (int i = startIndex; i < data.Length - 1; i++)
            {
                if (data[i] == 0xFF && data[i + 1] == 0xD9) return i;
            }
            return -1;
        }

        private void BtnStopCamera_Click(object? sender, RoutedEventArgs e)
        {
            BtnStopCamera.IsVisible = false;
            BtnStartCamera.IsVisible = true;

            // Cancelar el token para detener la lectura del stream
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
            PictureBox.Source = null;
            TxtPlaceholder.IsVisible = true;
            TxtStatus.Text = "Estado: Cámara Apagada";
        }

        private void BtnTakePhoto_Click(object? sender, RoutedEventArgs e)
        {
            
        }

        private void BtnSaveToDb_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedFilePath)) return;
            TxtStatus.Text = "Estado: Guardado en Base de Datos";
        }

        private void BtnOpenPreprocessing_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedFilePath))
            {
                TxtStatus.Text = "Estado: Primero selecciona una imagen";
                return;
            }

            MainWindow.Instance?.NavigateTo(new PreprocessingView(_selectedFilePath));
        }
    }
}
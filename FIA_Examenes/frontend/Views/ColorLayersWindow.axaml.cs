using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using frontend.Helpers;

namespace frontend.Views
{
    public partial class ColorLayersWindow : Window
    {
        private readonly string _imagePath;

        public ColorLayersWindow()
        {
            InitializeComponent();
            _imagePath = string.Empty;
        }

        public ColorLayersWindow(string imagePath) : this()
        {
            _imagePath = imagePath;
            LoadLayersAsync();
        }

        private async void LoadLayersAsync()
        {
            if (string.IsNullOrEmpty(_imagePath) || !File.Exists(_imagePath))
            {
                TxtStatus.Text = "Error: Archivo de imagen no encontrado.";
                return;
            }

            TxtStatus.Text = "Procesando y obteniendo capas...";

            // Llama a la API que procesa separar_capas en Python
            Dictionary<string, string>? capas = await ApiService.GetColorLayersAsync(_imagePath);

            if (capas != null)
            {
                SetImageFromHexOrBase64(ImgRojo, capas, "rojo");
                SetImageFromHexOrBase64(ImgVerde, capas, "verde");
                SetImageFromHexOrBase64(ImgAzul, capas, "azul");
                SetImageFromHexOrBase64(ImgCian, capas, "cian");
                SetImageFromHexOrBase64(ImgMagenta, capas, "magenta");
                SetImageFromHexOrBase64(ImgAmarillo, capas, "amarillo");

                TxtStatus.Text = "Capas cargadas correctamente.";
            }
            else
            {
                TxtStatus.Text = "Error al obtener capas desde la API.";
            }
        }

        private void SetImageFromHexOrBase64(Image imageControl, Dictionary<string, string> capas, string key)
        {
            if (!capas.TryGetValue(key, out string? rawData) || string.IsNullOrEmpty(rawData)) 
                return;

            try
            {
                byte[] bytes;
                // Detecta si la API respondió en formato Hexadecimal o Base64
                if (IsHexString(rawData))
                {
                    bytes = ConvertHexToBytes(rawData);
                }
                else
                {
                    bytes = Convert.FromBase64String(rawData);
                }

                using var stream = new MemoryStream(bytes);
                imageControl.Source = new Bitmap(stream);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error renderizando la capa {key}: {ex.Message}");
            }
        }

        private bool IsHexString(string input)
        {
            return input.Length % 2 == 0 && System.Text.RegularExpressions.Regex.IsMatch(input, @"\A\b[0-9a-fA-F]+\b\Z");
        }

        private byte[] ConvertHexToBytes(string hex)
        {
            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return bytes;
        }
    }
}
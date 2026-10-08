using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using frontend.Models; // Importación de la carpeta Models

namespace frontend.Helpers
{
    public static class ApiService
    {
        private static readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://127.0.0.1:8000")
        };

        //INICIO DE SESIÓN
        public static async Task<(bool Exito, string Mensaje)> LoginAsync(string usuario, string contrasena)
        {
            try
            {
                var dto = new UsuarioLoginDTO { Usuario = usuario, Contrasena = contrasena };
                string json = JsonSerializer.Serialize(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/login", content);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var res = JsonSerializer.Deserialize<RespuestaApiDTO>(responseBody);
                    return (true, res?.Mensaje ?? "Inicio de sesión correcto.");
                }
                else
                {
                    using var doc = JsonDocument.Parse(responseBody);
                    string detail = doc.RootElement.TryGetProperty("detail", out var elem) 
                        ? elem.GetString() ?? "Error de autenticación."
                        : "Error de autenticación.";
                    return (false, detail);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error al conectar con la API: {ex.Message}");
            }
        }

        //REGISTRO DE USUARIO
        public static async Task<(bool Exito, string Mensaje)> RegisterAsync(UsuarioRegistroDTO datos)
        {
            try
            {
                string json = JsonSerializer.Serialize(datos);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/registro", content);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var res = JsonSerializer.Deserialize<RespuestaApiDTO>(responseBody);
                    return (true, res?.Mensaje ?? "Usuario registrado exitosamente.");
                }
                else
                {
                    using var doc = JsonDocument.Parse(responseBody);
                    string detail = doc.RootElement.TryGetProperty("detail", out var elem) 
                        ? elem.GetString() ?? "Error al registrar usuario."
                        : "Error al registrar usuario.";
                    return (false, detail);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error al conectar con la API: {ex.Message}");
            }
        }

        // API's Para Imagenes y Video

        public static async Task<byte[]?> ProcessGrayscaleAsync(string imagePath)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                using var fileStream = File.OpenRead(imagePath);
                using var streamContent = new StreamContent(fileStream);

                content.Add(streamContent, "file", Path.GetFileName(imagePath));

                var response = await _httpClient.PostAsync("/img-procesada/", content);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public static async Task<Stream> GetVideo(CancellationToken token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/video/");
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
            return await response.Content.ReadAsStreamAsync(token);
        }
    }
}
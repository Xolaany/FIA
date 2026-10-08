using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using frontend.Models;

namespace frontend.Helpers
{
    public static class ApiService
    {
        private static readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://127.0.0.1:8000")
        };

        // INICIO DE SESIÓN
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
                    using var doc = JsonDocument.Parse(responseBody);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("usuario", out var userElem))
                    {
                        if (userElem.TryGetProperty("id_usuario", out var idElem))
                        {
                            UserSession.IdUsuario = idElem.GetInt32();
                        }
                        if (userElem.TryGetProperty("nombre", out var nameElem))
                        {
                            UserSession.Nombre = nameElem.GetString() ?? string.Empty;
                        }
                    }

                    UserSession.Usuario = usuario;

                    string mensaje = root.TryGetProperty("mensaje", out var msgElem) 
                        ? msgElem.GetString() ?? "Inicio de sesión correcto." 
                        : "Inicio de sesión correcto.";

                    return (true, mensaje);
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

        // REGISTRO DE USUARIO
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

        // PROCESAMIENTO DE IMÁGENES Y FILTROS
        public static async Task<byte[]?> ApplyFilterAsync(string imagePath, string accion, double param = 1.0)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                using var fileStream = File.OpenRead(imagePath);
                using var streamContent = new StreamContent(fileStream);

                content.Add(streamContent, "file", Path.GetFileName(imagePath));
                content.Add(new StringContent(accion), "accion");
                content.Add(new StringContent(param.ToString(System.Globalization.CultureInfo.InvariantCulture)), "param");

                var response = await _httpClient.PostAsync("/api/aplicar-filtro", content);

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

        public static async Task<Dictionary<string, string>?> GetColorLayersAsync(string imagePath)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                using var fileStream = File.OpenRead(imagePath);
                using var streamContent = new StreamContent(fileStream);

                content.Add(streamContent, "file", Path.GetFileName(imagePath));

                var response = await _httpClient.PostAsync("/api/separar-capas", content);
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public static async Task<bool> SavePreprocessedImageAsync(int idImagen, string tipoPreprocesamiento, string imagePath, double? parametro = null)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                using var fileStream = File.OpenRead(imagePath);
                using var streamContent = new StreamContent(fileStream);

                content.Add(new StringContent(idImagen.ToString()), "id_imagen");
                content.Add(new StringContent(tipoPreprocesamiento), "tipo_preprocesamiento");
                if (parametro.HasValue)
                {
                    content.Add(new StringContent(parametro.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)), "parametro");
                }
                content.Add(streamContent, "file", Path.GetFileName(imagePath));

                var response = await _httpClient.PostAsync("/api/guardar-preprocesamiento", content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // STREAMING DE VIDEO
        public static async Task<Stream> GetVideo(CancellationToken token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/video/");
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
            return await response.Content.ReadAsStreamAsync(token);
        }

        // Guardado de Imagen "Original"
        public static async Task<int> SaveOriginalImageAsync(int idUsuario, string nombreArchivo, string imagePath)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                using var fileStream = File.OpenRead(imagePath);
                using var streamContent = new StreamContent(fileStream);
        
                content.Add(new StringContent(idUsuario.ToString()), "id_usuario");
                content.Add(new StringContent(nombreArchivo), "nombre");
                content.Add(streamContent, "file", Path.GetFileName(imagePath));
        
                var response = await _httpClient.PostAsync("/api/guardar-imagen-original", content);
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("id_imagen", out var idElem))
                    {
                        return idElem.GetInt32();
                    }
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }
    }
}
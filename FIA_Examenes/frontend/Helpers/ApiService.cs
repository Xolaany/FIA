using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace frontend.Helpers
{
    public static class ApiService
    {
        private static readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://127.0.0.1:8000")
        };

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
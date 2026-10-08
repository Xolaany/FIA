using System.Text.Json.Serialization;

namespace frontend.Models
{
    public class UsuarioRegistroDTO
    {
        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [JsonPropertyName("apellido_paterno")]
        public string ApellidoPaterno { get; set; } = string.Empty;

        [JsonPropertyName("apellido_materno")]
        public string ApellidoMaterno { get; set; } = string.Empty;

        [JsonPropertyName("usuario")]
        public string Usuario { get; set; } = string.Empty;

        [JsonPropertyName("contrasena")]
        public string Contrasena { get; set; } = string.Empty;
    }

    public class UsuarioLoginDTO
    {
        [JsonPropertyName("usuario")]
        public string Usuario { get; set; } = string.Empty;

        [JsonPropertyName("contrasena")]
        public string Contrasena { get; set; } = string.Empty;
    }

    public class RespuestaApiDTO
    {
        [JsonPropertyName("estatus")]
        public string Estatus { get; set; } = string.Empty;

        [JsonPropertyName("mensaje")]
        public string Mensaje { get; set; } = string.Empty;

        [JsonPropertyName("id_usuario")]
        public int? IdUsuario { get; set; }
    }
}
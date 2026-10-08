namespace frontend.Helpers
{
    public static class UserSession
    {
        public static int IdUsuario { get; set; } = 0;
        public static string Nombre { get; set; } = string.Empty;
        public static string Usuario { get; set; } = string.Empty;

        public static bool IsLoggedIn => IdUsuario > 0;

        public static void Logout()
        {
            IdUsuario = 0;
            Nombre = string.Empty;
            Usuario = string.Empty;
        }
    }
}
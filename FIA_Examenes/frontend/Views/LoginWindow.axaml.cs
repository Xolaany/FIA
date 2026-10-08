using Avalonia.Controls;
using Avalonia.Interactivity;
using frontend.Helpers;
using frontend.Models;
using System;

namespace frontend.Views
{
    public partial class LoginWindow : UserControl
    {
        public event EventHandler? OnLoginSuccess;
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnShowRegister_Click(object? sender, RoutedEventArgs e)
        {
            LoginPanel.IsVisible = false;
            RegisterPanel.IsVisible = true;
        }

        private void BtnBackToLogin_Click(object? sender, RoutedEventArgs e)
        {
            RegisterPanel.IsVisible = false;
            LoginPanel.IsVisible = true;
        }

        private async void BtnLogin_Click(object? sender, RoutedEventArgs e)
        {
            string user = TxtUser.Text?.Trim() ?? "";
            string pass = TxtPassword.Text?.Trim() ?? "";

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                Console.WriteLine("[ALERTA] Por favor complete usuario y contraseña.");
                return;
            }

            var (exito, mensaje) = await ApiService.LoginAsync(user, pass);

            if (exito)
            {
                Console.WriteLine($"[ÉXITO] {mensaje}");
                OnLoginSuccess?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                Console.WriteLine($"[ERROR DE LOGIN] {mensaje}");
            }
        }

        private async void BtnSaveRegister_Click(object? sender, RoutedEventArgs e)
        {
            string nombre = RegNombre.Text?.Trim() ?? "";
            string appPaterno = RegAppPaterno.Text?.Trim() ?? "";
            string appMaterno = RegAppMaterno.Text?.Trim() ?? "";
            string usuario = RegUsuario.Text?.Trim() ?? "";
            string password = RegPassword.Text?.Trim() ?? "";

            if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(appPaterno) ||
                string.IsNullOrEmpty(appMaterno) || string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(password))
            {
                Console.WriteLine("[ALERTA] Todos los campos de registro son obligatorios.");
                return;
            }

            var dto = new UsuarioRegistroDTO
            {
                Nombre = nombre,
                ApellidoPaterno = appPaterno,
                ApellidoMaterno = appMaterno,
                Usuario = usuario,
                Contrasena = password
            };

            var (exito, mensaje) = await ApiService.RegisterAsync(dto);

            if (exito)
            {
                Console.WriteLine($"[ÉXITO] {mensaje}");
                
                RegNombre.Text = string.Empty;
                RegAppPaterno.Text = string.Empty;
                RegAppMaterno.Text = string.Empty;
                RegUsuario.Text = string.Empty;
                RegPassword.Text = string.Empty;

                RegisterPanel.IsVisible = false;
                LoginPanel.IsVisible = true;
            }
            else
            {
                Console.WriteLine($"[ERROR REGISTRO] {mensaje}");
            }
        }
    }
}

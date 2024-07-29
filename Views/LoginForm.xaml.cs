using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui;
using System.Net.Http;
using Microsoft.Maui.Controls;
using BasicAuthApp.Controllers;
using System.Text.Json;
using BasicAuthApp.Models;

namespace BasicAuthApp.Views
{
    public partial class LoginForm : ContentPage
    {
        private UsersDB controller;
        public LoginForm()
        {
            InitializeComponent();
            controller = new UsersDB();
            InitController();
        }

        private async void InitController()
        {
            await controller.Init();
        }

        private async void btnLogin_Clicked(object sender, EventArgs e)
        {
            var usuario = txtUser.Text;
            var password = txtPassword.Text;

            if (string.IsNullOrEmpty(usuario) ||
                  string.IsNullOrEmpty(password))
            {
                await DisplayAlert("Error", "Verifique todos los campos", "OK");
                return;
            }

            await LoginUsuario(usuario, password);

        }

        private async Task LoginUsuario(string username, string password)
        {
            Console.WriteLine("Intentando iniciar sesión con: " + username + " " + password);
            using (var client = new HttpClient())
            {
                var uri = new Uri($"http://34.42.1.3:3000/api/user/login/{Uri.EscapeDataString(username)}/{Uri.EscapeDataString(password)}");
                try
                {
                    Console.WriteLine("Enviando solicitud a: " + uri);
                    var response = await client.GetAsync(uri);
                    Console.WriteLine("Respuesta recibida con estado: " + response.StatusCode);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("Contenido de la respuesta: " + responseContent);

                        var userResponse = JsonSerializer.Deserialize<UserResponse>(responseContent);

                        if (userResponse != null && userResponse.user != null)
                        {
                            var dbUser = await controller.GetUltimoUsuario();
                            if (dbUser != null)
                            {
                                dbUser.usuario = userResponse.user.username;
                                dbUser.password = password;

                                int result = await controller.UpdateUsuario(dbUser);
                                Console.WriteLine($"UpdateUsuario result: {result}");
                            }
                            else
                            {
                                dbUser = new Usuarios();
                                dbUser.usuario = userResponse.user.username;
                                dbUser.password = password;

                                int nuevoUsuario = await controller.InsertarNuevoUsuario(dbUser);
                            }
                        }

                        Application.Current.MainPage = new NavigationPage(new MainForm());
                    }
                    else
                    {
                        await DisplayAlert("Error", "Hubo un problema al enviar la solicitud.", "OK");
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    Console.WriteLine("Error de conexión: " + httpEx.Message);
                    await DisplayAlert("Error de conexión", $"Error de conexión: {httpEx.Message}", "OK");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ocurrió un error: " + ex.Message);
                    await DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
                }
            }
        }

    }
}


using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui;
using System.Net.Http;
using Microsoft.Maui.Controls;
using BasicAuthApp.Controllers;
using System.Text.Json;
using BasicAuthApp.Models;
using System.Diagnostics;
using System.Runtime.CompilerServices;


namespace BasicAuthApp.Views
{
    public partial class MainForm : ContentPage
    {
        private UsersDB controller;
        public MainForm()
        {
            InitializeComponent();
            controller = new UsersDB();
            InitController();
            NavigationPage.SetHasBackButton(this, false);
        }

        private async void InitController()
        {
            await controller.Init();
            
        }

        private async Task SetUltimoUsuario(string nombre, string direccion)
        {
            var ultimoUsuario = await controller.GetUltimoUsuario();
            if (ultimoUsuario == null)
            {
                bool answer = await DisplayAlert("Error", "No hay usuario logeado", "Login","Cancel");
                if (answer) 
                {
                    Console.WriteLine("Ir a Formulario de Login");
                    await Navigation.PushAsync(new LoginForm());
                }
                else
                {
                    await Navigation.PushAsync(new MainForm());
                }

                
            }
            else
            {
                Console.WriteLine("Guardar la Persona");
                var usuario=ultimoUsuario.usuario;
                var password=ultimoUsuario.password;
                
                await crearPersona(usuario, password, nombre, direccion);


            }

        }

        private async void btnGuardar_Clicked(object sender, EventArgs e)
        {
            var nombrePersona = txtNombre.Text;
            var direccionPersona = txtDireccion.Text;
            if (string.IsNullOrEmpty(nombrePersona) || string.IsNullOrEmpty(direccionPersona))
            {
                await DisplayAlert("Error", "Verifique todos los campos", "OK");
                return;
            }
            await SetUltimoUsuario(nombrePersona, direccionPersona);
        }

        private async Task crearPersona(string usuario, string pass, string nombre, string direccion)
        {
            using (var client = new HttpClient())
            {
                // Codificar las credenciales en Base64
                var byteArray = Encoding.ASCII.GetBytes($"{usuario}:{pass}");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                var uri = new Uri("http://34.42.1.3:3000/api/person/create");
                var json = $"{{ \"personname\": \"{nombre}\" , \"address\": \"{direccion}\" }}";
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                Console.WriteLine(json); // Verifica que el JSON sea correcto

                try
                {
                    var response = await client.PostAsync(uri, content);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        await DisplayAlert("Exito", "Datos insertados exitosamente", "OK");
                        await Navigation.PushAsync(new MainForm());
                    }
                    else
                    {
                        await DisplayAlert("Error", "Hubo un problema al enviar la solicitud.", "OK");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"Ocurrió un error durante la solicitud HTTP: {ex.Message}", "OK");
                }
            }
        }

        private async void btnLogout_Clicked(object sender, EventArgs e)
        {
            int salir=await controller.DeleteUsuarios();
            if (salir>0)
            {
                await DisplayAlert("Aviso", "Sesion Finalizada", "OK");
            }
            await Navigation.PushAsync(new MainForm());
        }

        private async void btnLista_Clicked(object sender, EventArgs e)
        {
            
            await Navigation.PushAsync(new ListForm());
        }


    }
}


using System;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using BasicAuthApp.Controllers;
using BasicAuthApp.Models;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
//using Android.Views;
namespace BasicAuthApp.Views
{
    public partial class EditForm : ContentPage
    {
        private UsersDB controller;
        private int id;
        private string name;
        private string direccion;
        public EditForm(int id, string name, string direccion)
        {
            InitializeComponent();
            controller = new UsersDB();
            this.id = id;
            this.name = name;
            this.direccion = direccion;
            InitController();
        }

        private async void InitController()
        {
            await controller.Init();
            txtNombreEdit.Text = name;
            txtDireccionEdit.Text = direccion;

        }

        private async void btnInicio_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new MainForm());
        }

        private async void btnGuardarEdit_Clicked(object sender, EventArgs e)
        {
            var nombrePersona = txtNombreEdit.Text;
            var direccionPersona = txtDireccionEdit.Text;
            if (string.IsNullOrEmpty(nombrePersona) || string.IsNullOrEmpty(direccionPersona))
            {
                await DisplayAlert("Error", "Verifique todos los campos", "OK");
                return;
            }
            await SetUltimoUsuario(nombrePersona, direccionPersona);
        }

        private async Task SetUltimoUsuario(string nombre, string direccion)
        {
            var ultimoUsuario = await controller.GetUltimoUsuario();
            if (ultimoUsuario == null)
            {
                bool answer = await DisplayAlert("Error", "No hay usuario logeado", "Login", "Cancel");
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
                var usuario = ultimoUsuario.usuario;
                var password = ultimoUsuario.password;

                await editarPersona(id, usuario, password, nombre, direccion);


            }

        }

        private async Task editarPersona(int id, string usuario, string pass, string nombre, string direccion)
        {
            using (var client = new HttpClient())
            {
                // Codificar las credenciales en Base64
                var byteArray = Encoding.ASCII.GetBytes($"{usuario}:{pass}");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                var uri = new Uri($"http://34.42.1.3:3000/api/person/{id}");
                var json = $"{{ \"personname\": \"{nombre}\" , \"address\": \"{direccion}\" }}";
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                Console.WriteLine(json); // Verifica que el JSON sea correcto

                try
                {
                    var response = await client.PutAsync(uri, content);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        await DisplayAlert("Exito", "Datos actualizados exitosamente", "OK");
                        await Navigation.PushAsync(new ListForm());
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

    }
}


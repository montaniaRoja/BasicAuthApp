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
//using Android.Accounts;
//using Android.Health.Connect.DataTypes.Units;
//using Android.Accounts;

namespace BasicAuthApp.Views
{
    public partial class ListForm : ContentPage
    {
        private UsersDB controller;

        public ListForm()
        {
            InitializeComponent();
            controller = new UsersDB();
            NavigationPage.SetHasBackButton(this, false);
            InitController();
        }

        private async void InitController()
        {
            await controller.Init();
            await SetUltimoUsuario();
        }

        private async void btnBack_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MainForm());
        }

        private async Task SetUltimoUsuario()
        {
            var ultimoUsuario = await controller.GetUltimoUsuario();
            if (ultimoUsuario != null)
            {
                var usuario = ultimoUsuario.usuario;
                var password = ultimoUsuario.password;
                string url = $"http://34.42.1.3:3000/api/person/list";
                string json = await FetchClientData(url, usuario, password);

                if (!string.IsNullOrEmpty(json))
                {
                    Console.WriteLine(json);
                    BindingContext = new ListaPersonasViewModel(json, Navigation);
                }
            }
            else
            {
                await DisplayAlert("Aviso", "No se pudo obtener el nombre del usuario.", "OK");
            }
        }

        private async Task<string> FetchClientData(string url, string usuario, string pass)
        {
            using (var client = new HttpClient())
            {
                var byteArray = Encoding.ASCII.GetBytes($"{usuario}:{pass}");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo obtener los datos de las personas.", "OK");
                    return null;
                }
            }
        }
    }


    public class ListaPersonasViewModel : INotifyPropertyChanged
    {
        private readonly INavigation _navigation;
        private UsersDB controller;
        public ListaPersonasViewModel(string json, INavigation navigation)
        {
            _navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
            Persons = new ObservableCollection<Person>(DeserializeJson(json));
            controller = new UsersDB();
            EditCommand = new Command<Person>(EditPerson);
            DeleteCommand = new Command<Person>(DeletePerson);
        }

        private List<Person> DeserializeJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                throw new ArgumentException("El JSON proporcionado está vacío o es nulo.", nameof(json));
            }
            var personList = JsonConvert.DeserializeObject<List<Person>>(json);
            if (personList == null)
            {
                throw new InvalidOperationException("No se pudo deserializar el JSON a una lista de objetos Person.");
            }
            return personList;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableCollection<Person> persons;
        public ObservableCollection<Person> Persons
        {
            get => persons;
            set
            {
                persons = value;
                OnPropertyChanged(nameof(Persons));
            }
        }

        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }

        private async void DeletePerson(Person person)
        {
            bool answer = await Application.Current.MainPage.DisplayAlert(
                "Confirmar Eliminación",
                $"¿Estás seguro de que deseas eliminar a {person.PersonName}?",
                "Sí", "No");

            if (!answer)
            {
                return; // Si el usuario selecciona "No", simplemente salimos del método.
            }

            Console.WriteLine("id de la persona a eliminar");
            Console.WriteLine(person.Id);
            var ultimoUsuario = await controller.GetUltimoUsuario();
            if (ultimoUsuario != null)
            {
                var usuario = ultimoUsuario.usuario;
                var password = ultimoUsuario.password;
                using (var client = new HttpClient())
                {
                    var byteArray = Encoding.ASCII.GetBytes($"{usuario}:{password}");
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                    var uri = new Uri($"http://34.42.1.3:3000/api/person/{person.Id}");

                    try
                    {
                        var response = await client.DeleteAsync(uri);
                        if (response.IsSuccessStatusCode)
                        {
                            var responseContent = await response.Content.ReadAsStringAsync();
                            await Application.Current.MainPage.DisplayAlert("Exito","Datos Eliminados","OK");
                            Console.WriteLine("Éxito: Datos eliminados exitosamente");
                            if (_navigation != null)
                            {
                                await _navigation.PushAsync(new Views.ListForm());
                            }
                            else
                            {
                                Console.WriteLine("_navigation es nulo");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Error: Hubo un problema al enviar la solicitud. {response.ReasonPhrase}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: Ocurrió un error durante la solicitud HTTP: {ex.Message}");
                    }
                }
            }
            else
            {
                Console.WriteLine("No se pudo obtener el último usuario.");
            }
        }


        private async void EditPerson(Person person)
        {
            Console.WriteLine("id de la persona a editar");
            Console.WriteLine(person.Id);
            var id=person.Id;
            var nombre=person.PersonName;
            var direccion = person.Address;
            await _navigation.PushAsync(new Views.EditForm(id, nombre, direccion));

        }
    }


    public class Person
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("personname")]
        public string PersonName { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }
}

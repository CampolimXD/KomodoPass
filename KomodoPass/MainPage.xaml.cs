﻿using System.Text;

namespace KomodoPass
{
    public partial class MainPage : ContentPage
    {
        
        private readonly LocalDB _dbService;
        private int _editPasswordId;

        public MainPage(LocalDB dbService)
        {
            InitializeComponent();
            _dbService = dbService;

             Task.Run(async () => listView.ItemsSource = await _dbService.GetKomodoPasswords());             
        }      
        private async void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var password = (KomodoPassword)e.Item;
            var aesKey = await _dbService.GetOrCreateKeyAsync();
            var decryptedPassword = _dbService.Decrypt(Encoding.UTF8.GetString(password.Password), aesKey);        
            var action = await DisplayActionSheet("action", "cancel", null, "edit", "delete");

            switch (action)
            {
                case "edit":
                    _editPasswordId = password.Id;
                    NameEntryField.Text = password.Title;
                    // Mostra a senha criptografada
                    // SenhaEntryField.Text = Encoding.UTF8.GetString(password.Password);
                    SenhaEntryField.Text = decryptedPassword;
                    EmailEntryField.Text = password.Email;
                    UserEntryField.Text = password.Username;
                    NotasEntryField.Text = password.Notes;
                    WebsiteEntryField.Text = password.Website;
                    break;

                case "delete":
                    await _dbService.Delete(password);
                    listView.ItemsSource = await _dbService.GetKomodoPasswords();
                    break;
            }
        }

        private async void Salvar_Clicked(object sender, EventArgs e)
        {
            if (_editPasswordId == 0)
            {
                var aesKey = await _dbService.GetOrCreateKeyAsync();
                var password = SenhaEntryField.Text;               
                var Password = _dbService.Encrypt(password, aesKey);
                await _dbService.Create(new KomodoPassword
                {

                    Title = NameEntryField.Text,
                    Password = Encoding.UTF8.GetBytes(Password),                   
                    Email = EmailEntryField.Text,
                    Username = UserEntryField.Text,
                    Notes = NotasEntryField.Text,
                    Website = WebsiteEntryField.Text,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                });
            }
            else
            {
                var aesKey = await _dbService.GetOrCreateKeyAsync();
                var password = SenhaEntryField.Text;
                var Password = _dbService.Encrypt(password, aesKey);
                await _dbService.Update(new KomodoPassword
                {
                    Id = _editPasswordId,
                    Title = NameEntryField.Text,
                    Password = Encoding.UTF8.GetBytes(Password),
                    Email = EmailEntryField.Text,
                    Username = UserEntryField.Text,
                    Notes = NotasEntryField.Text,
                    Website = WebsiteEntryField.Text,                    
                    UpdatedAt = DateTime.Now,
                });

                _editPasswordId = 0;
            }
            // depois de incluir ou editar uma senha, reseta o texto 
            NameEntryField.Text = string.Empty;
            SenhaEntryField.Text = string.Empty;
            EmailEntryField.Text = string.Empty;
            UserEntryField.Text = string.Empty;
            NotasEntryField.Text = string.Empty;
            WebsiteEntryField.Text = string.Empty;

            // dar um refresh na lista
            listView.ItemsSource = await _dbService.GetKomodoPasswords();
        }

        private async void Busca_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = e.NewTextValue;


            if (string.IsNullOrWhiteSpace(query))
            {
                listView.ItemsSource = await _dbService.GetKomodoPasswords();
            }
            else
            {
                var results = await _dbService.SearchPasswords(query);
                listView.ItemsSource = results;
            }
        }        
        // alterar para que esse botao vire um botao que mande o usuario para a main page com as senhas !
        private async void Login_Clicked(object sender, EventArgs e)
        {

                var password = SenhaLogin.Text;
                var salt = _dbService.CreateSalt();
                string hash = _dbService.HashPassword(password, salt);   
                byte[]hashh = Convert.FromBase64String(hash);
                await _dbService.UpdateMaster(new MasterPassword
                {                    
                    Username = UserLogin.Text,
                    PasswordHash = hashh,
                    Salt = salt,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                });
                      
            // depois de incluir ou editar uma senha, reseta o texto 
            UserLogin.Text= string.Empty;
            SenhaLogin.Text  = string.Empty;           
        }
        // temporario 
        private async void criarLogin_Clicked(object sender, EventArgs e)
        {
            var password = SenhaLogin.Text;
            var salt = _dbService.CreateSalt();
            string hash = _dbService.HashPassword(password, salt);
            byte[] hashh = Convert.FromBase64String(hash);
            await _dbService.CreateMaster(new MasterPassword
            {
                Username = UserLogin.Text,
                PasswordHash = hashh,
                Salt = salt,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            });
        }       
    }
}

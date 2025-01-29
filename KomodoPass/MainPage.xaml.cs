using System.Text;

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
        private async void ShowPass_Clicked(object sender, EventArgs e)
        {
            if (sender is ImageButton button && button.CommandParameter is KomodoPassword passwordItem)
            {
                try
                {
                    if (button.Parent is Grid grid)
                    {
                        var showItLabel = grid.Children.OfType<Label>()
                            .FirstOrDefault(l => l.AutomationId == "ShowIt");

                        if (showItLabel != null)
                        {
                            if (showItLabel.Text == "••••••••")
                            {
                                var aesKey = await _dbService.GetOrCreateKeyAsync();
                                var decrypted = _dbService.Decrypt(Encoding.UTF8.GetString(passwordItem.Password),aesKey);
                                showItLabel.Text = decrypted;
                                button.Source = "eyeoffoutline.png";
                            }
                            else
                            {
                                showItLabel.Text = "••••••••";
                                button.Source = "eyeoutline.png";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Erro", $"Falha na descriptografia: {ex.Message}", "OK");
                }
            }
        }
        private async Task ClearClipBoard()
        {
            await Task.Delay(10000);
            await Clipboard.Default.SetTextAsync(null);
        }
        private async void CopyPass_Clicked(object sender, EventArgs e)
        {
            if (sender is ImageButton button && button.CommandParameter is KomodoPassword passwordItem)
            {
                try
                {
                    if (button.Parent is Grid grid)
                    {
                        var showItLabel = grid.Children.OfType<Label>()
                            .FirstOrDefault(l => l.AutomationId == "ShowIt");

                        if (showItLabel != null)
                        {                            
                            var aesKey = await _dbService.GetOrCreateKeyAsync();
                            var decrypted = _dbService.Decrypt(Encoding.UTF8.GetString(passwordItem.Password),aesKey);
                            string textToCopy = decrypted.ToString();
                            Clipboard.SetTextAsync(textToCopy);
                            ClearClipBoard();
                        }
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Erro", $"Falha na descriptografia: {ex.Message}", "OK");
                }
            }
        }
    }
}

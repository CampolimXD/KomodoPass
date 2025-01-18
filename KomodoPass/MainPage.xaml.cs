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
            var action = await DisplayActionSheet("action", "cancel", null, "edit", "delete");

            switch (action)
            {
                case "edit":
                    _editPasswordId = password.Id;
                    NameEntryField.Text = password.Title;
                    SenhaEntryField.Text = password.Password;
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

                await _dbService.Create(new KomodoPassword
                {
                    Title = NameEntryField.Text,
                    Password = SenhaEntryField.Text,
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

                await _dbService.Update(new KomodoPassword
                {
                    Id = _editPasswordId,
                    Title = NameEntryField.Text,
                    Password = SenhaEntryField.Text,
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
    }

}

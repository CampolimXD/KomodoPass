namespace KomodoPass;

public partial class ConfigPage : ContentPage
{
    private readonly LocalDB _dbService;
    public ConfigPage(LocalDB dbService)
	{
		InitializeComponent();
        _dbService = dbService;
    }

    private async void Update_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(UserConfig.Text) || string.IsNullOrWhiteSpace(OldSenha.Text) || string.IsNullOrWhiteSpace(NovaSenhaConfig.Text))
        {
            await DisplayAlert("Erro", "Por favor, preencha todos os campos.", "OK");
            return;
        }
        bool isValid = await _dbService.PreUpdateMaster(UserConfig.Text, OldSenha.Text);
        if (isValid)
        {
            string NovoUsername;
            var userId = await _dbService.GetFirstUser();
            var newPassword = NovaSenhaConfig.Text;
            var newSalt = _dbService.CreateSalt();
            string newHash = _dbService.HashPassword(newPassword, newSalt);
            byte[] Hashh = Convert.FromBase64String(newHash);
            if (NovoUserConfig.Text == null)
            {
                NovoUsername = UserConfig.Text;
            }
            else 
            { 
                NovoUsername = NovoUserConfig.Text;
            }
            await _dbService.UpdateMaster(new MasterPassword
            {
                Id = userId.Id,
                Username = NovoUsername,
                PasswordHash = Hashh,
                Salt = newSalt,
                UpdatedAt = DateTime.Now,
            });
            // reseta o texto
            UserConfig.Text = string.Empty;
            OldSenha.Text = string.Empty;
            NovoUserConfig.Text = string.Empty;
            NovaSenhaConfig.Text = string.Empty;


            await DisplayAlert("Sucesso", "Senha e/ou User atualizados com sucesso.", "OK");
        }
        else
        {
            await DisplayAlert("Erro", "Usuário e/ou senha incorretos. Não foi possível atualizar.", "OK");
        }
    }
}
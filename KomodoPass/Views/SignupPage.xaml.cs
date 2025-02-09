using SQLite;

namespace KomodoPass.Views;

public partial class SignupPage : ContentPage
{
    private readonly LocalDB _dbService;  
    public SignupPage(LocalDB dbService)
	{
		InitializeComponent();
        _dbService = dbService;
     
    }
    private async void CriarUserSenha_Clicked(object sender, EventArgs e)
    {
        // Verifica se ja existe e se existir cancela o login 
        var onlyUser = await _dbService.GetFirstUser();
        if (onlyUser != null)
        {
            await DisplayAlert("Erro", "Já existe um usuário cadastrado. Não é possível criar outro.", "OK");
            CriarUser.Text = string.Empty;
            CriarSenha.Text = string.Empty;
            return;
        }
        // Criar user
        var password = CriarSenha.Text;
        var salt = _dbService.CreateSalt();
        string hash = _dbService.HashPassword(password, salt);
        byte[] hashh = Convert.FromBase64String(hash);
        await _dbService.CreateMaster(new MasterPassword
        {
            Username = CriarUser.Text,
            PasswordHash = hashh,
            Salt = salt,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
        });
        await DisplayAlert("Sucesso", "Login criado com sucesso!", "OK");
        // Reseta o texto 
        CriarUser.Text = string.Empty;
        CriarSenha.Text = string.Empty;
        Application.Current.MainPage = new NavigationPage(new AppShell());
    }

    private void ShowPassword_Clicked(object sender, EventArgs e)
    {
        CriarSenha.IsPassword = !CriarSenha.IsPassword;
        if (CriarSenha.IsPassword == false)
        {
            ShowPassword.Source = "eyeoffoutiline.png";
        }
        else
        {
            ShowPassword.Source = "eyeoutline.png";
        }
    }
}
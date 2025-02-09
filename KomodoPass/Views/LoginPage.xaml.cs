namespace KomodoPass.Views;

public partial class LoginPage : ContentPage
{
    private readonly LocalDB _dbService;
    
    public LoginPage(LocalDB dbService)
	{
		InitializeComponent();
        _dbService = dbService;
    }
    // Mostrar senha digitada e trocar imagem
    private void ShowPassword_Clicked(object sender, EventArgs e)
    {
        SenhaLogin.IsPassword = !SenhaLogin.IsPassword;       
        if (SenhaLogin.IsPassword == false) {
            ShowPassword.Source = "eyeoffoutiline.png";
        }
        else {
            ShowPassword.Source = "eyeoutline.png";
        }
    }

    // Criar user apenas se nao tiver um usuario ja pre cadastrado 
    private async void criar_Clicked(object sender, EventArgs e)
    {
        // Verifica se ja existe e se existir cancela o login 
        var onlyUser = await _dbService.GetFirstUser();
        if (onlyUser != null)
        {
            await DisplayAlert("Erro", "Já existe um usuário cadastrado. Não é possível criar outro.", "OK");
            UserLogin.Text = string.Empty;
            SenhaLogin.Text = string.Empty;
            return;
        }
        // Criar user
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
        await DisplayAlert("Sucesso", "Login criado com sucesso!", "OK");
        // Reseta o texto 
        UserLogin.Text = string.Empty;
        SenhaLogin.Text = string.Empty;
    }
    private async void Login_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(UserLogin.Text) || string.IsNullOrWhiteSpace(SenhaLogin.Text))
        {
            await DisplayAlert("Erro", "Por favor, preencha todos os campos.", "OK");
            return;
        }

        bool isValid = await _dbService.LoginProcess(UserLogin.Text, SenhaLogin.Text);

        if (isValid)
        {
            await DisplayAlert("Sucesso", "Login realizado com sucesso!", "OK");
            Application.Current.MainPage = new NavigationPage(new AppShell());
            
        }
        else
        {
            await DisplayAlert("Erro", "Usuário ou senha inválidos.", "OK");
        }
    }
    private async void Update_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(UserLogin.Text) || string.IsNullOrWhiteSpace(SenhaLogin.Text) || string.IsNullOrWhiteSpace(NovaSenhaLogin.Text))
        {
            await DisplayAlert("Erro", "Por favor, preencha todos os campos.", "OK");
            return;
        }
        bool isValid = await _dbService.PreUpdateMaster(UserLogin.Text, SenhaLogin.Text);
        if (isValid)
        {           
            var userId = await _dbService.GetFirstUser();      
            var newPassword = NovaSenhaLogin.Text;
            var newSalt = _dbService.CreateSalt();
            string newHash = _dbService.HashPassword(newPassword, newSalt); 
            byte[] Hashh = Convert.FromBase64String(newHash);
            await _dbService.UpdateMaster(new MasterPassword
            {
                Id = userId.Id, 
                Username = UserLogin.Text,
                PasswordHash = Hashh, 
                Salt = newSalt,
                UpdatedAt = DateTime.Now,
            });
            // reseta o texto
            UserLogin.Text = string.Empty;
            SenhaLogin.Text = string.Empty;
            NovaSenhaLogin.Text = string.Empty;

            await DisplayAlert("Sucesso", "Senha atualizada com sucesso.", "OK");
        }
        else
        {
            await DisplayAlert("Erro", "Usuário e/ou senha incorretos. Não foi possível atualizar.", "OK");
        }
    }    
}
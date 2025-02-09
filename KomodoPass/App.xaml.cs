using KomodoPass.Views;

namespace KomodoPass
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();       
        }
        private async Task<bool> CheckIfUserExistsAsync()
        {
            var dbService = new LocalDB();
            var onlyUser = await dbService.GetFirstUser();
            return onlyUser != null; 
        }

        protected override async void OnStart()
        {
            base.OnStart();           
            bool userExists = await CheckIfUserExistsAsync();           
            if (userExists)
            {
                MainPage = new LoginPage(new LocalDB());
            }
            else
            {
                MainPage = new SignupPage(new LocalDB());
            }
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var dbService = new LocalDB();           
            return new Window(new LoadingPage());      
        }
    }
}
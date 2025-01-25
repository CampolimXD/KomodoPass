using KomodoPass.Views;

namespace KomodoPass
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();       
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var dbService = new LocalDB();
            return new Window(new LoginPage(dbService));
        }
    }
}
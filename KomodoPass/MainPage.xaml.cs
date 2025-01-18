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
            Task.Run(async () => ListView.ItemsSource = await _dbService.GetKomodoPasswords());
        }
        private async void saveButton_clicked(object sender, EventArgs e) 
        {
            if (_editPasswordId == 0){

                await _dbService
            }
            else { 
            
            }
        }
        private void listView_clicked(object sender, EventArgs e)
        {

        }


    }

}

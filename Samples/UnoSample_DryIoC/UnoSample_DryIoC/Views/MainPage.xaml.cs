// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UnoSample.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await App.ShellPageInstance.ViewModel.NavigationService.NavigateAsync("/" + nameof(LoginPage));
        }
    }
}

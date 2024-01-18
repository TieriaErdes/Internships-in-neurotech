using CommunityToolkit.Mvvm.Input;
using Internships_in_neurotech.Models;

namespace Internships_in_neurotech.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public string Greeting => "Welcome to Avalonia!";


        //private 
        [RelayCommand]
        public void TestSerializing()
        {
            SerializedChannel Channels = new SerializedChannel();
        }
    }
}
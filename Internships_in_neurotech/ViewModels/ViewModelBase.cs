using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Internships_in_neurotech.ViewModels
{
    public partial class ViewModelBase : ObservableObject
    {
        [ObservableProperty] 
        private ObservableCollection<string>? _errorMessages;

        protected ViewModelBase()
        {
            ErrorMessages = new ObservableCollection<string>();
        }
    }
}
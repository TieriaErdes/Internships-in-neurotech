using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Internships_in_neurotech.ViewModels;
using ReactiveUI;
using System.Collections;
using Tmds.DBus.Protocol;

namespace Internships_in_neurotech.Views
{
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();

            //SignalsPopupListBox.SelectedItem.BindCommand(((MainWindowViewModel)DataContext).UpdateUISignalsInfo);


        }

        private void ListBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
        {
            ((MainWindowViewModel)DataContext).UpdateUISignalsInfo(SignalsPopupListBox.SelectedItem);
            fileButton.Flyout.Hide();
        }

        private void Binding_1(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
        {
        }

    }
}
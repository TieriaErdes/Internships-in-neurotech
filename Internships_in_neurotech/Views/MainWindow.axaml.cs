using Avalonia.Controls;
using Internships_in_neurotech.ViewModels;
using System.Globalization;

namespace Internships_in_neurotech.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ListBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
        {
            if ((MainWindowViewModel?)DataContext != null)
                ((MainWindowViewModel)DataContext).UpdateUISignalsInfo(SignalsPopupListBox.SelectedItem);

            fileButton.Flyout!.Hide();
        }

        private void RuMenuItem_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            Lang.Resources.Culture = new CultureInfo("ru-RU");

            if ((MainWindowViewModel?)DataContext != null)
            {
                if (((MainWindowViewModel)DataContext).selectedSignal == -1)
                    ((MainWindowViewModel)DataContext).SetDefaultInformationText();
                else
                    ((MainWindowViewModel)DataContext).SetValuesToTheSignalInfo();

                InitializeComponent();
            }
        }

        private void EngMenuItem_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            Lang.Resources.Culture = new CultureInfo("en-US");

            if ((MainWindowViewModel?)DataContext != null)
            {
                if (((MainWindowViewModel)DataContext).selectedSignal == -1)
                    ((MainWindowViewModel)DataContext).SetDefaultInformationText();
                else
                    ((MainWindowViewModel)DataContext).SetValuesToTheSignalInfo();

                InitializeComponent();
            }
        }
    }
}
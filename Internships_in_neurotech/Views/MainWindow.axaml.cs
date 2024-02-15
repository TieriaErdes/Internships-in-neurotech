using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Internships_in_neurotech.ViewModels;
using System;
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
            ((MainWindowViewModel)DataContext).UpdateUISignalsInfo(SignalsPopupListBox.SelectedItem);
            fileButton.Flyout!.Hide();
        }

        private void RuMenuItem_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            Lang.Resources.Culture = new CultureInfo("ru-RU");
            CultureInfo.CurrentCulture = Lang.Resources.Culture;

            if (((MainWindowViewModel)DataContext).selectedSignal == -1)
                ((MainWindowViewModel)DataContext).SetDefaultInformationText();
            else
                ((MainWindowViewModel)DataContext).SetValuesToTheSignalInfo();

            InitializeComponent();
        }

        private void EngMenuItem_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            Lang.Resources.Culture = new CultureInfo("en-US");
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            if (((MainWindowViewModel)DataContext).selectedSignal == -1)
                ((MainWindowViewModel)DataContext).SetDefaultInformationText();
            else
                ((MainWindowViewModel)DataContext).SetValuesToTheSignalInfo();

            InitializeComponent();
        }
    }
}
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Internships_in_neurotech.ViewModels;
using Internships_in_neurotech.Views;
using Internships_in_neurotech.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;


namespace Internships_in_neurotech
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (CultureInfo.CurrentCulture.Name == "ru-Ru")
                Lang.Resources.Culture = new CultureInfo("ru-RU");
            else
                Lang.Resources.Culture = CultureInfo.CurrentCulture;

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };

                var services = new ServiceCollection();

                services.AddSingleton<IFilesService>(x => new FilesService(desktop.MainWindow));

                Services = services.BuildServiceProvider();
            }

            base.OnFrameworkInitializationCompleted();
        }

        public new static App? Current => Application.Current as App;

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
        /// </summary>
        public IServiceProvider? Services { get; private set; }
    }
}
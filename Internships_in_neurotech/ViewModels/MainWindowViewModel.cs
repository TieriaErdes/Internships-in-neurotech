using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Internships_in_neurotech.Models;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using Avalonia.Platform.Storage;
using Internships_in_neurotech.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Internships_in_neurotech.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public string Greeting => "Welcome to Avalonia!";

        #region Observable Properties

        // Набор динамических переменных для отображения динамической информации о сигналах
        [ObservableProperty]
        private string? _SignalsStatePresenter;
        [ObservableProperty]
        private string? _InformationNameOfSignal;
        [ObservableProperty]
        private string? _InformationTypeOfSignal;
        [ObservableProperty]
        private string? _InformationUnicNumberOfSignal;
        [ObservableProperty]
        private string? _InformationEffectiveFdOfSignal;


        private ObservableCollection<string>? _channelNames;
        /// <summary>
        /// Реализует в себе динамическое хранение списка строк названий файлов сигналов.
        /// Значения строк получают Item в SignalsPopupListBox (MainWindow)
        /// </summary>
        public ObservableCollection<string>? ChannelNames
        {
            get { return _channelNames; }
            set { SetProperty(ref _channelNames, value); }
        }

        #endregion

        /// <summary>
        /// Его дети хранят в себе информацию о массиве сигналов (их конфигурации)
        /// </summary>
        public SerializedChannel Channels;

        /// <summary>
        /// Хранит значения сигналов из соответствующих им сигналов.
        /// Индекс информации, соответствующий некоторому сигналу, равен его порядковому номеру в MethDescription.xml
        /// </summary>
        private SignalData? signalData;


        // Динамический список observable значений для графика
        private ObservableCollection<ObservableValue>? _ChartValues;
        // Динамический список observable графиков
        public ObservableCollection<ISeries> Series { get; set; }

        // Индекс выбранного сигнала. Существует только для того, чтобы из View  
        // вызывать функции присвоения значений строкам, которые образуются на основе данных сигналов
        public int selectedSignal = -1;


        private IFilesService? filesService;

        public MainWindowViewModel()
        {
            CreatingDefaultChart();

            SetDefaultInformationText();
        }


        #region Chart configuration

        private void CreatingDefaultChart()
        {
            _ChartValues = new ObservableCollection<ObservableValue>
            // { new ObservableValue(2), new(1.5), new(3), new(1), new(4), new(3.5), new(5), new(2.7), new(3.5), new(1)};
            { new ObservableValue(5), new(5), new(5), new(5), new(5), new(5), new(5), new(5), new(5), new(5)};

            Series = new ObservableCollection<ISeries>
            {
                new LineSeries<ObservableValue>
                {
                    Values = _ChartValues,
                    GeometrySize = 0,
                    GeometryStroke = null,
                    Stroke = new SolidColorPaint(SKColors.Silver) { StrokeThickness = 2},
                    Fill = null
                }
            };

            if (CultureInfo.CurrentCulture.Name == "ru-RU")
                ChartTitle.Text = "График";
            else
                ChartTitle.Text = "Chart";
        }

        private void CreatingSelectedChart()
        {
            _ChartValues = new ObservableCollection<ObservableValue>();

            foreach (var item in signalData!.DataFromFile[selectedSignal])
            {
                _ChartValues.Add(new(item));
            }

            Series[0].Values = _ChartValues;
        }

        [RelayCommand]
        public void DisplayMathPoints(string senderComponentName)
        {
            if (selectedSignal == -1)
            {
                Debug.WriteLine($"{selectedSignal} is equals to null");
                return;
            }

            switch (senderComponentName)
            {
                case "ExpectationButton":
                    PointGenerator(signalData!.AverageValue[selectedSignal].ToArray());
                    break;
                case "MinButton":
                    PointGenerator(signalData!.MinValue[selectedSignal].ToArray());
                    break;
                case "MaxButton":
                    PointGenerator(signalData!.MaxValue[selectedSignal].ToArray());
                    break;
                default:
                    Debug.WriteLine($"{senderComponentName} can not display points");
                    break;
            }
        }

        private void PointGenerator(double[] resultArray)
        {
            List<ObservablePoint> points = new List<ObservablePoint>();

            for (int i = 0; resultArray.Length > i; i++)
            {
                points.Add(new ObservablePoint(x: (i * 1000), y: resultArray[i]));
            }

            if (Series.Count > 1) Series.RemoveAt(1);
            Series.Add(new LineSeries<ObservablePoint?>
            {
                Values = points.ToArray(),
            });
        }

        [RelayCommand]
        public void PointCleaner()
        {
            if (Series.Count > 1) Series.RemoveAt(1);
        }

            public Axis[] XAxes { get; set; }
            = new Axis[]
            {
                new Axis
                {
                    Name = "X Axis",
                    NamePaint = new SolidColorPaint(SKColors.Silver),

                    LabelsPaint = new SolidColorPaint(SKColors.Silver),
                    TextSize = 12,

                }
            };

        public Axis[] YAxes { get; set; }
            = new Axis[]
            {
                new Axis
                {
                    Name = "Y Axis",
                    NamePaint = new SolidColorPaint(SKColors.Silver),

                    LabelsPaint = new SolidColorPaint(SKColors.Silver),
                    TextSize = 14,

                    SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray)
                    {
                        StrokeThickness = 2,
                        PathEffect = new DashEffect(new float[] { 8, 12})
                    }
                }
            };

        public LabelVisual ChartTitle { get; set; } =
            new LabelVisual
            {
                Text = "",
                TextSize = 25,
                Padding = new LiveChartsCore.Drawing.Padding(15),
                Paint = new SolidColorPaint(SKColors.Silver)
            };

        #endregion



        public void SetDefaultInformationText()
        {
            if (CultureInfo.CurrentCulture.Name == "ru-RU")
            {
                SignalsStatePresenter = "Сигналы не обнаружены. \nПожалуйста, выберите папку с сигналами.";
                InformationNameOfSignal = "Название: ";
                InformationTypeOfSignal = "Тип: ";
                InformationUnicNumberOfSignal = "Номер: ";
                InformationEffectiveFdOfSignal = "Частота: ";
            }
            else // для всех остальных языков английская локализация
            {
                SignalsStatePresenter = "No signals detected. \nPlease select a folder with signals.";
                InformationNameOfSignal = "Name: ";
                InformationTypeOfSignal = "Type: ";
                InformationUnicNumberOfSignal = "UnicNum: ";
                InformationEffectiveFdOfSignal = "EffectiveFd: ";
            }
        }

        private void UpdateSignalsState()
        {
            if (Channels is null)
            {
                if (CultureInfo.CurrentCulture.Name == "ru-Ru")
                    SignalsStatePresenter = "Сигналы не обнаружены. \nПожалуйста, выберите папку с сигналами.";
                else
                    SignalsStatePresenter = "No signals detected. \nPlease select a folder with signals.";
            }
            else
            {
                if (CultureInfo.CurrentCulture.Name == "ru-RU")
                    SignalsStatePresenter = "Названия сигналов: ";
                else
                    SignalsStatePresenter = "Names of signals: ";
            }
        }


        // Команда кнопки, которая отвечает за открытие окна выбора файла
        [RelayCommand]
        public async void OpenFolderDialog()
        {
            ErrorMessages?.Clear();
            try
            {
                filesService = App.Current?.Services?.GetService<IFilesService>();
                if (filesService is null) throw new NullReferenceException("Missing File Service instance.");

                var folder = await filesService.GetFolderAsync();
                // НУЖНО ПРИДУМАТЬ ЧТО ДЕЛАТЬ В СЛУЧАЕ, ЕСЛИ folder РАВЕН null
                if (folder is null) return;

                // класс десериализации MethDescroption.xml. Хранит в себе же все данные о сигналах 
                Channels = new SerializedChannel(folder.TryGetLocalPath());
                UpdateSignalsState();
                if (Channels is null) return;

                ChannelNames = new ObservableCollection<string>();
                foreach (var channel in Channels.bosMeth!.Channels!)
                {
                    ChannelNames.Add(channel.SignalFileName);
                }

                signalData = new SignalData(in Channels);
            }
            catch (Exception ex)
            {
                ErrorMessages.Add(ex.Message);
            }
        }


        // Обновление информации о сигналах в соответствующем табло и их графика
        [RelayCommand]
        public void UpdateUISignalsInfo(object? selectedItemName)
        {
            Debug.WriteLine("Call successful");

            for (int i = 0; i < Channels.bosMeth.Channels.Count; i++)
            {
                if (Channels.bosMeth.Channels[i].SignalFileName == (string?)selectedItemName)
                {
                    selectedSignal = i;
                    CreatingSelectedChart();
                    SetValuesToTheSignalInfo();
                    break;
                }
            }
        }

        private string GetType(Channel channel)
        {
            switch (channel.Type)
            {
                case 1 when CultureInfo.CurrentCulture.Name is "ru-RU":
                    return "ЭЭГ";
                case 2 when CultureInfo.CurrentCulture.Name is "ru-RU":
                    return "ЭКГ";
                case 3 when CultureInfo.CurrentCulture.Name is "ru-RU":
                    return "ЭМГ";
                case 1:
                    return "EEG";
                case 2:
                    return "ECG";
                case 3:
                    return "EMG";
                default:
                    return "";
            }
        }
        

        // Вставка текста в табло информации о сигналах
        public void SetValuesToTheSignalInfo()
        {
            if (CultureInfo.CurrentCulture.Name == "ru-RU")
            {
                InformationNameOfSignal = "Название: " + Channels.bosMeth.Channels[selectedSignal].SignalFileName;
                InformationTypeOfSignal = "Тип: " + GetType(Channels.bosMeth.Channels[selectedSignal]);
                InformationUnicNumberOfSignal = "Номер: " + Channels.bosMeth.Channels[selectedSignal].UnicNumber.ToString();
                InformationEffectiveFdOfSignal = "Частота: " + Channels.bosMeth.Channels[selectedSignal].EffectiveFd.ToString() + " Гц";
            }
            else // для всех остальных языков английская локализация
            {
                InformationNameOfSignal = "Name: " + Channels.bosMeth.Channels[selectedSignal].SignalFileName;
                InformationTypeOfSignal = "Type: " + GetType(Channels.bosMeth.Channels[selectedSignal]);
                InformationUnicNumberOfSignal = "UnicNum: " + Channels.bosMeth.Channels[selectedSignal].UnicNumber.ToString();
                InformationEffectiveFdOfSignal = "EffectiveFd: " + Channels.bosMeth.Channels[selectedSignal].EffectiveFd.ToString() + " Hz";
            }

            ChartTitle.Text = Channels.bosMeth.Channels[selectedSignal].SignalFileName;
        }

        
    }
}
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Internships_in_neurotech.Models;
using Internships_in_neurotech.Services;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using LiveChartsCore.SkiaSharpView.VisualElements;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

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
        public SerializedChannel? Channels;

        /// <summary>
        /// Хранит значения сигналов из соответствующих им сигналов.
        /// Индекс информации, соответствующий некоторому сигналу, равен его порядковому номеру в MethDescription.xml
        /// </summary>
        private SignalData? _signalData;


        // Динамический список observable значений для графика
        private ObservableCollection<ObservableValue>? _ChartValues;
        // Динамический список observable графиков
        public ObservableCollection<ISeries>? Series { get; set; }

        // Индекс выбранного сигнала. Существует только для того, чтобы из View  
        // вызывать функции присвоения значений строкам, которые образуются на основе данных сигналов
        public int selectedSignal = -1;


        private IFilesService? _filesService;

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

            ChartTitle.Text = Lang.Resources.ChartTitle;
        }

        private void CreatingSelectedChart()
        {
            _ChartValues = new ObservableCollection<ObservableValue>();

            foreach (var item in _signalData!.DataFromFile[selectedSignal])
            {
                _ChartValues.Add(new(item));
            }

            Series![0].Values = _ChartValues;
            ChartTitle.Text = Channels!.bosMeth!.Channels![selectedSignal].SignalFileName!;
        }

        [RelayCommand]
        public void DisplayMathPoints(string senderComponentParameter)
        {
            if (selectedSignal == -1)
            {
                Debug.WriteLine($"{selectedSignal} is equals to null");
                return;
            }

            switch (senderComponentParameter)
            {
                case "ExpectationButton":
                    PointGenerator(_signalData!.averageValue[selectedSignal]);
                    break;
                case "MinButton":
                    PointGenerator(_signalData!.minValue[selectedSignal]);
                    break;
                case "MaxButton":
                    PointGenerator(_signalData!.maxValue[selectedSignal]);
                    break;
                default:
                    Debug.WriteLine($"{senderComponentParameter} can not display points");
                    break;
            }
        }

        private void PointGenerator(double[] resultArray)
        {
            List<ObservablePoint> points = new List<ObservablePoint>();

            for (int i = 0; resultArray.Length > i; i++)
            {
                points.Add(new ObservablePoint(x: (i * Channels!.bosMeth!.Channels![selectedSignal].EffectiveFd), y: resultArray[i]));
            }

            if (Series!.Count > 1) Series.RemoveAt(1);
            Series.Add(new LineSeries<ObservablePoint?>
            {
                Values = points.ToArray(),
            });
        }

        [RelayCommand]
        public void PointCleaner()
        {
            if (Series!.Count > 1) Series.RemoveAt(1);
        }

         public Axis[] XAxes { get; set; }
            = new Axis[]
            {
                new Axis
                {
                    Name = "",
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
                    Name = "",
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
            SetSignalsState();

            InformationNameOfSignal = Lang.Resources.Name;
            InformationTypeOfSignal = Lang.Resources.Type;
            InformationUnicNumberOfSignal = Lang.Resources.Number;
            InformationEffectiveFdOfSignal = Lang.Resources.Fd;

            XAxes[0].Name = Lang.Resources.XAxis;
            YAxes[0].Name= Lang.Resources.YAxis;
        }

        private void SetSignalsState()
        {
            if (Channels is null)
                SignalsStatePresenter = Lang.Resources.SignalsState;
            else
                SignalsStatePresenter = Lang.Resources.SignalsStateCorrectly;
        }


        // Команда кнопки, которая отвечает за открытие окна выбора файла
        [RelayCommand]
        public async void OpenFolderDialog()
        {
            ErrorMessages?.Clear();
            try
            {
                _filesService = App.Current?.Services?.GetService<IFilesService>();
                if (_filesService is null) throw new NullReferenceException("Missing File Service instance.");

                var folder = await _filesService.GetFolderAsync();
                // НУЖНО ПРИДУМАТЬ ЧТО ДЕЛАТЬ В СЛУЧАЕ, ЕСЛИ folder РАВЕН null
                if (folder is null) return;

                // класс десериализации MethDescroption.xml. Хранит в себе же все данные о сигналах 
                Channels = new SerializedChannel(folder.TryGetLocalPath());
                SetSignalsState();
                if (Channels is null) return;

                ChannelNames = new ObservableCollection<string>();
                foreach (var channel in Channels.bosMeth!.Channels!)
                {
                    ChannelNames.Add(channel.SignalFileName!);
                }

                _signalData = new SignalData(in Channels);
            }
            catch (Exception ex)
            {
                ErrorMessages!.Add(ex.Message);
            }
        }


        // Обновление информации о сигналах в соответствующем табло и их графика
        [RelayCommand]
        public void UpdateUISignalsInfo(object? selectedItemName)
        {
            if (selectedItemName is null) return;

            Debug.WriteLine("Call successful");

            for (int i = 0; i < Channels!.bosMeth!.Channels!.Count; i++)
            {
                if (Channels.bosMeth.Channels[i].SignalFileName!.Equals(selectedItemName))
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
                case 1:
                    return Lang.Resources.SignalType1;
                case 2:
                    return Lang.Resources.SignalType2;
                case 3:
                    return Lang.Resources.SignalType3;
                default:
                    return "";
            }
        }


        // Вставка текста в табло информации о сигналах
        public void SetValuesToTheSignalInfo()
        {
            SetSignalsState();

            InformationNameOfSignal = Lang.Resources.Name + Channels!.bosMeth!.Channels![selectedSignal].SignalFileName;
            InformationTypeOfSignal = Lang.Resources.Type + GetType(Channels.bosMeth.Channels[selectedSignal]);
            InformationUnicNumberOfSignal = Lang.Resources.Number + Channels.bosMeth.Channels[selectedSignal].UnicNumber.ToString();
            InformationEffectiveFdOfSignal = Lang.Resources.Fd + Channels.bosMeth.Channels[selectedSignal].EffectiveFd.ToString() + Lang.Resources.Hz;

            XAxes[0].Name = Lang.Resources.XAxis;
            YAxes[0].Name = Lang.Resources.YAxis;

            // стираем точки обработанных значений, которые остались от предыдущего файла сигналов
            PointCleaner();
        }

        
    }
}
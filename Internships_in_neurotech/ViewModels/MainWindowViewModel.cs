using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Internships_in_neurotech.Models;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using Avalonia.Media;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView.Painting.Effects;

namespace Internships_in_neurotech.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        public string Greeting => "Welcome to Avalonia!";

        #region Observable Properties


        [ObservableProperty]
        private string? _InformationNameOfSignal;

        [ObservableProperty]
        private string? _InformationTypeOfSignal;

        [ObservableProperty]
        private string? _InformationUnicNumberOfSignal;

        [ObservableProperty]
        private string? _InformationEffectiveFdOfSignal;

        [ObservableProperty]
        private Channel? _LatestSelectedSignal;


        #endregion


        public SerializedChannel Channels;

        
        private Signal? currentSignal;
        private ObservableCollection<ObservableValue>? _ChartValues;
        public ObservableCollection<ISeries> Series { get; set; }


        private ObservableCollection<string>? _channelNames;
        public ObservableCollection<string> ChannelNames
        {
            get { return _channelNames; }
            set { SetProperty(ref _channelNames, value); }
        }

        public MainWindowViewModel()
        {
            Channels = new SerializedChannel();
            ChannelNames = new ObservableCollection<string>();

            if (Channels.bosMeth == null)
            {
                throw new Exception("BosMeth is equals to null");
            }

            foreach (var channel in Channels.bosMeth.Channels)
            {
                ChannelNames.Add(channel.SignalFileName);
            }


            CreatingChart();
        }


        #region Chart configuration

        private void CreatingChart()
        {
            currentSignal = new Signal(in Channels);
            _ChartValues = new ObservableCollection<ObservableValue>();

            foreach (var item in currentSignal.DataFromFile[0])
            {
                _ChartValues.Add(new(item));
            }

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
        }

            public Axis[] XAxes { get; set; }
            = new Axis[]
            {
                new Axis
                {
                    Name = "X Axis",
                    NamePaint = new SolidColorPaint(SKColors.Black),

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

        public LabelVisual Title { get; set; } =
            new LabelVisual
            {
                Text = "My chart title",
                TextSize = 25,
                Padding = new LiveChartsCore.Drawing.Padding(15),
                Paint = new SolidColorPaint(SKColors.Silver)
            };

        #endregion



        public void SetDefaultInformationText()
        {

        }

        [RelayCommand]
        public void UpdateUISignalsInfo(object? selectedItemName)
        {
            Debug.WriteLine("Call successful");

            foreach (var channel in Channels.bosMeth.Channels)
            {
                if (channel.SignalFileName == selectedItemName)
                    InsertValuesToTheSignalInfo(channel);
            }
        }

        public void InsertValuesToTheSignalInfo(Channel? channel)
        {
            if (CultureInfo.CurrentCulture.Name == "ru-RU")
            {
                InformationNameOfSignal = "Название: " + channel.SignalFileName;
                InformationTypeOfSignal = "Тип: " + channel.Type.ToString();
                InformationUnicNumberOfSignal = "UnicNum: " + channel.UnicNumber.ToString();
                InformationEffectiveFdOfSignal = "EffectiveFd: " + channel.EffectiveFd.ToString();
            }
            else // для всех остальных языков английская локализация
            {
                InformationNameOfSignal = "Name: " + channel.SignalFileName;
                InformationTypeOfSignal = "Type: " + channel.Type.ToString();
                InformationUnicNumberOfSignal = "UnicNum: " + channel.UnicNumber.ToString();
                InformationEffectiveFdOfSignal = "EffectiveFd: " + channel.EffectiveFd.ToString();
            }

            LatestSelectedSignal = channel;


        }

        
    }
}
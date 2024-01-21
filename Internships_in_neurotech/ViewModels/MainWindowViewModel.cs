using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Internships_in_neurotech.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

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
        }


        //private 
        [RelayCommand]
        public void TestSerializing()
        {
            //SerializedChannel Channels = new SerializedChannel();


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
            InformationNameOfSignal = "Name: " + channel.SignalFileName;
            InformationTypeOfSignal = "Type: " + channel.Type.ToString();
            InformationUnicNumberOfSignal = "UnicNum: " + channel.UnicNumber.ToString();
            InformationEffectiveFdOfSignal = "EffectiveFd: " + channel.EffectiveFd.ToString();

            LatestSelectedSignal = channel;


        }
    }
}
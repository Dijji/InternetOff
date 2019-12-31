// Copyright (c) 2019, Dijji, and released under Ms-PL.  This can be found in the root of this distribution. 

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace InternetOff
{
    class State : INotifyPropertyChanged
    {
        private bool? isConnected = null;
        private bool isWorking;
        
        public bool? IsConnected
        {
            get { return isConnected; }
            set
            {
                isConnected = value;
                OnPropertyChanged(nameof(StopGoColour));
                OnPropertyChanged(nameof(StatusKnown));
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(Command));
            }
        }

        public bool IsWorking
        {
            get { return isWorking; }
            set
            {
                isWorking = value;
                OnPropertyChanged(nameof(StopGoColour));
                OnPropertyChanged(nameof(StatusKnown));
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(Command));
            }
        }

        public bool StatusKnown
        {
            get { return IsConnected != null && !IsWorking; }
        }

        public string Status
        {
            get
            {
                if (IsWorking)
                    return "Working...";
                else
                    return "Internet is " + (IsConnected == true ? "ON" : "OFF");
            }
        }

        public string Command
        {
            get
            {
                if (IsWorking || IsConnected == null)
                    return "";
                else
                    return "Turn " + ((bool)IsConnected ? "Off" : "On");
            }
        }

        public Brush StopGoColour
        {
            get
            {
                if (IsConnected == null || IsWorking)
                    return Brushes.CornflowerBlue;
                else
                    return (bool)IsConnected ? Brushes.LightGreen : Brushes.OrangeRed;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                // Notify changes on the UI thread so that screen updates can be triggered
                Application.Current.Dispatcher.Invoke(() =>
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(info));
                });
            }
        }
    }
}

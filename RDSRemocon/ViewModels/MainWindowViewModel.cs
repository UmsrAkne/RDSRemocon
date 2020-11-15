﻿using Prism.Commands;
using Prism.Mvvm;
using RDSRemocon.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Threading;

namespace RDSRemocon.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {

        private Process process = new Process() {
            StartInfo = new ProcessStartInfo() {
                FileName = System.Environment.GetEnvironmentVariable("ComSpec"),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardInput = false,
                CreateNoWindow = true
            }
        };

        private string _title = "Prism Application";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private string state = "";
        public string State {
            get => state;
            private set => SetProperty(ref state, value);
        }

        private DateTime lastUpdateDateTime = new DateTime();
        public DateTime LastUpdateDateTime {
            get => lastUpdateDateTime;
            set => SetProperty(ref lastUpdateDateTime, value);
        }

        private ObservableCollection<Log> logs = new ObservableCollection<Log>();
        public ObservableCollection<Log> Logs {
            get => logs;
            set => SetProperty(ref logs, value);
        }

        private string updateIntervalMinuteString = "";
        public string UpdateIntervalMinuteString {
            get => updateIntervalMinuteString;
            set {
                if (int.TryParse(value, out int num)) {
                    SetProperty(ref updateIntervalMinuteString, value);
                    timer.Interval = new TimeSpan(0,num,0);
                }
                else {
                    SetProperty(ref updateIntervalMinuteString, updateIntervalMinuteString);
                }
            }
        }

        public MainWindowViewModel() {
            StartDBInstanceCommand = 
                new DelegateCommand(() => {
                    cliExecuter.startDBInstance(cliExecuter.getDBInstanceIdentifier());
                });

            StopDBInstanceCommand =
                new DelegateCommand(() => {
                    cliExecuter.stopDBInstance(cliExecuter.getDBInstanceIdentifier());
                });

            UpdateDBInstanceStatusCommand =
                new DelegateCommand(() => {
                    Output = cliExecuter.getDBInstanceStatus();
                    State = extractDBInstanceState(Output);
                    Logs.Add(new Log("getStatus" ,State, DateTime.Now));
                    LastUpdateDateTime = DateTime.Now;
                });

            int updateInterval = 20;
            UpdateIntervalMinuteString = updateInterval.ToString();

            timer.Tick += (sender, e) => UpdateDBInstanceStatusCommand.Execute();
            timer.Start();
        }

        public string Output { get => output; set => SetProperty(ref output, value); }
        private string output = "";

        private DispatcherTimer timer = new DispatcherTimer();

        public DelegateCommand StartDBInstanceCommand { get; private set;}
        public DelegateCommand StopDBInstanceCommand { get; private set;}
        public DelegateCommand UpdateDBInstanceStatusCommand { get; private set;}

        private CLICommands cliExecuter = new CLICommands();

        /// <summary>
        /// getDBInstanceStatus の戻り値から DBInstance の現在の状態を表す文字列のみを抽出します
        /// </summary>
        /// <returns></returns>
        private string extractDBInstanceState(string text) {
            var regex = new Regex("\"DBInstanceStatus\": \"(.*)\"", RegexOptions.IgnoreCase);
            return regex.Matches(text)[0].Groups[1].Value;
        }
    }
}

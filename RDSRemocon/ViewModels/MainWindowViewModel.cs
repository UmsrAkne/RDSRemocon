using Prism.Commands;
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
        public string Title
        {
            get { return $"DB status : {State}"; }
        }

        private string state = "";
        public string State {
            get => state;
            private set => SetProperty(ref state, value);
        }

        private string autoStartStopButtonText = "自動起動 ON";
        public string AutoStartStopButtonText {
            get => autoStartStopButtonText;
            set => SetProperty(ref autoStartStopButtonText, value);
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

        public int AutoStartMinutesCounter {
            get => autoStartMinutesCounter;
            set {
                SetProperty(ref autoStartMinutesCounter, value);
                RaisePropertyChanged(nameof(AutoStartAnnouncement));
            }
        }

        private int autoStartMinutesCounter = 0;

        public string AutoStartAnnouncement {
            get => AutoStartMinutesCounter > 0 ? $"{autoStartMinutesCounter}分後に起動します。" : "";
        }

        public string autoStartAnnouncement = "";

        public string IconImagePath {
            get {
                return (State == "available" || State == "starting" || State == "backing-up" || State == "configuring-enhanced-monitoring") 
                    ? "/Images/running.png" 
                    : "/Images/stopped.png" ;

            }
        }

        public MainWindowViewModel() {
            StartDBInstanceCommand = 
                new DelegateCommand(() => {
                    cliExecuter.startDBInstance(cliExecuter.getDBInstanceIdentifier());
                    additionCheckTimer.Start();
                    DisableAutoStartCommand.Execute();
                    updateDBInstanceStatus("start");
                });

            StopDBInstanceCommand =
                new DelegateCommand(() => {
                    cliExecuter.stopDBInstance(cliExecuter.getDBInstanceIdentifier());
                    updateDBInstanceStatus("stop");
                });

            UpdateDBInstanceStatusCommand =
                new DelegateCommand(() => {
                    updateDBInstanceStatus("getStatus");
                });

            updateDBInstanceStatus("getStatus(init)");

            int updateInterval = 30;
            UpdateIntervalMinuteString = updateInterval.ToString();

            timer.Tick += (sender, e) => UpdateDBInstanceStatusCommand.Execute();
            timer.Start();

            additionCheckTimer.Interval = new TimeSpan(0, 5, 0);
            additionCheckTimer.Tick += (sendre, e) => {
                var soundPlayer = new System.Media.SoundPlayer(@"C:\Windows\Media\Windows Notify Messaging.wav");
                soundPlayer.Play();
                additionCheckTimer.Stop();
                updateDBInstanceStatus("getStatus(auto)");
            };

            autoStartTimer.Interval = new TimeSpan(0, 1, 0);
            autoStartTimer.Tick += (sender, e) => {
                if (autoStartMinutesCounter < 0) {
                    return;
                }
                else {
                    AutoStartMinutesCounter--;
                }

                if (autoStartMinutesCounter == 0) {
                    var soundPlayer = new System.Media.SoundPlayer(@"C:\Windows\Media\Windows Notify Messaging.wav");
                    soundPlayer.Play();
                    StartDBInstanceCommand.Execute();
                }
            };

            if(State != "available") {
                autoStartMinutesCounter = 5;
            }

            autoStartTimer.Start();
        }

        public string Output { get => output; set => SetProperty(ref output, value); }
        private string output = "";

        private DispatcherTimer timer = new DispatcherTimer();
        private DispatcherTimer additionCheckTimer = new DispatcherTimer();
        private DispatcherTimer autoStartTimer = new DispatcherTimer();

        public DelegateCommand StartDBInstanceCommand { get; private set;}
        public DelegateCommand StopDBInstanceCommand { get; private set;}
        public DelegateCommand UpdateDBInstanceStatusCommand { get; private set;}

        private DelegateCommand disableAutoStartCommand;
        public DelegateCommand DisableAutoStartCommand {
            get => disableAutoStartCommand ?? (disableAutoStartCommand = new DelegateCommand(() => {
                autoStartTimer.Stop();
                AutoStartStopButtonText = "自動起動 OFF";
            }));
        }


        public DelegateCommand<Object> SetAutoStartCommand {
            #region
            get => setAutoStartCommand ?? (setAutoStartCommand = new DelegateCommand<Object>((Object numberString) => {
                AutoStartMinutesCounter = int.Parse((String)numberString);
                StopDBInstanceCommand.Execute();
            }));
        }
        private DelegateCommand<Object> setAutoStartCommand;
        #endregion

        private CLICommands cliExecuter = new CLICommands();

        /// <summary>
        /// getDBInstanceStatus の戻り値から DBInstance の現在の状態を表す文字列のみを抽出します
        /// </summary>
        /// <returns></returns>
        private string extractDBInstanceState(string text) {
            var regex = new Regex("\"DBInstanceStatus\": \"(.*)\"", RegexOptions.IgnoreCase);
            return regex.Matches(text)[0].Groups[1].Value;
        }

        private void updateDBInstanceStatus(string commandType) {
            Output = cliExecuter.getDBInstanceStatus();
            State = extractDBInstanceState(Output);
            RaisePropertyChanged(nameof(IconImagePath));
            RaisePropertyChanged(nameof(Title));
            Logs.Insert(0, new Log(commandType, State, DateTime.Now));
            LastUpdateDateTime = DateTime.Now;
        }
    }
}

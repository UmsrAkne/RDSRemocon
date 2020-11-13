using Prism.Commands;
using Prism.Mvvm;
using System;
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

        public MainWindowViewModel() {
            StartDBInstanceCommand = 
                new DelegateCommand(() => { startDBInstance(getDBInstanceIdentifier()); });

            StopDBInstanceCommand =
                new DelegateCommand(() => { stopDBInstance(getDBInstanceIdentifier()); });

            UpdateDBInstanceStatusCommand =
                new DelegateCommand(() => {
                    Output = getDBInstanceStatus();
                    State = extractDBInstanceState(Output);
                });

            timer.Interval = new TimeSpan(0, 15, 0);
            timer.Tick += (sender, e) => UpdateDBInstanceStatusCommand.Execute();
            timer.Start();
        }

        public string Output { get => output; set => SetProperty(ref output, value); }
        private string output = "";

        private DispatcherTimer timer = new DispatcherTimer();

        public DelegateCommand StartDBInstanceCommand { get; private set;}
        public DelegateCommand StopDBInstanceCommand { get; private set;}
        public DelegateCommand UpdateDBInstanceStatusCommand { get; private set;}

        /// <summary>
        /// 現状 DBInstance は一台しか使っていないので、返却値は単一の文字列。
        /// </summary>
        /// <returns></returns>
        private string getDBInstanceIdentifier (){
            process.StartInfo.Arguments = @"/c aws rds describe-db-instances";
            process.Start();

            string text = process.StandardOutput.ReadToEnd();

            var regex = new Regex("\"DBInstanceIdentifier\": \"(.*)\"", RegexOptions.IgnoreCase);
            var matches = regex.Matches(text);
            return matches[0].Groups[1].Value;
        }

        private void startDBInstance(string instanceName) {
            string commandText = @"/c aws rds start-db-instance --db-instance-identifier ";
            process.StartInfo.Arguments = commandText + instanceName;
            process.Start();
            Output = process.StandardOutput.ReadToEnd();
        }

        private void stopDBInstance(string instanceName) {
            string commandText = @"/c aws rds stop-db-instance --db-instance-identifier ";
            process.StartInfo.Arguments = commandText + instanceName;
            process.Start();
            Output = process.StandardOutput.ReadToEnd();
        }

        private string getDBInstanceStatus() {
            process.StartInfo.Arguments = @"/c aws rds describe-db-instances";
            process.Start();
            return process.StandardOutput.ReadToEnd();
        }

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

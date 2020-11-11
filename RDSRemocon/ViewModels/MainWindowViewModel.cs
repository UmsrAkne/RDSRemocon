using Prism.Mvvm;
using System.Diagnostics;
using System.Text.RegularExpressions;

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

        public MainWindowViewModel() {

        }

        public string Output { get => output; set => SetProperty(ref output, value); }
        private string output = "";

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
    }
}

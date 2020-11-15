using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RDSRemocon.Models {
    class CLICommands {

        private Process process = new Process() {
            StartInfo = new ProcessStartInfo() {
                FileName = System.Environment.GetEnvironmentVariable("ComSpec"),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardInput = false,
                CreateNoWindow = true
            }
        };

        /// <summary>
        /// 現状 DBInstance は一台しか使っていないので、返却値は単一の文字列。
        /// </summary>
        /// <returns></returns>
        public string getDBInstanceIdentifier() {
            process.StartInfo.Arguments = @"/c aws rds describe-db-instances";
            process.Start();

            string text = process.StandardOutput.ReadToEnd();

            var regex = new Regex("\"DBInstanceIdentifier\": \"(.*)\"", RegexOptions.IgnoreCase);
            var matches = regex.Matches(text);
            return matches[0].Groups[1].Value;
        }

        public string startDBInstance(string instanceName) {
            string commandText = @"/c aws rds start-db-instance --db-instance-identifier ";
            process.StartInfo.Arguments = commandText + instanceName;
            process.Start();
            return process.StandardOutput.ReadToEnd();
        }

        public string stopDBInstance(string instanceName) {
            string commandText = @"/c aws rds stop-db-instance --db-instance-identifier ";
            process.StartInfo.Arguments = commandText + instanceName;
            process.Start();
            return process.StandardOutput.ReadToEnd();
        }

        public string getDBInstanceStatus() {
            process.StartInfo.Arguments = @"/c aws rds describe-db-instances";
            process.Start();
            return process.StandardOutput.ReadToEnd();
        }


    }
}

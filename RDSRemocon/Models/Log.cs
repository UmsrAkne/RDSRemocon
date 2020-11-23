using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDSRemocon.Models {
    public class Log : BindableBase{

        private string executionCommandType;
        private string message;
        private DateTime executionDateTime;
        private string executionDateTimeShortString;

        public Log(string executionCommandType ,string message, DateTime executionDateTime) {
            ExecutionCommandType = executionCommandType;
            Message = message;
            ExecutionDateTime = executionDateTime;
        }

        public string ExecutionCommandType {
            get => executionCommandType; set => SetProperty(ref executionCommandType, value);
        }

        public string Message {
            get => message; set => SetProperty(ref message, value);
        }

        public DateTime ExecutionDateTime {
            get => executionDateTime;
            set {
                SetProperty(ref executionDateTime, value);
                ExecutionDateTimeShortString = executionDateTime.ToString("MM/dd/yy HH:mm:ss");
            }
        }

        public string ExecutionDateTimeShortString {
            get => executionDateTimeShortString;
            private set => SetProperty(ref executionDateTimeShortString, value);
        }

    }
}

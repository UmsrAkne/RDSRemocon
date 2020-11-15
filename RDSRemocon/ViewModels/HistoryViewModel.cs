using Prism.Mvvm;
using RDSRemocon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDSRemocon.ViewModels {
    public class HistoryViewModel : BindableBase {

        private List<Log> logs = new List<Log>();

        public HistoryViewModel() {
        }

        public List<Log> Logs {
            get => logs; set => SetProperty(ref logs, value);
        }
    }
}

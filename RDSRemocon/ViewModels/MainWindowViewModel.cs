using Prism.Mvvm;

namespace RDSRemocon.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
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
    }
}

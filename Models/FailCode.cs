using PlasticsApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlasticsApp.Models
{
    public class FailCode : ViewModelBase
    {
        private string _smallFailCode = string.Empty; 
        private string _largeFailCode = string.Empty;
        private string _description = string.Empty;
        public string SmallFailCode
        {
            get { return _smallFailCode; }
            set 
            { 
                _smallFailCode = value;
                OnPropertyChanged(nameof(SmallFailCode));
            }
        }
        public string FullFailCode
        {
            get { return _largeFailCode; }
            set
            {
                _largeFailCode = value;
                OnPropertyChanged(nameof(FullFailCode));
            }
        }
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }
    }
}

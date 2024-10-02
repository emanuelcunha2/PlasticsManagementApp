using PlasticsApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlasticsApp.Models
{
    public class ChangeOverProblem : ViewModelBase
    {
        private int _id = -1;
        public int Id
        {
            get { return _id; }
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _issue = string.Empty;
        public string Issue
        {
            get { return _issue; }
            set
            {
                if (_issue != value)
                {
                    _issue = value;
                    OnPropertyChanged();
                }
            }
        }

    }
}

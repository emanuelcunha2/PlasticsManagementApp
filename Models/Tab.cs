
using PlasticsApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlasticsApp.Models
{
    public class Tab : ViewModelBase
    {
        public int Number { get; set; }
        private bool _isVisible = false;
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                OnPropertyChanged(nameof(IsVisible));
            }
        }
        public Tab(int number)
        {
            Number = number;
        }
    }
}

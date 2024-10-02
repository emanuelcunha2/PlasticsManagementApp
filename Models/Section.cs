using PlasticsApp.Commands;
using PlasticsApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PlasticsApp.Models
{
    public class Section : ViewModelBase
    {
        public Func<bool> CanGoToThisSection { get; set; } = new Func<bool>(() => { return false; });
        public Func<bool> ButtonClicked { get; set; } =  new Func<bool>(() => { return false; });
        public int Number { get; set; }
        public bool IsActive { get => IsVisible; }

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
        private bool _isPartOfTemplate = true;
        public bool IsPartOfTemplate
        {
            get => _isPartOfTemplate;
            set
            {
                _isPartOfTemplate = value;
                OnPropertyChanged(nameof(IsPartOfTemplate));
            }
        }

        public Section(int number)
        {
            Number = number;
        }
    }
}

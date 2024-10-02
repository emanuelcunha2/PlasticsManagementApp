using PlasticsApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlasticsApp.Models
{
    public class Event : ViewModelBase
    {
        private int _id = -1;
        public int Id 
        {
            get {  return _id; }
            set
            {
                _id = value;

                if(value == 14 ) { IsCavitiesEvent = true; }
                else { IsCavitiesEvent = false; }

                OnPropertyChanged();
            }
        }

        private int _stopId = -1;
        public int StopId
        {
            get { return _stopId; }
            set
            {
                _stopId = value;
                OnPropertyChanged();
            }
        }

        private string _description = string.Empty;
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        private string _informations = string.Empty;
        public string Informations
        {
            get { return _informations; }
            set
            {
                _informations = value; 
                OnPropertyChanged();
            }
        }

        private bool _isCavitiesEvent = false;
        public bool IsCavitiesEvent
        {
            get => _isCavitiesEvent;
            set
            {
                _isCavitiesEvent = value;
                OnPropertyChanged();
            }
        }


        private string _causes = string.Empty;
        public string Causes
        {
            get { return _causes; }
            set
            {
                _causes = value;
                OnPropertyChanged();
            }
        }

    }
}

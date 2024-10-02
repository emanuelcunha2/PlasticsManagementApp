using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlasticsApp.Models
{
    /// <summary>
    /// Represents an item for the main page.
    /// This class contains information about a specific item that appears on the main page.
    /// </summary>
    public class PageSelectionItem
    {
        private string _designation = string.Empty;
        public string Designation
        {
            get { return _designation; }
            set { if(_designation != value) { _designation = value;} }
        }
        private string _imageSource = string.Empty;
        public string ImageSource
        {
            get { return _imageSource; }
            set { if (_imageSource != value) { _imageSource = value; } }
        }
        private Type _destinationPage { get; set; }
        public Type DestinationPage { get { return _destinationPage; } }

        public PageSelectionItem(Type destinationPage)
        {
            _destinationPage = destinationPage;
        }

    }
}

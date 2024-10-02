using Microsoft.IdentityModel.Tokens;
using PlasticsApp.Services;
using PlasticsApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlasticsApp.Models
{
    public class ProductionOrder : ViewModelBase
    {
        public bool Exists { get; } = false;
        private string _orderNumber = string.Empty;
        public string OrderNumber
        {
            get { return _orderNumber; }
            set
            {
                if (_orderNumber != value)
                {
                    _orderNumber = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private string _orderId = string.Empty;
        public string OrderId
        {
            get { return _orderId; }
            set
            {
                if (_orderId != value)
                {
                    _orderId = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _partNumber = string.Empty;
        public string PartNumber
        {
            get { return _partNumber; }
            set
            {
                if (_partNumber != value)
                {
                    _partNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _quantity = string.Empty;
        public string Quantity
        {
            get { return _quantity; }
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _machine = string.Empty;
        public string Machine
        {
            get { return _machine; }
            set
            {
                if (_machine != value)
                {
                    _machine = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _quantityWIP = 0;
        public int QuantityWIP
        {
            get { return _quantityWIP; }
            set
            {
                _quantityWIP = value;
                OnPropertyChanged();
            }
        }

        private string _material;
        public string Material
        {
            get { return _material; }
            set
            {
                _material = value;
            }
        }

        private string _material2;
        public string Material2
        {
            get { return _material2; }
            set
            {
                _material2 = value;
                OnPropertyChanged();
            }
        }

        private string _materialQuantity;

        private string _toolNumber;
        public string ToolNumber
        {
            get { return _toolNumber; }
            set
            {
                _toolNumber = value;
                OnPropertyChanged();
            }
        }
        private string _toolBarcode;
        public string ToolBarcode
        {
            get { return _toolBarcode; }
            set
            {
                _toolBarcode = value;
                OnPropertyChanged();
            }
        }

        public bool Blocked { get; } = false;

        private int _traceId = 0;
        private string _granulateBarcode;
        public string GranulateBarcode
        {
            get { return _granulateBarcode; }
            set
            {
                _granulateBarcode = value;
                OnPropertyChanged();
            }
        } 
        private string _granulateBarcode2;
        public string GranulateBarcode2
        {
            get { return _granulateBarcode2; }
            set
            {
                _granulateBarcode2 = value;
                OnPropertyChanged();
            }
        }

        private DateTime _injectionDate;
        public DateTime DateWIP { get; }

        private DateTime _warehouseDate;
        private string _warehouseBarcode;
        public string MtsBarcode { get; }
        public bool IsMts { get; }

        public ProductionOrder(string material, string material2, string materialQuantity, string toolNumber, string toolBarcode, bool blocked, int traceId, string granulateBarcode,
                               DateTime injectionDate, DateTime dateWIP, DateTime warehouseDate, string warehouseBarcode, string mtsBarcode)
        {
            Exists = true;
            _material = material;
            _material2 = material2;
            _materialQuantity = materialQuantity;
            _toolNumber = toolNumber;
            _toolBarcode = toolBarcode;
            Blocked = blocked;
            _traceId = traceId;
            _granulateBarcode = granulateBarcode;
            _injectionDate = injectionDate;
            DateWIP = dateWIP;
            _warehouseDate = warehouseDate;
            _warehouseBarcode = warehouseBarcode;
            MtsBarcode = mtsBarcode;

            IsMts = true;
            if (MtsBarcode.IsNullOrEmpty()) { IsMts = false; }
        }

        public ProductionOrder()
        {

        }
         
    }
}

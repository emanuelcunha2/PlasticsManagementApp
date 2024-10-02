using PlasticsApp.Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlasticsApp.Models
{
    public class Dehumidifier
    {
        private string _name;
        private string _granulateBC;
        private string _granulateBatch;
        private DateTime _startDate;
        private DateTime _endDate;
        private double _loadWeight;
        private bool _canBeUsed = false;
        private string _granulatePartnumber;
        private int _quantity;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string GranulateBatch
        {
            get { return _granulateBatch; }
            set { _granulateBatch = value; }
        }

        public string GranulateBC
        {
            get { return _granulateBC; }
            set { _granulateBC = value; }
        }

        public DateTime StartDate
        {
            get { return _startDate; }
            set { _startDate = value; }
        }

        public DateTime EndDate
        {
            get { return _endDate; }
            set { _endDate = value; }
        }

        public double LoadWeight
        {
            get { return _loadWeight; }
            set { _loadWeight = value; }
        }

        public bool CanBeUsed
        {
            get { return _canBeUsed; }
            set { _canBeUsed = value; }
        }

        public string GranulatePartnumber
        {
            get { return _granulatePartnumber; }
            set { _granulatePartnumber = value; }
        }

        public int Quantity
        {
            get { return _quantity; }
            set { _quantity = value; }
        }

        public Dehumidifier() 
        {

        }
    }
}

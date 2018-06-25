using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;

namespace Corron.CarService
{
    [DataContract]
    public class ServiceModel : PropertyChangedBase, IComparable<ServiceModel>, IDataErrorInfo, IEditableObject
    {
 
        private ServiceModel _editCopy;
        private List<ServiceLineModel> _editServiceLines;
 
        const string MONEY_FORMAT = "{0:0.00}";
        public readonly string[] _validateProperties = { "TechName", "ServiceDate" };



        // Constructors
        public ServiceModel(int carID)
        {
            CarID = carID;
            Initialize();
        }

        public ServiceModel()
        {
            Initialize();
        }

        private void Initialize()
        {
            _serviceLineList = new List<ServiceLineModel>();
        }

        // Properties
        public ServiceLineModel CurrentServiceLine { get; set; }

        [DataMember]
        public List<ServiceLineModel> ServiceLineList
        {
            get
            {
                return
                _serviceLineList;
            }
        }
        private List<ServiceLineModel> _serviceLineList;


        [DataMember]
        public int ServiceID { get; set; }

        [DataMember]
        public int CarID { get; set; }

        [DataMember]
        public DateTime ServiceDate
        {
            get { return _serviceDate; }
            set {
                _serviceDate = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => IsValidState);
            }
        }
        private DateTime _serviceDate;


        [DataMember]
        public string TechName
        {
            get { return _techName; }
            set {
                _techName = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(()=>IsValidState);
            }
        }
        private string _techName;

        [DataMember]
        public decimal LaborCost
        {
            get { return _laborCost; }
            set
            {
                _laborCost = value;
                NotifyOfPropertyChange(()=>LaborCostString);
            }
        }
        private decimal _laborCost;

        public string LaborCostString
        {
            get
            {
                return String.Format(MONEY_FORMAT, _laborCost);
            }
        }

        [DataMember]
        public decimal PartsCost
        {
            get { return _partsCost; }
            set
            {
                _partsCost = value;
                NotifyOfPropertyChange(()=>PartsCostString);
            }
        }
        private decimal _partsCost;

        public string PartsCostString
        {
            get
            {
                return String.Format(MONEY_FORMAT,_partsCost);
            }
 
        }

        public decimal TotalCost
        {
            get
            {
                return _partsCost + _laborCost;
            }
        }

        public bool IsValidState
        {
            get
            {
                if (_validateProperties.Any(s => !(this[s] is null)))
                    return false;
                return ServiceLinesAreValidState;
            }
        }
        
        public bool ServiceLinesAreValidState
        {
            get
            {
                if (ServiceLineList is null)
                    return false;
                if (ServiceLineList.Count == 0)
                    return false;
                return ServiceLineList.All(s => s.IsValidState);
            }
        }

        public void NotifyValidDetail() // called through a delegate from detail line when needed
        {
            NotifyOfPropertyChange(() => IsValidState);
        }

        public void RecalcCost()
        {
            decimal pCost=0, lCost=0;

            foreach(ServiceLineModel serviceLine in _serviceLineList)
            {
                if (serviceLine.Delete == 0)
                {
                    switch (serviceLine.ServiceLineType)
                    {
                        case ServiceLineModel.LineTypes.Labor:
                            lCost += serviceLine.ServiceLineCharge;
                            break;
                        case ServiceLineModel.LineTypes.Parts:
                            pCost += serviceLine.ServiceLineCharge;
                            break;
                    }
                }
            }
            LaborCost = lCost; PartsCost = pCost;
        }

        // Implements IComparable
        public int CompareTo(ServiceModel rightService)
        {
            ServiceModel leftService = this;
            return leftService.ServiceDate.CompareTo(rightService.ServiceDate);
        }

        // Implements IDataErrorInfo
        public string Error => throw new NotImplementedException();

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case "TechName": return Validation.FiftyNoBlanks(TechName);
                    case "ServiceDate":
                        if (ServiceDate.Year < 2010 || ServiceDate.Year > 2050) return "Date out of range.";
                        break;
                }
                return null;
            }
        }
      
        // Implements IEditableObject
        public void BeginEdit()
        {
            //make a copy of the original in case cancels
            ObjectCopier.CopyFields(_editCopy = new ServiceModel(0), this);
            CopyDetailLines(ref _editServiceLines,_serviceLineList);
            foreach (ServiceLineModel SL in _serviceLineList)
            {
                SL.SnapShotCharge();
            }
            ServiceLineModel.RecalcAction = RecalcCost;
            ServiceLineModel.ValidChangedAction = NotifyValidDetail;
            NotifyOfPropertyChange(()=>IsValidState);
        }

        public void EndEdit()
        {
            _editCopy = null;
            _editServiceLines = null;
            ServiceLineModel.RecalcAction = null;
        }

        public void CancelEdit()
        {
            ServiceLineModel.RecalcAction = null;

            ObjectCopier.CopyFields(this, _editCopy);
            _editCopy = null;

            CopyDetailLines(ref _serviceLineList,_editServiceLines);

        }

        private void CopyDetailLines(ref List<ServiceLineModel> to, List<ServiceLineModel> from)
        {
            if (to is null)
                to = new List<ServiceLineModel>();
            else
                to.Clear();
            foreach (ServiceLineModel item in from)
            {
                to.Add(new ServiceLineModel(item));
            }
        }


    }
}

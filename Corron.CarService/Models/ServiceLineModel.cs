using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Runtime.Serialization;

namespace Corron.CarService
{
    [DataContract]
    public class ServiceLineModel : PropertyChangedBase, IDataErrorInfo
    {

        public enum LineTypes : byte { Labor, Parts };
        const string MONEY_FORMAT = "{0:0.00}";

        private decimal[] _calcLineCharge;
        private static LineTypes _lastLineType;
        private bool? _validState;

        public delegate void RecalcDelegate(); //delegate type declaration for recalc
        public static RecalcDelegate RecalcAction; //store the recalc method once for all (static)

        public delegate void ValidChangedDelegate();
        public static ValidChangedDelegate ValidChangedAction;



        //constructors

        public ServiceLineModel()
        {
            ServiceLineType = _lastLineType;
            ServiceLineDesc = "";
            ServiceLineCharge = 0;
            _calcLineCharge = new decimal[] {0M,0M};
            _validState = null;

            if (ValidChangedAction is null)
                return;
            ValidChangedAction();
        }

        public ServiceLineModel(ServiceLineModel copy)
        {
            ObjectCopier.CopyFields(this, copy);
        }

        //properties

        [DataMember]
        public int ServiceID { get; set; }

        public byte ServiceLineOrder { get; set; } //used only to retrieve from storage in original order entered

        [DataMember]
        public LineTypes ServiceLineType
        {
            get { return _serviceLineType; }
            set
            {
                _serviceLineType = value;
                _lastLineType = value;
                DoRecalc();
                //NotifyOfPropertyChange();
            }
        }
        private LineTypes _serviceLineType;

        public string ServiceLineTypeString
        {
            get { return new[] { "Labor", "Parts" }[(int)_serviceLineType]; }
        }

        [DataMember]
        public string ServiceLineDesc
        {
            get { return _serviceLineDesc; }
            set
            {
                _serviceLineDesc = value;
                NotifyIfValidChanged();
            }
        }
        private string _serviceLineDesc;

        [DataMember]
        public decimal ServiceLineCharge
        {
            get { return _serviceLineCharge; }
            set
            {
                _serviceLineCharge = value;
                Validation.ValidateCost(value);
                NotifyIfValidChanged();
                DoRecalc();
            }
        }
        private decimal _serviceLineCharge;

        public byte Delete {
            get
            {
                return _delete;
            }
            set
            {
                _delete = value;
                DoRecalc();
                //NotifyOfPropertyChange();
            }
        }
        private byte _delete;

 

        public bool IsValidState
        {
            get
            {
                if (new string[] { "ServiceLineDesc", "ServiceLineCharge" }.Any(s => !(this[s] is null)))
                    return false;
                else
                    return true;
            }
        }

        // Methods

        private void NotifyIfValidChanged()
        {
            if (ValidChangedAction is null) //used stand-alone in web service
                return;

            bool newValidState = IsValidState;
            if (newValidState == _validState) //_validState is Nullable, so only test that works is equality
                return;
            ValidChangedAction();
            _validState = newValidState;
        }

        public void DoRecalc()
        {
            if (RecalcAction is null) //used stand-alone in web service
                return;
            RecalcAction();
        }
        public void SnapShotCharge() //allows efficient recalc whenever total charges are recalculated
        {
            if (Delete !=0)
            {
                _calcLineCharge[0] = 0M; _calcLineCharge[1] = 0M;
                return;
            }

            _calcLineCharge[0] = (_serviceLineType == LineTypes.Labor && Delete == 0) ? _serviceLineCharge : 0;
            _calcLineCharge[1] = (_serviceLineType == LineTypes.Parts && Delete == 0) ? _serviceLineCharge : 0;
        }

        public decimal[] ChargeChanges() //retrieves changes in line charge since the last snapshot, then takes a new snapshot
        {
            decimal[] change = new decimal[] { 0M , 0M } ;

            if (_serviceLineType == LineTypes.Labor && Delete == 0)
                change[0] = _serviceLineCharge - _calcLineCharge[0];
            else
                change[0] =  - _calcLineCharge[0];

            if (_serviceLineType == LineTypes.Parts && Delete == 0 )
                change[1] = _serviceLineCharge - _calcLineCharge[1];
            else
                change[1] = -_calcLineCharge[1];

            SnapShotCharge();
            return change;
        }


        // Implements IDataErrorInfo
        public string Error
        {
            get
            {
                return string.Empty;
            }
        }


        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case "ServiceLineDesc": return Validation.FiftyNoBlanks(ServiceLineDesc);
                    case "ServiceLineCharge": return Validation.ValidateCost(ServiceLineCharge);
                    default:
                        return "Invalid Column Name";
                }
            }
        }
    }
}

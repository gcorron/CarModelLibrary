using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Corron.CarService
{
    public interface IServiceModel
    {
//core properties
        int ServiceID { get; set; }
        int CarID { get; set; }

        string TechName { get; set; }
        DateTime ServiceDate { get; set; }

        decimal LaborCost { get; set; }
        decimal PartsCost { get; set; }
        decimal TotalCost { get; }
 //validation properties
        bool ServiceLinesAreValidState { get; }
        bool IsValidState { get; }

 //detail lines
        IServiceLineModel CurrentServiceLine { get; set; }
        List<IServiceLineModel> ServiceLineList { get; }

 //notification methods called from detail line
        void NotifyValidDetail();
        void RecalcCost();

// implements IEditableObject
        void BeginEdit();
        void CancelEdit();
        void EndEdit();

// implements IComparable
        int CompareTo(ServiceModel rightService);

//implements IDataErrorInfo
        string this[string columnName] { get; }
        string Error { get; }

    }
}
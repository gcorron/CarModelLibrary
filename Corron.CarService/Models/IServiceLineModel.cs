namespace Corron.CarService
{
    public interface IServiceLineModel
    {
//core properties
        ServiceLineModel.LineTypes ServiceLineType { get; set; }
        string ServiceLineTypeString { get; }
        string ServiceLineDesc { get; set; }
        decimal ServiceLineCharge { get; set; }
        bool Delete { get; set; }
//for database access only
        int ServiceID { get; set; }
        byte ServiceLineOrder { get; set; }

        bool IsValidState { get; }

        void DoRecalc();

 //implements IDataErrorInfo
        string Error { get; }
        string this[string columnName] { get; }

    }
}
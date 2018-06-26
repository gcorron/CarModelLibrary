namespace Corron.CarService
{
    public interface ICarModel
    {
 
        int CarID { get; set; }
        string Make { get; set; }
        string Model { get; set; }
        string Owner { get; set; }
        int Year { get; set; }

        string Error { get;}
        string this[string columnName] { get; }

        void BeginEdit();
        void CancelEdit();
        void EndEdit();
        int CompareTo(CarModel rightCar);

    }
}
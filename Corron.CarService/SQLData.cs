
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using Dapper;

namespace Corron.CarService
{
    public static class SQLData
    {
        public delegate void HandleError(string Message);

        private static HandleError _handleError;
 
        public static void Initialize(HandleError handleError)
        {
            _handleError = handleError;
        }

        public static List<CarModel> GetCars()
        {
            try
            {
                using (IDbConnection connection = GetJoesDBConnection())
                {
                    return connection.Query<CarModel>("SelectCars").ToList();
                }
            }
            catch (Exception e)
            {
                if (e.InnerException is null)
                    _handleError(e.Message);
                else
                    _handleError(e.InnerException.Message);
                return null;
            }
        }

        public static List<ServiceModel> GetServices(int CarID)
        {
            var lookup = new Dictionary<int, ServiceModel>();
            try
            {
                using (IDbConnection connection = GetJoesDBConnection())
                {
                    connection.Query<ServiceModel, ServiceLineModel, ServiceModel>
                        ($"SelectServicesForCar @CarID", (S, L) =>
                        {
                            ServiceModel SM;
                            if (!lookup.TryGetValue(S.ServiceID, out SM))
                            {
                                lookup.Add(S.ServiceID, SM = S);
                            }

                            SM.ServiceLineList.Add(L);
                            return SM;

                        }, new { CarID }, splitOn: "ServiceID");
                }
                return lookup.Values.ToList() as List<ServiceModel>;
            }
            catch (Exception e)
            {
                _handleError(e.Message);
                return null;
            }
        }

        public static bool UpdateCar(ICarModel car)
        {
            try
            {
                using (IDbConnection connection = GetJoesDBConnection())
                {

                    List<int> results;
                    results = connection.Query<int>("dbo.UpdateCar @CarID, @Make, @Model, @Year, @Owner", car) as List<int>;

                    if (results[0] >= 0 && car.CarID <= 0)
                        car.CarID = results[0];
                }
            }
            catch (Exception e)
            {
                _handleError(e.Message);
                return false;
            }
            return true;
        }

        public static bool UpdateService(ServiceModel service)
        {
            try
            {
                using (IDbConnection connection = GetJoesDBConnection())
                {
                    List<int> results;
                    results = connection.Query<int>("dbo.UpdateService @ServiceID, @ServiceDate, @TechName, @LaborCost, @PartsCost, @CarID", service) as List<int>;

                    if (results[0] >= 0 && service.ServiceID <= 0)
                        service.ServiceID = results[0];

                    List<IServiceLineModel> SL = service.ServiceLineList;
                    if (SL.Count > 255)
                        throw new Exception("Too many detail lines!");
                    else
                    {
                        byte b = 0;
                        foreach (ServiceLineModel s in SL)
                        {
                            s.ServiceLineOrder = b++; //save the order
                            s.ServiceID = service.ServiceID;
                        }
                        // stored procedure deletes all lines before inserting new ones
                        connection.Execute($"dbo.UpdateServiceLine @ServiceID, @ServiceLineOrder, @ServiceLineType, @ServiceLineDesc, @ServiceLineCharge", SL);
                    }
                }
            }
            catch (Exception e)
            {
                _handleError(e.Message);
                return false;
            }
            return true;
        }

        public static string WebConnection()
        {
            return CnnVal("WebConnection");
        }

        public static IDbConnection GetJoesDBConnection()
        {
            return new System.Data.SqlClient.SqlConnection(CnnVal("Joes"));
        }

        public static string CnnVal(string name)
        {
            try
            {
                return ConfigurationManager.ConnectionStrings[name].ConnectionString;
            }
            catch
            {
                throw new ArgumentException($"Database connection for {name} not found in app.config.");
            }
        }
    }
}

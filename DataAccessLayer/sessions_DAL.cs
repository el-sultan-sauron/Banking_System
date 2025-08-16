using Microsoft.Extensions.Configuration;
using Npgsql;



namespace DataAccessLayer
{
    public class sessions_DAL
    {
       static public int AccountLogedToSystem(int accountid)
        {
            var s = ConfigurationHelper.GetConfiguration();
            var ConnectionString = s.GetConnectionString("My_DB_Connection");

          const string query = "insert into sessions(accountid) values(@accountid) returning accountid;";
             

            try
            {
                using var connection = new NpgsqlConnection(ConnectionString);
                using var command = new NpgsqlCommand(query, connection);

                command.Parameters.AddWithValue("@accountid",accountid);
                connection.Open();
                
                return Convert.ToInt32( command.ExecuteScalar());
               
            }
            catch (PostgresException pgEx) { Console.Error.WriteLine(pgEx.Message); }
            catch (Exception ex) { Console.Error.WriteLine(ex.Message); }

            return -1;
        }
    }
}

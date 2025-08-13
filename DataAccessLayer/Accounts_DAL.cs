using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
namespace DataAccessLayer
{

  
   static class ConfigurationHelper
    {
        private static IConfiguration? _configuration;

        public static IConfiguration GetConfiguration()
        {
            if (_configuration == null)
            {
                _configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();
            }
            return _configuration;
        }
    }
   public  class Accounts_DAL
    {
        static public int  AddNewAccount(string fullname, string email,string passwordhash)
        {

            var s = ConfigurationHelper.GetConfiguration();
            var ConnectionString = s.GetConnectionString("My_DB_Connection");


            const string query = "insert into accounts (fullname,email,passwordhash) values (@fullname,@email,@passwordhash); select scope_identity();";
            try
            {
                using var connection = new SqlConnection(ConnectionString);
                using var command = new SqlCommand(query, connection);

                command.Parameters.Add("@fullname", SqlDbType.VarChar, 255).Value = fullname;
                command.Parameters.Add("@email", SqlDbType.VarChar, 255).Value = email;
                command.Parameters.Add("@passwordhash", SqlDbType.VarChar, 255).Value = passwordhash;

                connection.Open();

                var result = command.ExecuteScalar();

                return result != null ? Convert.ToInt32(result) : -1;

            }
            catch (SqlException sqlEx) {

                if (sqlEx.Number == 2627) Console.Error.WriteLine("Email already exists.");
                else Console.Error.WriteLine($"SQL Error {sqlEx.Number}: {sqlEx.Message}");
                
                    
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }

            return -1;
        }
        static public bool UpdateAccount(int AccountID, string fullname, string email,string passwordhash)
        {
            var s = ConfigurationHelper.GetConfiguration();
            var ConnectionString = s.GetConnectionString("My_DB_Connection");

            const string query = "update accounts set fullname = @fullname,email = @email,passwordhash = @passwordhash where accountid = @accountid;";
            try
            {
                using var connection = new SqlConnection(ConnectionString);
                using var command = new SqlCommand(query, connection);

                command.Parameters.Add("@accountid",SqlDbType.Int).Value = AccountID;
                command.Parameters.Add("@fullname",SqlDbType.VarChar,255).Value = fullname;
                command.Parameters.Add("@email", SqlDbType.VarChar, 255).Value = email;
                command.Parameters.Add("@passwordhash", SqlDbType.VarChar, 255).Value = passwordhash;

                connection.Open();              

                return  command.ExecuteNonQuery() > 0;
            }

            catch (SqlException sqlEx) { Console.Error.WriteLine(sqlEx.Message); }

            catch(Exception ex) { Console.Error.WriteLine(ex.Message); }

            return false;
    
        }
        static public bool DeleteAccount(int AccountID) {

            var s = ConfigurationHelper.GetConfiguration();
            var ConnectionString = s.GetConnectionString("My_DB_Connection");

           const string query = "update accounts set isdeleted = 1 where accountid = @accountid;";

            try
            {
               using var connection = new SqlConnection(ConnectionString);
               using var command = new SqlCommand(query,connection);

                command.Parameters.Add("@accountid",SqlDbType.Int).Value = AccountID;

                connection.Open();

                return command.ExecuteNonQuery() > 0;
            }

            catch(SqlException SqlEx) { Console.Error.WriteLine(SqlEx.Message); }

            catch (Exception ex) { Console.Error.WriteLine(ex.Message); }

            return false;
        }
        static public bool RetriveAccount(int AccountID) {

            var s = ConfigurationHelper.GetConfiguration();
            var ConnectionString = s.GetConnectionString("My_DB_Connection");

           const string query = "update accounts set isdeleted = 0 where accountid = @accountid;";

            try
            {
                using var connection = new SqlConnection(ConnectionString);
                using var command = new SqlCommand(query, connection);

                command.Parameters.Add("@accountid", SqlDbType.Int).Value = AccountID;

                connection.Open();

                return command.ExecuteNonQuery() > 0;
            }

            catch (SqlException SqlEx) { Console.Error.WriteLine(SqlEx.Message); }

            catch (Exception ex) { Console.Error.WriteLine(ex.Message); }

            return false;
        }
        static public bool IsAccountDeleted(int AccountID) {

            var s = ConfigurationHelper.GetConfiguration();
            var ConnectionString = s.GetConnectionString("My_DB_Connection");

           const string query = "select isdeleted from accounts where accountid = @accountid;";

            try
            {
                using var connection = new SqlConnection(ConnectionString);
                using var command = new SqlCommand(query, connection);

                command.Parameters.Add("@accountid", SqlDbType.Int).Value = AccountID;

                connection.Open();

                var Result = command.ExecuteScalar();
                if (Result!= null && Convert.ToBoolean(Result)) return true;
                
            }

            catch (SqlException SqlEx) { Console.Error.WriteLine(SqlEx.Message); }

            catch (Exception ex) { Console.Error.WriteLine(ex.Message); }

            return false;
        }
        static public bool IsAccountExists(int AccountID) {
            
            var s = ConfigurationHelper.GetConfiguration();
            var ConnectionString = s.GetConnectionString("My_DB_Connection");

            const string query = "select 1 from accounts where accountid = @accountid;";

            try
            {
                using var connection = new SqlConnection(ConnectionString);
                using var command = new SqlCommand(query, connection);

                command.Parameters.Add("@accountid", SqlDbType.Int).Value = AccountID;

                connection.Open();

                var Result = command.ExecuteScalar();
                if (Result != null && Convert.ToBoolean(Result)) return true;

            }

            catch (SqlException SqlEx) { Console.Error.WriteLine(SqlEx.Message); }

            catch (Exception ex) { Console.Error.WriteLine(ex.Message); }

            return false;
        }
        static public bool GetAccountInfo(int AccountID,ref string fullname,ref string email,ref string passwordhash,ref decimal balance, ref DateTime createdat,ref bool isdeleted)
        {
            var s = ConfigurationHelper.GetConfiguration();
            var ConnectionString = s.GetConnectionString("My_DB_Connection");

            const string query = "SELECT fullname, email, passwordhash, balance, createdat, isdeleted FROM accounts where accountid = @accountid;";

            try
            {
                using var connection = new SqlConnection(ConnectionString);
                using var command = new SqlCommand(query, connection);
               

                command.Parameters.Add("@accountid", SqlDbType.Int).Value = AccountID;

                connection.Open(); 
                
                using var reader = command.ExecuteReader();

                if (reader.Read()) {

                    fullname = reader.GetString(reader.GetOrdinal("fullname"));
                    email = reader.GetString(reader.GetOrdinal("email"));
                    passwordhash = reader.GetString(reader.GetOrdinal("passwordhash"));
                    balance = reader.GetDecimal(reader.GetOrdinal("balance"));
                    createdat = reader.GetDateTime(reader.GetOrdinal("createdat"));
                    isdeleted = reader.GetBoolean(reader.GetOrdinal("isdeleted"));

                    return true;
                }
                


            }

            catch (SqlException SqlEx) { Console.Error.WriteLine(SqlEx.Message); }

            catch (Exception ex) { Console.Error.WriteLine(ex.Message); }

            return false;
        }
    }
}
 
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;


namespace DataAccessLayer
{

    public class Accounts_DAL
    {
        static public int AddNewAccount(string fullname, string email, string passwordhash)
        {
            var s = ConfigurationHelper.GetConfiguration();
            var ConnectionString = s.GetConnectionString("My_DB_Connection");

            const string query = @"
                INSERT INTO accounts (fullname, email, passwordhash)
                VALUES (@fullname, @email, @passwordhash)
                RETURNING accountid;
            ";

            try
            {
                using var connection = new NpgsqlConnection(ConnectionString);
                using var command = new NpgsqlCommand(query, connection);

                command.Parameters.AddWithValue("@fullname", fullname);
                command.Parameters.AddWithValue("@email", email);
                command.Parameters.AddWithValue("@passwordhash", passwordhash);

                connection.Open();
                var result = command.ExecuteScalar();

                return result != null ? Convert.ToInt32(result) : -1;
            }
            catch (PostgresException pgEx)
            {
                if (pgEx.SqlState == "23505") 
                    Console.Error.WriteLine("Email already exists.");
                else
                    Console.Error.WriteLine($"Postgres Error {pgEx.SqlState}: {pgEx.Message}");
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }

            return -1;
        }

        static public bool UpdateAccount(int AccountID, string fullname, string email, string passwordhash)
        {
            var s = ConfigurationHelper.GetConfiguration();
            var ConnectionString = s.GetConnectionString("My_DB_Connection");

            const string query = @"
                UPDATE accounts 
                SET fullname = @fullname, email = @email, passwordhash = @passwordhash
                WHERE accountid = @accountid;
            ";

            try
            {
                using var connection = new NpgsqlConnection(ConnectionString);
                using var command = new NpgsqlCommand(query, connection);

                command.Parameters.AddWithValue("@accountid", AccountID);
                command.Parameters.AddWithValue("@fullname", fullname);
                command.Parameters.AddWithValue("@email", email);
                command.Parameters.AddWithValue("@passwordhash", passwordhash);

                connection.Open();
                return command.ExecuteNonQuery() > 0;
            }
            catch (PostgresException pgEx) { Console.Error.WriteLine(pgEx.Message); }
            catch (Exception ex) { Console.Error.WriteLine(ex.Message); }

            return false;
        }

        static public bool DeleteAccount(int AccountID)
        {
            var s = ConfigurationHelper.GetConfiguration();
            var ConnectionString = s.GetConnectionString("My_DB_Connection");

            const string query = "UPDATE accounts SET isdeleted = TRUE WHERE accountid = @accountid;";

            try
            {
                using var connection = new NpgsqlConnection(ConnectionString);
                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@accountid", AccountID);

                connection.Open();
                return command.ExecuteNonQuery() > 0;
            }
            catch (PostgresException pgEx) { Console.Error.WriteLine(pgEx.Message); }
            catch (Exception ex) { Console.Error.WriteLine(ex.Message); }

            return false;
        }

        static public bool RetriveAccount(int AccountID)
        {
            var s = ConfigurationHelper.GetConfiguration();
            var ConnectionString = s.GetConnectionString("My_DB_Connection");

            const string query = "UPDATE accounts SET isdeleted = FALSE WHERE accountid = @accountid;";

            try
            {
                using var connection = new NpgsqlConnection(ConnectionString);
                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@accountid", AccountID);

                connection.Open();
                return command.ExecuteNonQuery() > 0;
            }
            catch (PostgresException pgEx) { Console.Error.WriteLine(pgEx.Message); }
            catch (Exception ex) { Console.Error.WriteLine(ex.Message); }

            return false;
        }

        static public bool IsAccountDeleted(int AccountID)
        {
            var s = ConfigurationHelper.GetConfiguration();
            var ConnectionString = s.GetConnectionString("My_DB_Connection");

            const string query = "SELECT isdeleted FROM accounts WHERE accountid = @accountid;";

            try
            {
                using var connection = new NpgsqlConnection(ConnectionString);
                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@accountid", AccountID);

                connection.Open();
                var result = command.ExecuteScalar();
                return result != null && Convert.ToBoolean(result);
            }
            catch (PostgresException pgEx) { Console.Error.WriteLine(pgEx.Message); }
            catch (Exception ex) { Console.Error.WriteLine(ex.Message); }

            return false;
        }

        static public bool IsAccountExists(int AccountID)
        {
            var s = ConfigurationHelper.GetConfiguration();
            var ConnectionString = s.GetConnectionString("My_DB_Connection");

            const string query = "SELECT 1 FROM accounts WHERE accountid = @accountid;";

            try
            {
                using var connection = new NpgsqlConnection(ConnectionString);
                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@accountid", AccountID);

                connection.Open();
                var result = command.ExecuteScalar();
                return result != null;
            }
            catch (PostgresException pgEx) { Console.Error.WriteLine(pgEx.Message); }
            catch (Exception ex) { Console.Error.WriteLine(ex.Message); }

            return false;
        }

        static public bool GetAccountInfo(int AccountID, ref string fullname, ref string email, ref string passwordhash, ref decimal balance, ref DateTime createdat, ref bool isdeleted)
        {
            var s = ConfigurationHelper.GetConfiguration();
            var ConnectionString = s.GetConnectionString("My_DB_Connection");

            const string query = @"
                SELECT fullname, email, passwordhash, balance, createdat, isdeleted
                FROM accounts 
                WHERE accountid = @accountid;
            ";

            try
            {
                using var connection = new NpgsqlConnection(ConnectionString);
                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@accountid", AccountID);

                connection.Open();
                using var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    fullname = reader.GetString(reader.GetOrdinal("fullname"));
                    email = reader.GetString(reader.GetOrdinal("email"));
                    passwordhash = reader.GetString(reader.GetOrdinal("passwordhash"));
                    balance = reader.GetDecimal(reader.GetOrdinal("balance"));
                    createdat = reader.GetDateTime(reader.GetOrdinal("createdat"));
                    isdeleted = reader.GetBoolean(reader.GetOrdinal("isdeleted"));

                    return true;
                }
            }
            catch (PostgresException pgEx) { Console.Error.WriteLine(pgEx.Message); }
            catch (Exception ex) { Console.Error.WriteLine(ex.Message); }

            return false;
        }

        static public DataTable GetAllAccounts(byte opt)
        {
            var s = ConfigurationHelper.GetConfiguration();
            var ConnectionString = s.GetConnectionString("My_DB_Connection");

            DataTable dtAllAccounts = new DataTable();
            string query = opt switch
            {
                0 => "SELECT * FROM accounts;",
                1 => "SELECT * FROM accounts WHERE isdeleted = FALSE;",
                2 => "SELECT * FROM accounts WHERE isdeleted = TRUE;",
                _ => throw new ArgumentException("Invalid option")
            };

            try
            {
                using var connection = new NpgsqlConnection(ConnectionString);
                using var command = new NpgsqlCommand(query, connection);

                connection.Open();
                using var reader = command.ExecuteReader();
                dtAllAccounts.Load(reader);
            }
            catch (PostgresException pgEx) { Console.Error.WriteLine(pgEx.Message); }
            catch (Exception ex) { Console.Error.WriteLine(ex.Message); }

            return dtAllAccounts;
        }

        static public int? Login(string email, string password)
        {
            var s = ConfigurationHelper.GetConfiguration();
            var ConnectionString = s.GetConnectionString("My_DB_Connection");

            const string query = @"SELECT accountid FROM accounts
                WHERE email = @email AND passwordhash = @password;";

            try
            {
                using var connection = new NpgsqlConnection(ConnectionString);
                using var command = new NpgsqlCommand(query, connection);

                command.Parameters.AddWithValue("@email", email);
                command.Parameters.AddWithValue("@password", password);

                connection.Open();
                using var reader = command.ExecuteReader();
                
               
                if (reader.Read())
                { 
                    int CurrentID =  reader.GetInt32(reader.GetOrdinal("accountid"));
                    sessions_DAL.AccountLogedToSystem(CurrentID);
                     return CurrentID;
                }
              
                   
            }
            catch (PostgresException pgEx) { Console.Error.WriteLine(pgEx.Message); }
            catch (Exception ex) { Console.Error.WriteLine(ex.Message); }
            return null;
        }

        static public int? Login(int accountid, string password)
        {
            var s = ConfigurationHelper.GetConfiguration();
            var ConnectionString = s.GetConnectionString("My_DB_Connection");

            const string query = "SELECT accountid FROM accounts WHERE accountid = @accountid AND passwordhash = @password;";

            try
            {
                using var connection = new NpgsqlConnection(ConnectionString);
                using var command = new NpgsqlCommand(query, connection);

                command.Parameters.AddWithValue("@accountid", accountid);
                command.Parameters.AddWithValue("@password", password);

                connection.Open();
                using var reader = command.ExecuteReader();
                if (reader.Read()) {

                    int CurrentID = reader.GetInt32(reader.GetOrdinal("accountid"));
                     sessions_DAL.AccountLogedToSystem(CurrentID);
                    return CurrentID ;
                }
                 
              
                
            }
            catch (PostgresException pgEx) { 
                Console.Error.WriteLine(pgEx.Message);
            }
            catch (Exception ex) { 
                Console.Error.WriteLine(ex.Message);

            }
            return null;
        }

        static public bool ChangePassword(int accountid, string OldPassword, string NewPassword)
        {
            var s = ConfigurationHelper.GetConfiguration();
            var ConnectionString = s.GetConnectionString("My_DB_Connection");

            const string query = @"
                UPDATE accounts 
                SET passwordhash = @newpassword
                WHERE accountid = @accountid AND passwordhash = @oldpassword;
            ";

            try
            {
                using var connection = new NpgsqlConnection(ConnectionString);
                using var command = new NpgsqlCommand(query, connection);

                command.Parameters.AddWithValue("@newpassword", NewPassword);
                command.Parameters.AddWithValue("@accountid", accountid);
                command.Parameters.AddWithValue("@oldpassword", OldPassword);

                connection.Open();
                return command.ExecuteNonQuery() > 0;
            }
            catch (PostgresException pgEx) { Console.Error.WriteLine(pgEx.Message); }
            catch (Exception ex) { Console.Error.WriteLine(ex.Message); }

            return false;
        }

        static public decimal GetAccountBalance(int accountid) {

            var s = ConfigurationHelper.GetConfiguration();
            var ConnectionString = s.GetConnectionString("My_DB_Connection");

            const string query = "SELECT balance FROM accounts WHERE accountid = @accountid AND isdeleted = FALSE";
            try
            {
                using var connection = new NpgsqlConnection(ConnectionString);
                using var command = new NpgsqlCommand(query, connection);

                command.Parameters.AddWithValue("@accountid", accountid);
             

                connection.Open();
                
                var balance = command.ExecuteScalar() as decimal?; 
                return balance ?? 0m;

            }
            catch (PostgresException pgEx) { Console.Error.WriteLine(pgEx.Message); }
            catch (Exception ex) { Console.Error.WriteLine(ex.Message); }
            return 0m;

        }
    }
}

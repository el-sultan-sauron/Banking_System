using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Npgsql;
using System.Data;



namespace DataAccessLayer
{

    public class Transactions_DAL
    {
        static public DataTable GetTransactions(int accountId, bool sent)
        {
            var s = ConfigurationHelper.GetConfiguration();
            var ConnectionString = s.GetConnectionString("My_DB_Connection");

            DataTable dtAllTransactions = new DataTable();
            string query = sent
                ? "SELECT senderid, recieverid, amount, createdat FROM transactions WHERE senderid = @accountid;"
                : "SELECT recieverid, senderid, amount, createdat FROM transactions WHERE recieverid = @accountid;";

            try
            {
                using var connection = new NpgsqlConnection(ConnectionString);
                using var command = new NpgsqlCommand(query, connection);

                command.Parameters.AddWithValue("@accountid", accountId);

                connection.Open();
                using var reader = command.ExecuteReader();
                dtAllTransactions.Load(reader);
            }
            catch (PostgresException pgEx) { Console.Error.WriteLine(pgEx.Message); }
            catch (Exception ex) { Console.Error.WriteLine(ex.Message); }

            return dtAllTransactions;
        }

        public static bool TransferBetweenAccounts(int senderID, int receiverID, decimal amount, string description = "")
        {
            if (amount <= 0)
            {
                Console.Error.WriteLine("Amount must be more than 0!");
                return false;
            }

            var s = ConfigurationHelper.GetConfiguration();
            var connectionString = s.GetConnectionString("My_DB_Connection");

            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                decimal senderBalance = Accounts_DAL.GetAccountBalance(senderID);
                if (senderBalance < amount)
                    throw new Exception("Insufficient balance!");

                bool receiverExists = Accounts_DAL.IsAccountExists(receiverID) && !Accounts_DAL.IsAccountDeleted(receiverID);
                if (!receiverExists)
                    throw new Exception("Receiver ID not found!");

                // Decrease from sender
                using (var cmd = new NpgsqlCommand(
                    "UPDATE accounts SET balance = balance - @amount WHERE accountid = @senderaccountid", connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@amount", amount);
                    cmd.Parameters.AddWithValue("@senderaccountid", senderID);
                    cmd.ExecuteNonQuery();
                }

                // Increase to receiver
                using (var cmd = new NpgsqlCommand(
                    "UPDATE accounts SET balance = balance + @amount WHERE accountid = @receiveraccountid", connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@amount", amount);
                    cmd.Parameters.AddWithValue("@receiveraccountid", receiverID);
                    cmd.ExecuteNonQuery();
                }

                // Log transaction
                using (var cmd = new NpgsqlCommand(
                    "INSERT INTO transactions (senderid, recieverid, amount, description) VALUES (@sender, @receiver, @amount, @description)", connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@sender", senderID);
                    cmd.Parameters.AddWithValue("@receiver", receiverID);
                    cmd.Parameters.AddWithValue("@amount", amount);
                    cmd.Parameters.AddWithValue("@description", description);
                    cmd.ExecuteNonQuery();
                }

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.Error.WriteLine(ex.Message);
                return false;
            }
        }

        public static bool DepositIntoAccount(int accountID, decimal balance,string description ="")
        {
            var s = ConfigurationHelper.GetConfiguration();
            var ConnectionString = s.GetConnectionString("My_DB_Connection");

            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            using var Transaction = connection.BeginTransaction();

            try
            {
             
                const string updateBalanceQuery = "update accounts set balance = balance + @balance where accountid = @accountid;";
                using (var updateCmd = new NpgsqlCommand(updateBalanceQuery, connection, Transaction))
                {
                    updateCmd.Parameters.AddWithValue("@accountid", accountID);
                    updateCmd.Parameters.AddWithValue("@balance", balance);

                    if (updateCmd.ExecuteNonQuery() <= 0)
                        throw new Exception("Failed to update balance.");
                }

                const string insertDepositQuery = "insert into deposits(accountid, amount, description) values (@accountid, @balance, @description);";
                using (var insertCmd = new NpgsqlCommand(insertDepositQuery, connection, Transaction))
                {
                    insertCmd.Parameters.AddWithValue("@accountid", accountID);
                    insertCmd.Parameters.AddWithValue("@balance", balance);
                    insertCmd.Parameters.AddWithValue("@description", description);

                    if (insertCmd.ExecuteNonQuery() <= 0)
                        throw new Exception("Failed to insert into deposits.");
                }

                Transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Deposit error: " + ex.Message);
                Transaction.Rollback(); 
                return false;
            }
            
        }
        public static bool WithdrawFromAccount(int accountID,decimal balance,string description = "")
        {
            var s = ConfigurationHelper.GetConfiguration();
            var ConnectionString = s.GetConnectionString("My_DB_Connection");

            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                // for update used to not let the query execute till transaction finish
                const string BalanceQuery = "select balance from accounts where accountid = @accountid FOR UPDATE;";
                decimal currentBalance;

                using (var balanceCmd = new NpgsqlCommand(BalanceQuery, connection, transaction))
                {
                    balanceCmd.Parameters.AddWithValue("@accountid", accountID);
                    var result = balanceCmd.ExecuteScalar();

                    if (result == null)
                        throw new Exception("Account not found.");

                    currentBalance = Convert.ToDecimal(result);
                }


                if (balance > currentBalance)
                    throw new Exception("Insufficient Balance.");

                const string Query = "update accounts set balance = balance - @balance where accountid = @accountid;";

                using (var updateCmd = new NpgsqlCommand(Query, connection, transaction))
                {
                    updateCmd.Parameters.AddWithValue("@accountid", accountID);
                    updateCmd.Parameters.AddWithValue("@balance", balance);

                    if (updateCmd.ExecuteNonQuery() <= 0)
                        throw new Exception("Failed to update balance.");
                }

                const string insertQuery = "insert into withdraws(accountid, amount, description) values (@accountid, @balance, @description);";

                using (var insertCmd = new NpgsqlCommand(insertQuery, connection, transaction))
                {
                    insertCmd.Parameters.AddWithValue("@accountid", accountID);
                    insertCmd.Parameters.AddWithValue("@balance", balance);
                    insertCmd.Parameters.AddWithValue("@description", description);

                    if (insertCmd.ExecuteNonQuery() <= 0)
                        throw new Exception("Failed to insert into withdraws.");
                }

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Withdraw error: " + ex.Message);
                transaction.Rollback();
                return false;
            }
        }

        public static DataTable GetAllWithdrawsOfAccount(int accountID)
        {
            DataTable result = new DataTable();
            var s = ConfigurationHelper.GetConfiguration();
            var ConnectionString = s.GetConnectionString("My_DB_Connection");

            string query = "select * from withdraws where accountid  = @accountid;";

            try
            {
                using var connection = new NpgsqlConnection(ConnectionString);
                using var command = new NpgsqlCommand(query, connection);

                command.Parameters.AddWithValue("@accountid", accountID);

                connection.Open();
                using var reader = command.ExecuteReader();
               result.Load(reader);
            }
            catch (PostgresException pgEx) { Console.Error.WriteLine(pgEx.Message); }
            catch (Exception ex) { Console.Error.WriteLine(ex.Message); }

            return result;

        }
        public static DataTable GetAllDepositssOfAccount(int accountID)
        {
            DataTable result = new DataTable();
            var s = ConfigurationHelper.GetConfiguration();
            var ConnectionString = s.GetConnectionString("My_DB_Connection");

            string query = "select * from deposits where accountid  = @accountid;";

            try
            {
                using var connection = new NpgsqlConnection(ConnectionString);
                using var command = new NpgsqlCommand(query, connection);

                command.Parameters.AddWithValue("@accountid", accountID);

                connection.Open();
                using var reader = command.ExecuteReader();
                result.Load(reader);
            }
            catch (PostgresException pgEx) { Console.Error.WriteLine(pgEx.Message); }
            catch (Exception ex) { Console.Error.WriteLine(ex.Message); }

            return result;

        }


    }
}
    



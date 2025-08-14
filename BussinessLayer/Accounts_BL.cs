using DataAccessLayer;
using System.Data;


namespace BussinessLayer
{
    public class Accounts_BL
    {
        public  int? AccountId { get; private set; }
        public  string? fullname { get; set; }
        public  string? email { get; set; }
        public  string? hashedpassword { get; set; }
        public  decimal balance { get; set; }
        public DateTime? createdat { get;private set; }
        public bool? isDeleted { get; set; }

        public Accounts_BL(int accountid, string fullname,string email,string hashedpassword,decimal balance,DateTime createdat,bool isdeleted) { 
            this.AccountId = accountid;
            this.fullname = fullname;
            this.email = email; 
            this.hashedpassword = hashedpassword;
            this.balance = balance;
            this.createdat = createdat;
            this.isDeleted = isdeleted;
        }

        public static bool AddNewAccount(string fullname, string email, string password)
        {
            string hashedPassword = clsUtil.ComputeHash(password);
            return Accounts_DAL.AddNewAccount(fullname, email, hashedPassword) != -1;
        }
        public static bool UpdateAccount(int accountid, string fullname, string email, string password)
        {
            string hashedPassword = clsUtil.ComputeHash(password);
            return Accounts_DAL.UpdateAccount(accountid, fullname, email, hashedPassword);
        }
        public static bool DeleteAccount(int accountid) => Accounts_DAL.DeleteAccount(accountid);
        public static bool RetriveAccount(int accountid) => Accounts_DAL.RetriveAccount(accountid);
        public static bool IsAccountDeleted(int accountid) => Accounts_DAL.IsAccountDeleted(accountid);
        public static bool IsAccountExists(int accountid) => Accounts_DAL.IsAccountExists(accountid);
        public static Accounts_BL? GetAccountInfo(int accountid)
        {
            string fullname = "", email = "", hashedpassword = ""; decimal balance = 0; DateTime createdat = DateTime.MinValue; bool isdeleted = false;

            if (!IsAccountExists(accountid)) return null;
            bool IsFound = Accounts_DAL.GetAccountInfo(accountid,ref fullname,ref email,ref hashedpassword,ref balance,ref createdat,ref isdeleted);
            if (!IsFound) return null;
            return new Accounts_BL(accountid, fullname, email, hashedpassword, balance, createdat, isdeleted);
            
        }
        public static DataTable GetAllAccounts(byte opt) => Accounts_DAL.GetAllAccounts(opt);
        public static int? Login(string email, string password)
        {
            string HashedPassword = clsUtil.ComputeHash(password);
            return Accounts_DAL.Login(email, HashedPassword);
        }
        public static int? Login(int accountid,string password)
        {
            string hashedPassword = clsUtil.ComputeHash(password);
            return Accounts_DAL.Login(accountid, hashedPassword);
        }
        public static bool ChangePassword(int accountid,string oldpassword,string newpassword)
        {
            string HashedOldPassword= clsUtil.ComputeHash(oldpassword), HashedNewPassword = clsUtil.ComputeHash(newpassword);
            return Accounts_DAL.ChangePassword(accountid, HashedOldPassword, HashedNewPassword);
        }
     }
}

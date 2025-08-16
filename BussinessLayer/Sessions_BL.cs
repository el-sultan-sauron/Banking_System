using System;


namespace BussinessLayer
{
    public class Sessions_BL
    {
       public static int AccountLogedToSystem(int Accountid) => DataAccessLayer.sessions_DAL.AccountLogedToSystem(Accountid);

    }
}

using System;

namespace BussinessLayer
{
    public class Global_BL
    {
        static public decimal TotalBalanceAtSystem(bool Active) => DataAccessLayer.Global_DAL.GetTotalBalanceAtSystem(Active);
    }
}

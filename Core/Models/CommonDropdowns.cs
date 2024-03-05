using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class CommonDropdowns : BaseModel
    {
        public int DropdownKey { get; set; }
        [Display(Name = " Code")]
        public int? Code { get; set; }
    }
    public enum DropdownEnums
    {
        [Description("For returning the user gender")]
        GenderKey = 1,
        [Description("For returning the user Network")]
        NetworkKey = 2,
        [Description("For returning the user DataList")]
        ProductKey = 3,
        [Description("For returning the user PaymentOptions")]
        PaymentOptions = 4,
        [Description("For returning the List of Banks")]
        BankList = 5,
        [Description("For returning notifications")]
        AdminNotice = 6,
        [Description("For returning the user Network")]
        AirtimeNetwork = 7,
    }
}

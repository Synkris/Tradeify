using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Core.Config
{
    public class GeneralConfiguration : IGeneralConfiguration
    {
        public string TransitApiKey { get; set; }

        public string TransitUrl { get; set; }

        public string TransitUserId { get; set; }

        public string ProductUrl { get; set; }

        public string PayStakApiKey { get; set; }

        public string AdminEmail { get; set; }

        public string DeveloperEmail { get; set; }

        public decimal ReferralBonus { get; set; }
        public decimal MatchingPV { get; set; }

        public decimal WelcomeBonus { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal DollarRate { get; set; }

        public decimal PackageBonus { get; set; }

        public decimal DistributorBonus { get; set; }

        public double NewDollarRate { get; set; }

        public double MinWithdrawal { get; set; }

        public decimal GrantLimit { get; set; }

        public decimal GrantAmount { get; set; }

        public decimal ReferralBonusForAdifandCryptoClass { get; set; }

        public decimal GGCWithdrawalConversionToNaira { get; set; }
        public decimal GGCConversionToDollar { get; set; }

        public string TimeToRecurHangFire { get; set; }

        public string TimeZoneToRecur { get; set; }

        public decimal VTUActivationFee { get; set; }

        public decimal CryptoRegFeeAmount { get; set; }

        public int MTNProductCode { get; set; }

        public int GLOProductCode { get; set; }

        public int AIRTELProductCode { get; set; }

        public int ETISALATProductCode { get; set; }
        public decimal Gen1Bonus { get; set; }
        public decimal Gen2Bonus { get; set; }
        public decimal Gen3Bonus { get; set; }
        public decimal Gen4Bonus { get; set; }
        public decimal Gen5Bonus { get; set; }
        public decimal CoordinatorBonus { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public string OldGGCEndPoint { get; set; }
        public bool MapGenButton { get; set; }
        public string OldUsers { get; set; }

    }
}

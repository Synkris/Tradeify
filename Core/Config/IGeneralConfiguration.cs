using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Config
{
    public interface IGeneralConfiguration
    {
       string TransitApiKey { get; set; }

       string TransitUrl { get; set; }

       string TransitUserId { get; set; }

       string ProductUrl { get; set; }

       string PayStakApiKey { get; set; }

       string AdminEmail { get; set; }

       string DeveloperEmail { get; set; }

       decimal ReferralBonus { get; set; }

       decimal MatchingPV { get; set; }

       decimal WelcomeBonus { get; set; }    
        
       decimal DollarRate { get; set; }

       decimal PackageBonus { get; set; }

       decimal DistributorBonus { get; set; }

        double NewDollarRate { get; set; }

       double MinWithdrawal { get; set; }

       decimal GrantLimit { get; set; }

       decimal GrantAmount { get; set; }

       decimal GGCWithdrawalConversionToNaira { get; set; }
       decimal GGCConversionToDollar { get; set; }

        decimal ReferralBonusForAdifandCryptoClass { get; set; }

	   string TimeToRecurHangFire { get; set; }

       string TimeZoneToRecur { get; set; }

       decimal VTUActivationFee { get; set; }

       decimal CryptoRegFeeAmount { get; set; }

       int MTNProductCode { get; set; }

       int GLOProductCode { get; set; }

       int AIRTELProductCode { get; set; }

       int ETISALATProductCode { get; set; }

        decimal Gen1Bonus { get; set; }
        decimal Gen2Bonus { get; set; }
        decimal Gen3Bonus { get; set; }
        decimal Gen4Bonus { get; set; }
        decimal Gen5Bonus { get; set; }
        decimal CoordinatorBonus { get; set; }
		int PageSize { get; set; }
		int PageNumber { get; set; }
		string OldGGCEndPoint { get; set; }
		bool MapGenButton { get; set; }
		string OldUsers { get; set; }

    }
}


using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Core.Models
{
    public class UserBettings
    {
        [Key]
        public Guid Id { get; set; }

        public string UserId { get; set; }
        [Display(Name = "User")]
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [Display(Name = "Date Staked")]
        public DateTime DateStaked { get; set; }

        [Display(Name = "Date Won")]
        public DateTime DateWon { get; set; }

        [Display(Name = "Approved By")]
        public string BetApprovedBy { get; set; }

        [Display(Name = "Amount Staked")]
        public decimal AmountStaked { get; set; }

        [Display(Name = "User Bet Answer")]
        public string BetAnswerChosen { get; set; }

        public int BettingsId { get; set; }
        [Display(Name = "Bettings")]
        [ForeignKey("BettingsId")]
        public virtual Bettings Bettings { get; set; }
        
        [Display(Name = "Bet Status")]
        public Status BetStatus { get; set; }

        public bool Active { get; set; }

        public bool Deleted { get; set; }

        [Display(Name = "Bet Question Picked")]
        public string BetQuestion { get; set; }

        [NotMapped]
        public DateTime MaturedDate
        {
            get
            {
                var seconds = Bettings.NumberOfDays * 604800;
                var expectedTime = Bettings.DateBetCreated.AddSeconds(Convert.ToDouble(seconds));
                return expectedTime;
            }
        }

        [NotMapped]
        public string ExpectedMaturedDate
        {
            get
            {
                var daysInaWeek = 7;
                var TotalDaysInPackage = Bettings.NumberOfDays * daysInaWeek;
                var hours = Bettings.NumberOfDays * 168;
                var expectedTime = Bettings.DateBetCreated.AddHours(Convert.ToDouble(hours));
                if (expectedTime >= DateTime.Now)
                {
                    var matured = Bettings.DateBetCreated.AddDays(TotalDaysInPackage);

                    return matured.ToString();
                }
                return "Package Is Matured";

            }
        }

        [NotMapped]
        public double TimeSpentSoFar
        {
            get
            {
                var daysInaWeek = 7;
                var TotalDaysInPackage = Bettings.NumberOfDays * daysInaWeek;
                var timeSpent = (DateTime.Now - Bettings.DateBetCreated).TotalDays;
                if (TotalDaysInPackage >= Convert.ToDouble(timeSpent))
                {
                    return timeSpent;
                }
                
                return TotalDaysInPackage;

            }
        }







    }
}

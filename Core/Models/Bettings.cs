
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Core.Models
{
    public class Bettings
    {
        [Key]
        public int Id { get; set; }
        [Display(Name = "Bet Name")]
        public string BetName { get; set; }
        [Display(Name = "Bet Questions")]
        public string BetQuestions { get; set; }
        [Display(Name = "Bet Answer")]
        public string BetAnswers { get; set; }
        [Display(Name = "Number Of Days")]
        public int NumberOfDays { get; set; }
        [Display(Name = "Betting Amount")]
        public double BettingAmount { get; set; }
        [Display(Name = "Cashback to be received")]
        public double CashBack { get; set; }
        [Display(Name = " Created By ")]
        public string BetCreatedBy { get; set; }
        [Display(Name = "Date Bet Created")]
        public DateTime DateBetCreated { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
    }
}

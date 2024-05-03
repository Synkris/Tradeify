using Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Models
{
    public class Packages : BaseModel
    {

        [Display(Name = "Price")]
        public double? Price { get; set; }

        [Display(Name ="Bonus Amount")]
        public double? BonusAmount { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name="Max Generation")]
        public GenerationEnum MaxGeneration { get; set; }

    }
    public enum GenerationEnum
    {
        [Description("For First Generation")]
        FirstGeneration = 1,
        [Description("For Second Generation")]
        SecondGeneration = 2,
        [Description("For Third Generation")]
        ThirdGeneration = 3,
        [Description("For Fourth Generation")]
        FourthGeneration = 4,
        [Description("For Fifth Generation")]
        FifthGeneration = 5
    }
}

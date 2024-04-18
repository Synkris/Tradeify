using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ViewModels
{
    public class RegistrationPackageViewModel
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public double BonusAmount { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public int GenerationId { get; set; }
        public GenerationEnum MaxGeneration { get; set; }
        public DateTime DateCreated { get; set; }
        public int SerialNumber { get; set; }
    }
}

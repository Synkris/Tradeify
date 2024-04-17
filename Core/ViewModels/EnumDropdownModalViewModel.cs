using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Helper
{
    public class EnumDropdownModalViewModel
    {
        public int Id { get; set; }
        public string  Name { get; set; }
     
        public string NameToString { get; set; }
        public string Description { get; set; }
        public GenerationEnum MaxGeneration { get; set; }
    }
}

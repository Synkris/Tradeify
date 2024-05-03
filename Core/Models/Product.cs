using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Core.Models
{
    public class Product
    {
        public int Id { get; set; }

        public int ParentId { get; set; }
        [Display(Name = "Product Key")]
        [ForeignKey("ParentId")]
        public virtual CommonDropdowns Parent { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal Price { get; set; }

        public string Name { get; set; }

        [Display(Name = "Product Code")]
        public string Code { get; set; }

        public bool Deleted { get; set; }
        public bool Active { get; set; }
        [NotMapped]
        public string DisplayName
        {
            get
            {
                if (Name != null && Price > 0)
                {
                    return Name +" "+ " ==== " + " " + "NGN " + Price.ToString("G29");
                }
                return null;
            }
        }
           [Display(Name="")]
        [Column(TypeName = "decimal(18,4)")]
        public decimal Discount { get; set; }


    }
}

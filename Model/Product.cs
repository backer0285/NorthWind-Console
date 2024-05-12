using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NorthWind_Console.Model
{
    public partial class Product
    {
        public Product()
        {
            OrderDetails = new HashSet<OrderDetail>();
        }

        public int ProductId { get; set; }
        [Required]
        public string ProductName { get; set; }
        [Required]
        public int? SupplierId { get; set; }
        [Required]
        public int? CategoryId { get; set; }
        [Required]
        public string QuantityPerUnit { get; set; }
        [Required]
        [Range(0.0, double.MaxValue)]
        public decimal? UnitPrice { get; set; }
        [Required]
        [Range(0, short.MaxValue)]
        public short? UnitsInStock { get; set; }
        [Required]
        [Range(0, short.MaxValue)]
        public short? UnitsOnOrder { get; set; }
        [Required]
        [Range(0, short.MaxValue)]
        public short? ReorderLevel { get; set; }
        [Required]
        public bool Discontinued { get; set; }

        public virtual Category Category { get; set; }
        public virtual Supplier Supplier { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}

using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using Microsoft.AspNetCore.Mvc;

namespace ModelValidationsExample.Models
{

    
    public class Product {

        public long ProductId { get; set; }

        [Required(ErrorMessage = "Please enter a value")]
        public string Name { get; set; } = string.Empty;

        [Range(1, 999999, ErrorMessage = "Please enter a positive price")]
        [Column(TypeName = "decimal(8, 2)")]
        public decimal Price { get; set; }

        [Remote("CategoryKey", "Validation", ErrorMessage = "Enter an existing key")]
        public long CategoryId { get; set; }

        [Remote("SupplierKey", "Validation", ErrorMessage = "Enter an existing key")]
        public long SupplierId { get; set; }

    }
}

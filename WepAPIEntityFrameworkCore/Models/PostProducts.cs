using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace WepAPIEntityFrameworkCore.Models
{
    public class PostProducts
    {
        [Required]
        [StringLength(100, MinimumLength =3)]
        public string Name { get; set; } = string.Empty;


        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero." )]
        public double Price { get; set; }


        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Qty { get; set; }


        [Required]
        public IFormFile Image { get; set; } = null!;
    }
}

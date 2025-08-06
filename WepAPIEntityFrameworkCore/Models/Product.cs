using System.ComponentModel.DataAnnotations;

namespace WepAPIEntityFrameworkCore.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public double Price { get; set; }

        public int Qty { get; set; }

        public double Amount { get => Price * Qty; }

        public string ImageUrl { get; set; } = string.Empty;
    }
}

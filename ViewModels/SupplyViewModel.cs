using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SewingMaterialsStorage.ViewModels
{
    public class SupplyViewModel
    {
        public int SupplyId { get; set; }

        [Required(ErrorMessage = "Выберите материал")]
        [Display(Name = "Материал")]
        public int MaterialId { get; set; }

        [Required(ErrorMessage = "Укажите количество")]
        [Range(1, int.MaxValue, ErrorMessage = "Количество должно быть положительным")]
        [Display(Name = "Количество")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Укажите дату поступления")]
        [Display(Name = "Дата поступления")]
        public DateTime SupplyDate { get; set; } = DateTime.Today;

        [Display(Name = "Сумма")]
        public decimal TotalAmount { get; set; }

        public SelectList? Materials { get; set; }
    }
}
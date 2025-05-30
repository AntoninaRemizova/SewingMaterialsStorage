using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SewingMaterialsStorage.ViewModels
{
    public class ConsumptionViewModel
    {
        public int ConsumptionId { get; set; }

        [Required(ErrorMessage = "Выберите материал")]
        [Display(Name = "Материал")]
        public int MaterialId { get; set; }

        [Required(ErrorMessage = "Укажите количество")]
        [Range(1, int.MaxValue, ErrorMessage = "Количество должно быть положительным")]
        [Display(Name = "Количество")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Укажите дату расхода")]
        [Display(Name = "Дата расхода")]
        public DateTime ConsumptionDate { get; set; } = DateTime.Today;

        [Display(Name = "Номер заказа")]
        public string? OrderId { get; set; }

        [Display(Name = "Сумма")]
        public decimal TotalAmount { get; set; }

        public SelectList? Materials { get; set; }
    }
}
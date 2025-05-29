// Reports/ConsumptionReportViewModel.cs
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SewingMaterialsStorage.ViewModels
{
    public class ConsumptionReportViewModel
    {
        [Required(ErrorMessage = "Укажите начальную дату")]
        [Display(Name = "Начальная дата")]
        public DateTime StartDate { get; set; } = DateTime.Today.AddMonths(-1);

        [Required(ErrorMessage = "Укажите конечную дату")]
        [Display(Name = "Конечная дата")]
        public DateTime EndDate { get; set; } = DateTime.Today;

        [Display(Name = "Тип материала")]
        public int? MaterialTypeId { get; set; }

        public List<ConsumptionReportItem> Items { get; set; } = new List<ConsumptionReportItem>();
        public SelectList MaterialTypes { get; set; }
    }

    public class ConsumptionReportItem
    {
        public string MaterialType { get; set; }
        public string MaterialName { get; set; }
        public int Quantity { get; set; }
        public decimal PricePerUnit { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
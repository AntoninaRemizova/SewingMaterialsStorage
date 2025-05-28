// Material.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SewingMaterialsStorage.Models
{
    public class Material
    {
        public Material()
        {
            Colors = new List<MaterialColor>();
            Compositions = new List<MaterialComposition>();
            Supplies = new List<Supply>();
            Consumptions = new List<Consumption>();
        }

        public int MaterialId { get; set; }

        [Required(ErrorMessage = "Введите наименование материала")]
        public string MaterialName { get; set; }

        [Required(ErrorMessage = "Выберите тип материала")]
        public int TypeId { get; set; }

        [Required(ErrorMessage = "Выберите производителя")]
        public int? ManufacturerId { get; set; }
        public string? Notes { get; set; }
        public string Article { get; set; }

        [Required(ErrorMessage = "Введите стоимость материала")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Цена должна быть больше 0")]
        public decimal PricePerUnit { get; set; }

        [Required(ErrorMessage = "Введите минимальный уровень запаса")]
        public int MinThreshold { get; set; }

        public MaterialType MaterialType { get; set; }
        public Manufacturer Manufacturer { get; set; }
        public MaterialFabric FabricDetails { get; set; }
        public MaterialThread ThreadDetails { get; set; }
        public MaterialZipper ZipperDetails { get; set; }
        public MaterialButton ButtonDetails { get; set; }

        public List<Supply> Supplies { get; set; }
        public List<Consumption> Consumptions { get; set; }


        public ICollection<MaterialColor> Colors { get; set; } = new List<MaterialColor>();
        public ICollection<MaterialComposition> Compositions { get; set; } = new List<MaterialComposition>();
    }
}
using System.ComponentModel.DataAnnotations;

namespace SewingMaterialsStorage.Models
{
    public class Manufacturer
    {
        public int ManufacturerId { get; set; }

        [Required(ErrorMessage = "Укажите название производителя")]
        public string ManufacturerName { get; set; }

        [Required(ErrorMessage = "Выберите страну")]
        public int CountryId { get; set; }

  
        public Country? Country { get; set; }
    }
}
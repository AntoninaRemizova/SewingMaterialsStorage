using System.ComponentModel.DataAnnotations;

namespace SewingMaterialsStorage.Models
{
    public class Color
    {
        public int ColorId { get; set; }

        [Required(ErrorMessage = "Укажите название цвета")]
        public string ColorName { get; set; }

        public List<MaterialColor> MaterialColors { get; set; } = new List<MaterialColor>();
    }
}
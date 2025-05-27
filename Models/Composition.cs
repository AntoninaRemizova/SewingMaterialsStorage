using System.ComponentModel.DataAnnotations;

namespace SewingMaterialsStorage.Models
{
    public class Composition
    {
        public int CompositionId { get; set; }
        [Required(ErrorMessage = "Укажите название состава")]
        public string CompositionName { get; set; }

        public List<MaterialComposition> MaterialCompositions { get; set; } = new List<MaterialComposition>();
    }
}
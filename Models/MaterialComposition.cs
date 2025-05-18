namespace SewingMaterialsStorage.Models
{
    public class MaterialComposition
    {
        public int MaterialId { get; set; }
        public Material Material { get; set; }
        public int CompositionId { get; set; }
        public Composition Composition { get; set; }
    }
}
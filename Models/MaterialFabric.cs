namespace SewingMaterialsStorage.Models
{
    public class MaterialFabric
    {
        public int MaterialId { get; set; }
        public Material Material { get; set; }
        public decimal Width { get; set; }
        public decimal Density { get; set; }
    }
}
namespace SewingMaterialsStorage.Models
{
    public class MaterialFabric
    {
        public int MaterialId { get; set; }
        public Material Material { get; set; }
        public int? Width { get; set; }
        public int? Density { get; set; }
    }
}
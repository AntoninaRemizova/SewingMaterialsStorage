namespace SewingMaterialsStorage.Models
{
    public class MaterialZipper
    {
        public int MaterialId { get; set; }
        public Material Material { get; set; }
        public string? ZipperType { get; set; }
        public int? ZipperLength { get; set; }
    }
}
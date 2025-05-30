namespace SewingMaterialsStorage.Models
{
    public class MaterialColor
    {
        public int MaterialId { get; set; }
        public Material Material { get; set; }

        public int ColorId { get; set; }
        public Color Color { get; set; }
    }
}
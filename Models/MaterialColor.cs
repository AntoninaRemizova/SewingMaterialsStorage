namespace SewingMaterialsStorage.Models
{
    public class MaterialColor
    {
        public int MaterialId { get; set; }  // Внешний ключ для Material
        public Material Material { get; set; }

        public int ColorId { get; set; }     // Внешний ключ для Color
        public Color Color { get; set; }
    }
}
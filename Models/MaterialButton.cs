namespace SewingMaterialsStorage.Models
{
    public class MaterialButton
    {
        public int MaterialId { get; set; }
        public Material Material { get; set; }
        public string? Shape { get; set; }
        public int? ButtonSize { get; set; }
    }
}
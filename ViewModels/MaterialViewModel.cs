namespace SewingMaterialsStorage.ViewModels
{
    public class MaterialViewModel
    {
        // Основные поля
        public int MaterialId { get; set; }
        public string MaterialName { get; set; }
        public string Article { get; set; }
        public decimal PricePerUnit { get; set; }
        public int MinThreshold { get; set; }
        public string? Notes { get; set; }
        public int ManufacturerId { get; set; }
        public int TypeId { get; set; }
        public int[] SelectedColors { get; set; }

        // Поля для тканей
        public int? Width { get; set; }
        public int? Density { get; set; }

        public int[] SelectedCompositions { get; set; }

        // Поля для ниток
        public int? Thickness { get; set; }
        public int? LengthPerSpool { get; set; }

        // Поля для молний
        public string? ZipperType { get; set; }
        public int? ZipperLength { get; set; }

        // Поля для пуговиц
        public string? Shape { get; set; }
        public int? ButtonSize { get; set; }
    }
}

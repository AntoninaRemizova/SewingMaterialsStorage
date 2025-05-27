namespace SewingMaterialsStorage.ViewModels
{
    public class CreateMaterialViewModel
    {
        // Основные поля
        public string MaterialName { get; set; }
        public string Article { get; set; }
        public decimal PricePerUnit { get; set; }
        public int MinThreshold { get; set; }
        public string Notes { get; set; }
        public int ManufacturerId { get; set; }
        public int TypeId { get; set; }

        // Цвета и состав
        public int[] SelectedColors { get; set; }
        public int[] SelectedCompositions { get; set; }

        // Поля для тканей
        public decimal? Width { get; set; }
        public int? Density { get; set; }

        // Поля для ниток
        public decimal? Thickness { get; set; }
        public int? LengthPerSpool { get; set; }

        // Поля для молний
        public string ZipperType { get; set; }
        public decimal? ZipperLength { get; set; }

        // Поля для пуговиц
        public string Shape { get; set; }
        public int? ButtonSize { get; set; }
    }
}

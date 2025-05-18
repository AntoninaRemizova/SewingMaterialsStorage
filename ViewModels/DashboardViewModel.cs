using SewingMaterialsStorage.Models;

namespace SewingMaterialsStorage.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalMaterials { get; set; }
        public int FabricCount { get; set; }
        public int ThreadCount { get; set; }
        public int ZipperCount { get; set; }
        public int ButtonCount { get; set; }
        public int LowStockCount { get; set; }
        public List<Supply> RecentSupplies { get; set; }
        public List<Consumption> RecentConsumptions { get; set; }

        public decimal TotalInventoryValue { get; set; }
        public int MaterialsBelowThreshold { get; set; }
    }
}
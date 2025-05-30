namespace SewingMaterialsStorage.Models.ViewModels
{
    public class StockReportItem
    {
        public int MaterialId { get; set; }
        public string MaterialName { get; set; }
        public string Article { get; set; }
        public string MaterialType { get; set; }
        public int TotalSupplies { get; set; }
        public int TotalConsumptions { get; set; }
        public int Balance { get; set; }
        public int MinThreshold { get; set; }
        public bool IsLowStock { get; set; }
    }

    public class MovementReportItem
    {
        public DateTime Date { get; set; }
        public string OperationType { get; set; }
        public string MaterialName { get; set; }
        public int Quantity { get; set; }
        public string OrderId { get; set; }
    }

    public class LowStockReportItem
    {
        public int MaterialId { get; set; }
        public string MaterialName { get; set; }
        public string MaterialType { get; set; }
        public int CurrentBalance { get; set; }
        public int MinThreshold { get; set; }
        public int DeficitAmount { get; set; }
    }
}
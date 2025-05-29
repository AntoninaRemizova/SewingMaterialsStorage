namespace SewingMaterialsStorage.Models
{
    public class Consumption
    {
        public int ConsumptionId { get; set; }
        public int MaterialId { get; set; }
        public Material Material { get; set; }
        public DateTime ConsumptionDate { get; set; }
        public int Quantity { get; set; }
        public string? OrderId { get; set; }
    }
}
namespace SewingMaterialsStorage.Models
{
    public class Supply
    {
        public int SupplyId { get; set; }
        public int MaterialId { get; set; }
        public Material Material { get; set; }
        public DateTime SupplyDate { get; set; }
        public int Quantity { get; set; }
    }
}
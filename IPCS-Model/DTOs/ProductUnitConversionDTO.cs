namespace IPCS_Model.DTOs
{
    public class ProductUnitConversionDTO
    {
        public int ProductId { get; set; }
        public int FromUnitId { get; set; }
        public int ToUnitId { get; set; }
        public decimal Factor { get; set; }
        public int Level { get; set; }
    }
}
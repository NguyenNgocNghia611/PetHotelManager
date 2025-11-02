namespace PetHotelManager.DTOs.Product
{
    public class ProductDto
    {
        public int Id { get; set; }  
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Unit { get; set; }
    }
}

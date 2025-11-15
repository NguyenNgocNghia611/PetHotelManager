namespace PetHotelManager.DTOs.Product
{
    
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Unit { get; set; }

        // ⭐ THÊM MỚI - F7
        public int MinimumStock { get; set; }
        public int ReorderLevel { get; set; }
        public string? Category { get; set; }
        public bool IsActive { get; set; }

        /// <summary>
        /// ⭐ Computed Property - Trạng thái tồn kho
        /// - "OutOfStock": Hết hàng (= 0)
        /// - "Critical": Nghiêm trọng (< MinimumStock)
        /// - "Warning": Cảnh báo (< ReorderLevel)
        /// - "Normal": Đủ hàng
        /// </summary>
        public string StockStatus
        {
            get
            {
                if (StockQuantity == 0)
                    return "OutOfStock";
                if (StockQuantity < MinimumStock)
                    return "Critical";
                if (StockQuantity < ReorderLevel)
                    return "Warning";
                return "Normal";
            }
        }
    }
}
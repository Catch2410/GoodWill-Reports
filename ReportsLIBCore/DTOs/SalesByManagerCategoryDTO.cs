using ReportsLIBCore.Adomd;

namespace ReportsLIBCore.DTOs
{
    public class SalesByManagerCategoryDTO
    {
        [AdomdValue("Менеджер")]
        public string Manager { get; set; }
        [AdomdValue("Категория")]
        public string Category { get; set; }

        [AdomdValue("Продажи, шт")]
        public int SalesPieces { get; set; }
        [AdomdValue("Выручка, РУБ")]
        public decimal ProceedsRUB { get; set; }
        [AdomdValue("Выручка, $")]
        public decimal ProceedsUSD { get; set; }
    }
}
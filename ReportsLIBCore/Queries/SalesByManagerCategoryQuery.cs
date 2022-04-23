using ReportsLIBCore.DTOs;
using System;
using System.ComponentModel.DataAnnotations;
using GoodWill_Libraries.Queries;

namespace ReportsLIBCore.Queries
{
    public class SalesByManagerCategoryQuery : IQuery<SalesByManagerCategoryDTO[]>
    {
        [DataType(DataType.DateTime)]
        public DateTime? Date { get; set; }
    }
}

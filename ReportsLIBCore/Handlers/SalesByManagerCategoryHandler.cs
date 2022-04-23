using ReportsLIBCore.DTOs;
using ReportsLIBCore.Adomd;
using System.Threading.Tasks;
using ReportsLIBCore.Queries;
using GoodWill_Libraries.Queries;

namespace ReportsLIBCore.Handlers
{
    public class SalesByManagerCategoryHandler : IQueryHandler<SalesByManagerCategoryQuery, SalesByManagerCategoryDTO[]>
    {
        public async Task<SalesByManagerCategoryDTO[]> HandleAsync(SalesByManagerCategoryQuery query)
        {
            // «Продажи шт.», «Выручка, РУБ», «Выручка, $» по менеджерам и категориям за выбранный месяц. По умолчанию текущий месяц.
            string mdxQuery =
                @"SELECT 
                NON EMPTY [Контрагент].[Менеджер].[Менеджер] ON 0 
                , NON EMPTY [Товар].[Категория].[Категория] ON 1 
                , {[Measures].[Продажи, шт], [Measures].[Выручка, РУБ], [Measures].[Выручка, $]} ON 2 
                FROM [Модель] 
                WHERE [Дата].[Календарь.Г-М].[Год].&[" + query.Date?.Year + "].&[" + query.Date?.ToString("MMMM") + "]";

            return await AdomdContext.GetReportAsync<SalesByManagerCategoryDTO>(mdxQuery);
        }
    }
}

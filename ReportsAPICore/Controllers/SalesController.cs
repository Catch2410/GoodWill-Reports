using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using ReportsAPICore.Services;
using ReportsLIBCore.DTOs;
using ReportsLIBCore.Queries;
using GoodWill_Libraries.Queries.Dispatcher;

namespace ReportsAPICore.Controllers
{
    /// <summary>
    /// Контроллер отчетов по продажам.
    /// </summary>
    [Route("api/sales")]
    [ApiController]
    public class SalesController : Controller
    {
        public SalesController(IQueryDispatcher queryDispatcher)
        {
            CacheOrDBHandler.QueryDispatcher = queryDispatcher;
        }

        /// <summary>
        /// Отправка отчета на frontend: «Продажи шт.», «Выручка, РУБ», «Выручка, $» по менеджерам и категориям за выбранный месяц. 
        /// По умолчанию текущий месяц.
        /// </summary>
        /// <param name="query">Параментры для отчета:
        /// Date - выбранный месяц</param>
        /// <returns>Отчет в формате json</returns>
        [HttpGet]
        [Route("managercategory")]
        public async Task<IActionResult> GetSalesByManagerCategory([FromQuery]SalesByManagerCategoryQuery query)
        {
            //устанавливаем значение для параметров отчета по умолчанию, если не заданы
            query.Date ??= DateTime.Now;

            //формируем ключ для данных в кэше
            string key = query.GetType().Name + query.Date?.ToString("MMyyyy");

            //передаем параметры и ключ в метод получения отчета из кэша, если ключ уже содержится в памяти или из бд с записью в кэш по ключу.
            return Json(await CacheOrDBHandler.GetResultAsync<SalesByManagerCategoryQuery, SalesByManagerCategoryDTO[]>(key, query) ?? Array.Empty<object>(), Program.JsonLanguageOptions);
        }
    }
}

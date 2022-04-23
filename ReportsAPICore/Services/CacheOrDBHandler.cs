using GoodWill_Libraries.Queries;
using GoodWill_Libraries.Queries.Dispatcher;
using System;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace ReportsAPICore.Services
{
    internal static class CacheOrDBHandler
    {
        /// <summary>
        /// Диспатчер
        /// </summary>
        public static IQueryDispatcher QueryDispatcher;

        /// <summary>
        /// Получает отчет из кэша по ключу или из бд, а так же сохраняет отчет в кэш, если ранее его там не было
        /// </summary>
        /// <typeparam name="TQuery">Тип обьекта параментров отчета</typeparam>
        /// <typeparam name="TResult">Тип DTO массива возвращаемого отчета</typeparam>
        /// <param name="key">ключ кэширования отчета</param>
        /// <param name="query">параметры отчета для запроса из бд</param>
        /// <returns></returns>
        internal static async Task<TResult> GetResultAsync<TQuery, TResult>(string key, TQuery query) where TQuery : IQuery<TResult>
        {
            // кэш
            MemoryCache memoryCache = MemoryCache.Default;

            // если ключ уже есть в кэше, тогда возвращаем отчет из кэша
            if (memoryCache.Contains(key))
                return (TResult)memoryCache.Get(key);

            // получаем отчет из бд
            TResult result = await QueryDispatcher.HandleAsync<TQuery, TResult>(query);

            // сохраняем отчет в кэш с ключем
            memoryCache.Add(key, result, DateTimeOffset.UtcNow.AddHours(14));

            // возвращаем отчет, после получения из бд и сохранения в кэш
            return result;
        }
    }
}

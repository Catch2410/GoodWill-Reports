using Microsoft.AnalysisServices.AdomdClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Reflection;

namespace ReportsLIBCore.Adomd
{
    public sealed class AdomdValueAttribute : Attribute
    {
        public string Name { get; set; }

        public AdomdValueAttribute(string name)
        {
            Name = name;
        }
    }

    internal class AdomdNode
    {
        internal Dictionary<string, object> Node = new Dictionary<string, object>();
    }

    public static class AdomdContext
    {
        /* Достоинства: 
         * 1) Лучше организован маппинг в DTO, указывается иерархия "Категория" вместо "[Товар].[Категория].[Категория].[MEMBER_CAPTION]"
         * 2) Отсутсвуют конвертор вставки данных в DTO, так как путь к данным в атрибутах указывается проще, что не плодит класс конвертеров
         * 3) Когда выйдет CORE версия adomd нужно только поменять библиотеку, нет привязки к System.Data
         * Недостатки:
         * 1) Чтобы указать год-месяц необходимо менять запрос
         * Нейтрально:
         * 1) Запрос MDX проще, однако возвращает н-мерную таблицу CellSet и чтобы использовать в манаджере нужно обьединять оси в кортеж
         */

        /// <summary>
        /// Строка соединения с бд
        /// </summary>
        public static string ConnectionString { get; set; }

        /// <summary>
        /// Получаем отчет
        /// </summary>
        /// <typeparam name="Tdto">Тип DTO</typeparam>
        /// <param name="mdxQuery">Запрос</param>
        /// <returns>Массив DTO с данными</returns>
        public static async Task<Tdto[]> GetReportAsync<Tdto>(string mdxQuery) where Tdto : new()
        {
            //возвращаемый массив DTO
            List<Tdto> resultList = new List<Tdto>();

            //получаем массив с данными из бд
            List<AdomdNode> dataList = await GetDataListAsync(mdxQuery);

            //парсим данные в массив DTO
            foreach (AdomdNode data in dataList)
            {
                Tdto dto = new Tdto();

                //обходим все свойства DTO
                foreach (PropertyInfo property in typeof(Tdto).GetProperties())
                {
                    //если у свойства есть атрибут adomd тогда вставляем данные
                    if (!(property.GetCustomAttribute(typeof(AdomdValueAttribute)) is AdomdValueAttribute attr))
                        continue;

                    //если под это свойство есть данные в массиве данных то устанавливаем их 
                    if (data.Node != null && data.Node.ContainsKey(attr.Name))
                        property.SetValue(dto, Convert.ChangeType(data.Node[attr.Name], property.PropertyType));
                }

                resultList.Add(dto);
            }

            //возвращаем массив DTO с данными
            return resultList.ToArray();
        }

        /// <summary>
        /// Получаем список с данными из бд
        /// </summary>
        /// <param name="mdxQuery">строка запроса</param>
        /// <returns>массив с даннными</returns>
        private static async Task<List<AdomdNode>> GetDataListAsync(string mdxQuery)
        {
            if (!string.IsNullOrEmpty(ConnectionString))
            {
                AdomdConnection connection = new AdomdConnection(ConnectionString);

                try
                {
                    //открываем соединение с бд
                    connection.Open();

                    CellSet cs = await Task.Factory.StartNew(() => new AdomdCommand(mdxQuery, connection).ExecuteCellSet());

                    //если в запросе есть оси тогда устанавливаем данные
                    //возвращаем массив с данными
                    return (cs.Axes.Count > 0) ? await GetDBNodeListAsync(cs, 0, new int[cs.Axes.Count]) : new List<AdomdNode>() { };
                }
                finally
                {
                    //закрываем соединение
                    connection.Close();
                    connection = null;
                }
            }
            throw new ArgumentNullException();
        }

        /// <summary>
        /// Собираем список с данными
        /// </summary>
        /// <param name="cs">ячейки</param>
        /// <param name="axisNum">номер оси</param>
        /// <param name="ordinals">координаты ячейки</param>
        /// <returns>возвращем массив с набором данных по указанным параметрам</returns>
        private static async Task<List<AdomdNode>> GetDBNodeListAsync(CellSet cs, int axisNum, int[] ordinals)
        {
            //возвращаемый список с набором данных указанных в параметрах
            List<AdomdNode> resultList = new List<AdomdNode>();

            //если ось не последняя
            if ((axisNum + 1) < cs.Axes.Count)
            {
                //обходим все позиции на оси
                foreach (Position axisPos in cs.Axes[axisNum].Positions)
                {
                    //устанавливаем координаты ячейки
                    ordinals[axisNum] = axisPos.Ordinal;

                    //получаем массив с набором данных по следующей оси и обходим строки
                    foreach (var node in await GetDBNodeListAsync(cs, axisNum + 1, ordinals))
                    {
                        //добавляем запись текущей позиции на оси к каждой записи возвращенного массива следующих осей - рекурсия
                        node.Node[axisPos.Members[0].ParentLevel.Caption] = axisPos.Members[0].Caption;
                        //добавляем строку к возвращаемому списку
                        resultList.Add(node);
                    }
                }
            }
            else
            {
                //если ось последняя тогда обходим позиции оси и возвращаем список с данными всех ячеек
                AdomdNode node = new AdomdNode();

                foreach (Position axisPos in cs.Axes[axisNum].Positions)
                {
                    ordinals[axisNum] = axisPos.Ordinal;
                    node.Node[axisPos.Members[0].Caption] = GetCellValue(cs.Cells, ordinals);
                }

                resultList.Add(node);
            }

            return resultList;
        }

        /// <summary>
        /// Присваиваем значение ячейки в зависимости от кол-ва осей в запросе
        /// </summary>
        /// <param name="cells"></param>
        /// <param name="ordinals"></param>
        /// <returns></returns>
        private static object GetCellValue(CellCollection cells, int[] ordinals)
        {
            switch (ordinals.Length)
            {
                case 1:
                    return cells[ordinals[0]].Value;
                case 2:
                    return cells[ordinals[0], ordinals[1]].Value;
                case 3:
                    return cells[ordinals[0], ordinals[1], ordinals[2]].Value;
                case 4:
                    return cells[ordinals[0], ordinals[1], ordinals[2], ordinals[3]].Value;
                case 5:
                    return cells[ordinals[0], ordinals[1], ordinals[2], ordinals[3], ordinals[4]].Value;
                case 6:
                    return cells[ordinals[0], ordinals[1], ordinals[2], ordinals[3], ordinals[4], ordinals[5]].Value;
                default:
                    throw new ArgumentException();
            }
        }
    }
}

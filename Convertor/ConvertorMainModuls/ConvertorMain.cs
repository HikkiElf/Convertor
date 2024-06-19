using Convertor.Database;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Convertor.ConvertorMainModuls
{
    public delegate void UpdateLogCallback(string message);
    public delegate void UpdateProgressCallback(int progress);
    internal class ConvertorMain
    {

        private UpdateLogCallback updateLogDelegate;
        private UpdateProgressCallback updateProgressDelegate;

        public ConvertorMain(UpdateLogCallback logCallback, UpdateProgressCallback progressCallback)
        {
            updateLogDelegate = logCallback;
            updateProgressDelegate = progressCallback;
        }

        /// <summary>
        /// Асинхронно конвертирует данные из SQL Server в JSON файл.
        /// </summary>
        /// <param name="selectedConnectionStringName">Имя строки подключения к базе данных.</param>
        public async Task ConvertSqlServerToJSONAsync(string selectedConnectionStringName)
        {
            try
            {
                await Task.Run(async () =>
                {
                    // Устанавливаем соединение с БД
                    using (var dbContext = new qw5Entities(selectedConnectionStringName))
                    {
                        // Словарь для хранения данных таблиц
                        var tablesData = new Dictionary<string, List<Dictionary<string, object>>>();

                        // Получаем DbSet свойства контекста БД
                        var dbSetProperties = dbContext.GetType().GetProperties()
                        .Where(p => p.PropertyType.IsGenericType &&
                                    p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

                        // Путь к лог-файлу
                        string logPath = $@"E:\Diploma\toFilelogs\Log-{DateTime.Now.ToString().Replace(":", "-")}.txt";

                        // Запись логов в файл
                        using (StreamWriter sw = File.CreateText(logPath))
                        {

                            UpdateLog($"[{DateTime.Now:HH:mm:ss.fff}] [BEGIN] Начало конвертации");
                            await sw.WriteLineAsync($"[BEGIN] Начало конвертации {DateTime.Now:HH:mm:ss.fff}");

                            int totalTables = dbSetProperties.Count();
                            int processedTables = 0;

                            // Перебираем все таблицы (DbSet свойства)
                            foreach (var dbSetProperty in dbSetProperties)
                            {

                                // Получаем тип сущности таблицы
                                var entityType = dbSetProperty.PropertyType.GetGenericArguments().First();

                                try
                                {
                                    // Получаем DbSet для текущей таблицы
                                    var dbSet = dbContext.Set(entityType);

                                    // Вызываем метод ToList() для получения всех записей из таблицы
                                    var toListMethod = typeof(Enumerable).GetMethod("ToList").MakeGenericMethod(entityType);
                                    var entities = toListMethod.Invoke(null, new object[] { dbSet });

                                    // Преобразуем данные в список словарей
                                    var filteredEntities1 = ((IEnumerable<object>)entities).Select(e =>
                                    {
                                        var entityProperties = entityType.GetProperties()
                                            .Where(property => !property.GetMethod?.IsVirtual ?? true)
                                            .ToDictionary(property => property.Name, property => property.GetValue(e));

                                        return entityProperties;
                                    }).ToList();

                                    // Добавляем данные таблицы в общий словарь
                                    tablesData.Add(entityType.Name, filteredEntities1);

                                    processedTables++;
                                    int progress = (int)((double)processedTables / totalTables * 100);
                                    UpdateProgress(progress);

                                    UpdateLog($"[INFO] Таблица '{entityType.Name}' добавлена {DateTime.Now:HH:mm:ss.fff}");
                                    await sw.WriteLineAsync($"[INFO] Таблица '{entityType.Name}' добавлена {DateTime.Now:HH:mm:ss.fff}");
                                }
                                catch(Exception ex)
                                {
                                    UpdateLog($"[ERROR] Ошибка при обработке таблицы '{entityType.Name}': {ex.Message}");
                                    await sw.WriteLineAsync($"[ERROR] Ошибка при обработке таблицы '{entityType.Name}': {ex.Message}");
                                }
                                
                            }

                            // Сохранение JSON файла
                            SaveFileDialog saveFileDialog = new SaveFileDialog();
                            saveFileDialog.Filter = "JSON Files (*.json)|*.json";
                            saveFileDialog.Title = "Save JSON File";
                            saveFileDialog.FileName = "EXPORTED.json";

                            if (saveFileDialog.ShowDialog() == true)
                            {
                                string jsonFilePath = saveFileDialog.FileName;
                                SerializeToJsonAndSave(tablesData, jsonFilePath);
                                UpdateLog($"[SUCCESS] Конвертация завершена успешно {DateTime.Now:HH:mm:ss.fff}");
                                await sw.WriteLineAsync($"[SUCCESS] Конвертация завершена успешно {DateTime.Now:HH:mm:ss.fff}");
                            }

                        }
                    }

                });
            }
            catch (Exception ex)
            {
                // Обработка исключений на верхнем уровне
                MessageBox.Show($"Произошла ошибка: {ex.Message}");
            }
        }

        /// <summary>
        /// Асинхронно конвертирует данные из JSON файла в SQL Server.
        /// </summary>
        /// <param name="selectedConnectionStringName">Имя строки подключения к базе данных.</param>
        public async Task ConvertJSONToSqlServerAsync(string selectedConnectionStringName)
        {
            await Task.Run(async () =>
            {
                // Диалог выбора JSON файла
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "JSON Files (*.json)|*.json";
                openFileDialog.Title = "Select a JSON file";

                if (openFileDialog.ShowDialog() == true)
                {
                    string jsonPath = openFileDialog.FileName;

                    // Порядок таблиц для импорта
                    string[] tablesOrder = {
    "Виды_пикетов",
    "Договор",
    "Должность",
    "Заказчик",
    "Измерительное_оборудование",
    "Квалификация",
    "Сотрудник",
    "Координаты_точки",
    "Электромагнитные_измерения",
    "Фильтры_обработок",
    "Трансформанта_измерения",
    "Телеметрические_измерения",
    "Обработки_на_профиле",
    "Результаты_измерения",
    "Методика",
    "Проект",
    "Площадь",
    "Профиль",
    "Пикет",
    "Площадь_УглыПериметра",
    "Профиль_СписокОбработок",
    "Профиль_ТелеметрическиеИзмерения",
    "Профиль_ТочкиИзломов",
    "Профиль_ЭлектромагнитныеИзмерения",
    "Список_фильтров_над_обработкой",
    "Пикет_ИзмерительноеОборудование",
    "Пикет_ПромежуточныеИзмерения",
    "Пикет_ПромежуточныеТрансформантыИзмерения",
    "Пикет_Сотрудники",
    "Пикет_ТрансформантаИзмерения"
};

                    // Устанавливаем соединение с БД
                    using (var dbContext = new qw5Entities(selectedConnectionStringName))
                    {
                        if (!File.Exists(jsonPath))
                        {
                            Console.WriteLine("JSON файл не найден.");
                            return;
                        }

                        // Чтение и десериализация JSON файла
                        var jsonData = File.ReadAllText(jsonPath);
                        var data = JsonConvert.DeserializeObject<Dictionary<string, List<object>>>(jsonData);

                        // Путь к лог-файлу
                        string logPath = $@"E:\Diploma\toSqllogs\Log-{DateTime.Now.ToString().Replace(":", "-")}.txt";

                        // Запись логов в файл
                        using (StreamWriter sw = File.CreateText(logPath))
                        {
                            UpdateLog($"[BEGIN] Начало конвертации {DateTime.Now:HH:mm:ss.fff}");
                            await sw.WriteLineAsync($"[BEGIN] Начало конвертации {DateTime.Now:HH:mm:ss.fff}");

                            int totalTables = tablesOrder.Length;
                            int processedTables = 0;

                            // Начало транзакции
                            using (var transaction = dbContext.Database.BeginTransaction())
                            {
                                try
                                {
                                    // Перебираем таблицы в заданном порядке
                                    foreach (var tableName in tablesOrder)
                                    {
                                        var tableData = data[tableName];

                                        int totalEntities = tableData.Count;
                                        int processedEntities = 0;

                                        // Обрабатываем каждую сущность (строку) в таблице
                                        foreach (var entity in tableData)
                                        {
                                            var columns = new List<string>();
                                            var values = new List<string>();
                                            var parameters = new List<SqlParameter>();

                                            var jEntity = (JObject)entity;

                                            // Формируем SQL-запрос на вставку данных
                                            foreach (var property in jEntity.Properties())
                                            {
                                                var propertyName = property.Name;
                                                var propertyValue = property.Value;

                                                if (propertyName.ToLower() != "id")
                                                {
                                                    columns.Add($"[{propertyName.Replace("_", " ")}]");
                                                    object parameterValue = propertyValue.ToObject<object>();
                                                    if (parameterValue == null)
                                                    {
                                                        parameterValue = DBNull.Value;
                                                    }
                                                    parameters.Add(new SqlParameter($"@{propertyName}", parameterValue));
                                                }
                                            }
                                            var columnNames = string.Join(", ", columns);
                                            var paramNames = string.Join(", ", parameters.Select(p => p.ParameterName));

                                            var insertQuery = $"INSERT INTO [{tableName.Replace("_", " ")}] ({columnNames}) VALUES ({paramNames})";
                                            Console.WriteLine(insertQuery);
                                            var tst = parameters.ToArray();
                                            dbContext.Database.ExecuteSqlCommand(insertQuery, parameters.ToArray());

                                            processedEntities++;
                                            double tableProgress = (double)processedTables / totalTables;
                                            double entityProgress = (double)processedEntities / totalEntities;
                                            int progress = (int)((tableProgress + (entityProgress / totalTables)) * 100);
                                            UpdateProgress(progress);

                                            // Логирование успешной вставки
                                            string logString = "";
                                            foreach (var prop in parameters)
                                            {
                                                if (prop != null && prop.Value != null)
                                                {
                                                    logString += $"{prop}={prop.Value} ";
                                                }
                                                else
                                                {
                                                    string nullValueString = prop != null ? $"{prop.ParameterName} = NULL" : "NULL Parameter";
                                                    logString += $"{nullValueString} ";
                                                    UpdateLog($"[WARNING] Обнаружено нулевое значение: {nullValueString} {DateTime.Now:HH:mm:ss.fff}");
                                                    await sw.WriteLineAsync($"[WARNING] Обнаружено нулевое значение: {nullValueString} {DateTime.Now:HH:mm:ss.fff}");
                                                }
                                            }
                                            UpdateLog($"[INFO] Команда [{logString}] выполнена успешно {DateTime.Now:HH:mm:ss.fff}");
                                            await sw.WriteLineAsync($"[INFO] Команда [{logString}] выполнена успешно {DateTime.Now:HH:mm:ss.fff}");
                                        }
                                        processedTables++;
                                    }

                                    // Подтверждение транзакции после успешного импорта всех таблиц
                                    transaction.Commit();
                                    UpdateLog($"[SUCCESS] Конвертация завершена успешно {DateTime.Now:HH:mm:ss.fff}");
                                    await sw.WriteLineAsync($"[SUCCESS] Конвертация завершена успешно {DateTime.Now:HH:mm:ss.fff}");
                                }
                                catch (Exception ex)
                                {
                                    // Откат транзакции в случае ошибки
                                    transaction.Rollback();
                                    UpdateLog($"[ERROR] Ошибка при конвертации: {ex.Message} {DateTime.Now:HH:mm:ss.fff}");
                                    await sw.WriteLineAsync($"[ERROR] Ошибка при конвертации: {ex.Message} {DateTime.Now:HH:mm:ss.fff}");
                                }
                            }
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Сериализует данные в JSON формат и сохраняет в файл.
        /// </summary>
        /// <param name="data">Данные для сериализации.</param>
        /// <param name="filePath">Путь к файлу для сохранения.</param>
        void SerializeToJsonAndSave(object data, string filePath)
        {
            string json = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);

            File.WriteAllText(filePath, json);

            Console.WriteLine($"Data has been successfully saved to {filePath}");
        }

        /// <summary>
        /// Вызывает делегат для обновления лога.
        /// </summary>
        /// <param name="message">Сообщение для лога.</param>
        private void UpdateLog(string message)
        {
            updateLogDelegate?.Invoke(message);
        }

        /// <summary>
        /// Вызывает делегат для обновления прогресса.
        /// </summary>
        /// <param name="progress">Значение прогресса.</param>
        private void UpdateProgress(int progress)
        {
            updateProgressDelegate?.Invoke(progress);
        }
    }
}


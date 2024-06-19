using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Convertor.ConvertorMainModuls;
using Convertor.Windows;

namespace Convertor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ConvertorMain convertorMain;
        private ProgressWindow progressWindow;
        private string selectedConnectionStringName;
        public MainWindow()
        {
            InitializeComponent();
            // Инициализация конвертера и передача делегатов для обновления лога и прогресса
            convertorMain = new ConvertorMain(UpdateLog, UpdateProgress);
        }

        /// <summary>
        /// Обработчик события нажатия кнопки "Конвертировать в файл".
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private async void ConvertToFile_Click(object sender, RoutedEventArgs e)
        {
            // Окно выбора базы данных
            DatabaseSelectionWindow selectionWindow = new DatabaseSelectionWindow();
            if (selectionWindow.ShowDialog() == true)
            {
                selectedConnectionStringName = selectionWindow.SelectedConnectionStringName;
                // Создание и отображение окна прогресса
                progressWindow = new ProgressWindow();
                progressWindow.Show();
                // Запуск асинхронной операции конвертации в файл
                await convertorMain.ConvertSqlServerToJSONAsync(selectedConnectionStringName);
            }
        }


        /// <summary>
        /// Обработчик события нажатия кнопки "Конвертировать на сервер".
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private async void ConvertOnServer_Click(object sender, RoutedEventArgs e)
        {
            // Окно выбора базы данных
            DatabaseSelectionWindow selectionWindow = new DatabaseSelectionWindow();
            if (selectionWindow.ShowDialog() == true)
            {
                selectedConnectionStringName = selectionWindow.SelectedConnectionStringName;
                // Создание и отображение окна прогресса
                progressWindow = new ProgressWindow();
                progressWindow.Show();
                // Запуск асинхронной операции конвертации на сервер
                await convertorMain.ConvertJSONToSqlServerAsync(selectedConnectionStringName);
            }
             
        }

        /// <summary>
        /// Обновляет лог в окне прогресса.
        /// </summary>
        /// <param name="message">Сообщение для лога.</param>
        private void UpdateLog(string message)
        {
            progressWindow.UpdateLog(message);
        }

        /// <summary>
        /// Обновляет прогресс в окне прогресса.
        /// </summary>
        /// <param name="progress">Значение прогресса.</param>
        private void UpdateProgress(int progress)
        {
            progressWindow.UpdateProgress(progress);
        }

        /// <summary>
        /// Открывает папку с логами в проводнике.
        /// </summary>
        /// <param name="folderPath">Путь к папке с логами.</param>
        private void OpenLogsFolder(string folderPath)
        {
            // Ensure the folder exists before trying to open it
            if (Directory.Exists(folderPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = folderPath,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            else
            {
                MessageBox.Show($"Папка с логами не найдена: {folderPath}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Обработчик события нажатия кнопки "Логи JSON".
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void ToJSONlogs_Click(object sender, RoutedEventArgs e)
        {
            OpenLogsFolder(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "E:\\Diploma\\toFileLogs"));
        }

        /// <summary>
        /// Обработчик события нажатия кнопки "Логи SQL".
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void ToSQLlogs_Click(object sender, RoutedEventArgs e)
        {
            OpenLogsFolder(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "E:\\Diploma\\toSqlLogs"));
        }
    }
}

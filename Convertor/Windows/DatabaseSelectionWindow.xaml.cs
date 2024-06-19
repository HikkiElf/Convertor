using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using System.Configuration;
using System.Net.NetworkInformation;

namespace Convertor.Windows
{
    /// <summary>
    /// Interaction logic for DatabaseSelectionWindow.xaml
    /// </summary>
    public partial class DatabaseSelectionWindow : Window
    {
        // Свойство для хранения имени выбранной строки подключения
        public string SelectedConnectionStringName { get; private set; }

        // Получение имен строк подключения из конфигурационного файла, исключая первую (по умолчанию)
        string[] connectionStringNames = ConfigurationManager.ConnectionStrings
            .Cast<ConnectionStringSettings>()
            .Skip(1)
            .Select(c => c.Name)
            .ToArray();

        public DatabaseSelectionWindow()
        {
            InitializeComponent();
            // Заполнение списка ListBox доступными именами строк подключения
            databaseListBox.ItemsSource = connectionStringNames;
        }

        /// <summary>
        /// Обработчик события нажатия на кнопку "Выбрать".
        /// </summary>
        /// <param name="sender">Объект, вызвавший событие.</param>
        /// <param name="e">Аргументы события.</param>
        private void ConvertButton_Click(object sender, RoutedEventArgs e)
        {
            // Сохранение выбранного имени строки подключения
            SelectedConnectionStringName = databaseListBox.SelectedItem as string;
            // Установка результата диалога как true (успешно)
            this.DialogResult = true;
        }
    }
}

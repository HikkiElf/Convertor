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

namespace Convertor.Windows
{
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        // Максимальное количество сообщений в логе
        private const int MaxLogMessages = 10;
        // Очередь для хранения сообщений лога
        private Queue<string> logMessages = new Queue<string>(MaxLogMessages);
        // Счетчик сообщений для очистки лога
        private int messageCount = 0;
        public ProgressWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Обновляет лог новым сообщением.
        /// </summary>
        /// <param name="message">Сообщение для добавления в лог.</param>
        public void UpdateLog(string message)
        {
            Dispatcher.Invoke(() =>
            {
                // Добавляет сообщение в очередь
                logMessages.Enqueue(message);

                // Если очередь превышает определенное количество сообщений, удаляет самое старое сообщение
                if (logMessages.Count > MaxLogMessages)
                {
                    logMessages.Dequeue();
                }

                // Обновляет текс в элементе в окне
                LogTextBlock.Text = string.Join(Environment.NewLine, logMessages);

                // Прокручивает сообщения до последней строчки
                LogScrollViewer.ScrollToEnd();
            });
        }

        /// <summary>
        /// Обновляет значение прогресса.
        /// </summary>
        /// <param name="progress">Новое значение прогресса.</param>
        public void UpdateProgress(int progress)
        {
            Dispatcher.Invoke(() =>
            {
                ConversionProgressBar.Value = progress;
            });
        }

        /// <summary>
        /// Очищает лог.
        /// </summary>
        private void ClearLog()
        {
            LogTextBlock.Text = "";
            logMessages.Clear();
        }
    }
}

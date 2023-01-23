using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
using LibPayments;

namespace SortInJsonWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        string fileInput = @"D:\FilesToRead\payments.json";
        string fileIncomes = @"D:\FilesToRead\Incomes.json";
        string fileOutcomes = @"D:\FilesToRead\Outcomes.json";

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Создание задачи вне UI потока
            Task deserialize = Task.Run(() =>
            {
                Dispatcher.Invoke(() => pbStatus.IsIndeterminate = true);
                Dispatcher.Invoke(() => textBoxInput.Text = fileInput);
                Dispatcher.Invoke(() => textBoxIncomes.Text = fileIncomes);
                Dispatcher.Invoke(() => textBoxOutcomes.Text = fileOutcomes);
                // Проверка существования файла с данными
                if (!File.Exists(fileInput))
                {
                    MessageBox.Show("Нет такого файла!");
                    return;
                }

                // определение наборов данных для хранения исходного файла и обработанных данных                
                List<Payment> allPayments = new List<Payment>();
                List<Payment> incomePayments = new List<Payment>();
                List<Payment> outcomePayments = new List<Payment>();

                // Потоковый метод чтения данных из файла json (Newtonsoft)
                JsonSerializer serializer = new JsonSerializer();
                using (FileStream s = File.Open(fileInput, FileMode.Open))
                using (StreamReader sr = new StreamReader(s))
                using (JsonReader reader = new JsonTextReader(sr))
                {
                    while (!sr.EndOfStream)
                    {
                        allPayments = serializer.Deserialize<List<Payment>>(reader);
                    }
                }

                // Проверка заполнения данных в созданный объект
                if (allPayments.Count == 0)
                {
                    MessageBox.Show("Файл пустой или ошибка чтения!");
                    return;

                }

                // объекты блокировки для исключения одновременной записи в объекты list
                object _objLockIn = new();
                object _objLockOut = new();

                // Распределение данных по расходам/доходам в нескольких потоках
                Parallel.ForEach(allPayments, (payment) =>
                {
                    if (payment.amount > 0)
                    {
                        lock (_objLockIn)
                        {
                            incomePayments.Add(payment);
                        }
                    }
                    else
                    {
                        lock (_objLockOut)
                        {
                            outcomePayments.Add(payment);
                        }
                    }
                });

                // Запись данных в файлы json в двух потоках
                Parallel.Invoke(() => writeValuesToJson(incomePayments, fileIncomes), () => writeValuesToJson(outcomePayments, fileOutcomes));

                Dispatcher.Invoke(() => pbStatus.IsIndeterminate = false);
                MessageBox.Show("Задача выполнена успешно!\nДанные записаны и упорядочены по дате!");

            });
        }

        // Запись данных в файл с предварительной сортировкой
        void writeValuesToJson(List<Payment> payments, string fileName)
        {
            // Сортировка данных по дате
            var paymentsSorted = payments.OrderBy(d => d.PaidOn).ToList();

            // Потоковая запись данных в файл json (Newtonsoft)
            using (StreamWriter file = File.CreateText(fileName))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, paymentsSorted);
            }
        }

    }
    
}

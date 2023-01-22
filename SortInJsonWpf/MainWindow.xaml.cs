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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Task deserialize = Task.Run(() =>
            {
                if (!File.Exists(@"D:\FilesToRead\payments.json"))
                {
                    MessageBox.Show("Нет такого файла!");
                    return;
                }
                FileInfo inputFile = new FileInfo(@"D:\FilesToRead\payments.json");

                JsonSerializer serializer = new JsonSerializer();
                List<Payment> payments;
                using (FileStream s = File.Open(inputFile.FullName, FileMode.Open))
                using (StreamReader sr = new StreamReader(s))
                using (JsonReader reader = new JsonTextReader(sr))
                {
                    while (!sr.EndOfStream)
                    {
                        payments = serializer.Deserialize<List<Payment>>(reader);
                    }
                }


            });
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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

namespace ClientStreet
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static public Socket socket;
        IPEndPoint ipPoint;
        public MainWindow()
        {
            InitializeComponent();
            btnConnect.IsEnabled = true;
            btDisconnect.IsEnabled = false;
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            // адрес и порт сервера, к которому будем подключаться
            int port;
            string address;
            address = tbIpAdress.Text;
            port = Convert.ToInt32(tbPort.Text);
            try
            {
                ipPoint = new IPEndPoint(IPAddress.Parse(address), port);
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // подключаемся к удаленному хосту
                socket.Connect(ipPoint);
                btnConnect.IsEnabled = false;
                btDisconnect.IsEnabled = true;
            }
            catch (Exception exc)
            {
                ListDataAdd(exc.Message + "\n");
                return;
            }
        }

        private async void btSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //                byte[] data = Encoding.UTF8.GetBytes(tbSend.Text);

                StringBuilder sb = await ThreadSendReceiveAsync(tbSend.Text);

                ListDataAdd(sb.ToString() + "\n");

            }
            catch(Exception exc)
            {
                ListDataAdd(exc.Message + "\n");
                return;
            }
        }

        static StringBuilder ThreadSendReceive(string text)
        {
            byte[] data = Encoding.UTF8.GetBytes(text);
            socket.Send(data);
            // получаем ответ
            data = new byte[4 * 1024]; // буфер для ответа
            StringBuilder builder = new StringBuilder();
            int bytes = 0; // количество полученных байт
            do
            {
                bytes = socket.Receive(data, data.Length, 0);
                builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
            }
            while (socket.Available > 0);
            return builder;
        }

        // определение асинхронного метода
        static async Task<StringBuilder> ThreadSendReceiveAsync(string text)
        {
            return await Task.Run(() => ThreadSendReceive(text));
        }

        private void btClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btDisconnect_Click(object sender, RoutedEventArgs e)
        {
            // закрываем сокет
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            btnConnect.IsEnabled = true;
            btDisconnect.IsEnabled = false;
        }

        public void ListDataAdd(string text)
        {
            var textBlockWork = new TextBlock();
            textBlockWork.Width = 450;
            textBlockWork.Height = 15;
            textBlockWork.Text = text;
            ListData.Items.Add(textBlockWork);

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace ServerStreet
{
    class Program
    {
        public static Thread SrvThread;
        public static  Socket socServer;
        static List<Street> streets;

        static void Main(string[] args)
        {
            //List<Street> streetsNew = new List<Street>();

            //streetsNew.Add(new Street()
            //{
            //    Index = "010101",
            //    Name = "ул.Алиханова",
            //});
            //streetsNew.Add(new Street()
            //{
            //    Index = "020202",
            //    Name = "ул.Борисевич",
            //});
            //streetsNew.Add(new Street()
            //{
            //    Index = "030303",
            //    Name = "ул.Валиханова",
            //});

            XmlSerializer formatter = new XmlSerializer(typeof(List<Street>));

            //// сериализация в файл
            //using (FileStream fs = new FileStream("streets.xml", FileMode.Create))
            //{
            //    formatter.Serialize(fs, streetsNew);
            //}

            // десериализация
            using (FileStream fs = new FileStream("streets.xml", FileMode.Open))
            {
                streets = (List<Street>)formatter.Deserialize(fs);
            }

            socServer =
                new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            
                string srvAddress = "0.0.0.0";
                int srvPort = 12345;
                socServer.Bind(new
                IPEndPoint(IPAddress.Parse(srvAddress), srvPort));
                socServer.Listen(100);
            // далее должна быть команда Accept()
            SrvThread = new Thread(ServerThreadRoutine);
            //SrvThread.IsBackground = true;
            SrvThread.Start(socServer);


            while (true)
                {
                    Socket client = socServer.Accept();
                    Console.WriteLine("Клиент подключен: ");
                    Console.WriteLine(
                       client.RemoteEndPoint.ToString());
                    ThreadPool.QueueUserWorkItem(
                      ClientThreadProc, client);
                }
        }

        static void ServerThreadRoutine(object obj)
        {
            TcpListener srvSock = obj as TcpListener;
            // синхронный вариант сервера
            try
            {
                while (true)
                {
                    // не ассинхроннвй блокирующий вызов Accept()
                    // работа с клиентом в отдельном потоке
                    TcpClient client = srvSock.AcceptTcpClient();
                    //   запуск клиентского потока -
                    ThreadPool.QueueUserWorkItem(
                        ClientThreadProc, client);
                }
            }
            catch
            {
                return;
            }
        }


        // поток обслуживания удаленного клиента
        static void ClientThreadProc(object obj)
        {
            // протокол работы сервера - эхо-сервер
            Socket client = (Socket)obj;// as Socket;
            string index, message;
            try
            {
                while (true)
                {
                    // получаем ответ
                    byte[] data = new byte[4 * 1024]; // буфер для ответа
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0; // количество полученных байт

                    do
                    {
                        bytes = client.Receive(data, data.Length, 0);
                        builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
                    }
                    while (client.Available > 0);

                    // отправляем ответ
                    index = builder.ToString();
                    var findStreet = streets.Where(t => t.Index == index); ;
                    if (findStreet.Count() == 0)
                    {
                        message = "Улица с индексом " + index + " не найдена.";
                    }
                    else
                    {
                        message = "Улица с индексом " + index + " - " + findStreet.FirstOrDefault().Name;
                    }

                    data = Encoding.UTF8.GetBytes(message);
                    client.Send(data);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR:" + ex.Message);
            }
            client.Shutdown(SocketShutdown.Both);
            client.Close();

        }
    }
}

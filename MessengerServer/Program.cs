using MessengerServer.DB;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;

namespace MessengerServer
{
    internal partial class Program
    {
        private static IDataBase _dataBase = new pgDataBase("Host=localhost;Port=5432;Database=Crassage;Username=group1;Password=12345");
        public static IDataBase DataBase { get => _dataBase;  }

        static void Main(string[] args)
        {
            StartServer();
        }



        static void StartServer()
        {
            TcpListener _server = new TcpListener(IPAddress.Any, 2000); //port = 2000

            _server.Start();
            Console.WriteLine("Server started");

            string request = "";



            while (request != "STOP_WORK")
            {
                TcpClient _client = _server.AcceptTcpClient();      //Ожидание подключения клиента

                Thread myThread1 = new Thread( new ParameterizedThreadStart(ConnectClient));
                myThread1.Start(_client);
            }
        }

        

        /// <summary>
        /// Отправляет ответ клиенту
        /// </summary>
        /// <param name="stream"> Поток через который идет общение с клиентом</param>
        /// <param name="message"></param>
        private static void SendAnswer(NetworkStream stream, string message)
        {
            var answer = Encoding.UTF8.GetBytes(message);
            stream.Write(answer);
            stream.Flush();
        }

        /// <summary>
        /// Ожидание запроса от клиента
        /// </summary>
        /// <param name="stream">Поток через который идет общение с клиентом</param>
        /// <returns>Возвращает строку с запросом клиента</returns>
        private static string GetRequest(NetworkStream stream)
        {
            string request = "";
            byte[] buffer = new byte[1024];

            int lenght = stream.Read(buffer, 0, buffer.Length);
            if (lenght > 0) request = Encoding.UTF8.GetString(buffer, 0, lenght);

            return request;
        }

        /// <param name="path"></param>
        /// <returns> Возвращает строку-ответ из имен файлов и директорий, находящихся по адресу пути</returns>
        private static string CreateAnswer(string path)
        {
            string result = "";

            string[] dir = Directory.GetFileSystemEntries(path);
            foreach (string dirEntry in dir)
            {
                result += dirEntry + "\n";
            }


            return result != "" ? result : "Здесь пока что пусто";
        }


    }
}
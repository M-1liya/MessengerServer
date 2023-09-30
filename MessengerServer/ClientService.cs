using MessengerServer.DB;
using MessengerServer.Exeptions;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MessengerServer
{
    internal partial class Program
    {

        static void ConnectClient(object? _client)
        {
            int? _IdClient = null;
            TcpClient client;

            if (_client == null || _client is not TcpClient) return;
            client = (TcpClient)_client;

            NetworkStream _stream = client.GetStream();       //Получение потока


            //Отправка первого сообщения

            const string FirstMessage = "HEAR_ME?";
            SendAnswer(_stream, FirstMessage);

            Console.WriteLine("Server sent first message: \n" + FirstMessage);
            //
            string request = GetRequest(_stream);

            if (request == null || request == "")
            {
                _stream.Close();
                client.Close();
                return;
            }

            request = GetRequest(_stream);

            if (request == "LOG_IN")
            {
                string log_in_str = GetRequest(_stream);
                LogIn(log_in_str, ref _IdClient);
                //Результат входа
            }
            else if (request == "REGIST")
            {
                string auth_str = GetRequest(_stream);
                Registration(auth_str, ref _IdClient, _stream);
                //Результат авторизации
            }
            else
                SendAnswer(_stream, "Error: Expected 'LOG_IN' or 'AUTH'");



            try
            {
                while (true)
                {

                    request = GetRequest(_stream); //Ожидание запроса

                    string[] arrRequest = request.Split("::");
                    if (arrRequest[0] == "SEND")
                    {
                        if (arrRequest.Length < 3)
                        {
                            SendAnswer(_stream, "WRONG_TEMPLATE");
                            continue;
                        }

                        if (arrRequest.Length > 3)
                            for (int i = 3; i < arrRequest.Length; i++)
                                arrRequest[2] += arrRequest[i];

                        SendMessageToClient(_IdClient.ToString(), arrRequest[1], arrRequest[2]);
                    }
                    else if (false) { }
                    else
                        SendAnswer(_stream, "INVALID_REQUEST");

                }
            }
            catch { Console.WriteLine("Соединение с клиентом потеряно"); }
        }



        private static string LogIn(string log_in_str, ref int? id)
        {
            //Обращение к БД проверка наличия пользователя и ответ
            return "SUCCESS";
        }

        private static string Registration(string auth_str, ref int? id, NetworkStream _stream)
        {
            //Обращение к БД, проверка совпадений, результат
            string[] tmp = auth_str.Split("::");
            string username = tmp[0], user_password = tmp[1];

            if (!DataBase.CheckUniqueLogin(username))
                return "ERROR";

            SendAnswer(_stream, "SUCCESS");

            UserData ud = new UserData(GetRequest(_stream));
            
            try
            {
                int user_id = DataBase.Registration(username, user_password, ud);
                SendAnswer(_stream, user_id.ToString());
            }
            catch (RegistrationError)
            {
                SendAnswer(_stream, "ERROR");
            }
            
            return "SUCCESS";
        }


        /// <param name="id_client">id отправителя</param>
        /// <param name="id_addressee">id получателя сообщения</param>
        /// <param name="message"></param>
        /// <param name="_stream"></param>
        private static void SendMessageToClient(string? id_client, string id_addressee, string message)
        {
            //Поиск получателя по БД, если есть такой, то ноходим его файл и записываем в него сообщение

            string ME = id_client == null? "" : id_client;
            string[] lines = File.ReadAllLines(id_addressee + ".txt");

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i] == $"FROM::{ME}:")
                {
                    for (int j = i; j < lines.Length; j++)
                        if (lines[j + 1] == $"<{ME}>")
                        {
                            lines[j] = message;
                            break;
                        }
                    break;
                }
                if (i + 1  == lines.Length)
                {
                    File.AppendAllText(id_addressee,
                        $"FROM::{ME}:\n" +
                        $"{message}\n" +
                        $"\n" +
                        $"<{ME}>");
                }
            }


        }
    }
}

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

            if (_client == null || !(_client is TcpClient)) return;
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

            string EnterResult = "";
            while(_IdClient == null || _IdClient < 0) //Цикл входа и регистрации.
            {

                request = GetRequest(_stream);

                if (request == "LOG_IN")
                {
                    string log_in_str = GetRequest(_stream);
                    _IdClient = LogIn(log_in_str, ref _IdClient, _stream);
                    //Результат входа
                }
                else if (request == "REGIST")
                {
                    string auth_str = GetRequest(_stream);
                    _IdClient = Registration(auth_str, ref _IdClient, _stream);
                    //Результат авторизации
                }
                else
                    SendAnswer(_stream, "Error: Expected 'LOG_IN' or 'REGIST'");
            }



            try
            {
                while (true)
                {

                    request = GetRequest(_stream); //Ожидание запроса

                    string[] arrRequest = request.Split("::");
                    if (arrRequest[0] == "SEND")
                    {
                        if (arrRequest.Length < 5)
                        {
                            SendAnswer(_stream, "WRONG_TEMPLATE");
                            continue;
                        }

                        if (arrRequest.Length > 3)
                            for (int i = 5; i < arrRequest.Length; i++)
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



        private static int LogIn(string log_in_str, ref int? id, NetworkStream _stream)
        {
            //Обращение к БД проверка наличия пользователя и ответ
            string[] tmp = log_in_str.Split("::");
            string username = tmp[0], user_password = tmp[1];

            if (DataBase.CheckUniqueLogin(username))
            {
                SendAnswer(_stream, "ERROR");
                return -1;
            }

            try
            {
                int user_id = DataBase.Log_In(username, user_password);

                if(user_id > 0)
                {
                    UserData userData = DataBase.GetUserData(user_id);
                    if (userData.FirstName != "")
                    {
                        SendAnswer(_stream, userData.ToString());
                        return user_id;
                    }
                }
                
            }
            catch (LogInError e)
            {
                if(e.Message == "Ошибка с чтением")
                    SendAnswer(_stream, "ERROR");
                else
                    SendAnswer(_stream, e.Message);
                return -1;
            }

            SendAnswer(_stream, "ERROR");
            return -1;
        }

        private static int Registration(string auth_str, ref int? id, NetworkStream _stream)
        {
            //Обращение к БД, проверка совпадений, результат
            string[] tmp = auth_str.Split("::");
            string username = tmp[0], user_password = tmp[1];

            if (!DataBase.CheckUniqueLogin(username))
            {
                SendAnswer(_stream, "ERROR");
                return -1;
            }

            SendAnswer(_stream, "SUCCESS");

            UserData ud = new UserData(GetRequest(_stream));
            
            try
            {
                int user_id = DataBase.Registration(username, user_password, ud);
                SendAnswer(_stream, user_id.ToString());
                return user_id;
            }
            catch (RegistrationError)
            {
                SendAnswer(_stream, "ERROR");
                return -1;
            }
            
        }


        /// <param name="id_client">id отправителя</param>
        /// <param name="id_addressee">id получателя сообщения</param>
        /// <param name="message"></param>
        /// <param name="_stream"></param>
        private static void SendMessageToClient(string str_chat_id, string? id_client, string id_addressee, string message, string time)
        {
            /* Если ранее занятой комнаты нет, то она выделяется и записывается сообщение,  
            иначе проверяется доступ и после записывается сообщение. */

            int chat_id = str_chat_id == "null" ? -1 : int.Parse(str_chat_id);

            

        }
    }
}

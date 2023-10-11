using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerServer.DB
{
    internal interface IDataBase
    {
        /// <summary>
        /// Проверка логина на уникальность.
        /// </summary>
        /// <param name="username"></param>
        /// <returns>Возвращает false если пользователь с таким логином уже существует, иначе true</returns>
        bool CheckUniqueLogin(string username);
        /// <summary>
        /// Регистрация пользователя в БД
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="userData"></param>
        /// <returns>Возвращает id зарегистрированного пользователя</returns>
        /// <exception cref="RegistrationError">Ошибка регистрации в одной из таблиц.</exception>
        int Registration(string username, string password, UserData userData);
        int Log_In(string username, string password);
        UserData GetUserData(int user_id);
        /// <summary>
        /// Отправка сообщения в чат с id = chat_id, 
        /// <br>если chat_id меньше или равен 0 то чат выделяется.</br>
        /// </summary>
        /// <param name="id_client">id отправителя</param>
        /// <param name="str_messageBox">Строка вида message::time</param>
        /// <returns>Возвращает id чата если все прошло успешно иначе -1</returns>
        int SendMessageTo(int chat_id, string str_messageBox, int id_client, string id_recipient);
    }
}

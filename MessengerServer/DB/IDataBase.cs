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
    }
}

﻿using Npgsql;
using MessengerServer.Exeptions;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace MessengerServer.DB
{
    public class pgDataBase : IDataBase
    {
        private string _connectionString = "Host=localhost;Port=5432;Database=Crassage;Username=group1;Password=12345";
        private NpgsqlConnection connection;
        protected string connectionStr { get => _connectionString; } 

        public pgDataBase(string ConnectionString)
        {
            _connectionString = ConnectionString;
            connection = new NpgsqlConnection(connectionStr);
            connection.Open();
        }



        public int Log_In(string username, string password)
        {
            try
            {
                using NpgsqlCommand command = new NpgsqlCommand 
                    (
                        $"SELECT  user_id, password_hash, salt FROM user_accounts WHERE username = '{username}' ;",
                    connection);


                using NpgsqlDataReader reader = command.ExecuteReader();
                if(reader.Read() != false ) 
                {
                    if (reader["password_hash"].ToString() == _hashPassword(password, reader["salt"].ToString()))
                    {
                        return int.Parse(reader["user_id"].ToString());
                    }
                    else
                        throw new LogInError("Неверный пароль");

                }
                else
                {
                    throw new LogInError("Ошибка с чтением");
                }

            }
            catch (Exception)
            {
                return -1;
            }

        }
        public UserData GetUserData(int user_id)
        {
            using NpgsqlCommand command = new NpgsqlCommand
                (
                    $"SELECT * FROM profiles WHERE id = {user_id} ;",
                connection);


            using NpgsqlDataReader reader = command.ExecuteReader();
            if (reader.Read() != false)
            {
                UserData user = new UserData(reader["first_name"].ToString(), reader["last_name"].ToString(), reader["date_of_birth"].ToString());
                return user;

            }

            return new UserData();
        }
        public bool CheckUniqueLogin(string username)
        {
            using NpgsqlCommand command = new NpgsqlCommand($"SELECT * FROM user_accounts WHERE username = '{username}';", connection);

            using NpgsqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows)
                return false;
            else
                return true;

        }        
        public int Registration(string username, string password, UserData userData)
        {
            string salt = _generateSalt();
            string hash_password = _hashPassword(password, salt);
            int user_id;

            
            NpgsqlCommand command = new NpgsqlCommand //Регистрация в таблицу аккаунтов
                (
                    $"INSERT INTO user_accounts (username, password_hash, salt) VALUES ('{username}', '{hash_password}', '{salt}') ;",
                connection);

            if (command.ExecuteNonQuery() == 0)
                throw new RegistrationError("Ошибка регистрации в таблице аккаунтов.");
            else
            {
                command = new NpgsqlCommand
                (
                    $"SELECT user_id FROM user_accounts WHERE username = '{username}' ;",
                connection);

                using NpgsqlDataReader reader = command.ExecuteReader();
                reader.Read();
                user_id = int.Parse(reader["user_id"].ToString());
            }

            command = new NpgsqlCommand //Регистрация в таблицу пользователей
                (
                    "INSERT INTO profiles (id, first_name, last_name, date_of_birth)" +
                    $" VALUES ({user_id}, '{userData.FirstName}', '{userData.LastName}', '{userData.DateOfBirth}') ;",
                connection);

            if (command.ExecuteNonQuery() == 0)
            {
                command = new NpgsqlCommand
                (
                    $"DELETE FROM user_accounts WHERE user_id = {user_id};"
                ,connection);

                command.ExecuteNonQuery();

                throw new RegistrationError("Ошибка регистрации в таблице профилей.");
            }

            return user_id;
        }


        public int SendMessageTo(int chat_id, string str_messageBox, int id_client, string id_recipient)
        {
            int NameTable = 1;
            while(chat_id <= 0)//Поиск свободной комнаты по таблицам
            {
                try
                {
                    using NpgsqlCommand _command = new NpgsqlCommand(
                        $"SELECT number FROM dialogs{1000 * NameTable - 999}_{1000 * NameTable - 1} WHERE participants is null ;",
                    connection);

                    using NpgsqlDataReader _reader = _command.ExecuteReader();

                    if(_reader.Read())
                    {
                        int.TryParse(_reader["number"].ToString(), out chat_id);
                    }

                }
                catch(NpgsqlException ex)
                {
                    if (ex.SqlState == "42P01") // Код состояния, который соответствует "таблица не найдена"
                    {
                        _createDialogTable($"{1000 * NameTable - 999}_{1000 * NameTable - 1}", $"1000 * NameTable - 1");
                        continue;
                    }
                }
                catch (Exception)
                {
                    return -1;
                }

                NameTable++;
            }

            
            while (1000 * NameTable - 1 < chat_id)//Определение имени таблицы
                NameTable++;

            //Проверка есть ли доступ к этой комнате у id_client
            NpgsqlCommand command = new NpgsqlCommand(
                        $"SELECT participants FROM dialogs{1000 * NameTable - 999}_{1000 * NameTable - 1} " +
                        $"WHERE number = {chat_id};",
                    connection);

            NpgsqlDataReader reader = command.ExecuteReader();

            bool flag = false;  //переменная разрешающая или запрещающая писать в данную комнату
            if (reader.Read())
            {
                string[] participants = reader["participants"].ToString().Split("::");

                if (participants[0] == "null")
                {
                    using NpgsqlCommand command1 = new NpgsqlCommand(
                        $"UPDATE dialogs{1000 * NameTable - 999}_{1000 * NameTable - 1} SET participants = '{id_client + "::" + id_recipient}'" +
                        $"WHERE number = {chat_id};"
                        , connection);
                }

                foreach (string p in participants)
                {
                    if (int.Parse(p) == id_client)
                    {
                        flag = true;
                        break;
                    }
                }
            }

            if (flag)//запись сообщения
            {
                command = new NpgsqlCommand(
                           $"UPDATE dialogs{1000 * NameTable - 999}_{1000 * NameTable - 1} " +
                           $"SET content = content || '{id_client + "::" + str_messageBox + "\n"}' " +
                           $"WHERE number = {chat_id};",
                       connection);
                command.ExecuteNonQuery();

                return chat_id;
            }
            else
                return -1;

        }

        private void _createDialogTable(string NameTable, string maxValue)
        {
            using NpgsqlCommand _command = new NpgsqlCommand(
                        $"SELECT create_dialogs_table('dialogs{NameTable}', {maxValue});",
                    connection);
            _command.ExecuteNonQuery();
        }
        private static string _generateSalt()
        {
            byte[] saltBytes = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }
        private static string _hashPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] saltBytes = Convert.FromBase64String(salt);
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] combinedBytes = new byte[saltBytes.Length + passwordBytes.Length];

                Buffer.BlockCopy(saltBytes, 0, combinedBytes, 0, saltBytes.Length);
                Buffer.BlockCopy(passwordBytes, 0, combinedBytes, saltBytes.Length, passwordBytes.Length);

                byte[] hashBytes = sha256.ComputeHash(combinedBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }

    }
}

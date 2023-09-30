using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerServer.Exeptions
{
    internal class RegistrationError: Exception
    {
        static string? message = "Ошибка регистрации";

        public RegistrationError(string message)
            : base(message) { }
        public RegistrationError() 
            : base(message) { }
    }
}

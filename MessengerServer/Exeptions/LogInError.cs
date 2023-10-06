using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerServer.Exeptions
{
    internal class LogInError : Exception
    {
        static string? message = "Ошибка входа";

        public LogInError(string message)
            : base(message) { }
        public LogInError()
            : base(message) { }
    }
}

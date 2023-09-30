using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerServer
{
    public struct UserData
    {

        private string _firstName;
        private string _lastName;
        public string DateOfBirth { get; set; }

        public UserData(string str)
        {
            string[] tmp = str.Split("::");
            _firstName = tmp[0];
            _lastName = tmp[1];
            DateOfBirth = tmp[2];
        }
        public UserData(string firstName, string lastName, string dateOfBirth)
        {
            _firstName = firstName;
            _lastName = lastName;
            DateOfBirth = dateOfBirth;
            
        }

        public string FirstName {  get => _firstName;  }
        public string LastName {  get => _lastName;  }
 

    }
}

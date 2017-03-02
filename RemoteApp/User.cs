using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteApp
{
    [Serializable]
    public class User
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Add { get; set; }
        public int Age { get; set; }
        public User()
        {
            this.ID = default(int);
            this.Name = default(string);
            this.Add = default(string);
            this.Age = default(int);
        }
    }  
}

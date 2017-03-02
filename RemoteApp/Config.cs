using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace RemoteApp
{
    [Serializable]
    public class Config
    {
        public string RemoteProgramePath { get; set; }

        public string Server { get; set; }

        public string Domain { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public bool AutoConnect { get; set; }

        public Config()
        {
            this.RemoteProgramePath = default(string);
            this.Server=default(string);
            this.Domain=default(string);
            this.UserName=default(string);
            this.Password=default(string);
            this.AutoConnect=default(bool);
        }
    }
}

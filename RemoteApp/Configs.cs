using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteApp
{
    [Serializable]
    public class Configs
    {
        public List<Config> Datas
        { get; set; }
        public Configs()
        {
            this.Datas = new List<Config>();
        }
    }  
}

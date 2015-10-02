using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IngresoRPC
{
    public class RPCFan
    {
        public string ID { get; set; }
        public string Nombre { get; set; }
        public string CardNum { get; set; }
        public string DNI { get; set; }
        public string Ingreso { get; set; }

        public bool YaEntro { get; set; }

        public string LastCell { get; set; }
    }
}

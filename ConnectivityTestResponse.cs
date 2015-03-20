using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Camurphy.InternetConnectionMonitor
{
    class ConnectivityTestResponse
    {
        public bool Online { get; set; }
        public string PublicIpAddress { get; set; }
        public WebException WebException { get; set; }
    }
}

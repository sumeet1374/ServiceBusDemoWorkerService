using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBusDemoWorkerService
{
    public class ServiceBusConfigOption
    {
        public string NameSpace { get; set; }
        public string SasKeyName { get; set; }

        public string SasKeyValue { get; set; }

        public string FirstQueue { get; set; }
        public string SecondQueue { get; set; }

        public bool UseManagedIdentity { get; set; }

        public string ManagedIdentity { get; set; }

        public static string SectionName = "ServiceBus";
    }
}

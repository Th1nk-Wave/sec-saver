using System;
using System.Net.Http;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Linq;
using sec_saver.dependancies;
using sec_saver;

namespace sec_saver
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            SecSaver main = new SecSaver();
            await main.Main(args);

        }
    }
}

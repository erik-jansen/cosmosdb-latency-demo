using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System.Configuration;

namespace GlobalDemo
{
    class Program
    {
        private static DocumentClient client;

        static void Main(string[] args)
        {
            string endpointUri = string.Format("https://{0}.documents.azure.com:443/", ConfigurationManager.AppSettings["CosmosDbAccountName"]);
            string primaryKey = ConfigurationManager.AppSettings["CosmosDbAuthorizationKey"];

            Console.WriteLine("Starting...");

            var connectionPolicy = new ConnectionPolicy()
            {
                ConnectionMode = ConnectionMode.Direct,
                ConnectionProtocol = Protocol.Tcp
            };
            //Setting read region selection preference
            connectionPolicy.PreferredLocations.Add(LocationNames.AustraliaSoutheast); // 1st preference
            connectionPolicy.PreferredLocations.Add(LocationNames.WestEurope); // 2nd preference

            client = new DocumentClient(new Uri(endpointUri), primaryKey, connectionPolicy: connectionPolicy);

            client.OpenAsync().ConfigureAwait(false);

            while (true)
            {
                var sw = new Stopwatch();
                sw.Start();

                Car car = client.CreateDocumentQuery<Car>(UriFactory.CreateDocumentCollectionUri("inventory", "cars"))
                                                            .Where(c => c.Model == "Passat")
                                                            .AsEnumerable()
                                                            .FirstOrDefault();

                sw.Stop();

                var readtime = sw.ElapsedMilliseconds;

                car.Updated = DateTime.Now;
                sw.Restart();

                var bla = UriFactory.CreateDocumentUri("inventory", "cars", "1");
                var response = client.ReplaceDocumentAsync(bla, car).Result;

                var upserted = response.Resource;
                sw.Stop();

                Console.WriteLine($"Read time: {readtime} ms\t\t Write time: {sw.ElapsedMilliseconds} ms");

                Thread.Sleep(200);
            }
        }
    }
}

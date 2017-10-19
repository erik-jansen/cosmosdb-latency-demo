using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using web.Models;

namespace web.Controllers
{
    public static class GlobalVariables
    {
        private static readonly object _syncRoot = new object();
        private static DocumentClient _client;

        public static DocumentClient GetClient()
        {
            string endpointUri = string.Format("https://{0}.documents.azure.com:443/", ConfigurationManager.AppSettings["CosmosDbAccountName"]);
            string primaryKey = ConfigurationManager.AppSettings["CosmosDbAuthorizationKey"];

            lock (_syncRoot)
            {
                if (_client == null)
                {
                    var preferredDCs = ConfigurationManager.AppSettings["PreferredDCs"].Split(',').ToList<string>();
                    ConnectionPolicy connectionPolicy = new ConnectionPolicy
                    {
                        ConnectionMode = ConnectionMode.Direct,
                        ConnectionProtocol = Protocol.Tcp
                    };

                    foreach (var dc in preferredDCs)
                    {
                        connectionPolicy.PreferredLocations.Add(dc);
                    }

                    _client = new DocumentClient(new Uri(endpointUri), primaryKey, connectionPolicy);
                }

                return _client;
            }
        }

    }
    public class HomeController : Controller
    {
        // ADD THIS PART TO YOUR CODE

        private const string Database = "inventory";
        private const string Collection = "cars";
        private DocumentClient client;
        public HomeController()
        {
            client = GlobalVariables.GetClient();
        }

        public ActionResult Index()
        {
            client.OpenAsync().ConfigureAwait(false);

            var sw = new Stopwatch();
            sw.Start();

            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1, PartitionKey = new PartitionKey("1") };

            var cars = client.CreateDocumentQuery<Car>(
                UriFactory.CreateDocumentCollectionUri("inventory", "cars"),
                "SELECT TOP 10 * FROM c",
                queryOptions).ToList();

            sw.Stop();

            ViewBag.TheMainDC = client.ConnectionPolicy.PreferredLocations[0].ToString();
            ViewBag.ElapsedTime = sw.ElapsedMilliseconds.ToString();

            return View(cars);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
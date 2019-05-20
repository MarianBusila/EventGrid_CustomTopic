using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.EventGrid;
using System.Collections.Generic;

namespace EventGridPublisher
{
    class ItemReceivedEventData
    {
        [JsonProperty(PropertyName = "itemSku")]
        public string ItemSku { get; set; }

    }

    public static class EventGridPublisherFunction
    {
        [FunctionName("EventGridPublisherFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var topicEndpoint = config["TopicEndpoint"];
            var topicKey = config["TopicKey"];

            string item = req.Query["item"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            item = item ?? data?.name;

            if (item != null)
            {
                string topicHostname = new Uri(topicEndpoint).Host;
                TopicCredentials topicCredentials = new TopicCredentials(topicKey);
                EventGridClient client = new EventGridClient(topicCredentials);                
                await client.PublishEventsAsync(topicHostname, GetEventsList(item));
                log.LogInformation("Published event to EventGrid topic");

                return (ActionResult) new OkObjectResult($"ItemReceived event generated for {item}");
            }
            else
            {
                return new BadRequestObjectResult("Please pass an item on the query string or in the request body");
            }
        }

        static IList<EventGridEvent> GetEventsList(string item)
        {
            List<EventGridEvent> eventsList = new List<EventGridEvent>();

            eventsList.Add(new EventGridEvent()
            {
                Id = Guid.NewGuid().ToString(),
                EventType = "Items.ItemReceived",
                Data = new ItemReceivedEventData()
                {
                    ItemSku = "Item SKU " + item
                },
                EventTime = DateTime.Now,
                Subject = "Door",
                DataVersion = "2.0"
            });

            return eventsList;
        }

    }
}

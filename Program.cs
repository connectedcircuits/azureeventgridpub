using System;
using System.Collections.Generic;
using System.Configuration;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;

namespace nz.co.connectedcircuits.eventpublisher
{
    class Program
    {
        
        static void Main(string[] args)
        {
            const string LIT_KEYVAULT_SECRET_NAME = "lob-event-grid";


            string keyVaultUrl = ConfigurationManager.AppSettings["KEY_VAULT_URL"]; //Azure keyvault URL
            string clientId = ConfigurationManager.AppSettings["CLIENT_ID"]; //AAD App registration Client Id
            string clientSecret = ConfigurationManager.AppSettings["CLIENT_SECRET"]; //AAD App registration secret
            string tentantId = ConfigurationManager.AppSettings["TENTANT_ID"]; //AAD Tentant Id
            string eventgridTopicEndpoint = ConfigurationManager.AppSettings["EVENT_GRID_TOPIC_ENDPOINT"]; //Azure event grid topic endpoint


            //Get the event grid access key secret from key vault
            var client = new SecretClient(new Uri(keyVaultUrl),
                new ClientSecretCredential(tentantId, clientId, clientSecret));
            var topicKey = client.GetSecret(LIT_KEYVAULT_SECRET_NAME).Value.Value;

            

            TopicCredentials topicCredentials = new TopicCredentials(topicKey);
            EventGridClient eventclient = new EventGridClient(topicCredentials);

            for(int eventIndex=0; eventIndex < 3; eventIndex++)
            {
                //send an event to simulate address details have been updated
                var entityId = Guid.NewGuid().ToString("D");

                // Add EventGridEvents to a list to publish to the topic
                List<EventGridEvent> eventsList = new List<EventGridEvent>
                {
                    new EventGridEvent
                    {
                        DataVersion = "1.0",
                        EventTime = DateTime.UtcNow,
                        EventType = "nz.co.connectedcircuits.crm",
                        Id = Guid.NewGuid().ToString("D"),
                        Subject = "entity/address/update",
                        Data = new EventData
                        {
                            CallbackUrl = new UriBuilder("https", "en431qbm266j.x.pipedream.net/" + entityId + "/address").ToString(),
                            EntityId = entityId,
                            Source = "crm"
                        }
                    }
                };
                eventclient.PublishEventsAsync(new Uri(eventgridTopicEndpoint).Host, eventsList).GetAwaiter().GetResult();
            }                     
        }
    }

    class EventData
    {
        public string EntityId { get; set; }
        public string Source { get; set; }
        public string CallbackUrl { get; set; }
    }
}

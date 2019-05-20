# Publisher / Subscriber using Azure EventGrid

## Overview
Publish and subscribe to topic events using EventGrid based on the following [repository](https://github.com/Azure-Samples/event-grid-dotnet-publish-consume-events).

* Create resource group 
```bash
az group create --name EventGrid_CustomTopic --location eastus
```

* Create topic: 
```bash
az eventgrid topic create --resource-group EventGrid_CustomTopic --name ItemReceived --location eastus
```

* To run locally the EventGridConsumer function, use [ngrok](https://ngrok.com/download). Create subscription which has as endpoint the ngrok url

```bash
ngrok http -host-header=localhost 7071
az eventgrid event-subscription create --resource-group EventGrid_CustomTopic --topic-name ItemReceived --name NGrokAzureFunctionItemReceivedSubscription --endpoint https://c3626893.ngrok.io/api/EventGridConsumerFunction
```

* (Alternative) Publish Azure function EventGridConsumer and get the function URL. Create an event subscription to the topic and provide the function url as the endpoint
```bash
az eventgrid event-subscription create --resource-group EventGrid_CustomTopic --topic-name ItemReceived --name AzureFunctionItemReceivedSubscription --endpoint https://eventgridconsumerappservice.azurewebsites.net/api/EventGridConsumerFunction
```

* Run locally or deploy in Azure the EventGridPublisher Azure function and call its endpoint which will publish an event

* Check in the log of the EventGridConsumer function that the event was received
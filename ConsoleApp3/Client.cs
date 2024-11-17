using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Samples.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConsoleApp3
{
    public class Client
    {
        IMqttClient mqttClient;
        MqttFactory clientFactory;

        public Client()
        {
            this.clientFactory = new MqttFactory();
            this.mqttClient = this.clientFactory.CreateMqttClient();
        }

        public async Task ConnectToServer(string host = "localhost", int port = 1883)
        {
            this.mqttClient.ApplicationMessageReceivedAsync += async e => await this.getMessage(e);

            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(host, port)
                .Build();

            var response = await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

            Console.WriteLine("The MQTT client is connected.");

            await this.SubscribeToTopic();
        }

        public async Task SubscribeToTopic(string topic = "Test")
        {
            MqttTopicFilterBuilder topicFilter = new MqttTopicFilterBuilder().WithTopic(topic);

            var mqttSubscribeOptions = this.clientFactory
                .CreateSubscribeOptionsBuilder()
                .WithTopicFilter(topicFilter)
                .Build();

            await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

            Console.WriteLine("MQTT client subscribed to topic.");
        }

        public async Task PublishMessage(string topic = "Test", string payload = "Message published from a client!")
        {
            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .Build();

            await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
        }

        public async Task DisconnectFromServer()
        {
            var mqttClientDisconnectOptions = this.clientFactory.CreateClientDisconnectOptionsBuilder().Build();

            await mqttClient.DisconnectAsync(mqttClientDisconnectOptions, CancellationToken.None);
            mqttClient.Dispose();
            Console.WriteLine("Client disconnected from server!");
        }

        private async Task<Task> getMessage(MqttApplicationMessageReceivedEventArgs e)
        {
            Console.WriteLine("Received application message.");
            // Serialize asynchronously
            using var serializeStream = new MemoryStream();
            await JsonSerializer.SerializeAsync(serializeStream, e.ApplicationMessage.PayloadSegment, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            // Reset the stream position for reading
            serializeStream.Position = 0;

            // Deserialize asynchronously
            var deserializedArray = await JsonSerializer.DeserializeAsync<int[]>(serializeStream) ?? new int[1];

            // Transform the deserialized data into a string
            var deserializedMessage = string.Join("", deserializedArray
                .ToList()
                .Select(n => (char)n)
                .ToList());

            Console.WriteLine($"Deserialized Message: {deserializedMessage}");

            return Task.CompletedTask;
        }
    }
}

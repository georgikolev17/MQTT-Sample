using MQTTnet;
using MQTTnet.Client;
using System.Text;

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
        }

        public async Task SubscribeToTopic(string topic = "Test")
        {
            MqttTopicFilterBuilder topicFilter = new MqttTopicFilterBuilder().WithTopic(topic);

            var mqttSubscribeOptions = this.clientFactory
                .CreateSubscribeOptionsBuilder()
                .WithTopicFilter(topicFilter)
                .Build();

            await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

            Console.WriteLine($"MQTT client subscribed to topic: {topic}");
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

        public virtual async Task<Task> getMessage(MqttApplicationMessageReceivedEventArgs e)
        {
            var message = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
            var topic = e.ApplicationMessage.Topic;

            Console.WriteLine($"{topic}: {message}");

            return Task.CompletedTask;
        }
    }
}

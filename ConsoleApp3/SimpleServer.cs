using MQTTnet;
using MQTTnet.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp3
{
    public class SimpleServer
    {
        private MqttServerOptions mqttServerOptions;
        private MqttFactory mqttServerFactory;
        private MqttServer mqttServer;

        public SimpleServer(MqttServerOptions? options = null)
        {
            this.mqttServerOptions = options 
                ?? new MqttServerOptionsBuilder()
                .WithDefaultEndpoint()
                .Build();
            this.mqttServerFactory = new MqttFactory();
            this.mqttServer = this.mqttServerFactory.CreateMqttServer(this.mqttServerOptions);
        }

        public async Task StartServer()
        {
            await this.mqttServer.StartAsync();
            Console.WriteLine("MQTT Server successfully started!");

            var client = new Client();
            await client.ConnectToServer();

            await client.PublishMessage();
            await this.InjectMessage();

            // Gives time to get all the messages
            await Task.Delay(50);
            await client.DisconnectFromServer();

            Console.WriteLine("\nPress enter to stop the server...");
            Console.ReadLine();

            await this.StopServer();
        }

        public async Task InjectMessage(string topic = "Test", string payload = "Hello from test topic!", string senderId = "TestId")
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .Build();

            await this.mqttServer.InjectApplicationMessage(
                    new InjectedMqttApplicationMessage(message)
                    {
                        SenderClientId = senderId
                    }
                );
            Console.WriteLine("Message successfully injected!");
        }

        public async Task StopServer()
        {
            await mqttServer.StopAsync();
            this.mqttServer.Dispose();
            Console.WriteLine("MQTT Server successfully stopped!");
        }
    }
}

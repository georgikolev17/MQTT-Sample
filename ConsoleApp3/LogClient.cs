using ConsoleApp3;
using MQTTnet.Client;
using ServerClientTimeScaleLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTT_Sample
{
    public class LogClient : Client
    {
        private readonly PostgreSqlClient sqlClient;

        public LogClient()
        {
            this.sqlClient=new PostgreSqlClient();
        }

        public async Task configureConnection()
        {
            await this.sqlClient.OpenConnection();
            await this.sqlClient.CreateMessageLogTable();
        }


        public override async Task<Task> getMessage(MqttApplicationMessageReceivedEventArgs e)
        {
            var message = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
            var topic = e.ApplicationMessage.Topic;

            // Console.WriteLine($"{topic}: {message}");

            await this.sqlClient.InsertData(e.ApplicationMessage.Topic, message);
            return Task.CompletedTask;
        }
    }
}

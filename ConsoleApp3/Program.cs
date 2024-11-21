using ConsoleApp3;
using Microsoft.Extensions.Logging;
using MQTT_Sample;

var server = new SimpleServer();
await server.StartServer();

var client1 = new Client();
await client1.ConnectToServer();

var logClient = new LogClient();
await logClient.configureConnection();
await logClient.ConnectToServer();
await logClient.SubscribeToTopic("#");


while (true)
{
    await Task.Delay(100);
    await client1.PublishMessage();
}
await Task.Delay(1000);
await client1.DisconnectFromServer();
await logClient.DisconnectFromServer();
await server.StopServer();

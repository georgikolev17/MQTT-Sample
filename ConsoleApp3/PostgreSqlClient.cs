namespace ServerClientTimeScaleLog;

using Microsoft.Extensions.Logging;
using Npgsql;

public class PostgreSqlClient
{
    string connectionString;
    private NpgsqlConnection connection;

    public PostgreSqlClient()
    {
        using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());
        NpgsqlLoggingConfiguration.InitializeLogging(factory, parameterLoggingEnabled: true);
        connectionString = "Host=localhost;Port=5432;Database=postgres;User Id=postgres;Password=admin;";
        connection = new NpgsqlConnection(connectionString);
    }

    public async Task OpenConnection()
    {
        await connection.OpenAsync();
    }

    public async Task CloseConnection()
    {
        await connection.CloseAsync();
    }

    public async Task CreateMessageLogTable()
    {
        string ensureExtensionCmd = await File.ReadAllTextAsync("./SqlQueries/EnsureTimeScaleExtension.sql");
        using NpgsqlCommand cmdEnsureExtension = new NpgsqlCommand(ensureExtensionCmd, connection);
        cmdEnsureExtension.ExecuteNonQuery();

        string createMessageLogTableCmd = await File.ReadAllTextAsync("./SqlQueries/CreateLogTable.sql");
        using NpgsqlCommand cmdCreateMessageLogTable = new NpgsqlCommand(createMessageLogTableCmd, connection);
        await cmdCreateMessageLogTable.ExecuteNonQueryAsync();
    }

    public async Task InsertData(string topic, string message)
    {
        string updateTableCmd = await File.ReadAllTextAsync("./SqlQueries/UpdateTable.sql");
        using NpgsqlCommand cmdUpdateTable = new NpgsqlCommand(updateTableCmd, connection);

        cmdUpdateTable.Parameters.AddWithValue("topic", topic);
        cmdUpdateTable.Parameters.AddWithValue("message", message);


        // Console.WriteLine(cmdUpdateTable.CommandText + " --- " + topic + " -- " + message);
        foreach (NpgsqlParameter parameter in cmdUpdateTable.Parameters)
        {
            Console.WriteLine($"Parameter: {parameter.ParameterName}, Value: {parameter.Value}");
        }

        await cmdUpdateTable.ExecuteNonQueryAsync();
    }
}
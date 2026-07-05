using System.Text.Json;
using System.Threading.Channels;
using DutchMetar.Core.Features.DataWarehouse.Features.Notification.Contracts;
using DutchMetar.Core.Features.DataWarehouse.Features.Notification.EventArgs;
using DutchMetar.Core.Features.DataWarehouse.Features.Notification.Interfaces;
using DutchMetar.Core.Features.DataWarehouse.Features.Notification.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Exceptions;
using MQTTnet.Formatter;
using MQTTnet.Protocol;

namespace DutchMetar.Core.Features.DataWarehouse.Features.Notification;

public class KnmiNotificationClient : IKnmiNotificationClient
{
    private readonly IOptions<KnmiMqttOptions> _options;
    private readonly IMqttClient _mqttClient;
    private readonly ILogger<KnmiNotificationClient> _logger;
    private readonly Channel<FileEvent> _channel = Channel.CreateUnbounded<FileEvent>();
    
    private MqttClientOptions MqttClientOptions => new MqttClientOptionsBuilder()
        .WithWebSocketServer(builder => builder.WithUri(new Uri("wss://" + _options.Value.Host).AbsoluteUri))
        .WithProtocolVersion(MqttProtocolVersion.V500)
        .WithClientId(_options.Value.ClientId)
        .WithWillQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
        .WithCleanStart(false)
        .WithCredentials("token", _options.Value.Token)
        .WithSessionExpiryInterval((uint)TimeSpan.FromHours(24).TotalSeconds)
        .Build();
    
    public ChannelReader<FileEvent> ChannelReader => _channel.Reader;

    public KnmiNotificationClient(IOptions<KnmiMqttOptions> options, ILogger<KnmiNotificationClient> logger)
    {
        _options = options;
        _logger = logger;
        _mqttClient = new MqttClientFactory().CreateMqttClient();
        _mqttClient.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;
    }

    public void Dispose()
    {
        _mqttClient.Dispose();
    }

    public Task ConnectAndReceiveAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Connecting to KNMI MQTT host: {_options.Value.Host}");
        
        // This segment of the code was lifted from the sample docs: https://github.com/dotnet/MQTTnet/blob/master/Samples/Client/Client_Connection_Samples.cs
        // Author recommends this approach instead of using the clients Disconnected event due to high risk of deadlocks.
        _ = Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // This code will also do the very first connect! So no call to _ConnectAsync_ is required in the first place.
                    if (!await _mqttClient.TryPingAsync(cancellationToken))
                    {
                        var result = await _mqttClient.ConnectAsync(MqttClientOptions, cancellationToken);
                        
                        _logger.LogInformation($"Connection to MQTT successfully established.");
                        if (result.IsSessionPresent)
                        {
                            _logger.LogInformation($"Previous MQTT session has been re-used.");
                        }
                    }
                }
                catch (MqttCommunicationException ex)
                {
                    _logger.LogError(ex, "Failed to establish connection to KNMI Notification service.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected exception occured while trying to connect to KNMI Notification service.");
                    throw;
                }
                finally
                {
                    // Check the connection state every 5 seconds and perform a reconnect if required.
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                }
            }
        }, cancellationToken);
        
        return Task.CompletedTask;
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Disconnecting from MQTT host: {_options.Value.Host}");

        var disconnectOptions = new MqttClientDisconnectOptionsBuilder()
            .WithReason(MqttClientDisconnectOptionsReason.NormalDisconnection)
            .Build();
        
        await _mqttClient.DisconnectAsync(disconnectOptions, cancellationToken);
    }

    private async Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
    {
        var rawPayload = e.ApplicationMessage?.ConvertPayloadToString() ?? string.Empty;

        if (string.IsNullOrEmpty(rawPayload))
        {
            _logger.LogWarning("Received null or empty payload from MQTT notification.");
            return;
        }

        try
        {
            var fileEvent = JsonSerializer.Deserialize<FileEvent>(rawPayload);
            if (fileEvent != null)
            {
                await _channel.Writer.WriteAsync(fileEvent);
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error while converting payload to file event.");
        }
    }
}
using GtsTest.Core;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using System;
using System.Threading.Tasks;

namespace GtsTest.Services
{
    public class MqttService : IMqttService
    {
        private IMqttClient _client;
        private bool _disposed;

        public event EventHandler<bool> ConnectionStateChanged;
        public event EventHandler<string> MessageReceived;  // "topic|payload"

        public bool IsConnected => _client != null && _client.IsConnected;
        public string BrokerAddress { get; private set; } = "";

        public async Task<bool> ConnectAsync(string brokerAddress, int port, string username = null, string password = null)
        {
            if (string.IsNullOrEmpty(brokerAddress))
                throw new ArgumentException("Broker 地址不能为空");

            try
            {
                var factory = new MqttFactory();
                _client = factory.CreateMqttClient();

                var options = new MqttClientOptionsBuilder()
                    .WithTcpServer(brokerAddress, port)
                    .WithCleanSession()
                    .WithKeepAlivePeriod(TimeSpan.FromSeconds(60));

                if (!string.IsNullOrEmpty(username))
                    options.WithCredentials(username, password);

                await _client.ConnectAsync(options.Build());

                BrokerAddress = brokerAddress;
                ConnectionStateChanged?.Invoke(this, true);
                AppLogger.Info($"MQTT 已连接到 {brokerAddress}:{port}", "MQTT");

                // ✅ 修正：使用异步事件
                _client.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;

                return true;
            }
            catch (Exception ex)
            {
                AppLogger.Error($"MQTT 连接失败: {ex.Message}", "MQTT");
                return false;
            }
        }

        // ✅ 修正：异步事件处理方法
        private Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            try
            {
                var topic = e.ApplicationMessage.Topic;
                var payload = System.Text.Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
                MessageReceived?.Invoke(this, $"{topic}|{payload}");
            }
            catch (Exception ex)
            {
                AppLogger.Error($"MQTT 消息处理异常: {ex.Message}", "MQTT");
            }
            return Task.CompletedTask;
        }

        public void Disconnect()
        {
            if (_client != null && _client.IsConnected)
            {
                _client.DisconnectAsync().Wait();
                // ✅ 修正：取消订阅异步事件
                _client.ApplicationMessageReceivedAsync -= OnMessageReceivedAsync;
                ConnectionStateChanged?.Invoke(this, false);
                AppLogger.Info("MQTT 已断开", "MQTT");
            }
            _client?.Dispose();
            _client = null;
            BrokerAddress = "";
        }

        public async Task PublishAsync(string topic, string payload, bool retain = false)
        {
            if (!IsConnected) throw new InvalidOperationException("MQTT 未连接");

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag(retain)
                .Build();

            await _client.PublishAsync(message);
            AppLogger.Info($"MQTT 发布: {topic} = {payload}", "MQTT");
        }

        public async Task SubscribeAsync(string topic, int qos = 1)
        {
            if (!IsConnected) throw new InvalidOperationException("MQTT 未连接");

            var options = new MqttClientSubscribeOptionsBuilder()
                .WithTopicFilter(topic, (MqttQualityOfServiceLevel)qos)
                .Build();

            await _client.SubscribeAsync(options);
            AppLogger.Info($"MQTT 订阅主题: {topic}", "MQTT");
        }

        public async Task UnsubscribeAsync(string topic)
        {
            if (!IsConnected) return;
            var options = new MqttClientUnsubscribeOptionsBuilder()
                .WithTopicFilter(topic)
                .Build();

            await _client.UnsubscribeAsync(options);
            AppLogger.Info($"MQTT 取消订阅主题: {topic}", "MQTT");
        }

        public void UnsubscribeAll()
        {
            AppLogger.Warn("UnsubscribeAll 未实现详细取消，建议断开重连", "MQTT");
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Disconnect();
        }
    }
}
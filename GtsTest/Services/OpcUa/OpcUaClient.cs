using GtsTest.Core;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClientSession = Opc.Ua.Client.Session;
using ClientSubscription = Opc.Ua.Client.Subscription;
using ClientMonitoredItem = Opc.Ua.Client.MonitoredItem;

namespace GtsTest.Services.OpcUa
{
    public class OpcUaClient : IOpcUaClient
    {
        private Opc.Ua.Client.Subscription _subscription;
        private Dictionary<string, Opc.Ua.Client.MonitoredItem> _items = new();
        private ClientSession _session;
        private readonly object _lock = new object();
        private bool _disposed;

        public event EventHandler<bool> ConnectionStateChanged;
        public event EventHandler<string> DataValueChanged;
        public event EventHandler<string> ErrorOccurred;

        public bool IsConnected => _session != null && _session.Connected;
        public string ServerUrl { get; private set; } = "";

        // ---------- 连接管理 ----------
        public async Task<bool> ConnectAsync(string serverUrl)
        {
            if (string.IsNullOrEmpty(serverUrl))
            {
                ErrorOccurred?.Invoke(this, "服务器 URL 不能为空");
                return false;
            }

            try
            {
                var config = new ApplicationConfiguration
                {
                    ApplicationName = "GtsTest OPC UA Client",
                    ApplicationType = ApplicationType.Client,
                    ClientConfiguration = new ClientConfiguration(),
                    SecurityConfiguration = new SecurityConfiguration
                    {
                        ApplicationCertificate = new CertificateIdentifier(),
                        TrustedPeerCertificates = new CertificateTrustList(),
                        TrustedIssuerCertificates = new CertificateTrustList(),
                        RejectedCertificateStore = new CertificateStoreIdentifier(),
                        AutoAcceptUntrustedCertificates = true
                    },
                    TransportQuotas = new TransportQuotas
                    {
                        OperationTimeout = 15000,
                        MaxStringLength = 1048576,
                        MaxByteStringLength = 1048576,
                        MaxArrayLength = 65535,
                        MaxMessageSize = 4194304,
                        MaxBufferSize = 65535,
                        ChannelLifetime = 300000,
                        SecurityTokenLifetime = 3600000
                    }
                };

                // 使用 CoreClientUtils 自动选择无安全端点（1.4.x 支持）
                var endpointDesc = CoreClientUtils.SelectEndpoint(serverUrl, false);

                // 创建端点配置
                var endpointConfiguration = EndpointConfiguration.Create(config);
                var endpoint = new ConfiguredEndpoint(null, endpointDesc, endpointConfiguration);

                // 匿名身份
                var userIdentity = new UserIdentity();

                var session = await ClientSession.Create(
                    config,
                    endpoint,
                    false,          // updateBeforeConnect
                    false,          // checkDomain
                    "GtsTest Client",
                    60000,
                    userIdentity,
                    null
                );

                lock (_lock)
                {
                    _session?.Close();
                    _session = session;
                }

                ServerUrl = serverUrl;
                ConnectionStateChanged?.Invoke(this, true);
                AppLogger.Info($"OPC UA 已连接到 {serverUrl}", "OPC UA");

                CreateSubscription();
                return true;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"连接失败: {ex.Message}");
                AppLogger.Error($"OPC UA 连接失败: {ex.Message}", "OPC UA");
                return false;
            }
        }

        private void CreateSubscription()
        {
            if (_subscription != null)
            {
                AppLogger.Warn("订阅已存在，跳过创建", "OPC UA");
                return;
            }

            if (_session == null || !_session.Connected)
            {
                AppLogger.Error("会话未连接，无法创建订阅", "OPC UA");
                return;
            }

            try
            {
                var subscription = new Opc.Ua.Client.Subscription
                {
                    // 注意：不设置 Session，由 AddSubscription 自动赋值
                    PublishingInterval = 1000,
                    KeepAliveCount = 10,
                    LifetimeCount = 20,
                    MaxNotificationsPerPublish = 1000,
                    Priority = 0,
                    PublishingEnabled = true,
                    TimestampsToReturn = TimestampsToReturn.Both
                };

                _session.AddSubscription(subscription);
                subscription.Create();

                if (subscription.Id == 0)
                {
                    AppLogger.Error("❌ 订阅创建失败：服务器返回 ID=0", "OPC UA");
                    return;
                }

                AppLogger.Info($"✅ OPC UA 订阅已创建 (ID={subscription.Id})", "OPC UA");

                // 直接存储底层 Subscription，不包装
                _subscription = subscription;
            }
            catch (Exception ex)
            {
                AppLogger.Error($"❌ 订阅创建异常: {ex.Message}", "OPC UA");
                _subscription = null;
                ErrorOccurred?.Invoke(this, $"订阅创建失败: {ex.Message}");
            }
        }

        public void Disconnect()
        {
            lock (_lock)
            {
                UnsubscribeAll();
                _subscription?.Delete(true);   // 删除订阅
                _subscription = null;
                _session?.Close();
                _session = null;
                ServerUrl = "";
                ConnectionStateChanged?.Invoke(this, false);
                AppLogger.Info("OPC UA 已断开", "OPC UA");
            }
        }

        // ---------- 数据操作 ----------
        public async Task<T> ReadNodeValueAsync<T>(string nodeId)
        {
            if (!IsConnected)
                throw new InvalidOperationException("未连接到 OPC UA 服务器");

            try
            {
                var node = new NodeId(nodeId);
                var result = await _session.ReadValueAsync(node);
                if (result.StatusCode == StatusCodes.Good)
                    return (T)result.Value;
                throw new Exception($"读取失败: {result.StatusCode}");
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"读取节点 {nodeId} 失败: {ex.Message}");
                throw;
            }
        }

        public async Task WriteNodeValueAsync<T>(string nodeId, T value)
        {
            if (!IsConnected)
                throw new InvalidOperationException("未连接到 OPC UA 服务器");

            try
            {
                var node = new NodeId(nodeId);
                var dataValue = new DataValue(new Variant(value));
                var writeValue = new WriteValue
                {
                    NodeId = node,
                    AttributeId = Attributes.Value,
                    Value = dataValue
                };
                var collection = new WriteValueCollection { writeValue };
                var response = await _session.WriteAsync(null, collection, CancellationToken.None);
                var result = response.Results?[0] ?? StatusCodes.BadUnexpectedError;
                if (result != StatusCodes.Good)
                    throw new Exception($"写入失败: {result}");
                AppLogger.Info($"OPC UA 写入节点 {nodeId} = {value}", "OPC UA");
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"写入节点 {nodeId} 失败: {ex.Message}");
                throw;
            }
        }

        public async Task<List<string>> BrowseNodesAsync(string nodeId = null)
        {
            if (!IsConnected)
                throw new InvalidOperationException("未连接到 OPC UA 服务器");

            var results = new List<string>();
            try
            {
                var browseId = string.IsNullOrEmpty(nodeId) ? ObjectIds.RootFolder : new NodeId(nodeId);
                var description = new BrowseDescription
                {
                    NodeId = browseId,
                    BrowseDirection = BrowseDirection.Forward,
                    IncludeSubtypes = true,
                    NodeClassMask = (uint)(NodeClass.Object | NodeClass.Variable | NodeClass.Method),
                    ResultMask = (uint)BrowseResultMask.All
                };
                var collection = new BrowseDescriptionCollection { description };
                var response = await _session.BrowseAsync(null, null, 0, collection, CancellationToken.None);
                var references = response.Results?[0]?.References;
                if (references != null)
                {
                    foreach (var refs in references)
                        results.Add($"{refs.DisplayName.Text} ({refs.NodeId})");
                }
                return results;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"浏览节点失败: {ex.Message}");
                throw;
            }
        }

        // ---------- 订阅 ----------
        public void Subscribe(string nodeId, int samplingInterval = 1000)
        {
            if (!IsConnected || _subscription == null) return;

            try
            {
                if (_items.ContainsKey(nodeId))
                    Unsubscribe(nodeId);

                var node = ParseNodeId(nodeId);
                var item = new Opc.Ua.Client.MonitoredItem
                {
                    StartNodeId = node,
                    AttributeId = Attributes.Value,
                    SamplingInterval = samplingInterval,
                    QueueSize = 10,
                    DiscardOldest = true,
                    MonitoringMode = MonitoringMode.Reporting  // 确保为 Reporting
                };

                // 绑定事件（事件参数类型不同，但签名兼容）
                item.Notification += OnMonitoredItemNotification;

                // 添加到订阅（本地）
                _subscription.AddItem(item);
                _items[nodeId] = item;

                // ✅ 关键：提交创建请求到服务器
                _subscription.CreateItems();   // 或 ApplyChanges()

                // 检查创建结果
                if (item.Status == null || item.Status.Id == 0)
                {
                    AppLogger.Error($"❌ 监控项创建失败：服务器未分配有效 ID (Status.Id={item.Status?.Id})", "OPC UA");
                    _items.Remove(nodeId);
                    return;
                }

                AppLogger.Info($"✅ 监控项创建成功, ID={item.Status.Id}, SamplingInterval={item.SamplingInterval}", "OPC UA");
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"订阅节点 {nodeId} 失败: {ex.Message}");
                AppLogger.Error($"❌ 订阅节点 {nodeId} 失败: {ex.Message}", "OPC UA");
            }
        }

        private NodeId ParseNodeId(string nodeIdString)
        {
            if (string.IsNullOrEmpty(nodeIdString))
                throw new ArgumentException("节点 ID 不能为空");

            // 尝试直接构造（如果是纯数字，假定为 Numeric 类型，命名空间为 0）
            if (int.TryParse(nodeIdString, out int numericId))
                return new NodeId((uint)numericId); // 转换为 uint

            // 尝试解析 "ns=3;i=1004" 或 "ns=3;s=SomeString" 格式
            var parts = nodeIdString.Split(';');
            int ns = 0;
            string identifier = "";
            foreach (var part in parts)
            {
                if (part.StartsWith("ns="))
                    ns = int.Parse(part.Substring(3));
                else if (part.StartsWith("i="))
                    identifier = part.Substring(2);
                else if (part.StartsWith("s="))
                    identifier = part.Substring(2);
                else
                    identifier = part; // 如果直接是标识符
            }

            // 尝试将标识符解析为数字（Numeric 类型）
            if (int.TryParse(identifier, out int id))
                return new NodeId((uint)id, (ushort)ns);

            // 否则作为 String 类型
            return new NodeId(identifier, (ushort)ns);
        }

        //回调
        private void OnMonitoredItemNotification(Opc.Ua.Client.MonitoredItem item, MonitoredItemNotificationEventArgs e)
        {
            AppLogger.Info($"📥 收到数据变化通知, NodeId: {item.StartNodeId}", "OPC UA");

            try
            {
                // ✅ 正确获取通知值
                var notification = e.NotificationValue as MonitoredItemNotification;
                if (notification != null)
                {
                    var value = notification.Value;  // DataValue 类型
                    if (value != null && value.Value != null)
                    {
                        var nodeId = item.StartNodeId.ToString();
                        DataValueChanged?.Invoke(this, $"{nodeId}|{value.Value}");
                        AppLogger.Info($"📤 触发 DataValueChanged 事件, 值: {value.Value}", "OPC UA");
                    }
                    else
                    {
                        AppLogger.Warn($"⚠️ 通知值为空或无效", "OPC UA");
                    }
                }
                else
                {
                    AppLogger.Warn($"⚠️ 无法将 e.NotificationValue 转换为 MonitoredItemNotification", "OPC UA");
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error($"❌ 数据回调异常: {ex.Message}", "OPC UA");
            }
        }

        public void Unsubscribe(string nodeId)
        {
            if (_items.TryGetValue(nodeId, out var item))
            {
                item.Notification -= OnMonitoredItemNotification;
                _subscription?.RemoveItem(item);   // 从本地集合移除
                _items.Remove(nodeId);
                // 注意：RemoveItem 会将 item 加入删除列表，需调用 ApplyChanges 或 DeleteItems 真正删除
                _subscription?.ApplyChanges();     // 提交删除请求
                AppLogger.Info($"OPC UA 取消订阅节点 {nodeId}", "OPC UA");
            }
        }

        public void UnsubscribeAll()
        {
            foreach (var kv in _items.ToList())
                Unsubscribe(kv.Key);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Disconnect();
        }
    }
}
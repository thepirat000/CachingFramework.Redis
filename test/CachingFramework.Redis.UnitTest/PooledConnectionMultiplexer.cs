using System;
using StackExchange.Redis;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using StackExchange.Redis.Maintenance;
using StackExchange.Redis.Profiling;

namespace CachingFramework.Redis.UnitTest
{
    /// <summary>
    /// original from https://github.com/uliian/StackExchange.Redis.Pool/blob/master/StackExchange.Redis.ConnectionPool/src/StackExchange.Redis.Pool/PooledConnectionMultiplexer.cs
    /// </summary>
    public class PooledConnectionMultiplexer : IConnectionMultiplexer
    {
        private readonly ConnectionMultiplexer _connectionMultiplexer;

        public PooledConnectionMultiplexer(ConfigurationOptions config)
        {
            this._connectionMultiplexer = ConnectionMultiplexer.Connect(config);
        }

        public PooledConnectionMultiplexer(string configStr)
            : this(ConfigurationOptions.Parse(configStr))
        {
        }

        /// <summary>Indicates whether any servers are connected</summary>
        public bool IsConnecte => this._connectionMultiplexer.IsConnected;

        public ServerCounters GetCounters()
        {
            return this._connectionMultiplexer.GetCounters();
        }

        public EndPoint[] GetEndPoints(bool configuredOnly = false)
        {
            return this._connectionMultiplexer.GetEndPoints(configuredOnly);
        }

        public void Wait(Task task)
        {
            this._connectionMultiplexer.Wait(task);
        }

        public T Wait<T>(Task<T> task)
        {
            return this._connectionMultiplexer.Wait(task);
        }

        public void WaitAll(params Task[] tasks)
        {
            this._connectionMultiplexer.WaitAll(tasks);
        }

        public int HashSlot(RedisKey key)
        {
            return this._connectionMultiplexer.HashSlot(key);
        }

        public ISubscriber GetSubscriber(object asyncState = null)
        {
            return this._connectionMultiplexer.GetSubscriber(asyncState);
        }

        public IDatabase GetDatabase(int db = -1, object asyncState = null)
        {
            return this._connectionMultiplexer.GetDatabase(db, asyncState);
        }

        public IServer GetServer(string host, int port, object asyncState = null)
        {
            return this._connectionMultiplexer.GetServer(host, port, asyncState);
        }

        public IServer GetServer(string hostAndPort, object asyncState = null)
        {
            return this._connectionMultiplexer.GetServer(hostAndPort, asyncState);
        }

        public IServer GetServer(IPAddress host, int port)
        {
            return this._connectionMultiplexer.GetServer(host, port);
        }

        public IServer GetServer(EndPoint endpoint, object asyncState = null)
        {
            return this._connectionMultiplexer.GetServer(endpoint, asyncState);
        }

        public IServer[] GetServers()
        {
            return _connectionMultiplexer.GetServers();
        }

        public Task<bool> ConfigureAsync(TextWriter log = null)
        {
            return this._connectionMultiplexer.ConfigureAsync(log);
        }

        public bool Configure(TextWriter log = null)
        {
            return this._connectionMultiplexer.Configure(log);
        }

        public string GetStatus()
        {
            return this._connectionMultiplexer.GetStatus();
        }

        public void GetStatus(TextWriter log)
        {
            this._connectionMultiplexer.GetStatus(log);
        }

        public void Close(bool allowCommandsToComplete = true)
        {
            this._connectionMultiplexer.Close(allowCommandsToComplete);
        }

        public Task CloseAsync(bool allowCommandsToComplete = true)
        {
            return this._connectionMultiplexer.CloseAsync(allowCommandsToComplete);
        }

        public string GetStormLog()
        {
            return this._connectionMultiplexer.GetStormLog();
        }

        public void ResetStormLog()
        {
            this._connectionMultiplexer.ResetStormLog();
        }

        public long PublishReconfigure(CommandFlags flags = CommandFlags.None)
        {
            return this._connectionMultiplexer.PublishReconfigure(flags);
        }

        public Task<long> PublishReconfigureAsync(CommandFlags flags = CommandFlags.None)
        {
            return this._connectionMultiplexer.PublishReconfigureAsync(flags);
        }

        public void Dispose()
        {
        }

        public void RegisterProfiler(Func<ProfilingSession> profilingSessionProvider)
        {
            this._connectionMultiplexer.RegisterProfiler(profilingSessionProvider);
        }

        public int GetHashSlot(RedisKey key)
        {
            return _connectionMultiplexer.GetHashSlot(key);
        }

        public void ExportConfiguration(Stream destination, ExportOptions options = (ExportOptions)(-1))
        {
            _connectionMultiplexer.ExportConfiguration(destination, options);
        }

        public string ClientName => this._connectionMultiplexer.ClientName;

        public string Configuration => this._connectionMultiplexer.Configuration;

        public int TimeoutMilliseconds => this._connectionMultiplexer.TimeoutMilliseconds;

        public long OperationCount => this._connectionMultiplexer.OperationCount;

        public bool PreserveAsyncOrder
        {
            get => this._connectionMultiplexer.PreserveAsyncOrder;
            set => this._connectionMultiplexer.PreserveAsyncOrder = value;
        }

        /// <summary>Indicates whether any servers are connected</summary>
        public bool IsConnected => this._connectionMultiplexer.IsConnected;

        public bool IncludeDetailInExceptions
        {
            get => this._connectionMultiplexer.IncludeDetailInExceptions;
            set => this._connectionMultiplexer.IncludeDetailInExceptions = value;
        }

        public int StormLogThreshold
        {
            get => this._connectionMultiplexer.StormLogThreshold;
            set => this._connectionMultiplexer.StormLogThreshold = value;
        }

        public bool IsConnecting => throw new NotImplementedException();

        event EventHandler<RedisErrorEventArgs> IConnectionMultiplexer.ErrorMessage
        {
            add => this._connectionMultiplexer.ErrorMessage += value;
            remove => this._connectionMultiplexer.ErrorMessage -= value;
        }

        event EventHandler<ConnectionFailedEventArgs> IConnectionMultiplexer.ConnectionFailed
        {
            add { this._connectionMultiplexer.ConnectionFailed += value; }
            remove { this._connectionMultiplexer.ConnectionFailed -= value; }
        }

        event EventHandler<InternalErrorEventArgs> IConnectionMultiplexer.InternalError
        {
            add { this._connectionMultiplexer.InternalError += value; }
            remove { this._connectionMultiplexer.InternalError -= value; }
        }

        event EventHandler<ConnectionFailedEventArgs> IConnectionMultiplexer.ConnectionRestored
        {
            add { this._connectionMultiplexer.ConnectionRestored += value; }
            remove { this._connectionMultiplexer.ConnectionRestored -= value; }
        }

        event EventHandler<EndPointEventArgs> IConnectionMultiplexer.ConfigurationChanged
        {
            add { this._connectionMultiplexer.ConfigurationChanged += value; }
            remove { this._connectionMultiplexer.ConfigurationChanged -= value; }
        }

        event EventHandler<EndPointEventArgs> IConnectionMultiplexer.ConfigurationChangedBroadcast
        {
            add { this._connectionMultiplexer.ConfigurationChangedBroadcast += value; }
            remove { this._connectionMultiplexer.ConfigurationChangedBroadcast -= value; }
        }

        public event EventHandler<ServerMaintenanceEvent> ServerMaintenanceEvent
        {
            add { this._connectionMultiplexer.ServerMaintenanceEvent += value; }
            remove { this._connectionMultiplexer.ServerMaintenanceEvent -= value; }
        }

        event EventHandler<HashSlotMovedEventArgs> IConnectionMultiplexer.HashSlotMoved
        {
            add { this._connectionMultiplexer.HashSlotMoved += value; }
            remove { this._connectionMultiplexer.HashSlotMoved -= value; }
        }
        
        public ValueTask DisposeAsync()
        {
            return new ValueTask();
        }
    }
}

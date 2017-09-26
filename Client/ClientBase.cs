using Microsoft.AspNet.SignalR.Client;
using System;
using static z.Diagnostics.Log;

namespace z.Signal.Client
{
    /// <summary>
    /// LJ Gomez, 
    /// </summary>
    public abstract class ClientBase
    {
        protected HubConnection _hubConnection;
        protected IHubProxy _myHubProxy;

        public string HubConnectionUrl { get; set; }
        public string HubProxyName { get; set; }
        public TraceLevels HubTraceLevel { get; set; }
        public System.IO.TextWriter HubTraceWriter { get; set; }

        public virtual Action OnConnected { get; set; }
        public virtual Action OnDisconnected { get; set; }

        public ConnectionState State
        {
            get { return _hubConnection.State; }
        }

        public ClientBase()
        {
            this.HubTraceLevel = TraceLevels.All;
            this.HubTraceWriter = Console.Out;
            OnLog += ClientBase_OnLog;
        }

        protected void Init()
        {
            _hubConnection = new HubConnection(HubConnectionUrl)
            {
                TraceLevel = HubTraceLevel,
                TraceWriter = HubTraceWriter
            };

            _myHubProxy = _hubConnection.CreateHubProxy(HubProxyName);

            _hubConnection.Received += _hubConnection_Received;
            _hubConnection.Reconnected += _hubConnection_Reconnected;
            _hubConnection.Reconnecting += _hubConnection_Reconnecting;
            _hubConnection.StateChanged += _hubConnection_StateChanged;
            _hubConnection.Error += _hubConnection_Error;
            _hubConnection.ConnectionSlow += _hubConnection_ConnectionSlow;
            _hubConnection.Closed += _hubConnection_Closed;

        }

        public void CloseHub()
        {
            _hubConnection.Stop();
            _hubConnection.Dispose();
        }

        protected void StartHubInternal()
        {
            try
            {
                _hubConnection.Start().Wait();
            }
            catch (Exception ex)
            {
                v(ex.Message + " " + ex.StackTrace);
            }

        }

        public abstract void StartHub();

        void _hubConnection_Closed()
        {
          //  l("_hubConnection_Closed New State:" + _hubConnection.State + " " + _hubConnection.ConnectionId);
        }

        void _hubConnection_ConnectionSlow()
        {
          //  l("_hubConnection_ConnectionSlow New State:" + _hubConnection.State + " " + _hubConnection.ConnectionId);
        }

        void _hubConnection_Error(Exception obj)
        {
           // l("_hubConnection_Error New State:" + _hubConnection.State + " " + _hubConnection.ConnectionId);
        }

        public virtual void _hubConnection_StateChanged(StateChange obj)
        {
            switch (obj.NewState)
            {
                case ConnectionState.Connected: this.OnConnected?.Invoke(); break;
                case ConnectionState.Disconnected: this.OnDisconnected?.Invoke(); break;
            }
          //  l("_hubConnection_StateChanged New State:" + _hubConnection.State + " " + _hubConnection.ConnectionId);
        }

        void _hubConnection_Reconnecting()
        {
          //  l("_hubConnection_Reconnecting New State:" + _hubConnection.State + " " + _hubConnection.ConnectionId);
        }

        public virtual void _hubConnection_Reconnected()
        {
           // l("_hubConnection_Reconnected New State:" + _hubConnection.State + " " + _hubConnection.ConnectionId);
        }

        void _hubConnection_Received(string obj)
        {
            //l("_hubConnection_Received New State:" + _hubConnection.State + " " + _hubConnection.ConnectionId);
        }

        public virtual void Invoker(string Method, params object[] args)
        {
            try
            {
                _myHubProxy.Invoke(Method, args).ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        e("There was an error opening the connection:" + task.Exception.GetBaseException());
                    }
                }).Wait(3000);
                // l("Client " + Method + " sent to server");
            }
            catch (Exception ex)
            {
                e($"Client {Method} Stack: {ex.Message}");
            }
        }

        public virtual void Invoker<T>(string Method, params object[] args)
        {
            _myHubProxy.Invoke<T>(Method, args).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    e("There was an error opening the connection:" + task.Exception.GetBaseException());
                }
            }).Wait(3000);
            // l("Client " + Method + " sent to server");
        }

        protected IHubProxy Proxy
        {
            get
            {
                return _myHubProxy;
            }
        }

        public virtual IDisposable On(string EventName, Action OnData)
        {
            return Proxy.On(EventName, OnData);
        }

        public virtual IDisposable On<T1>(string EventName, Action<T1> OnData)
        {
            return Proxy.On<T1>(EventName, OnData);
        }

        public virtual IDisposable On<T1, T2>(string EventName, Action<T1, T2> OnData)
        {
            return Proxy.On<T1, T2>(EventName, OnData);
        }

        public virtual IDisposable On<T1, T2, T3>(string EventName, Action<T1, T2, T3> OnData)
        {
            return Proxy.On<T1, T2, T3>(EventName, OnData);
        }

        public virtual IDisposable On<T1, T2, T3, T4>(string EventName, Action<T1, T2, T3, T4> OnData)
        {
            return Proxy.On<T1, T2, T3, T4>(EventName, OnData);
        }

        public virtual IDisposable On<T1, T2, T3, T4, T5>(string EventName, Action<T1, T2, T3, T4, T5> OnData)
        {
            return Proxy.On<T1, T2, T3, T4, T5>(EventName, OnData);
        }

        public virtual IDisposable On<T1, T2, T3, T4, T5, T6>(string EventName, Action<T1, T2, T3, T4, T5, T6> OnData)
        {
            return Proxy.On<T1, T2, T3, T4, T5, T6>(EventName, OnData);
        }

        public virtual IDisposable On<T1, T2, T3, T4, T5, T6, T7>(string EventName, Action<T1, T2, T3, T4, T5, T6, T7> OnData)
        {
            return Proxy.On<T1, T2, T3, T4, T5, T6, T7>(EventName, OnData);
        }

        //protected void OnInvoke(Form mform, Action action)
        //{
        //    mform.BeginInvoke(new MethodInvoker(delegate { action(); }));
        //}


        public virtual void ClientBase_OnLog(string message, DateTime LogTime, LogType type)
        {
            if (_hubConnection != null && _hubConnection.State == ConnectionState.Connected)
                Invoker("Log", message, LogTime, Convert.ToInt32(type));
        }

        //public virtual void CLog(string Msg, ConsoleColor clr = ConsoleColor.White)
        //{
        //    Console.ForegroundColor = clr;
        //    Console.WriteLine(Msg);
        //    Console.ForegroundColor = ConsoleColor.White;    
        //}

        //public enum LogType
        //{
        //    Information = 0,
        //    Warning = 1,
        //    Error = 2
        //}

    }
}

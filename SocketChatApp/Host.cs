using Common;
using SocketChatClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketListener
{
    class Host: IDisposable
    {
        private readonly int _localPort;
        private readonly SocketListener _listener;
        private readonly ChatClient _client;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public Host(int localPort, int remotePort)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _localPort = localPort;
            _listener = new SocketListener(Process);
            _client = new ChatClient(remotePort, Process);
        }

        public void Start()
        {
            Task.Run(() => _listener.Run(_localPort, 5000, _cancellationTokenSource.Token), _cancellationTokenSource.Token);
            _client.Start();
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
        }

        public void Dispose()
        {
            _client.Dispose();
            _listener.Dispose();
            _cancellationTokenSource.Dispose();
        }

        private string Process(string input)
        {
            return $"{DateTime.Now.TimeOfDay} {input}";
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerApp
{
    class Server
    {
        private readonly TcpListener _tcp = new TcpListener(System.Net.IPAddress.Any, 8181);
        private Thread _acceptThread;

        static Server()
        {
            Client.Log+= Console.WriteLine;
        }

        public void Start()
        {
            _tcp.Start();
            _acceptThread = new Thread(Accept) { IsBackground = true };
            _acceptThread.Start();
        }

        public void Stop()
        {
            _acceptThread.Abort();
        }

        private void Accept()
        {
            while (true)
            {

                TcpClient client = _tcp.AcceptTcpClient();

                try
                {
                    new Thread(() =>
                    {
                        using (var cl = new Client(client))
                        {
                            cl.ValidateUser();
                        }
                    })
                    { IsBackground = true }.Start();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            }
        }
        
    }
}

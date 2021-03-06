﻿using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentSocket.Reactive;

namespace FluentSocket.Example
{
    public class Program
    {
        const int Port = 3000;
        const int BufferSize = 65536;
        static readonly Encoding Encoding = Encoding.UTF8;

        public static void Main(string[] args)
        {
            var arg0 = args.FirstOrDefault();

            if (arg0 == "server")
            {
                RunServerAsync(Port).Wait();
            }
            else if (arg0 == "client")
            {
                RunClientAsync(Port).Wait();
            }
        }

        static Task RunServerAsync(int port)
        {
            var server = NetSocketFactory.Default.CreateServer();

            server.OnConnections()
                .Subscribe(conn => {
                    var received = conn.SetReceiveBufferSize(BufferSize).BeginReceive();
                    conn.BeginSend(received);

                    received.Subscribe(d => {
                        Console.WriteLine("{0} > {1}", conn.RemoteEndPoint, d.ToString(Encoding));
                    });

                    conn.OnClosed().Subscribe(x => {
                        Console.WriteLine("{0} # offline", conn.RemoteEndPoint);
                    });
                });

            Console.WriteLine("Echo server start at port: {0}", port);

            return server.ListenAsync(port);
        }

        static async Task RunClientAsync(int port)
        {
            var client = await NetSocketFactory.Default.ConnectAsync("127.0.0.1", port);
            
            var toSend = new Subject<string>();
                
            client
                .SetSendBufferSize(BufferSize)
                .BeginSend(toSend.Select(x => Buffer.FromString(x, Encoding)));

            client
                .SetReceiveBufferSize(BufferSize)
                .BeginReceive()
                .Subscribe(d => Console.WriteLine("> {0}", d.ToString(Encoding)));

            client.OnClosed().Subscribe(x => Console.WriteLine("bye bye !!!"));

            await Task.Factory.StartNew(
                () => {
                    while (true)
                    {
                        var line = Console.ReadLine();

                        if (line.ToLower() == "quit")
                        {
                            return;
                        }
                        else
                        {
                            toSend.OnNext(line);
                            continue;
                        }
                    }
                }, 
                CancellationToken.None, 
                TaskCreationOptions.LongRunning, 
                TaskScheduler.Default);

            client.Close();                
        }
    }
}

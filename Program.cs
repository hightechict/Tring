using System;
using System.Net;
using System.Net.Sockets;


namespace QuickConnect
{
    class Program
    {
        static void Main(string[] args)
        {
			if (args.Length<2) return;
			var host = args[0];
			var port = (short)int.Parse(args[1]);
			Console.WriteLine($"Connecting to {host}:{port}");
            var response = TryConnect(host, port);
            var startColor = Console.ForegroundColor;
            if (string.IsNullOrEmpty(response))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("OK");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(response);
            }
            Console.ForegroundColor = startColor;
        }
		
		static string TryConnect(string host, short port)
		{
		    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Connect using a timeout (5 seconds)

            IAsyncResult result = socket.BeginConnect( host, port, null, null );

            result.AsyncWaitHandle.WaitOne(1000, true);

            if (socket.Connected)
            {
	            socket.EndConnect(result);
	            return "";
            }
            else
            {
                socket.Close();
                return "FAIL";
            }
        }			
    }
}

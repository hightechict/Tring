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
			Console.Write($"Connecting to {host}:{port}");
			Console.WriteLine( TryConnect(host, port)?" OK":" NOK");
        }
		
		static bool TryConnect(string host, short port)
		{
		Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

// Connect using a timeout (5 seconds)

IAsyncResult result = socket.BeginConnect( host, port, null, null );

bool success = result.AsyncWaitHandle.WaitOne(1000, true);

if (socket.Connected)
{
	socket.EndConnect(result);
	return true;
}
else
{
	// NOTE, MUST CLOSE THE SOCKET

	socket.Close();
	return false;
}}			
    }
}

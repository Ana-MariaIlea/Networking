using System;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using shared;
using System.Threading;
using System.Text;
using System.Linq;


class TCPServerSample
{
	/**
	 * This class implements a simple concurrent TCP Echo server.
	 * Read carefully through the comments below.
	 */

	
	public static void Main (string[] args)
	{
		Console.WriteLine("Server started on port 55555");

		TcpListener listener = new TcpListener (IPAddress.Any, 55555);
		listener.Start ();

		List<TcpClient> clients = new List<TcpClient>();
		Dictionary<TcpClient, string> clientData = new Dictionary<TcpClient, string>();

		while (true)
		{
			//First big change with respect to example 001
			//We no longer block waiting for a client to connect, but we only block if we know
			//a client is actually waiting (in other words, we will not block)
			//In order to serve multiple clients, we add that client to a list
			while (listener.Pending()) {
				TcpClient client = listener.AcceptTcpClient();
				clients.Add(client);
				clientData.Add(client, "Guest"+clients.Count);
				for (int i = 0; i < clients.Count - 1; i++)
				{
					NetworkStream stream = clients[i].GetStream();
					byte[] outBytes = Encoding.UTF8.GetBytes("Guest" + clients.Count+" has joined the chat");
					StreamUtil.Write(stream, outBytes);
				}

				NetworkStream clientstream = client.GetStream();
				byte[] clientoutBytes = Encoding.UTF8.GetBytes("You joined the chat as Guest" + clients.Count);
				StreamUtil.Write(clientstream, clientoutBytes);
				Console.WriteLine("Accepted new client.");
			}

			//Second big change, instead of blocking on one client, 
			//we now process all clients IF they have data available
			List<byte[]> messeges=new List<byte[]>();
			foreach (TcpClient client in clients)
			{
				if (client.Available == 0) continue;
				NetworkStream stream = client.GetStream();

				DateTime now = DateTime.Now;
				Console.WriteLine(now.ToString("F"));
				byte[] outBytesClientName = Encoding.UTF8.GetBytes(clientData[client]+": ");
				byte[] outBytesClientStream = StreamUtil.Read(stream);
				byte[] outBytesTime = Encoding.UTF8.GetBytes("-at "+now.ToString("F"));
				byte[] outBytes = outBytesClientName.Concat(outBytesClientStream).ToArray();
				outBytes = outBytes.Concat(outBytesTime).ToArray();

				messeges.Add(outBytes);

				//StreamUtil.Write(stream, outBytes);
			}

			foreach (TcpClient client in clients)
			{
				NetworkStream stream = client.GetStream();
                foreach (byte[] messege in messeges)
                {
					StreamUtil.Write(stream, messege);
				}
			}


			//Although technically not required, now that we are no longer blocking, 
			//it is good to cut your CPU some slack
			Thread.Sleep(100);
		}
	}
}



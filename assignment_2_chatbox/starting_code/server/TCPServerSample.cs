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


    public static void Main(string[] args)
    {
        Console.WriteLine("Server started on port 55555");

        TcpListener listener = new TcpListener(IPAddress.Any, 55555);
        listener.Start();

        List<TcpClient> clients = new List<TcpClient>();
        Dictionary<TcpClient, string> clientData = new Dictionary<TcpClient, string>();

        while (true)
        {
            //First big change with respect to example 001
            //We no longer block waiting for a client to connect, but we only block if we know
            //a client is actually waiting (in other words, we will not block)
            //In order to serve multiple clients, we add that client to a list
            try
            {
                while (listener.Pending())
                {
                    TcpClient client = listener.AcceptTcpClient();
                    SendMessegeToAllClients(clients, "guest" + (clients.Count + 1) + " has joined the chat");

                    clients.Add(client);
                    clientData.Add(client, "guest" + clients.Count);

                    SendMessegeToClient("You joined the chat as Guest" + clients.Count, client);
                    Console.WriteLine("Accepted new client.");
                }

                //Second big change, instead of blocking on one client, 
                //we now process all clients IF they have data available

                Dictionary<TcpClient, byte[]> messeges = new Dictionary<TcpClient, byte[]>();
                List<TcpClient> faultyClients = new List<TcpClient>();
                foreach (TcpClient client in clients)
                {

                    if (client.Available == 0)
                    {
                        continue;
                    }
                    NetworkStream stream = client.GetStream();
                    byte[] outBytesClientStream = StreamUtil.Read(stream);

                    messeges.Add(client, outBytesClientStream);

                }


                if (messeges.Count > 0)
                {
                    Dictionary<TcpClient, byte[]> newMesseges = new Dictionary<TcpClient, byte[]>();
                    MakeMesseges(ref clientData, messeges, newMesseges,clients);

                    foreach (KeyValuePair<TcpClient, byte[]> messege in newMesseges)
                    {
                        SendMessegeToAllClients(clients, messege.Value);
                    }

                }

                //Although technically not required, now that we are no longer blocking, 
                //it is good to cut your CPU some slack
                Thread.Sleep(100);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                RemoveClients(clients, clientData);
            }
        }
    }

    private static void RemoveClients(List<TcpClient> clients, Dictionary<TcpClient, string> clientData)
    {
        List<TcpClient> faultyClients = new List<TcpClient>();

        foreach (TcpClient client in clients)
        {
            if (client.Connected == false)
            {
                Console.WriteLine("Faulty client directly: " + clientData[client]);
                faultyClients.Add(client);
            }
        }
        Console.WriteLine(faultyClients.Count);
        foreach (var client in faultyClients)
        {
            Console.WriteLine("Removed client: " + clientData[client]);
            clientData.Remove(client);
            clients.Remove(client);
            client.Close();
        }
    }

    private static void SendMessegeToAllClients(List<TcpClient> clients, string messege)
    {
        for (int i = 0; i < clients.Count; i++)
        {
            SendMessegeToClient(messege, clients[i]);
        }
    }

    private static void SendMessegeToAllClientsButOne(List<TcpClient> clients, string messege, TcpClient omittedClient)
    {
        for (int i = 0; i < clients.Count; i++)
        {
            if (clients[i] != omittedClient)
                SendMessegeToClient(messege, clients[i]);
        }
    }

    private static void SendMessegeToClient(string messege, TcpClient client)
    {
        NetworkStream clientstream = client.GetStream();
        byte[] clientoutBytes = Encoding.UTF8.GetBytes(messege);
        StreamUtil.Write(clientstream, clientoutBytes);
    }

    private static void SendMessegeToAllClients(List<TcpClient> clients, byte[] messege)
    {
        for (int i = 0; i < clients.Count; i++)
        {
            SendMessegeToClient(messege, clients[i]);
        }
    }

    private static void SendMessegeToAllClientsButOne(List<TcpClient> clients, byte[] messege, TcpClient omittedClient)
    {
        for (int i = 0; i < clients.Count; i++)
        {
            if (clients[i] != omittedClient)
                SendMessegeToClient(messege, clients[i]);
        }
    }

    private static void SendMessegeToClient(byte[] messege, TcpClient client)
    {
        NetworkStream clientstream = client.GetStream();
        StreamUtil.Write(clientstream, messege);
    }

    private static void MakeMesseges(ref Dictionary<TcpClient, string> clientData, Dictionary<TcpClient, byte[]> messeges, in Dictionary<TcpClient, byte[]> newMesseges, List<TcpClient> clients)
    {
        foreach (KeyValuePair<TcpClient, byte[]> messege in messeges)
        {
            string inString = Encoding.UTF8.GetString(messege.Value);
            string[] subs = inString.Split(' ');
            switch (subs[0])
            {
                case "/setname":
                    ChangeNickname(clientData, clients, messege, subs);
                    break;
                case "/sn":
                    ChangeNickname(clientData, clients, messege, subs);
                    break;
                case "/list":
                    ListAllClientsMessege(clientData, clients, messege);
                    break;
                case "/help":
                    HelpMesseges(messege);
                    break;
                case "/whisper":
                    break;
                case "/w":
                    break;
                default:
                    DateTime now = DateTime.Now;
                    Console.WriteLine(now.ToString("F"));
                    byte[] outBytesClientName = Encoding.UTF8.GetBytes(clientData[messege.Key] + ": ");
                    byte[] outBytesTime = Encoding.UTF8.GetBytes("-at " + now.ToString("F"));
                    byte[] outBytes = outBytesClientName.Concat(messege.Value).ToArray();
                    outBytes = outBytes.Concat(outBytesTime).ToArray();
                    newMesseges.Add(messege.Key, outBytes);
                    break;
            }
        }

    }

    private static void ListAllClientsMessege(Dictionary<TcpClient, string> clientData, List<TcpClient> clients, KeyValuePair<TcpClient, byte[]> messege)
    {
        SendMessegeToClient("There are " + clients.Count + " clients connected:", messege.Key);
        foreach (var client in clients)
        {
            SendMessegeToClient(clientData[client], messege.Key);
        }
    }

    private static void HelpMesseges(KeyValuePair<TcpClient, byte[]> messege)
    {
        SendMessegeToClient("/setname or /sn to change the nickname", messege.Key);
        SendMessegeToClient("/list to list all the connected clients", messege.Key);
        SendMessegeToClient("/whisper or /w nickname messege to whisper to target", messege.Key);
    }

    private static void ChangeNickname(Dictionary<TcpClient, string> clientData, List<TcpClient> clients, KeyValuePair<TcpClient, byte[]> messege, string[] subs)
    {
        if (subs[1].Length > 0)
            if (clientData.ContainsValue(subs[1]))
            {
                SendMessegeToClient("This nickname is taken", messege.Key);
            }
            else
            {
                subs[1] = subs[1].ToLower();
                SendMessegeToClient("Nickname changed to " + subs[1], messege.Key);
                SendMessegeToAllClientsButOne(clients, clientData[messege.Key]+" changed nickname to "+ subs[1], messege.Key);
                clientData[messege.Key] = subs[1];
            }
    }
}



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
        int index = 0;

        while (true)
        {
            //First big change with respect to example 001
            //We no longer block waiting for a client to connect, but we only block if we know
            //a client is actually waiting (in other words, we will not block)
            //In order to serve multiple clients, we add that client to a list
            List<TcpClient> faultyClients = new List<TcpClient>();

            try
            {
                while (listener.Pending())
                {

                    TcpClient client = listener.AcceptTcpClient();
                    index++;
                    Console.WriteLine("Accepted new client.");

                    SendMessegeToAllClients(clients, "guest" + index + " has joined the chat", clientData, faultyClients);
                    SendMessegeToClient("You joined the chat as Guest" + index, client, clients, clientData, faultyClients);
                    clients.Add(client);
                    clientData.Add(client, "guest" + index);
                    Console.WriteLine("Accepted client " + "guest" + index);


                }

                //Second big change, instead of blocking on one client, 
                //we now process all clients IF they have data available

                Dictionary<TcpClient, byte[]> messeges = new Dictionary<TcpClient, byte[]>();
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
                    MakeMesseges(ref clientData, messeges, newMesseges, clients, faultyClients);

                    foreach (KeyValuePair<TcpClient, byte[]> messege in newMesseges)
                    {
                        SendMessegeToAllClients(clients, messege.Value, clientData, faultyClients);
                    }

                }
                //Although technically not required, now that we are no longer blocking, 
                //it is good to cut your CPU some slack
                RemoveFaultyClients(clients, clientData, faultyClients);


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
        CatchFaultyClients(clients, clientData, faultyClients);

        Console.WriteLine("Number of faulty clients: " + faultyClients.Count);
        RemoveFaultyClients(clients, clientData, faultyClients);
    }

    private static void RemoveFaultyClients(List<TcpClient> clients, Dictionary<TcpClient, string> clientData, List<TcpClient> faultyClients)
    {
        foreach (var client in faultyClients)
        {
            Console.WriteLine("Removed client: " + clientData[client]);
            if (clientData.ContainsKey(client))
                clientData.Remove(client);
            if (clients.Contains(client))
                clients.Remove(client);
            client.Close();
        }
    }

    private static void CatchFaultyClients(List<TcpClient> clients, Dictionary<TcpClient, string> clientData, List<TcpClient> faultyClients)
    {
        foreach (TcpClient client in clients)
        {
            if (client.Connected == false)
            {
                Console.WriteLine("Faulty client directly: " + clientData[client]);
                if (faultyClients.Contains(client) == false)
                    faultyClients.Add(client);
            }
        }
    }

    private static void SendMessegeToAllClients(List<TcpClient> clients, string messege, Dictionary<TcpClient, string> clientData, List<TcpClient> faultyClients)
    {
        // try
        // {
        for (int i = 0; i < clients.Count; i++)
        {
            Console.WriteLine("Messege sent to: " + clientData[clients[i]]);
            SendMessegeToClient(messege, clients[i], clients, clientData, faultyClients);
        }
        //}
        // catch (Exception e)
        //{
        //    Console.WriteLine(e.Message);
        //     CatchFaultyClients(clients, clientData, faultyClients);

        //}

    }

    private static void SendMessegeToAllClientsButOne(List<TcpClient> clients, string messege, TcpClient omittedClient, Dictionary<TcpClient, string> clientData, List<TcpClient> faultyClients)
    {
        for (int i = 0; i < clients.Count; i++)
        {
            if (clients[i] != omittedClient)
                SendMessegeToClient(messege, clients[i], clients, clientData, faultyClients);
        }
    }

    private static void SendMessegeToClient(string messege, TcpClient client, List<TcpClient> clients, Dictionary<TcpClient, string> clientData, List<TcpClient> faultyClients)
    {
        try
        {
            NetworkStream clientstream = client.GetStream();
            byte[] clientoutBytes = Encoding.UTF8.GetBytes(messege);
            StreamUtil.Write(clientstream, clientoutBytes);
            Console.WriteLine("Messege sent: " + messege);

        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            CatchFaultyClients(clients, clientData, faultyClients);

        }

    }
    private static void SendMessegeToAllClients(List<TcpClient> clients, byte[] messege, Dictionary<TcpClient, string> clientData, List<TcpClient> faultyClients)
    {
        // try
        //{
        for (int i = 0; i < clients.Count; i++)
        {
            SendMessegeToClient(messege, clients[i], clients, clientData, faultyClients);
        }
        //}
        //catch (Exception e)
        //{
        //   Console.WriteLine(e.Message);
        //    CatchFaultyClients(clients, clientData, faultyClients);

        //}

    }

    private static void SendMessegeToAllClientsButOne(List<TcpClient> clients, byte[] messege, TcpClient omittedClient, Dictionary<TcpClient, string> clientData, List<TcpClient> faultyClients)
    {
        for (int i = 0; i < clients.Count; i++)
        {
            if (clients[i] != omittedClient)
                SendMessegeToClient(messege, clients[i], clients, clientData, faultyClients);
        }
    }

    private static void SendMessegeToClient(byte[] messege, TcpClient client, List<TcpClient> clients, Dictionary<TcpClient, string> clientData, List<TcpClient> faultyClients)
    {
        try
        {
            NetworkStream clientstream = client.GetStream();
            StreamUtil.Write(clientstream, messege);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            CatchFaultyClients(clients, clientData, faultyClients);

        }

    }

    private static void MakeMesseges(ref Dictionary<TcpClient, string> clientData, Dictionary<TcpClient, byte[]> messeges, in Dictionary<TcpClient, byte[]> newMesseges, List<TcpClient> clients, List<TcpClient> faultyClients)
    {
        foreach (KeyValuePair<TcpClient, byte[]> messege in messeges)
        {
            string inString = Encoding.UTF8.GetString(messege.Value);
            string[] subs = inString.Split(' ');
            switch (subs[0])
            {
                case "/sn":
                case "/setname":
                    ChangeNickname(clientData, clients, messege, subs, faultyClients);
                    break;
                case "/list":
                    ListAllClientsMessege(clientData, clients, messege, faultyClients);
                    break;
                case "/help":
                    HelpMesseges(messege, clients, clientData, faultyClients);
                    break;
                case "/w":
                case "/whisper":
                    Whisper(clientData, messege, subs, clients, faultyClients);
                    break;
                default:
                    byte[] outBytesClientName = Encoding.UTF8.GetBytes(clientData[messege.Key] + ": ");
                    byte[] outBytes = outBytesClientName.Concat(messege.Value).ToArray();
                    newMesseges.Add(messege.Key, outBytes);
                    break;
            }
        }

    }

    private static void Whisper(Dictionary<TcpClient, string> clientData, KeyValuePair<TcpClient, byte[]> messege, string[] subs, List<TcpClient> clients, List<TcpClient> faultyClients)
    {
        if (clientData.ContainsValue(subs[1]))
        {
            string messegeFormat = "";
            for (int i = 2; i < subs.Length; i++)
            {
                messegeFormat += subs[i];
                messegeFormat += " ";
            }

            TcpClient rightClient = null;

            foreach (KeyValuePair<TcpClient, string> client in clientData)
            {
                if (client.Value == subs[1])
                {
                    rightClient = client.Key;
                    break;
                }
            }
            SendMessegeToClient(clientData[messege.Key] + " whispers to you: " + messegeFormat, rightClient, clients, clientData, faultyClients);
            SendMessegeToClient("You whisper to: " + subs[1] + " : " + messegeFormat, messege.Key, clients, clientData, faultyClients);
        }
        else
        {
            SendMessegeToClient("Target " + subs[1] + " does not exist ", messege.Key, clients, clientData, faultyClients);
        }
    }

    private static void ListAllClientsMessege(Dictionary<TcpClient, string> clientData, List<TcpClient> clients, KeyValuePair<TcpClient, byte[]> messege, List<TcpClient> faultyClients)
    {
        SendMessegeToClient("There are " + clients.Count + " clients connected:", messege.Key, clients, clientData, faultyClients);
        foreach (var client in clients)
        {
            SendMessegeToClient(clientData[client], messege.Key, clients, clientData, faultyClients);
        }
    }

    private static void HelpMesseges(KeyValuePair<TcpClient, byte[]> messege, List<TcpClient> clients, Dictionary<TcpClient, string> clientData, List<TcpClient> faultyClients)
    {
        SendMessegeToClient("/setname or /sn to change the nickname", messege.Key, clients, clientData, faultyClients);
        SendMessegeToClient("/list to list all the connected clients", messege.Key, clients, clientData, faultyClients);
        SendMessegeToClient("/whisper or /w nickname messege to whisper to target", messege.Key, clients, clientData, faultyClients);
    }

    private static void ChangeNickname(Dictionary<TcpClient, string> clientData, List<TcpClient> clients, KeyValuePair<TcpClient, byte[]> messege, string[] subs, List<TcpClient> faultyClients)
    {
        if (subs.Length > 2)
            if (subs[1].Length > 0)
                if (clientData.ContainsValue(subs[1]))
                {
                    SendMessegeToClient("This nickname is taken", messege.Key, clients, clientData, faultyClients);
                }
                else
                {
                    subs[1] = subs[1].ToLower();
                    SendMessegeToClient("Nickname changed to " + subs[1], messege.Key, clients, clientData, faultyClients);
                    SendMessegeToAllClientsButOne(clients, clientData[messege.Key] + " changed nickname to " + subs[1], messege.Key, clientData, faultyClients);
                    clientData[messege.Key] = subs[1];
                }
            else SendMessegeToClient("This nickname is invalid", messege.Key, clients, clientData, faultyClients);
        else SendMessegeToClient("Did not write a nickmane", messege.Key, clients, clientData, faultyClients);
    }
}



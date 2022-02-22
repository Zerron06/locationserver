using System;
using System.Net.Sockets;
using System.IO;
using System.Net;
using System.Collections.Generic;
public class Whois
{
   static Dictionary<string, string> clientData = new Dictionary<string, string>();
    static void Main(string[] args)
    {
        runServer();
    }
    static void runServer()
    {
        TcpListener listener;
        Socket connection;
        NetworkStream socketStream;
        try
        {
            listener = new TcpListener(IPAddress.Any, 43);
            listener.Start();
            Console.WriteLine("Server started listening: ");
            while (true)
            {
                connection = listener.AcceptSocket();
                connection.SendTimeout = 1000;
                connection.ReceiveTimeout = 1000;
                socketStream = new NetworkStream(connection);
                Console.WriteLine("Connection Received");
                doRequest(socketStream);
                socketStream.Close();
                connection.Close();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception:" + e.ToString());
        }
    }

    static void doRequest(NetworkStream socketStream)
    { 
        try
        {
            StreamWriter sw = new StreamWriter(socketStream);
            StreamReader sr = new StreamReader(socketStream);

            string line = sr.ReadLine();
            Console.WriteLine("Response Received " + line);
            string[] sections = line.Split(new char[] {' '},2);

            string location = null;
            string username = null;

            if (sections.Length == 1)
            {
                username = sections[0];
                
                if (clientData.ContainsKey(username))
                {
                    location = clientData[username];
                    sw.WriteLine(location);
                    sw.Flush();

                }
                else
                {
                    location = "ERROR: no entries found";
                    sw.WriteLine(location);
                    sw.Flush();
                }
            }
            else if (sections.Length == 2)
            {
                username = sections[0];
                location = sections[1];
                
                if (clientData.ContainsKey(username))
                {
                    clientData[username] = location;
                    sw.WriteLine("OK");
                    sw.Flush();

                }
                else
                {
                    clientData.Add(username, location);
                    sw.WriteLine("OK");
                    sw.Flush();
                }
            }
            
           
          
        }
        catch
        {
            Console.WriteLine("Something went wrong");
        }
    }
}

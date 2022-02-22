using System;
using System.Net.Sockets;
using System.IO;
using System.Net;
public class Whois
{
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

            //sw.WriteLine(args[0]);
            //sw.Flush();
            //Console.WriteLine(sr.ReadToEnd());

            string line = sr.ReadLine().Trim();
            Console.WriteLine("Response Received " + line);
            string[] sections = line.Split(new char[] {' '},2);
            switch (sections[0])
            {
                case "lookup":
                    Console.WriteLine("lookup Performed: Returned: " + sections[1]);
                    sw.WriteLine(sections[1]);
                    sw.Flush();
                    break;
                
                default:
                    Console.WriteLine("Unrecognised command");
                    break;

            }
        }
        catch
        {
            Console.WriteLine("Something went wrong");
        }
    }
}

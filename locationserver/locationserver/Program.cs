using System;
using System.Net.Sockets;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Threading;
public class Whois
{
    public Dictionary<string, string> clientData = new Dictionary<string, string>();

    public int readTimeout = 1000;
    public int writeTimeout = 1000;
    static void Main(string[] args)
    {
        runServer();
    }
    public static void runServer()
    {
        TcpListener listener;
        Socket connection;
        Handler RequestHandler;
        try
        {
            listener = new TcpListener(IPAddress.Any, 43);
            listener.Start();
            Console.WriteLine("Server started listening: ");
            while (true)
            {
                connection = listener.AcceptSocket();
                RequestHandler = new Handler();
                Thread t = new Thread(() => RequestHandler.doRequest(connection));
                t.Start();

                //connection.SendTimeout = 1000;
                //connection.ReceiveTimeout = 1000;
                //socketStream = new NetworkStream(connection);
                //Console.WriteLine("Connection Received");
                //doRequest(socketStream);
                //socketStream.Close();
                //connection.Close();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception:" + e.ToString());
        }
    }
}

class Handler 
{
    Whois whois = new Whois();

    static string username = null;
    static string location = null;

    public void doRequest(Socket connection)
    {
        NetworkStream socketStream;
        socketStream = new NetworkStream(connection);
        Console.WriteLine("Connection Received");
        string line = null;
        string status = "OK";
        try
        {

            socketStream.ReadTimeout = whois.readTimeout;
            socketStream.WriteTimeout = whois.writeTimeout;
            StreamWriter sw = new StreamWriter(socketStream);
            StreamReader sr = new StreamReader(socketStream);

            line = sr.ReadLine();
            Console.WriteLine("Response Received " + line);

            if (line.StartsWith("GET /?name=") && line.EndsWith(" HTTP/1.1"))
            {
                string[] sections = line.Split(new char[] { ' ' });
                sr.ReadLine();
                username = sections[1].Remove(0, 7);

                if(whois.clientData.ContainsKey(username))
                {
                    location = whois.clientData[username];
                    sw.WriteLine($"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n{location}\r\n");
                    sw.Flush();
                }
                else
                {
                    sw.Write("HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n\r\n");
                    sw.Flush();
                }
            }

            else if (line.Equals("POST / HTTP/1.1"))
            {
                sr.ReadLine();
                sr.ReadLine();
                sr.ReadLine();

                string[] split = sr.ReadLine().Split(new char[] { '&' });
                username = split[0].Remove(0, 5);
                location = split[1].Remove(0, 9);

                if (whois.clientData.ContainsKey(username))
                {
                    whois.clientData[username] = location;
                }
                else
                {
                    whois.clientData.Add(username, location);
                }
                sw.WriteLine("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                sw.Flush();
            }

            else if (line.StartsWith("GET /?") && line.EndsWith(" HTTP/1.0"))
            {
                string[] sections = line.Split(new char[] { ' ' });
                username = sections[1].TrimStart('/', '?');
                if(whois.clientData.ContainsKey(username))
                {
                    location = whois.clientData[username];
                    sw.WriteLine($"HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n{location}\r\n");
                    sw.Flush();
                }
                else
                {
                    sw.Write("HTTP/1.0 404 Not Found\r\nContent-Type: text/plain\r\n\r\n");
                    sw.Flush();
                }
            }

            else if (line.StartsWith("Post /") && line.EndsWith(" HTTP/1.0"))
            {
                string[] split = line.Split(new char[] { ' ' });
                username = split[1].Remove(0, 1);
                sr.ReadLine();
                sr.ReadLine();
                location = sr.ReadLine();

                if(whois.clientData.ContainsKey(username))
                {
                    whois.clientData[username] = location;
                }
                else
                {
                    whois.clientData.Add(username, location);
                }
                sw.WriteLine("HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                sw.Flush();
            }

            else if (line.StartsWith("GET /"))
            {
                string[] split = line.Split(new char[] { ' ' });
                username = split[1];
                if(whois.clientData.ContainsKey(username))
                {
                    location = whois.clientData[username];
                    sw.WriteLine($"HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n{location}\r\n");
                    sw.Flush();
                }
                else
                {
                    sw.Write("HTTP/0.9 404 Not Found\r\nContent-Type: text/plain\r\n\r\n");
                    sw.Flush();
                }
            }

            else if(line.StartsWith("PUT /"))
            {
                string[] split = line.Split(new char[] { '/' });
                username = split[1];
                if(sr.Peek() >= 0)
                {
                    sr.ReadLine();
                    location = sr.ReadLine();

                    if(whois.clientData.ContainsKey(username))
                    {
                        whois.clientData[username] = location;
                        sw.WriteLine($"HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n{location}\r\n");
                        sw.Flush();
                    }
                    else
                    {
                        whois.clientData.Add(username, location);
                        sw.WriteLine("HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                        sw.Flush();
                    }
                }
                else
                {
                    string[] sections = line.Split(new char[] { ' ' }, 2);
                    username = sections[0];
                    location = sections[1];

                    if(whois.clientData.ContainsKey(username))
                    {
                        whois.clientData[username] = location;
                    }
                    else
                    {
                        whois.clientData.Add(username, location);
                    }
                    sw.WriteLine("OK");
                    sw.Flush();

                }
            }
            else
            {
                string[] sections = line.Split(new char[] { ' ' }, 2);


                if (sections.Length == 1)
                {
                    username = sections[0];

                    if (whois.clientData.ContainsKey(username))
                    {
                        location = whois.clientData[username];
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

                    if (whois.clientData.ContainsKey(username))
                    {
                        whois.clientData[username] = location;
                        sw.WriteLine("OK");
                        sw.Flush();

                    }
                    else
                    {
                        whois.clientData.Add(username, location);
                        sw.WriteLine("OK");
                        sw.Flush();
                    }
                }
            }
           // string[] sections = line.Split(new char[] {' '},2);
           //
           // string location = null;
           // string username = null;
           //
           // if (sections.Length == 1)
           // {
           //     username = sections[0];
           //     
           //     if (clientData.ContainsKey(username))
           //     {
           //         location = clientData[username];
           //         sw.WriteLine(location);
           //         sw.Flush();
           //
           //     }
           //     else
           //     {
           //         location = "ERROR: no entries found";
           //         sw.WriteLine(location);
           //         sw.Flush();
           //     }
           // }
           // else if (sections.Length == 2)
           // {
           //     username = sections[0];
           //     location = sections[1];
           //     
           //     if (clientData.ContainsKey(username))
           //     {
           //         clientData[username] = location;
           //         sw.WriteLine("OK");
           //         sw.Flush();
           //
           //     }
           //     else
           //     {
           //         clientData.Add(username, location);
           //         sw.WriteLine("OK");
           //         sw.Flush();
           //     }
           // }
            
           
          
        }
        catch
        {
            Console.WriteLine("Something went wrong");
        }
    }
}

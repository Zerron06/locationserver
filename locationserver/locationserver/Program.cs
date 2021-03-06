using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace locationserver
{

    public class Whois
    {
        //static Dictionary<string, string> clientData = new Dictionary<string, string>();

        public static int readTimeout = 1000;
        public static int WriteTimeout = 1000;

        public static Logging Log;

        static bool Interface = false;




        public static void Main(string[] args)
        {
            string filename = null;
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-w":
                        Interface = true;
                        break;
                    case "-l":
                        filename = args[++i];
                        break;
                    default:
                        Console.WriteLine("Unknown Option" + args[i]);
                        break;
                }
            }

            Log = new Logging(filename);
            if (Interface == false)
            {
                runServer();
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new ServerForm());
            }
        }

        static Dictionary<string, string> clientDic = new Dictionary<string, string>();

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
                    Thread t = new Thread(() => RequestHandler.doRequest(connection, Log));
                    t.Start();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public class Handler
        {

            public void doRequest(Socket connection, Logging Log)
            {
                string Host = ((IPEndPoint)connection.RemoteEndPoint).Address.ToString();
                NetworkStream socketStream;
                socketStream = new NetworkStream(connection);
                string line = null;
                string status = "OK";
                try
                {
                    StreamWriter sw = new StreamWriter(socketStream);
                    StreamReader sr = new StreamReader(socketStream);

                    sw.AutoFlush = true; //Allows StreamWriter to flush automatically

                    socketStream.ReadTimeout = readTimeout;
                    socketStream.WriteTimeout = WriteTimeout;

                    line = sr.ReadLine();

                    string[] split = line.Split(new char[] { '/' }, 2); //Splits for username and location assignment

                    //HTTP 1.1 Lookup
                    if (line.StartsWith("GET /?name=") && line.Contains(" HTTP/1.1"))
                    {
                        string[] split2 = line.Split(new char[] { ' ' });
                        sr.ReadLine();
                        string username = split2[1].Remove(0, 7);

                        if (clientDic.ContainsKey(username))//Check to see if we have the client in the dictionary
                        {
                            string location = clientDic[username];
                            sw.WriteLine("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n" + location + "\r\n");
                        }
                        else
                        {
                            sw.WriteLine("HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n\r\n");
                            status = "UNKNOWN";
                        }
                    }

                    //HTTP 1.1 update
                    else if (line.StartsWith("POST / HTTP/1.1") && (sr.Peek() >= 0))
                    {
                        string[] location;

                        sr.ReadLine();//This function allows to read next line of the request
                        sr.ReadLine();
                        sr.ReadLine();
                        line = sr.ReadLine();
                        string[] split2 = line.Split(new char[] { '&' }, 2);//This array is spliting the line by & symbol
                        location = split2[1].Split(new char[] { '=' }, 2);
                        string[] username = split2[0].Split(new char[] { '=' }, 2);

                        if (clientDic.ContainsKey(username[1]))//Check to see if we have the client in the dictionary
                        {
                            clientDic[username[1]] = location[1];
                            sw.WriteLine("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                        }
                        else
                        {
                            clientDic.Add(username[1], location[1]);
                            sw.WriteLine("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                        }
                    }

                    //HTTP 1.0 Lookup
                    else if (line.StartsWith("GET /?") && line.Contains(" HTTP/1.0"))
                    {
                        string[] split2 = line.Split(new char[] { '?' }, 2); //Splits to get username
                        string[] username = split2[1].Split(new char[] { ' ' }, 2);

                        if (clientDic.ContainsKey(username[0]))//Check to see if we have the client in the dictionary
                        {
                            sw.WriteLine("HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n" + clientDic[username[0]] + "\r\n");
                        }
                        else
                        {
                            sw.WriteLine("HTTP/1.0 404 Not Found\r\nContent-Type: text/plain\r\n\r\n");
                            status = "UNKNOWN";
                        }
                    }

                    //HTTP 1.0 Update
                    else if (line.StartsWith("POST /") && (sr.Peek() >= 0) && line.Contains(" HTTP/1.0"))
                    {
                        //Read multiple lines to get to the last line which contains username and location
                        sr.ReadLine();
                        sr.ReadLine();
                        string lastLine = sr.ReadLine();
                        string[] m = split[1].Split(new char[] { ' ' }, 2);
                        string username = m[0];

                        if (clientDic.ContainsKey(username))//Check to see if we have the client in the dictionary
                        {
                            clientDic[username] = lastLine;
                            sw.WriteLine("HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                        }
                        else
                        {
                            clientDic.Add(username, lastLine);
                            sw.WriteLine("HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                        }

                    }

                    //HTTP 0.9 Lookup
                    else if (line.StartsWith("GET /"))
                    {
                        if (clientDic.ContainsKey(split[1]))//Check to see if we have the client in the dictionary
                        {
                            sw.WriteLine("HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n" + clientDic[split[1]] + "\r\n");
                        }
                        else //if user is not in dictionary send and error message to client
                        {
                            sw.WriteLine("HTTP/0.9 404 Not Found\r\nContent-Type: text/plain\r\n\r\n");
                            status = "Unknown";
                        }
                    }
                    //HTTP 0.9 Update
                    else if (line.StartsWith("PUT /") && (sr.Peek() >= 0))
                    {
                        sr.ReadLine();
                        string lastLine = sr.ReadLine(); //last line contains the location when using this protocol
                        if (clientDic.ContainsKey(split[1]))//Check to see if we have the client in the dictionary
                        {
                            clientDic[split[1]] = lastLine;
                            sw.WriteLine("HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n" + lastLine);
                        }
                        else
                        {
                            clientDic.Add(split[1], lastLine);
                            sw.WriteLine("HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                        }
                    }
                    //Whois request
                    else
                    {
                        string[] spaceSplit = line.Split(new char[] { ' ' }, 2);
                        //Lookup
                        if (spaceSplit.Length == 1) //if length is one this means just username
                        {
                            if (clientDic.ContainsKey(spaceSplit[0]))//checks to see if we have the client in dictionary
                            {
                                sw.WriteLine(clientDic[spaceSplit[0]] + "\r\n");
                            }
                            else // Gives error
                            {
                                sw.WriteLine("ERROR: no entries found");
                                status = "Unknown";
                            }
                        }
                        //Update
                        else if (spaceSplit.Length == 2)//username and location
                        {
                            if (clientDic.ContainsKey(spaceSplit[0])) //if client is in dictionary update location
                            {
                                clientDic[spaceSplit[0]] = spaceSplit[1];
                                sw.WriteLine("OK\r\n");
                            }
                            else // else create new client with new location
                            {
                                clientDic.Add(spaceSplit[0], spaceSplit[1]);
                                sw.WriteLine("OK\r\n");
                            }
                        }

                    }
                }
                catch (Exception)
                {
                    //This is to catch any exception that would crash the server
                    Console.WriteLine("ERROR!!!!");
                    status = "EXCEPTION";
                }
                finally
                {
                    //Now the request is complete close in the sockets as they are n longer needed
                    socketStream.Close();
                    connection.Close();
                    Log.WriteToLog(Host, line, status);
                }
            }
        }
    }

    public class Logging
    {
        public static string LogFile = null;

        public Logging(string filename)
        {
            LogFile = filename;
        }

        private static readonly object locker = new object();

        public void WriteToLog(string hostname, string message, string status)
        {
            //Creates a line in common log format
            string line = hostname + " - - " + DateTime.Now.ToString("'['dd'/'mm'/'yyyy'/'':'HH':'mm':'ss zz00']'") + " \"" + message + "\" " + status;
            //lock the file write to prevent concurrent threaded writes
            lock (locker)
            {
                Console.WriteLine(line);
                if (LogFile == null) return;
                //if there isnt a log file will exit after writing to console
                try
                {
                    StreamWriter sw;
                    sw = File.AppendText(LogFile);
                    sw.WriteLine(line);
                    sw.Close();
                }
                catch
                {
                    Console.WriteLine("Unable to Write Log File " + LogFile);
                }
            }
        }
    }
}


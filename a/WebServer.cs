//Kevin Toombs - 11412225

using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;

namespace CS422
{

    public class WebServer
    {
        static bool _foundHost = false;
        static TcpListener _tcpl;
        static TcpClient _tcpc;
        static IPAddress _localAddr = IPAddress.Parse("127.0.0.1");
        static int _EmptyLineCount = 0;
        static ParsingState _State = ParsingState.dead;
        static string _url;

        //new
        static ConcurrentDictionary<string, WebService> _services;
        static bool _serverActive = true;
        static BlockingCollection<TcpClient> _clients;
        static Thread[] _workers;
        private static Thread _listener;

        public static bool Start(int port, int threads)
        {
            _clients = new BlockingCollection<TcpClient>();
            _workers = new Thread[threads];
            _services = new ConcurrentDictionary<string, WebService>();
            try
            {
                for (int i = 0; i < threads; i++)
                {
                    _workers[i] = new Thread(WebServer.ThreadWork);
                    _workers[i].Start();
                }

                _tcpl = new TcpListener(_localAddr, port);
                _tcpl.Start();
                _listener = new Thread(new ThreadStart(WebServer.ThreadListen));
                _listener.Start();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            
            /*The static “Start” function is a non-blocking function that starts the server listening on the specified
            port. Since it is non-blocking, this implies that you should be creating a new “listen thread”, where you
            do the following:
             Accept new TCP socket connection
             Get a thread from the thread pool and pass it the TCP socket
             Repeat*/
        }

        private static WebRequest BuildRequest(TcpClient client)
        {
            RequestParser parser = new RequestParser();
            WebRequest req = parser.Parse(client);
            return req;
        }

        //insturctions say this method has to be public, but WebService is less accessible... so im making webservice public.
        public static void AddService(WebService service)
        {
            _services.TryAdd(service.ServiceURI, service);
            Console.WriteLine("Added service for {0}", service.ServiceURI);
        }

        static void ThreadListen()
        {
            while (_serverActive)
            {
                Console.WriteLine("ListenLoop");

                try
                {
                    var tcpSocket = _tcpl.AcceptTcpClient();
                    Console.WriteLine("Got Client");
                    _clients.TryAdd(tcpSocket);
                }
                catch (Exception)
                {
                    //catch socket acception, break loop
                }
                
            }
        }

        static void ThreadWork()
        {
            while (_serverActive)
            {
                try
                {
                    TcpClient client;
                    bool have = _clients.TryTake(out client, Timeout.Infinite);

                    WebRequest req = BuildRequest(client);
                    if (!Object.ReferenceEquals(req, null)) {
                        WebService service = FindServiceFor(req);
                        if (!Object.ReferenceEquals(service, null)) {
                            service.Handler(req);
                            Console.WriteLine("Req was handled");
                        }
                        else
                        {
                            string a = String.Format("404: {0}", req._uri);
                            Console.WriteLine(a);
                            string s = "<!DOCTYPE html> <html> <head><title>404</title></head><body></body></html>";
                            req.WriteNotFoundResponse(s);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Req was invalid");

                    }
                    client.Close();
                }
                catch (Exception)
                {
                    //catch disposed exception, break loop;
                }
                
            }
        }

        static private WebService FindServiceFor(WebRequest req)
        {
            WebService ws;
            bool found = _services.TryGetValue(req._uri, out ws);
            if (found)
                return ws;
            else
                return null;
        }

        public static void Stop()
        {
            _serverActive = false;
            for (int i = 0; i < _workers.Length; i++)
            {
                _workers[i].Join();
            }
            _listener.Join();
            _clients.Dispose();
            _tcpl.Stop();
            /*Add a public, static function named “Stop” that has no parameters, to the WebServer class.
            Implement this as a blocking function that lets all threads in the thread pool finish the current task
            they’re on, and then terminates all threads in the pool. If Stop is called when no threads are processing
            clients, then the call should return almost immediately, because shutting down all the idle threads will
            not take much time. If one or more threads is processing a client, it should finish that client’s transaction
            in an orderly fashion, then terminate. Make sure that the “Stop” function waits for each thread in the
            thread pool to complete entirely before returning.It must also stop the listening thread.*/

        }


        public static bool oldStart(int port, string responseTemplate)
        {
            int bytesRead;
            byte[] readBytes = new byte[1024];
            NetworkStream stream;
            string[] stringSeparators = new string[] { "\r\n" };
            string data = "";

            //Make listener
            try
            {
                //TCP boilerplate.
                _tcpl = new TcpListener(_localAddr, port);
                _tcpl.Start();
                _State = ParsingState.listening;
                Console.WriteLine("1: Listening on port " + port.ToString());
                _tcpc = _tcpl.AcceptTcpClient();
                _State = ParsingState.open;
                Console.WriteLine("2: Connected");

                stream = _tcpc.GetStream();

                //This dowhile loop reads in bytes, the processes each individual line.
                //if an incomplete line ends the byte array (no \r\n\)
                do
                {
                    bytesRead = stream.Read(readBytes, 0, readBytes.Length);
                    data += System.Text.Encoding.ASCII.GetString(readBytes, 0, bytesRead);
                    //Console.WriteLine(String.Format("Received: {0}", data));
 
                    //Console.WriteLine("bytesRead: " + bytesRead.ToString());

                    //lines are divided by /r/n
                    String[] lines = data.Split(stringSeparators, StringSplitOptions.None);
                    foreach (string s in lines)
                    {
                    if (String.IsNullOrEmpty(s)) {
                        if (!HandleEmptyString(s))
                        {
                            return StopRequest();
                        }
                    }
                    else
                    {
                        if (!HandleString(s))
                        {
                            return StopRequest();
                        }
                    }
                    }
                    data = lines[lines.Length - 1];
                }
                while (stream.DataAvailable);

                //Ending server boilerplate, disposing of stream.
                string response = string.Format(responseTemplate, "11412225", DateTime.Now, _url);
                byte[] msg = System.Text.Encoding.ASCII.GetBytes(response);
                stream.Write(msg, 0, msg.Length);
                stream.Dispose();
                Console.WriteLine("4: Response sent, stream disposed.");

                //Console.WriteLine(String.Format("Sent: {0}", response));

                _State = ParsingState.done;
                return StopRequest();
            }
            catch
            {
                Console.WriteLine("E: Caught Exception");
               return StopRequest();
            }

        }

        //Handles all non empty strings read from the request
        //and passes the results back to the start function to saying if that line is valid
        private static bool HandleString(string s)
        {
            //In the initial state of the server the only acceptable line is "GET (URL) HTTP/1.1"
            if (_State == ParsingState.open)
            {
                string expected = @"^GET \S* HTTP/1.1$";
                Regex expr = new Regex(expected);
                if (expr.IsMatch(s))
                {
                    //Console.WriteLine("Method matched, moving to headers");
                    _url = s.Split(' ')[1];
                    _State = ParsingState.headers;
                    return true;
                }
                else {
                    Console.WriteLine(s);
                    Console.WriteLine("Method did not match.");
                    return false;
                }
            }
            //After the method line this block checks for the Host header and any other headers, tossing all information into the ether.
            //after one single header is found, we change the state to "midHeaders" so that we can account for
            //headers that go beyond one line.
            else if (_State == ParsingState.headers || _State == ParsingState.midHeaders)
            {
                string expected = @"^\S+:\s*.*$";
                Regex expr = new Regex(expected);
                if (expr.IsMatch(s))
                {
                    if (!_foundHost)
                    {
                        string filledHost = @"^Host:\s*.*$";
                        Regex eHR = new Regex(filledHost);
                        if (eHR.IsMatch(s)) _foundHost = true;
                        //Console.WriteLine("Found filled host");
                        return true;
                    }
                     _State = ParsingState.midHeaders;
                    return true;
                }
                else
                {
                    if (!_foundHost)
                    {
                        string emptyHost = @"^Host:\s*$";
                        Regex eHR = new Regex(emptyHost);
                        if (eHR.IsMatch(s))
                        {
                            _foundHost = true;
                            //Console.WriteLine("Found empty host");
                            return true;
                        }
                    }

                    string continuedHeader = @"^[\v ].+";
                    Regex cHR = new Regex(continuedHeader);
                    if (cHR.IsMatch(s))
                    {
                        return true;
                    }
                    //Console.WriteLine(s);
                    Console.WriteLine("Error while going through headers. Host header is required for 1.1 requets");
                    return false;
                }
            }

            return true;
        }

        //Empty lines are permitted in two places, after the headers (there must be at least one) and then in the body
        //This checks for those, returning false if an empty line is found elsewhere.
        private static bool HandleEmptyString(string s)
        {
            _EmptyLineCount++;
            //Console.WriteLine("E: " + _EmptyLineCount.ToString());
            if (_State == ParsingState.body) return true;

            if (_State == ParsingState.midHeaders)
            {
                Console.WriteLine("3: New line found after headers, moving to body");
                _State = ParsingState.body;
                return true;
            }
            else
            {
                return false;
            }
        }

        //Function to "stop" the server
        //Sets static values back to 0, closes to TCP client, and stops the TCP listener.
        //Then uses the current state to tell the demo program if the request was valid or not.
        private static bool StopRequest()
        {
            Console.WriteLine("F: Stopping on state " + _State.ToString());


            _EmptyLineCount = 0;
            _foundHost = false;

            if (_State == ParsingState.dead)
            {
                return false; 
            }

            if (_State == ParsingState.listening)
            {
                _tcpl.Stop();
                return false;
            }

            if (_State == ParsingState.open ||
                _State == ParsingState.headers ||
                _State == ParsingState.midHeaders)
            {
                _tcpc.Close();
                _tcpl.Stop();
                return false;
            }

            _tcpc.Close();
            _tcpl.Stop();
            return true;
        }



    }
}

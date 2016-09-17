//Kevin Toombs - 11412225

using System;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace CS422
{
    internal enum ServerState { dead, listening, open, methodVerified, headers, midHeaders, body, done }


    public class WebServer
    {
        static bool _foundHost = false;
        static TcpListener _tcpl;
        static TcpClient _tcpc;
        static IPAddress _localAddr = IPAddress.Parse("127.0.0.1");
        static int _EmptyLineCount = 0;
        static ServerState _State = ServerState.dead;
        static string _url;
        
        public static bool Start(int port, string responseTemplate)
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
                _State = ServerState.listening;
                Console.WriteLine("1: Listening on port " + port.ToString());
                _tcpc = _tcpl.AcceptTcpClient();
                _State = ServerState.open;
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
                            return Stop();
                        }
                    }
                    else
                    {
                        if (!HandleString(s))
                        {
                            return Stop();
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

                _State = ServerState.done;
                return Stop();
            }
            catch
            {
                Console.WriteLine("E: Caught Exception");
               return Stop();
            }

        }

        //Handles all non empty strings read from the request
        //and passes the results back to the start function to saying if that line is valid
        private static bool HandleString(string s)
        {
            //In the initial state of the server the only acceptable line is "GET (URL) HTTP/1.1"
            if (_State == ServerState.open)
            {
                string expected = @"^GET \S* HTTP/1.1$";
                Regex expr = new Regex(expected);
                if (expr.IsMatch(s))
                {
                    //Console.WriteLine("Method matched, moving to headers");
                    _url = s.Split(' ')[1];
                    _State = ServerState.headers;
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
            else if (_State == ServerState.headers || _State == ServerState.midHeaders)
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
                     _State = ServerState.midHeaders;
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
            if (_State == ServerState.body) return true;

            if (_State == ServerState.midHeaders)
            {
                Console.WriteLine("3: New line found after headers, moving to body");
                _State = ServerState.body;
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
        private static bool Stop()
        {
            Console.WriteLine("F: Stopping on state " + _State.ToString());


            _EmptyLineCount = 0;
            _foundHost = false;

            if (_State == ServerState.dead)
            {
                return false; 
            }

            if (_State == ServerState.listening)
            {
                _tcpl.Stop();
                return false;
            }

            if (_State == ServerState.open ||
                _State == ServerState.headers ||
                _State == ServerState.midHeaders)
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

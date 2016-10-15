//Kevin Toombs - 11412225

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace CS422
{
    internal enum ParsingState { dead, listening, open, methodVerified, headers, midHeaders, body, done }

    public class RequestParser
    {
        bool _foundHost = false;
        ParsingState _State = ParsingState.dead;
        int _bytesRead = 0;
        byte[] _readBytes = new byte[1024];
        NetworkStream _stream;
        string[] _stringSeparators = new string[] { "\r\n" };
        string _data = "";
        private string _lastHeader;

        public WebRequest Parse(TcpClient client)
        {
            WebRequest req = new WebRequest();
            try
            {
                string bodyStart = "";
                req._networkStream = client.GetStream();
                _stream = req._networkStream;
                _State = ParsingState.open;
                do
                {
                    _bytesRead += _stream.Read(_readBytes, 0, _readBytes.Length);
                    _data += System.Text.Encoding.ASCII.GetString(_readBytes, 0, _bytesRead);
                    string[] lines = _data.Split(_stringSeparators, StringSplitOptions.None);
                    for (int i = 0; i < lines.Length - 1; i++)
                    {
                        string s = lines[i];
                        if (_State == ParsingState.done)
                        {
                            bodyStart += s;
                            if (i + 1 == lines.Length - 1)
                            {
                                bodyStart += lines[lines.Length - 1];
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(s))
                            {

                                if (!HandleEmptyString(s, req))
                                {
                                    Console.WriteLine("Bad Handle empty");
                                    return null;
                                }
                            }
                            else
                            {
                                if (!HandleString(s, req))
                                {
                                    Console.WriteLine("Bad Handle");
                                    return null;
                                }
                            }
                        }
                    }
                    _data = lines[lines.Length - 1];
                }
                while (_State != ParsingState.done);

                //make and set stream
                MemoryStream ms = new  MemoryStream(Encoding.UTF8.GetBytes(bodyStart ?? ""));
                long len = WebRequest.GetContentLengthOrDefault(req._headers, (long)(int)-1);
                ConcatStream cs;
                if (len < 0)
                    cs = new ConcatStream(ms, req._networkStream);
                else
                    cs = new ConcatStream(ms, req._networkStream, len);
                req._body = cs;

                return req;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Parser fail: " + ex.Message);
                return null;
            }

        }

        /*
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
                        if (String.IsNullOrEmpty(s))
                        {
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
        */

        //Handles all non empty strings read from the request
        //and passes the results back to the start function to saying if that line is valid
        private bool HandleString(string s, WebRequest req)
        {
            //In the initial state of the server the only acceptable line is "GET (URL) HTTP/1.1"
            //Console.WriteLine("Handling: " + s);
            if (_State == ParsingState.open)
            {
                string expected = @"^GET \S* HTTP/1.1$";
                Regex expr = new Regex(expected);
                if (expr.IsMatch(s))
                {
                    Console.WriteLine("Method matched, moving parsing");
                    req._method = s.Split(' ')[0];
                    req._uri = s.Split(' ')[1];
                    req._version = float.Parse((s.Split(' ')[2]).Split('/')[1]);
                    _State = ParsingState.headers;
                    Console.WriteLine("Method parsed, moving to headers");
                    return true;
                }
                else {
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
                        BuildHeader(s, req);
                        Console.WriteLine("Found filled host");
                        return true;
                    }
                     _State = ParsingState.midHeaders;
                    BuildHeader(s, req);
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
                            Console.WriteLine("Found empty host");
                            BuildHeader(s, req);
                            return true;
                        }
                    }

                    string continuedHeader = @"^[\v ].+";
                    Regex cHR = new Regex(continuedHeader);
                    if (cHR.IsMatch(s))
                    {
                        AddToLastHeader(s, req);
                        return true;
                    }
                    //Console.WriteLine(s);
                    Console.WriteLine("Error while going through headers. Host header is required for 1.1 requets");
                    return false;
                }
            }

            return true;
        }

        private void BuildHeader(string s, WebRequest req)
        {
            var key = s.Split(':')[0].ToLower();
            var value = s.Split(':')[1];
            _lastHeader = key;
  
            req._headers.AddOrUpdate(key, value, (okey, oldValue)=> oldValue + ", "+value);
        }

        private void AddToLastHeader(string s, WebRequest req)
        {
            req._headers.AddOrUpdate(_lastHeader, s, (okey, oldValue) => oldValue + s);
        }

        //Empty lines are permitted in two places, after the headers (there must be at least one) and then in the body
        //This checks for those, returning false if an empty line is found elsewhere.
        private bool HandleEmptyString(string s, WebRequest req)
        {
            if (_State == ParsingState.midHeaders)
            {
                //Console.WriteLine("3: New line found after headers, moving to body");
                _State = ParsingState.done;
                
                return true;
            }
            else
            {
                return false;
            }
        }




    }
}

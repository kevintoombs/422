using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace CS422
{
    public class WebRequest
    {

        internal string _method;
        internal string _uri;
        internal float _version;
        internal ConcurrentDictionary<string, string> _headers = new ConcurrentDictionary<string, string>();
        internal Stream _body;
        internal NetworkStream _networkStream;

        public void WriteNotFoundResponse(string pageHTML)
        {
            int code = 404;
            string message = "Not found";
            string type = "text/html";
            int lengthInBytes = pageHTML.Length * sizeof(Char);
            writeStatusLine(code, message);
            writeHeaders(lengthInBytes, type);
            writeBody(pageHTML);

            Console.WriteLine("404 sent");
        }

        public bool WriteHTMLResponse(string htmlString)
        {
            int code = 200;
            string message = "OK";
            string type = "text/html";
            int lengthInBytes = htmlString.Length * sizeof(Char);
            writeStatusLine(code, message);
            writeHeaders(lengthInBytes, type);
            writeBody(htmlString);

            return true;
        }

        private void writeBody(string bodyString)
        {
            byte[] buffer = Encoding.ASCII.GetBytes("\r\n"+bodyString);
            _networkStream.Write(buffer, 0, buffer.Length);
        }

        private void writeHeaders(int lengthInBytes, string type)
        {
            string contentType = "Content-Type: " + type + "\r\n";
            string contentLength = "Content-Length: " + lengthInBytes.ToString() + "\r\n";
            string headers = contentLength + contentType;
            byte[] buffer = Encoding.ASCII.GetBytes(headers);
            _networkStream.Write(buffer, 0, buffer.Length);
        }

        private void writeStatusLine(int code, string message)
        {
            string status = "HTTP/1.1 " + code.ToString() + " " + message + "\r\n";
            byte[] buffer = Encoding.ASCII.GetBytes(status);
            _networkStream.Write(buffer, 0, buffer.Length);
        }

        public static long GetContentLengthOrDefault(ConcurrentDictionary<string, string> headers,
            long defaultValue)
        {
            if (headers.ContainsKey("Content-Length".ToLower()))
            {
                string val = headers["Content-Length".ToLower()];
                long len;
                if (long.TryParse(val, out len))
                {
                    return len;
                }
                return defaultValue;
            }
            return defaultValue;
        }


    }
}

using System;
using System.Net;
using System.Net.Sockets;

namespace CS422
{
    class WebServerDemo
    {
        static void Main(string[] args)
        {
            string s =
                "HTTP/1.1 200 OK\r\n" +
                 "Content-Type: text/html\r\n" +
                 "\r\n\r\n" +
                 "<html>ID Number: {0}<br>" +
                 "DateTime.Now: {1}<br>" +
                 "Requested URL: {2}</html>";

            while (true)
            {
                WebServer.Start(8345, s);
            }

            if (!WebServer.Start(8345, s))
            {
                Console.WriteLine("wow");
            }

            return;
        }
    }
}

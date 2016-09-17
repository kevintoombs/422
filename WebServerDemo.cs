//Kevin Toombs - 11412225

using System;


namespace CS422
{
    //Simpe demo app that just constantly calls Start;
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
                if (WebServer.Start(8345, s))
                    Console.WriteLine("               Demo: Good request");
                else
                    Console.WriteLine("               Demo: Bad request");

            }

            return;
        }
    }
}

//Kevin Toombs - 11412225

using System;


namespace CS422
{
    //Simpe demo app that just constantly calls Start;
    class WebServerDemo
    {
        static void Main(string[] args)
        {
            WebServer.Start(4200, 5);
            DemoService ds = new DemoService();
            OtherDemo od = new OtherDemo();
            WebServer.AddService(ds);
            WebServer.AddService(od);
            Console.WriteLine("Server Started");
            return;
        }

        class OtherDemo: DemoService
        {
            public override string ServiceURI
            {
                get
                {
                    return "/j";
                }
            }
        }
    }
}

using System;


namespace CS422
{
    class DemoService : WebService
    {
        private const string c_template =
         "<html>This is the response to the request:<br>" +
         "Method: {0}<br>Request-Target/URI: {1}<br>" +
         "Request body size, in bytes: {2}<br><br>" +
         "Student ID: {3}</html>";

        public override string ServiceURI
        {
            get
            {
                return "/";
            }
        }

        public override void Handler(WebRequest req)
        {
            string method = req._method;
            string uri = req._uri;
            string size;
            try
            {
                size = req._body.Length.ToString();
            }
            catch
            {
                size = "Unknown";
            }
            string s = String.Format(c_template, method, uri, size, 11412225);
            req.WriteHTMLResponse(s);
        }
    }
}

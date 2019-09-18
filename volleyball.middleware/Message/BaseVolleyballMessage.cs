using System;

namespace volleyball.middleware.message
{
    public abstract class BaseVolleyballMessage
    {
        public string RequestMethod { get; set; }
        public string RequestPath { get; set; }
        public int StatusCode { get; set; }
        public double Elapsed { get; set; }

        public DateTime Time { get; set; }
    }
}
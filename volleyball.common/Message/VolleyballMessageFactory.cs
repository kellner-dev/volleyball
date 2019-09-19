
namespace volleyball.common.message
{
    public static class VolleyballMessageFactory
    {
        public static BaseVolleyballMessage Create(string extension,
                                                   string requestMethod,
                                                   string requestPath,
                                                   int statusCode,
                                                   double elapsed,
                                                   string httpContext)
        {
            if(extension.Contains(";"))
                extension = extension.Split(';')[0];
            switch (extension)
            {
                case "text/html":
                    return new HtmlVolleyballMessage();
                case "text/javascript":
                    return new AnyVolleyballMessage();
                default :
                    return new AnyVolleyballMessage();

            }
        }

        public static BaseVolleyballMessage CreateByName(string name)
        {
            switch (name)
            {
                case "HtmlVolleyballMessage":
                    return new HtmlVolleyballMessage();
                case "AnyVolleyballMessage":
                    return new AnyVolleyballMessage();
                default :
                    return new AnyVolleyballMessage();

            }
        }
    }
}
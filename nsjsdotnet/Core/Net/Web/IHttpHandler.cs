namespace nsjsdotnet.Core.Net.Web
{
    public interface IHttpHandler
    {
        void ProcessRequest(HttpContext context);
    }
}

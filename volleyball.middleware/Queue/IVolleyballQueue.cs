using System.Threading.Tasks;
using volleyball.middleware.message;

namespace volleyball.middleware.queue
{
    public interface IVolleyballQueue
    {
        Task Publish(BaseVolleyballMessage message);

    }
}
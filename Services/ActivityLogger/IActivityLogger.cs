using System.Threading.Tasks;

namespace EeD_BE_EeD.Services.ActivityLogger
{
    public interface IActivityLogger
    {
        Task LogAsync(string? userId, string action, string? description = null);
    }
}

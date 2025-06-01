using BreganTwitchBot.Domain.Database.Models;

namespace BreganTwitchBot.Domain.Interfaces.Helpers
{
    public interface IUserContextHelper
    {
        string GetUserId();
        string GetUserFirstName();
        User GetUser();
    }
}

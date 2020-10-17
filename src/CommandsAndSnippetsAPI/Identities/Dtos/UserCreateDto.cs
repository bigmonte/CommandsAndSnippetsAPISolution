using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CommandsAndSnippetsAPI.Dtos.User
{
    public class UserCreateDto
    {
        public string UserName { get; init; }
        public string Email { get; init; }
        public string PasswordHash { get; init; }
    }
}
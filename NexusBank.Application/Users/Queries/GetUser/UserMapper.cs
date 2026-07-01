using Riok.Mapperly.Abstractions;
using NexusBank.Domain.Entities;

namespace NexusBank.Application.Users.Queries.GetUser;

[Mapper]
public partial class UserMapper
{
    public partial UserDto ToDto(User user);
}

using PhoeNix.Domain.Entities.Users;

namespace PhoeNix.Domain.Models.Users;

public record UserListResponse();

public record UserResponse(
    UserId Id
);
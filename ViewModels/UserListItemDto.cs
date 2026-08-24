namespace POS.Web.ViewModels;

public record UserListItemDto(string Id, string FullName, string? Email, string Roles, bool IsSuspended);

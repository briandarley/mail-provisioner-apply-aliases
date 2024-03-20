using UNC.ActiveDirectory.Common.Pagination;

public interface IActiveDirectoryService
{
    Task<PagedResponse<UNC.ActiveDirectory.Common.Entities.Users.UserDetail>> GetAdUsers(UNC.ActiveDirectory.Common.Criteria.Users.UsersCriteria criteria);
}
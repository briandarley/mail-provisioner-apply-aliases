public interface IDataAccessService
{
    Task<UNC.DataAccessAPI.Common.Entities.MailProvisionDb.UserAccount> GetUserAccount(string username);
    Task<bool> UpdateUserAccount(UNC.DataAccessAPI.Common.Entities.MailProvisionDb.UserAccount userAccount);
}
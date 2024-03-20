using UNC.ActiveDirectory.Common.Entities.Users;

namespace mail_provisioner_apply_aliases.Interfaces;

public interface IFileService
{
    void SaveAdResultsToFile(List<UNC.ActiveDirectory.Common.Entities.Users.UserDetail> users, string fileName);
    List<UserDetail> ReadAdResultsFromFile(string fileName);
}

using mail_provisioner_apply_aliases.Interfaces;
using Newtonsoft.Json;
using UNC.ActiveDirectory.Common.Entities.Users;

namespace mail_provisioner_apply_aliases.Services;

public class FileService : IFileService
{
    public FileService()
    {

    }



    public void SaveAdResultsToFile(List<UserDetail> users, string fileName)
    {
        var json = JsonConvert.SerializeObject(users);
        System.IO.File.WriteAllText(fileName, json);
    }

    public List<UserDetail> ReadAdResultsFromFile(string fileName)
    {
        if (!System.IO.File.Exists(fileName)) return null;
        var json = System.IO.File.ReadAllText(fileName);
        return JsonConvert.DeserializeObject<List<UserDetail>>(json);
    }
}

using System.Text;
using Newtonsoft.Json;
using UNC.Services.Responses;

public class DataAccessService : IDataAccessService
{
    private IHttpClientFactory _httpClientFactory;

    public DataAccessService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }


    public async Task<UNC.DataAccessAPI.Common.Entities.MailProvisionDb.UserAccount> GetUserAccount(string username)
    {
        using var client = _httpClientFactory.CreateClient("LOCAL_DATA");
        client.Timeout = TimeSpan.FromMinutes(5);
        var response = await client.GetAsync($"mail-provision-db/user-accounts?uid={username}");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var pagedResponse = JsonConvert.DeserializeObject<PagedResponse<UNC.DataAccessAPI.Common.Entities.MailProvisionDb.UserAccount>>(content);
            if (pagedResponse is null || pagedResponse.TotalRecords != 1) return null;
            return pagedResponse.Entities.Single();
        }
        else
        {
            return null;
        }
    }

    public async Task<bool> UpdateUserAccount(UNC.DataAccessAPI.Common.Entities.MailProvisionDb.UserAccount userAccount)
    {
        using var client = _httpClientFactory.CreateClient("LOCAL_DATA");
        client.Timeout = TimeSpan.FromMinutes(5);

        var json = JsonConvert.SerializeObject(userAccount);
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await client.PutAsync($"mail-provision-db/user-accounts/{userAccount.Pid}", data);

        if (response.IsSuccessStatusCode)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
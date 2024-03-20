using Newtonsoft.Json;
using UNC.ActiveDirectory.Common.Pagination;
using UNC.Extensions.General;
public class ActiveDirectoryService : IActiveDirectoryService
{
    private readonly IHttpClientFactory _httpClientFactory;


    public ActiveDirectoryService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;

    }

    public async Task<PagedResponse<UNC.ActiveDirectory.Common.Entities.Users.UserDetail>> GetAdUsers(UNC.ActiveDirectory.Common.Criteria.Users.UsersCriteria criteria)
    {

        using var client = _httpClientFactory.CreateClient("LOCAL_AD");
        var queryParams = UNC.Extensions.General.CriteriaExtensions.ToQueryParams(criteria);
        var response = await client.GetAsync($"users?{queryParams}");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var pgResposne = JsonConvert.DeserializeObject<PagedResponse<UNC.ActiveDirectory.Common.Entities.Users.UserDetail>>(content);
            return pgResposne;
        }
        else
        {
            return null;
        }


    }




}
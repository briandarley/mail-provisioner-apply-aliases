using System.Collections.Concurrent;
using System.Diagnostics;
using mail_provisioner_apply_aliases.Interfaces;
using UNC.ActiveDirectory.Common.Entities.Users;
using UNC.ActiveDirectory.Common.Pagination;
using UNC.DataAccessAPI.Common.Entities.MailProvisionDb;
using UNC.Extensions.General;

namespace mail_provisioner_apply_aliases.WorkTasks;

public class WorkerTask : IWorkerTask
{
    private IActiveDirectoryService _activeDirectoryService;
    private IDataAccessService _dataAccessService;
    private IFileService _fileService;


    public WorkerTask(IActiveDirectoryService activeDirectoryService, IDataAccessService dataAccessService, IFileService fileService)
    {
        _activeDirectoryService = activeDirectoryService;
        _dataAccessService = dataAccessService;
        _fileService = fileService;
    }


    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            //a8675309@adminliveunc.mail.onmicrosoft.com
            // Your background task logic here
            var adUsers = await GetAdUsers();
            //a24elsea@adminliveunc.mail.onmicrosoft.com
            var ignoreSamAccountEndsWith = new List<string> { ".smb", ".rmb", ".svc", ".adm", ".com", ".dom", ".guest" };
            var ignoreProxyAddressesEndWith = new List<string> { "@ad.unc.edu", "adminliveunc.mail.onmicrosoft.com", "adminliveunc.onmicrosoft.com" };
            var listIgnoreFuture = new ConcurrentBag<string>();
            adUsers = adUsers
            .Where(c => !c.SamAccountName.IsNumeric() && !ignoreSamAccountEndsWith.Any(d => c.SamAccountName.EndsWith(d)))
            .Where(c => c.ProxyAddresses != null && c.ProxyAddresses.Any(d => d.StartsWith("smtp:") || d.StartsWith("SMTP:")))
                            .OrderBy(c => c.SamAccountName)
            .ToList();

            await Parallel.ForEachAsync(adUsers, async (user, token) =>
            {
                //admin.20090124,campussvcs_lencnf.rm DOM_qualys.scn dsa_ecw.gst neuro_neurolaptop.rm cas_wh-adminoffice.r sop_kashubalab.inst
                if (user.SamAccountName.Contains("."))
                {
                    listIgnoreFuture.Add(user.SamAccountName);
                    Console.WriteLine(user.SamAccountName);
                }


                var userAccount = await _dataAccessService.GetUserAccount(user.SamAccountName);
                if (userAccount != null && user.ProxyAddresses != null && (userAccount.Aliases is null || !userAccount.Aliases.Any()))
                {
                    userAccount.Aliases = user.ProxyAddresses
                    .Where(c => c.StartsWith("SMTP:") || c.StartsWith("smtp:"))
                    .Where(c => !ignoreProxyAddressesEndWith.Any(d => c.EndsWith(d)))
                    .Select(c =>
                            new Alias
                            {
                                AliasEmail = c[5..],
                                IsPrimary = c.StartsWith("SMTP:")
                            }).ToList();


                    await _dataAccessService.UpdateUserAccount(userAccount);
                }
            });
            // foreach (var user in adUsers)
            // {
            //     //admin.20090124,campussvcs_lencnf.rm DOM_qualys.scn dsa_ecw.gst neuro_neurolaptop.rm cas_wh-adminoffice.r sop_kashubalab.inst
            //     if (user.SamAccountName.Contains("."))
            //     {
            //         listIgnoreFuture.Add(user.SamAccountName);
            //         Console.WriteLine(user.SamAccountName);
            //     }


            //     var userAccount = await _dataAccessService.GetUserAccount(user.SamAccountName);
            //     if (userAccount != null && user.ProxyAddresses != null && (userAccount.Aliases is null || !userAccount.Aliases.Any()))
            //     {
            //  userAccount.Aliases = user.ProxyAddresses
            //                    .Where(c => c.StartsWith("SMTP:") || c.StartsWith("smtp:"))
            //                    .Where(c => !ignoreProxyAddressesEndWith.Any(d => c.EndsWith(d)))
            //                    .Select(c =>
            //                            new Alias
            //                            {
            //                                AliasEmail = c[5..],
            //                                IsPrimary = c.StartsWith("SMTP:")
            //                            }).ToList();


            //         await _dataAccessService.UpdateUserAccount(userAccount);
            //     }


            // }


            break;

        }
    }

    async Task<List<UserDetail>> GetAdUsers()
    {
        const string filePath = $"f:\\temp\\adusers20240319.json";
        var adUsers = _fileService.ReadAdResultsFromFile(filePath);

        if (adUsers is null)
        {
            adUsers = new List<UNC.ActiveDirectory.Common.Entities.Users.UserDetail>();


            var criteria = new UNC.ActiveDirectory.Common.Criteria.Users.UsersCriteria
            {
                PageSize = 1000,
                Index = 0
            };

            var adRequest = new PagedResponse<UNC.ActiveDirectory.Common.Entities.Users.UserDetail>();

            do
            {
                adRequest = await _activeDirectoryService.GetAdUsers(criteria);

                adUsers.AddRange(adRequest.Entities);
                criteria.Index++;


            } while (adRequest.Entities.Count() > 0);

            adUsers = adUsers.Distinct().ToList();

            _fileService.SaveAdResultsToFile(adUsers, filePath);
        }
        return adUsers;

    }
}

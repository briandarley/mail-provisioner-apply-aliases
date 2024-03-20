using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using mail_provisioner_apply_aliases.Interfaces;
using mail_provisioner_apply_aliases.Pocos;
using mail_provisioner_apply_aliases.Services;
using mail_provisioner_apply_aliases.WorkTasks;


public static class DependencyInjection
{

    public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddHttpClient();
        ConfigurHttpClient(services, configuration);
        services.AddSingleton<ITokenService, TokenService>();
        services.AddTransient<BearerTokenHandler>();

        services.AddTransient<IFileService, FileService>();
        services.AddTransient<IDataAccessService, DataAccessService>();
        services.AddTransient<IActiveDirectoryService, ActiveDirectoryService>();
        //Other services       



        services.AddSingleton<IWorkerTask, WorkerTask>();
        services.AddHostedService<Worker>();


    }

    private static void ConfigurHttpClient(IServiceCollection services, IConfiguration configuration)
    {

        //TODO when setting up new APP
        //Initialize user secrets using the below command
        //dotnet user-secrets init
        //dotnet user-secrets set "LOCAL_IDP_CLIENT_ID" "local_idp_client_id"
        //dotnet user-secrets set "LOCAL_IDP_CLIENT_SECRET" "local_idp_client_secret"

        //To list curent secrets use the below command
        //dotnet user-secrets list


        var idpConfigurations = configuration.GetSection("IdpConfigurations").Get<List<IdpConfiguration>>();
        if (idpConfigurations is null)
        {
            Console.WriteLine("IdpConfigurations is null");
            throw new ArgumentNullException("IdpConfigurations is null");
        }
        //Retrieve configuration from user secrets
        idpConfigurations.Single(c => c.Name == "LOCAL_IDP").ClientId = configuration["LOCAL_IDP:CLIENT_ID"] ?? Environment.GetEnvironmentVariable("LOCAL_IDP:CLIENT_ID");
        idpConfigurations.Single(c => c.Name == "LOCAL_IDP").ClientSecret = configuration["LOCAL_IDP:CLIENT_SECRET"] ?? Environment.GetEnvironmentVariable("LOCAL_IDP:CLIENT_SECRET");

        idpConfigurations.Single(c => c.Name == "UAT_IDP").ClientId = configuration["UAT_IDP:CLIENT_ID"] ?? Environment.GetEnvironmentVariable("UAT_IDP:CLIENT_ID");
        idpConfigurations.Single(c => c.Name == "UAT_IDP").ClientSecret = configuration["UAT_IDP:CLIENT_SECRET"] ?? Environment.GetEnvironmentVariable("UAT_IDP:CLIENT_ID"); ;



        services.AddHttpClient("LOCAL_AD", client =>
        {
            client.BaseAddress = new Uri("https://localhost:5503/v1/");
            client.Timeout = new TimeSpan(0, 5, 0);

        })
        .AddHttpMessageHandler(provider => new BearerTokenHandler(new TokenService("LOCAL_IDP", idpConfigurations)))
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        });

        services.AddHttpClient("LOCAL_DATA", client =>
        {
            client.BaseAddress = new Uri("https://localhost:5501/v1/");
            client.Timeout = new TimeSpan(0, 5, 0);
        })
        .AddHttpMessageHandler(provider => new BearerTokenHandler(new TokenService("LOCAL_IDP", idpConfigurations)))
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        });

        services.AddHttpClient("UAT_DATA", client =>
         {
             client.BaseAddress = new Uri("https://its-idmuat-web.ad.unc.edu/services/data.api/v1/");
             client.Timeout = new TimeSpan(0, 5, 0);
         })
         .AddHttpMessageHandler(provider => new BearerTokenHandler(new TokenService("UAT_IDP", idpConfigurations)))
         .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
         {
             ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
         });

        services.AddHttpClient("UAT_MASSMAIL_DATA", client =>
         {
             client.BaseAddress = new Uri("https://its-idmuat-web.ad.unc.edu/services/dal.massmail/v1/");
             client.Timeout = new TimeSpan(0, 5, 0);
         })
         .AddHttpMessageHandler(provider => new BearerTokenHandler(new TokenService("UAT_IDP", idpConfigurations)))
         .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
         {
             ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
         });


        services.AddHttpClient("UAT_SMTP", client =>
        {
            client.BaseAddress = new Uri("https://its-idmuat-web.ad.unc.edu/services/smtp.api/v1/");
            client.Timeout = new TimeSpan(0, 5, 0);
        })
        .AddHttpMessageHandler(provider => new BearerTokenHandler(new TokenService("UAT_IDP", idpConfigurations)))
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        });

    }





}
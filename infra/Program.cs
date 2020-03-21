using System.Collections.Generic;
using System.Threading.Tasks;
using Pulumi;
using Pulumi.Azure.AppService;
using Pulumi.Azure.AppService.Inputs;
using Pulumi.Azure.Core;
using Pulumi.Azure.Dns;
using Pulumi.Azure.Dns.Inputs;

internal class Program
{
    private const string DnsId =
        "/subscriptions/6bd72948-e492-4ae3-8bee-dd49284a07ac/resourceGroups/emoos/providers/Microsoft.Network/dnszones/emoos.solutions";

    private static Task<int> Main()
    {
        return Deployment.RunAsync(Run);
    }

    private static IDictionary<string, object> Run()
    {
        var resourceGroup = new ResourceGroup("emoos-dev", new ResourceGroupArgs {Location = "WestEurope"});
        Output<string> resourceGroupName = resourceGroup.Name;

        var planSkuArgs = new PlanSkuArgs {Tier = "Basic", Size = "B1"};
        var plan = new Plan("DevAppServicePlan",
                            new PlanArgs {ResourceGroupName = resourceGroupName, Kind = "Linux", Sku = planSkuArgs, Reserved = true});

        var appSettings = new InputMap<string> {{"WEBSITES_ENABLE_APP_SERVICE_STORAGE", "false"}};

        var image = "sqeezy/emoos.solutions:latest";
        var siteConfig = new AppServiceSiteConfigArgs {AlwaysOn = false, LinuxFxVersion = $"DOCKER|{image}"};
        var appService = new AppService("DevAppService",
                                        new AppServiceArgs
                                        {
                                            ResourceGroupName = resourceGroupName,
                                            AppServicePlanId = plan.Id,
                                            AppSettings = appSettings,
                                            HttpsOnly = false,
                                            SiteConfig = siteConfig
                                        });

        var emoosDns = new Zone("emoos.solutions",
                                new ZoneArgs {ResourceGroupName = "emoos", Name = "emoos.solutions"},
                                new CustomResourceOptions {ImportId = DnsId, Protect = true});

        var txtRecord = new TxtRecord("@",
                                      new TxtRecordArgs
                                      {
                                          Name = "@",
                                          ResourceGroupName = emoosDns.ResourceGroupName,
                                          Ttl = 60,
                                          ZoneName = emoosDns.Name,
                                          Records = new InputList<TxtRecordRecordsArgs>
                                                    {
                                                        new TxtRecordRecordsArgs {Value = appService.DefaultSiteHostname}
                                                    }
                                      });

        var cname = new CNameRecord("dev",
                                    new CNameRecordArgs
                                    {
                                        Name = "dev",
                                        ResourceGroupName = emoosDns.ResourceGroupName,
                                        Ttl = 60,
                                        ZoneName = emoosDns.Name,
                                        Record = appService.DefaultSiteHostname
                                    });

        var hostNameBinding = new CustomHostnameBinding("dev.emoos.solutions",
                                                        new CustomHostnameBindingArgs
                                                        {
                                                            AppServiceName = appService.Name,
                                                            Hostname = "dev.emoos.solutions",
                                                            ResourceGroupName = resourceGroupName
                                                            // SslState = "SniEnabled",
                                                            // Thumbprint = "19A0220DE45552EE931E0959B10F6DDDAD5F946B"
                                                        });

        return new Dictionary<string, object?>
               {
                   {"default route", appService.DefaultSiteHostname}, {"resource group", resourceGroup.Name}
                   // {"domain binding", hostNameBinding.Hostname}
               };
    }
}

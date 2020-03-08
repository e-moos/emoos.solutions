using System.Collections.Generic;
using System.Threading.Tasks;
using Pulumi;
using Pulumi.Azure.AppService;
using Pulumi.Azure.AppService.Inputs;
using Pulumi.Azure.Dns;
using Pulumi.Azure.Dns.Inputs;

internal class Program
{
    private static Task<int> Main()
    {
        return Deployment.RunAsync(Run);
    }

    private static IDictionary<string, object> Run()
    {
        var resourceGroupName = "emoos";

        var planSkuArgs = new PlanSkuArgs {Tier = "Basic", Size = "B1"};
        var plan = new Plan("DevAppServicePlan",
                            new PlanArgs {ResourceGroupName = resourceGroupName, Kind = "Linux", Sku = planSkuArgs, Reserved = true});

        var appSettings = new InputMap<string> {{"WEBSITES_ENABLE_APP_SERVICE_STORAGE", "false"}};

        var image = "sqeezy/emoos.solutions:1577994214";
        var siteConfig = new AppServiceSiteConfigArgs {AlwaysOn = false, LinuxFxVersion = $"DOCKER|{image}"};
        var appService = new AppService("DevAppService",
                                        new AppServiceArgs
                                        {
                                            ResourceGroupName = resourceGroupName,
                                            AppServicePlanId = plan.Id,
                                            AppSettings = appSettings,
                                            HttpsOnly = true,
                                            SiteConfig = siteConfig
                                        });

        var emoosDns = new Zone("emoos.solutions", new ZoneArgs {Name = "emoos.solutions", ResourceGroupName = resourceGroupName});

        var cname = new CNameRecord("dev",
                                    new CNameRecordArgs
                                    {
                                        Name = "dev",
                                        ResourceGroupName = resourceGroupName,
                                        Ttl = 60,
                                        ZoneName = emoosDns.Name,
                                        Record = appService.DefaultSiteHostname
                                    });

        var txtRecord = new TxtRecord("@",
                                      new TxtRecordArgs
                                      {
                                          Name = "@",
                                          ResourceGroupName = resourceGroupName,
                                          Ttl = 60,
                                          ZoneName = emoosDns.Name,
                                          Records = new InputList<TxtRecordRecordsArgs>
                                                    {
                                                        new TxtRecordRecordsArgs {Value = appService.DefaultSiteHostname}
                                                    }
                                      });

        var hostNameBinding = new CustomHostnameBinding("dev.emoos.solutions",
                                                        new CustomHostnameBindingArgs
                                                        {
                                                            AppServiceName = appService.Name,
                                                            Hostname = "dev.emoos.solutions",
                                                            ResourceGroupName = resourceGroupName
                                                        });

        return new Dictionary<string, object?> {{"default route", appService.DefaultSiteHostname}, {"domain binding", hostNameBinding.Hostname}};
    }
}

module Program

open Pulumi.Azure.AppService
open Pulumi.Azure.AppService.Inputs
open Pulumi.Azure.Core
open Pulumi.FSharp
open FSharp.Core

let infra () =
    // Create an Azure Resource Group
    let resourceGroup = ResourceGroup "Emoos-Group-Dev"

    let sku = PlanSkuArgs
                (Tier = input "Basic",
                 Size = input "B1")
    let plan =
        Plan("EMoos-AppPlan-Dev",
                PlanArgs
                    (ResourceGroupName = io resourceGroup.Name,
                     Kind = input "Linux",
                     Sku = input sku,
                     Reserved = input true
                     ))

    let image = "sqeezy/emoos.solutions:latest"
        
    let appSettings = [
                             "WEBSITES_ENABLE_APP_SERVICE_STORAGE", input "false"
                      ]

    let siteConfig =
        AppServiceSiteConfigArgs(AlwaysOn = input false,
                                 LinuxFxVersion = input (sprintf "DOCKER|%s" image))

    let app =
        AppService("EMoos-App-Dev",
                    AppServiceArgs
                        (
                            ResourceGroupName = io resourceGroup.Name,
                            AppServicePlanId = io plan.Id,
                            AppSettings = inputMap appSettings,
                            HttpsOnly = input true,
                            SiteConfig = input siteConfig
                        ))

    // Export the connection string for the storage account
    [
        ("name", app.Name :> obj);
        ("url" , app.DefaultSiteHostname :> obj);
    ]
    |> dict

[<EntryPoint>]
let main _ =
  Deployment.run infra

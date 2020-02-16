namespace EMoos.Server

open Bolero
open Bolero.Remoting.Server
open Bolero.Templating.Server
open EMoos
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.StaticFiles
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.FileProviders
open System.IO

type Startup() =

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    member this.ConfigureServices(services: IServiceCollection) =
        services.AddMvc().AddRazorRuntimeCompilation() |> ignore
        services.AddServerSideBlazor() |> ignore
        services
            .AddAuthorization()
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie()
                .Services
            .AddRemoting<PictureService>()
#if DEBUG
            .AddHotReload(templateDir = __SOURCE_DIRECTORY__ + "/../EMoos.Client")
#endif
        |> ignore

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member this.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        let provider = FileExtensionContentTypeProvider()
        provider.Mappings.[".fsx"] <- "text/x-fsharp"
        app
            .UseAuthentication()
            .UseRemoting()
            .UseStaticFiles(
            StaticFileOptions(
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(
                        Path.GetDirectoryName(Directory.GetCurrentDirectory()),
                        "EMoos.Client", "wwwroot")),
                ContentTypeProvider = provider))
            .UseRouting()
            .UseClientSideBlazorFiles<Client.Main.MyApp>()
            .UseEndpoints(fun endpoints ->
#if DEBUG
                endpoints.UseHotReload()
#endif
                endpoints.MapBlazorHub() |> ignore
                endpoints.MapFallbackToPage("/_Host") |> ignore)
        |> ignore

module Program =

    [<EntryPoint>]
    let main args =
        WebHost
            .CreateDefaultBuilder(args)
            .UseStartup<Startup>()
            .Build()
            .Run()
        0

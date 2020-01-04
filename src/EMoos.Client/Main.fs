module EMoos.Client.Main

open System
open Elmish
open Bolero
open Bolero.Html
open Bolero.Json
open Bolero.Remoting
open Bolero.Remoting.Client
open Bolero.Templating.Client
open SixLabors.ImageSharp
open System.IO

/// Routing endpoints definition.
type Page =
    | [<EndPoint "/">] Home

/// The Elmish application's model.
type Model =
    {
        page: Page
        error: string option
        icon: Image option
    }

let initModel =
    {
        page = Home
        error = None
        icon = None
    }

/// Remote service definition.
type PictureService =
    {
        getIcon: unit -> Async<byte[]>
    }

    interface IRemoteService with
        member this.BasePath = "/books"

/// The Elmish application's update messages.
type Message =
    | SetPage of Page
    | GetPictures
    | RecvPictures of byte[]
    | Error of exn
    | ClearError

let update remote message model =
    match message with
    | SetPage page ->
        { model with page = page }, Cmd.none
    | GetPictures ->
        model, Cmd.ofAsync remote.getIcon () RecvPictures Error
    | RecvPictures image -> 
        {model with icon = Image.Load(image) :> Image |> Some}, Cmd.ofMsg (SetPage Home)
    | Error RemoteUnauthorizedException ->
        { model with error = Some "You have been logged out."}, Cmd.none
    | Error exn ->
        { model with error = Some exn.Message }, Cmd.none
    | ClearError ->
        { model with error = None }, Cmd.none

/// Connects the routing system to the Elmish application.
let router = Router.infer SetPage (fun model -> model.page)

type Main = Template<"wwwroot/main.html">

let homePage model dispatch =
    Main.Home().Elt()

let menuItem (model: Model) (page: Page) (text: string) =
    Main.MenuItem()
        .Active(if model.page = page then "is-active" else "")
        .Url(router.Link page)
        .Text(text)
        .Elt()

let view model dispatch =
    Main()
        .Menu(concat [
            menuItem model Home "Home"
        ])
        .Body(
            cond model.page <| function
            | Home -> homePage model dispatch
        )
        .Error(
            cond model.error <| function
            | None -> empty
            | Some err ->
                Main.ErrorNotification()
                    .Text(err)
                    .Hide(fun _ -> dispatch ClearError)
                    .Elt()
        )
        .Elt()

type MyApp() =
    inherit ProgramComponent<Model, Message>()

    override this.Program =
        let bookService = this.Remote<PictureService>()
        let update = update bookService
        Program.mkProgram (fun _ -> initModel, Cmd.ofMsg GetPictures) update view
        |> Program.withRouter router
#if DEBUG
        |> Program.withHotReload
#endif

namespace EMoos.Server

open System
open System.IO
open Microsoft.AspNetCore.Hosting
open Bolero
open Bolero.Remoting
open Bolero.Remoting.Server
open EMoos
open SixLabors.ImageSharp
open SixLabors.ImageSharp.PixelFormats

type PictureService(ctx: IRemoteContext, env: IWebHostEnvironment) =
    inherit RemoteHandler<Client.Main.PictureService>()

    override this.Handler =
        {
            getIcon = fun () -> async {
                use image = new Image<Rgba32>(32, 32)
                use outStream = new MemoryStream()
                image.SaveAsJpeg(outStream)
                return outStream.ToArray()
            }
        }

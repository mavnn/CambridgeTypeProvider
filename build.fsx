open System.Diagnostics

let sh cmd args =
  printfn "running: %s %s" cmd args
  use p = new Process()
  p.StartInfo.FileName <- cmd
  p.StartInfo.Arguments <- args
  p.Start() |> ignore
  p.WaitForExit()

let pt = "paket-files/fsprojects/FSharp.TypeProviders.StarterPack/src/ProvidedTypes.fs"
let em = "paket-files/mavnn/EmParsec/EmParsec.fsx"

let compile files refs =
  let args =
    [ files |> List.map (fun f -> sprintf "\"%s\"" f)
      refs |> List.map (fun r -> sprintf "\"-r:%s\"" r)
      ["--target:library"] ]
    |> List.concat
    |> String.concat " "
  sh "fsharpc" args

let compile1 () =
  compile [pt;"1_minimal.fsx"] []

let compile2 () =
  compile [pt;"2_static_type.fsx"] []

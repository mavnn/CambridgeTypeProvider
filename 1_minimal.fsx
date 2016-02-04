#if INTERACTIVE
#load "paket-files/fsprojects/FSharp.TypeProviders.StarterPack/src/ProvidedTypes.fs"
#else
module ``F# |> Cambridge``
#endif

open FSharp.Core.CompilerServices
open ProviderImplementation.ProvidedTypes

[<TypeProvider>]
type CambridgeProvider() =
  inherit TypeProviderForNamespaces()

[<assembly:TypeProviderAssembly>]
do()

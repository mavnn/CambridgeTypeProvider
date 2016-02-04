#if INTERACTIVE
#load "paket-files/fsprojects/FSharp.TypeProviders.StarterPack/src/ProvidedTypes.fs"
#else
module ``F# |> Cambridge``
#endif

open System.Reflection
open FSharp.Core.CompilerServices
open ProviderImplementation.ProvidedTypes

[<TypeProvider>]
type CambridgeProvider() as this =
  inherit TypeProviderForNamespaces()

  let ns = "F# |> Cambridge"
  let asm = Assembly.GetExecutingAssembly()

  let myProp =
    ProvidedProperty(
      "StaticProperty",
      typeof<string>,
      IsStatic = true,
      GetterCode = fun _ -> <@@ "Hello world" @@>)

  let myType =
    ProvidedTypeDefinition(asm, ns, "StaticType", Some typeof<obj>)

  do
    myType.AddMember myProp
    this.AddNamespace(ns, [myType])

[<assembly:TypeProviderAssembly>]
do()

#if INTERACTIVE
;;
#load "build.fsx"
compile2 ()
;;
#r "2_static_type.dll"
open ``F# |> Cambridge``

StaticType.StaticProperty

#endif

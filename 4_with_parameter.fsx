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

  let createType typeName (parameters : obj []) =
    let aString = parameters.[0] :?> string
    let myProp =
      ProvidedProperty(
        "StaticProperty",
        typeof<string>,
        IsStatic = true,
        GetterCode = fun _ -> <@@ aString @@>)

    let myType =
      ProvidedTypeDefinition(asm, ns, typeName, Some typeof<obj>)
    myType.AddMember myProp
    myType

  let provider =
    ProvidedTypeDefinition(asm, ns, "ParaProvider", Some typeof<obj>)
  let parameters =
    [ProvidedStaticParameter("AString", typeof<string>)]

  do
    provider.DefineStaticParameters(parameters, createType)
    this.AddNamespace(ns, [provider])

[<assembly:TypeProviderAssembly>]
do()

#if INTERACTIVE
;;
#load "build.fsx"
compile4 ()
;;
#r "4_with_parameter.dll"
open ``F# |> Cambridge``

type ProvidedTypeWithParameter = ParaProvider<"A string!">
ProvidedTypeWithParameter.StaticProperty

#endif

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
    let myStaticProp =
      ProvidedProperty(
        "StaticProperty",
        typeof<string>,
        IsStatic = true,
        GetterCode = fun _ -> <@@ aString @@>)

    let myInstanceProp =
      ProvidedProperty(
        "InstanceProperty",
        typeof<string>,
        IsStatic = false,
        GetterCode = fun args -> <@@ unbox<string> (%%args.[0]:obj) @@>)

    let myMethod =
      ProvidedMethod(
        "InstanceMethod",
        [ProvidedParameter("Prefix", typeof<string>)],
        typeof<string>,
        IsStaticMethod = false,
        InvokeCode = fun args -> <@@ (%%args.[1]:string) + unbox<string> (%%args.[0]:obj) @@>)

    let myCstor =
      ProvidedConstructor(
        [ProvidedParameter("InstanceString", typeof<string>)],
        InvokeCode = fun args -> <@@ (%%args.[0]:string) @@>)

    let myType =
      ProvidedTypeDefinition(asm, ns, typeName, Some typeof<obj>)

    myType.AddMember myStaticProp
    myType.AddMember myInstanceProp
    myType.AddMember myMethod
    myType.AddMember myCstor
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
compile5 ()
;;
#r "5_classes.dll"
open ``F# |> Cambridge``

type ProvidedTypeWithParameter = ParaProvider<"A string!">
ProvidedTypeWithParameter.StaticProperty

let providedType = ProvidedTypeWithParameter("Instance string")
providedType.InstanceProperty
providedType.InstanceMethod("Mr. ")
#endif

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

  let createMethod methodName (parameters : obj []) =
    let numberOfInts = parameters.[0] :?> int
    let stringOrIntReturn = parameters.[1] :?> string
    let methodParameters =
      [for i in 1..numberOfInts ->
        ProvidedParameter(sprintf "%d" i, typeof<int>)]
    match stringOrIntReturn with
    | "string" ->
      ProvidedMethod(methodName, methodParameters, typeof<string>)
    | "int" ->
      ProvidedMethod(methodName, methodParameters, typeof<int>)
    | returnName ->
      failwithf "Return type can only be string or int"

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

    // Note: no invoke code
    let myDependentMethod =
      ProvidedMethod(
        "Dependent",
        [],
        typeof<obj>,
        IsStaticMethod = false)

    let dependencyParameters =
      [ProvidedStaticParameter("NumberOfInts", typeof<int>)
       ProvidedStaticParameter("StringOrIntReturn", typeof<string>)]

    let define (container : ProvidedTypeDefinition) name (ps : obj []) =
      let numberOfInts = ps.[0] :?> int
      let stringOrInt = ps.[1] :?> string
      let inputs =
        [for i in 1..numberOfInts -> ProvidedParameter(string i, typeof<int>)]
      let sumCode exprs =
        List.fold (fun state next -> <@@ (%%state:int) + (%%next:int) @@>) <@@ 0 @@> (exprs |> List.skip 1)
      let convertToString expr =
        <@@ string (%%expr:int) @@>
      let invokeCode, returnType =
        match stringOrInt.ToLowerInvariant() with
        | "int" -> sumCode, typeof<int>
        | "string" -> sumCode >> convertToString, typeof<string>
        | _ -> failwithf "Unknown return type for method '%s', should be int or string" stringOrInt
      let actualMethod = ProvidedMethod(name, inputs, returnType, InvokeCode = invokeCode)
      container.AddMember actualMethod
      actualMethod

    let myType =
      ProvidedTypeDefinition(asm, ns, typeName, Some typeof<obj>)

    myDependentMethod.DefineStaticParameters(dependencyParameters, define myType)

    let myCstor =
      ProvidedConstructor(
        [ProvidedParameter("InstanceString", typeof<string>)],
        InvokeCode = fun args -> <@@ (%%args.[0]:string) @@>)

    myType.AddMember myStaticProp
    myType.AddMember myInstanceProp
    myType.AddMember myDependentMethod
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
compile6 ()
;;
#r "6_dependent_functions.dll"
open ``F# |> Cambridge``

type ProvidedTypeWithParameter = ParaProvider<"A string!">
ProvidedTypeWithParameter.StaticProperty

let providedType = ProvidedTypeWithParameter("Instance string")
providedType.InstanceProperty
providedType.Dependent<5, "string">(1, 2, 3, 4, 5)
providedType.Dependent<1, "int">(110)
providedType.Dependent<2, "int">(10, 10)
#endif

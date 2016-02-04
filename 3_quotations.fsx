open FSharp.Quotations

let add x y = x + y

let adding = <@@ add 1 2 @@>

let addX x =
  <@@ add %%x @@>

addX <@@ 10 @@>

let literalNotation =
  <@@ fun x -> System.Math.Cos 5. * x @@>

let classNotation =
  let var = Var("x", typeof<float>)
  Expr.Lambda(
    var,
    Expr.Call(typeof<System.Math>.GetMethod("Cos"), [Expr.Var(var)]))

let literalMember =
  <@@ fun (g : System.Guid) -> g.ToByteArray() @@>

let classMember =
  let var = Var("guid", typeof<System.Guid>)
  Expr.Lambda(
    var,
    Expr.Call(Expr.Var(var), typeof<System.Guid>.GetMethod("ToByteArray"), []))

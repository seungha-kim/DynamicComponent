namespace Formula.ValueRepresentation

open Formula.Errors

type FormulaValue =
    | NumberValue of float32
    | TextValue of string
    | NullValue

    member this.asFloat =
        match this with
        | NumberValue f -> Ok f
        | _ -> Error InvalidCasting
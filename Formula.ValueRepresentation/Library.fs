namespace Formula.ValueRepresentation

type FormulaValue =
    | NumberValue of float32
    | TextValue of string
    | NullValue

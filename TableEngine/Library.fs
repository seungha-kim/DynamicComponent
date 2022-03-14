namespace TableEngine

open System.Collections.Generic
open Formula.AST
open Formula.Interface
open ObservableTable.Engine
open Formula.Parsing
open Formula.Evaluation
open TableEngine

[<Struct>]
type TableId = { TableID: string }

[<Struct>]
type PropertyDescriptor =
    { TableID: TableId
      PropertyName: string }

type TableScript(tableId: TableId, name: string) =
    member this.TableId = tableId
    member this.Name = name

type ParsingContext() =
    interface IParsingContext with


type PropertyInvalidateDelegate = delegate of PropertyDescriptor -> unit
type TableInvalidateDelegate = delegate of TableId -> unit

type TableScriptManager() =
    
    
//x    PropertyInvalidateDelegate
//x    TableInvalidateDelegate
//x    _relationManager:RelationManager
//x    _parser:IFormulaParser
//x    _parsingContext:ParsingContext
//x    _propertyExpressions:Dictionary<PropertyDescriptor,Expression>
//x    _scripts:Dictionary<TableId,TableScript>
//x    TableScriptManager(RelationManager relationManager) -> 이렇게 안 하기로
//    GetTableScript(TableId id):TableScript
//    GetChildren(TableId id):IEnumerable<TableScript>
//    OnPropertyInvalidated:PropertyInvalidateDelegate
//    OnTableInvalidated:TableInvalidateDelegate
//    CreateTableScript(TableId id, string name):TableScript
//    RemoveTableScript(TableId id):void
//    HandlePropertyFormulaUpdate(TableScript sender, string propertyName):void
//    ParsingContext
    let parser = createParser ()
    let parsingContext = ParsingContext()

    let propertyExpressions =
        Dictionary<PropertyDescriptor, Expression>()

    let scripts = Dictionary<TableId, TableScript>()

    member this.GetTableScript(tableId, name) =
        let mutable script
        scripts.TryGetValue()

module TTT =
    let ts = TableScript({ TableID = "" }, "")

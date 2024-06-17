/*
 * EIGERLANG INTERPRETER
 * WRITTEN BY VARDAN PETROSYAN
*/

using EigerLang.Errors;
using EigerLang.Execution.BuiltInTypes;
using EigerLang.Parsing;
using Microsoft.CSharp.RuntimeBinder;
using Array = EigerLang.Execution.BuiltInTypes.Array;

using Boolean = EigerLang.Execution.BuiltInTypes.Boolean;

namespace EigerLang.Execution;

class Interpreter
{
    // add Built-in functions
    public static BuiltInFunctions.EmitFunction emitFunction = new();
    public static BuiltInFunctions.EmitlnFunction emitlnFunction = new();
    public static BuiltInFunctions.InFunction inFunction = new();
    public static BuiltInFunctions.IncharFunction incharFunction = new();
    public static BuiltInFunctions.ClsFunction clsFunction = new();

    // global symbol table
    public static Dictionary<string, Value> globalSymbolTable = new() {
        {"emit",emitFunction},
        {"emitln",emitlnFunction},
        {"in",inFunction},
        {"inchar",incharFunction},
        {"cls",clsFunction}
    };

    // function for visiting a node
    public static (bool, Value) VisitNode(ASTNode node, Dictionary<string, Value> symbolTable)
    {
        switch (node.type)
        {
            case NodeType.Block: return VisitBlockNode(node, symbolTable);
            case NodeType.If: return VisitIfNode(node, symbolTable);
            case NodeType.ForTo: return VisitForToNode(node, symbolTable);
            case NodeType.While: return VisitWhileNode(node, symbolTable);
            case NodeType.FuncCall: return VisitFuncCallNode(node, symbolTable);
            case NodeType.FuncDef: return VisitFuncDefNode(node, symbolTable);
            case NodeType.Class: return VisitClassNode(node, symbolTable);
            case NodeType.Return: return VisitReturnNode(node, symbolTable);
            case NodeType.BinOp: return VisitBinOpNode(node, symbolTable);
            case NodeType.Literal: return VisitLiteralNode(node, symbolTable);
            case NodeType.Identifier: return VisitIdentifierNode(node, symbolTable);
            case NodeType.Array: return VisitArrayNode(node, symbolTable);
            case NodeType.ElementAccess: return VisitElementAccessNode(node, symbolTable);
            case NodeType.AttrAccess: return VisitAttrAccessNode(node, symbolTable);
            default:
                break;
        }
        return (false, new Nix(node.filename, node.line, node.pos));
    }

    // visit return node
    static (bool, Value) VisitReturnNode(ASTNode node, Dictionary<string, Value> symbolTable)
    {
        // ReturnNode structure
        // ReturnNode
        // -- value

        return (true, VisitNode(node.children[0], symbolTable).Item2);
    }

    // visit block node 
    public static (bool, Value) VisitBlockNode(ASTNode node, Dictionary<string, Value> symbolTable, Dictionary<string, Value>? parentSymbolTable = null)
    {
        // BlockNode structure
        // BlockNode
        // -- statement1
        // -- statement2
        // -- ...

        // loop through each statement
        foreach (var child in node.children)
        {
            // visit each statement
            (bool didReturn, Value value) = VisitNode(child, symbolTable);

            // Sync local and parent symbol tables
            // basically syncing the variables in this scope with the ones in the parent scope
            // to enable changing global variables from a child scope (like an if statement or a function)
            // this is not a very optimized solution but I can't come up with a better one
            if (parentSymbolTable != null)
                foreach (var symbol in parentSymbolTable.Keys)
                {
                    if (symbolTable.ContainsKey(symbol))
                    {
                        parentSymbolTable[symbol] = symbolTable[symbol];
                    }
                }

            // if the current statement returned a value, return the value and stop block execution
            if (didReturn)
            {
                return (true, value);
            }
        }
        // return nothing
        return (false, new Nix(node.filename, node.line, node.pos));
    }

    // visit for..to node
    static (bool, Value) VisitForToNode(ASTNode node, Dictionary<string, Value> symbolTable)
    {
        // ForToNode structure
        // ForToNode
        // -- Identifier : <iterator variable name>
        // -- <expr> (iterator initial value)
        // -- <expr> (iterator to value)
        // -- Block
        // ---- statement1
        // ---- statement2
        // ---- ...

        Dictionary<string, Value> localSymbolTable = new(symbolTable);

        ASTNode toNode = node.children[2];

        ASTNode forBlock = node.children[3];

        double value = ((Number)VisitNode(node.children[1], symbolTable).Item2).value;
        double toValue = ((Number)VisitNode(toNode, symbolTable).Item2).value;

        int step = value < toValue ? 1 : -1;

        SetSymbol(localSymbolTable, node.children[0], new Number(node.filename, node.line, node.pos, value));

        while (value != toValue)
        {
            VisitBlockNode(forBlock, localSymbolTable, symbolTable);
            value += step;
            SetSymbol(localSymbolTable, node.children[0], new Number(node.filename, node.line, node.pos, value));
        }

        return (false, new Nix(node.filename, node.line, node.pos));
    }

    // visit while node
    static (bool, Value) VisitWhileNode(ASTNode node, Dictionary<string, Value> symbolTable)
    {
        // WhileNode structure
        // WhileNode
        // -- condition
        // -- whileBlock

        // visit Condition
        Boolean condition = (Boolean)VisitNode(node.children[0], symbolTable).Item2;
        while (Convert.ToBoolean(condition.value))
        {
            // visit the body of the loop
            VisitBlockNode(node.children[1], new(symbolTable), symbolTable);

            // update the condition
            condition = (Boolean)VisitNode(node.children[0], symbolTable).Item2;
        }
        // return nothing
        return (false, new Nix(node.filename, node.line, node.pos));
    }

    // visit if node
    static (bool, Value) VisitIfNode(ASTNode node, Dictionary<string, Value> symbolTable)
    {
        // IfNode structure
        // IfNode
        // -- condition
        // -- ifBlock
        // -- elseBlock

        // if the node has 3 children (condition,ifBlock,elseBlock), it has an else block
        // otherwise not
        bool hasElseBlock = node.children.Count == 3;

        // visit condition
        Boolean condition = (Boolean)VisitNode(node.children[0], symbolTable).Item2;

        // if the condition is true
        if (Convert.ToBoolean(condition.value))
        {
            // visit the if block (2nd child node)
            return VisitBlockNode(node.children[1], new(symbolTable), symbolTable);
        }
        // if the condition is false and it has an else block
        else if (hasElseBlock)
        {
            // visit the else block (3rd child node)
            return VisitBlockNode(node.children[2], new(symbolTable), symbolTable);
        }
        // if the condition is false and it does not have an else block
        else
        {
            // do nothing
            return (false, new Nix(node.filename, node.line, node.pos));
        }
    }

    // visit function call node
    static (bool, Value) VisitFuncCallNode(ASTNode node, Dictionary<string, Value> symbolTable)
    {
        // FuncCallNode structure:
        // FuncCallNode : <function name>
        // -- arg1
        // -- arg2
        // -- ...

        // if the symbol we're calling is a function (it might be something else, like a variable)
        if (GetSymbol(symbolTable, node) is BaseFunction) // BaseFunction is the class both custom and built-in functions extend from
        {
            List<Value> args = []; // prepare a list for args
            foreach (ASTNode child in node.children) //
            {
                Value? val = VisitNode(child, symbolTable).Item2 ?? throw new EigerError(node.filename, node.line, node.pos, Globals.ArgumentErrorStr,EigerError.ErrorType.ArgumentError);
                args.Add(val);
            }

            return ((BaseFunction)symbolTable[node.value]).Execute(args, node.line, node.pos, node.filename);
        }
        else if(GetSymbol(symbolTable, node) is Class) // we're calling a class constructor
        {
            List<Value> args = []; // prepare a list for args
            foreach (ASTNode child in node.children) //
            {
                Value? val = VisitNode(child, symbolTable).Item2 ?? throw new EigerError(node.filename, node.line, node.pos, Globals.ArgumentErrorStr, EigerError.ErrorType.ArgumentError);
                args.Add(val);
            }

            return ((Class)symbolTable[node.value]).Execute(args);
        }
        else
            throw new EigerError(node.filename, node.line, node.pos, $"{node.value} is not a function", EigerError.ErrorType.RuntimeError);
    }

    // Visit class definition node
    static (bool, Value) VisitClassNode(ASTNode node,  Dictionary<string, Value> symbolTable)
    {
        // ClassNode structure
        // ClassNode : <class name>
        // -- block
        // ---- statement1
        // ---- statement2
        // ---- ...

        Class classdef = new(node.filename, node.line, node.pos, node.value, symbolTable, node.children[0]);

        SetSymbol(symbolTable, node.value, classdef);

        return (false, classdef);
    }

    // Visit function definition node
    static (bool, Value) VisitFuncDefNode(ASTNode node, Dictionary<string, Value> symbolTable)
    {
        // FuncDefNode structure
        // FuncDefNode : <function name>
        // -- funcBody
        // -- argName1
        // -- argName2
        // -- ...

        // get function name
        string funcName = node.value ?? "";

        // get funciton body
        ASTNode root = node.children[0];

        // prepare list for argnames
        List<string> argnames = new();

        // loop through each arg (starting from 2nd child node
        for (int i = 1; i < node.children.Count; i++)
        {
            // assuming the node is guaranteed to be an identifier, we can get it's name and add to the name
            // this will be useful later when calling the function, we will add each given argument with that
            // name in the function's local scope
            argnames.Add(node.children[i].value);
        }

        // add the function to the current scope
        symbolTable[funcName] = new Function(node, funcName, argnames, root, symbolTable);

        // return nothing
        return (false, new Nix(node.filename, node.line, node.pos));
    }

    // visit binary operator node
    static (bool, Value) VisitBinOpNode(ASTNode node, Dictionary<string, Value> symbolTable)
    {
        // prepare a variable for return value
        Value retVal;

        Value leftSide = new Nix(node.filename, node.line, node.pos);

        if (node.value != "=")
            leftSide = VisitNode(node.children[0], symbolTable).Item2;

        Value rightSide = VisitNode(node.children[1], symbolTable).Item2;

        switch (node.value)
        {
            // check each available binary operator
            case "+": retVal = leftSide.AddedTo(rightSide); break;
            case "-": retVal = leftSide.SubbedBy(rightSide); break;
            case "*": retVal = leftSide.MultedBy(rightSide); break;
            case "/": retVal = leftSide.DivedBy(rightSide); break;
            case "?=": retVal = leftSide.ComparisonEqeq(rightSide); break;
            case "<": retVal = leftSide.ComparisonLT(rightSide); break;
            case ">": retVal = leftSide.ComparisonGT(rightSide); break;
            case "<=": retVal = leftSide.ComparisonLTE(rightSide); break;
            case ">=": retVal = leftSide.ComparisonGTE(rightSide); break;
            case "!=": retVal = leftSide.ComparisonNeqeq(rightSide); break;
            // assignment operator (assign a value to a variable and return it)
            case "=":
                {
                    SetSymbol(symbolTable, node.children[0], rightSide);
                    retVal = rightSide;
                }
                break;
            // compound assignment operators (change the variable's value and return the new value)
            case "+=":
                {
                    SetSymbol(symbolTable, node.children[0], GetSymbol(symbolTable, node.children[0]).AddedTo(rightSide));
                    retVal = rightSide;
                }
                break;
            case "-=":
                {
                    SetSymbol(symbolTable, node.children[0], GetSymbol(symbolTable, node.children[0]).SubbedBy(rightSide));
                    retVal = rightSide;
                }
                break;
            case "*=":
                {
                    SetSymbol(symbolTable, node.children[0], GetSymbol(symbolTable, node.children[0]).MultedBy(rightSide));
                    retVal = rightSide;
                }
                break;
            case "/=":
                {
                    SetSymbol(symbolTable, node.children[0], GetSymbol(symbolTable, node.children[0]).DivedBy(rightSide));
                    retVal = rightSide;
                }
                break;
            default: throw new EigerError(node.filename, node.line, node.pos, Globals.InvalidOperationStr, EigerError.ErrorType.InvalidOperationError);
        }
        return (false, retVal);
    }

    // visit literal node (pretty self-explanatory)
    static (bool, Value) VisitLiteralNode(ASTNode node, Dictionary<string, Value> symbolTable)
    {
        if (node.value is string && node.value == "true")
            return (false, new Boolean(node.filename, node.line, node.pos, true));
        else if (node.value is string && node.value == "false")
            return (false, new Boolean(node.filename, node.line, node.pos, false));
        else if (node.value is string && node.value == "nix")
            return (false, new Nix(node.filename, node.line, node.pos));

        return (false, Value.ToEigerValue(node.filename, node.line, node.pos, node.value));
    }

    // visit identifier node (pretty self-explanatory too)
    static (bool, Value) VisitIdentifierNode(ASTNode node, Dictionary<string, Value> symbolTable)
    {
        return (false, GetSymbol(symbolTable, node));
    }

    // visit array node
    static (bool, Value) VisitArrayNode(ASTNode node, Dictionary<string, Value> symbolTable)
    {
        // ArrayNode structure
        // ArrayNode
        // -- element1
        // -- element2
        // -- ...

        List<Value> list = [];
        foreach (ASTNode item in node.children)
        {
            list.Add(VisitNode(item, symbolTable).Item2);
        }
        return (false, new Array(node.filename, node.line, node.pos, [.. list]));
    }

    private static (bool,Value) VisitAttrAccessNode(ASTNode node, Dictionary<string, Value> symbolTable)
    {
        // AttrAccessNode structure
        // AttrAccessNode
        // -- left
        // -- right

        if (node.children[1].type == NodeType.FuncCall)
        {
            List<Value> args = []; // prepare a list for args
            foreach (ASTNode child in node.children[1].children) //
            {
                Value val = VisitNode(child, symbolTable).Item2 ?? throw new EigerError(node.filename, node.line, node.pos, Globals.ArgumentErrorStr, EigerError.ErrorType.ArgumentError);
                args.Add(val);
            }

            Value v = VisitNode(node.children[0], symbolTable).Item2.GetAttr(node.children[1]);
            if(v is BaseFunction f)
                return (false, f.Execute(args, node.line, node.pos, node.filename).Item2);
            else
               throw new EigerError(node.filename, node.line, node.pos, $"{node.children[1].value} is not a function", EigerError.ErrorType.RuntimeError);
        }
        else
            return (false, VisitNode(node.children[0], symbolTable).Item2.GetAttr(node.children[1]));
    }

    private static (bool, Value) VisitElementAccessNode(ASTNode node, Dictionary<string, Value> symbolTable)
    {
        // ElementAccessNode structure
        // ElementAccessNode
        // -- value
        // -- idx

        Value? iter = VisitNode(node.children[0], symbolTable).Item2;
        (bool, Value?) value = VisitNode(node.children[1], symbolTable);
        try
        {
            int idx = Convert.ToInt32(((Number)(value.Item2 ?? throw new EigerError(node.filename, node.line, node.pos, "Index is unknown",EigerError.ErrorType.ParserError))).value);
            return (false, iter.GetIndex(idx));
        }
        catch (IndexOutOfRangeException)
        {
            throw new EigerError(node.filename, node.line, node.pos, Globals.IndexErrorStr,EigerError.ErrorType.IndexError);
        }
    }

    // get symbol from a symboltable with error handling
    public static Value GetSymbol(Dictionary<string, Value> symbolTable, ASTNode key)
    {
        try
        {
            if (key.type == NodeType.Identifier || key.type == NodeType.FuncCall)
                return symbolTable[key.value];
            else if (key.type == NodeType.ElementAccess)
            {
                ASTNode listNode = key.children[0];
                ASTNode idxNode = key.children[1];

                if (listNode.type != NodeType.Identifier)
                    throw new EigerError(key.filename, key.line, key.pos, "Must be identifier", EigerError.ErrorType.RuntimeError);

                string listKey = key.children[0].value ?? throw new EigerError(key.filename, key.line, key.pos, "Identifier value not found",EigerError.ErrorType.ParserError);
                int idx = (int)((Number)(VisitNode(idxNode, symbolTable).Item2)).value;

                return symbolTable[listKey].GetIndex(idx);
            }
            else if(key.type == NodeType.AttrAccess)
            {
                ASTNode leftNode = key.children[0];
                ASTNode rightNode = key.children[1];
                return symbolTable[leftNode.value].GetAttr(rightNode);
            }
            else throw new EigerError(key.filename, key.line, key.pos, $"Invalid Node {key.type}", EigerError.ErrorType.ParserError);
        }
        catch (KeyNotFoundException)
        {
            throw new EigerError(key.filename, key.line, key.pos, $"Variable is undefined",EigerError.ErrorType.RuntimeError);
        }
    }

    public static void SetSymbol(Dictionary<string, Value> symbolTable, string key, Value value)
    {
        symbolTable[key] = value;
    }

    // set symbol from a symboltable with error handling
    public static void SetSymbol(Dictionary<string, Value> symbolTable, ASTNode key, Value value)
    {
        try
        {
            if (key.type == NodeType.ElementAccess)
            {
                ASTNode listNode = key.children[0];
                ASTNode idxNode = key.children[1];

                if (listNode.type != NodeType.Identifier)
                    throw new EigerError(key.filename, key.line, key.pos, "Assignee must be identifier",EigerError.ErrorType.RuntimeError);

                string listKey = key.children[0].value ?? throw new EigerError(key.filename, key.line, key.pos, "Identifier value not found",EigerError.ErrorType.ParserError);
                int idx = (int)((Number)(VisitNode(idxNode, symbolTable).Item2)).value;

                symbolTable[listKey].SetIndex(idx, value);
            }
            else if (key.type == NodeType.Identifier)
            {
                symbolTable[key.value] = value;
            }
            else if (key.type == NodeType.AttrAccess)
            {
                ASTNode leftNode = key.children[0];
                ASTNode rightNode = key.children[1];
                symbolTable[leftNode.value].SetAttr(rightNode,value);
            }
            else
            {
                throw new EigerError(key.filename, key.line, key.pos, "Assignee must be identifier",EigerError.ErrorType.RuntimeError);
            }
        }
        catch (KeyNotFoundException)
        {
            throw new EigerError(key.filename, key.line, key.pos, $"Variable is undefined",EigerError.ErrorType.RuntimeError);
        }
    }
}
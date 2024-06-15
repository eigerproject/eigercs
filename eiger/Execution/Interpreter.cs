/*
 * EIGERLANG INTERPRETER
 * WRITTEN BY VARDAN PETROSYAN
*/

using EigerLang.Errors;
using EigerLang.Execution.BuiltInTypes;
using EigerLang.Parsing;
using Microsoft.CSharp.RuntimeBinder;
using System.Text;

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
    public static Dictionary<string, Value?> globalSymbolTable = new() {
        {"emit",emitFunction},
        {"emitln",emitlnFunction},
        {"in",inFunction},
        {"inchar",incharFunction},
        {"cls",clsFunction}
    };

    // function for visiting a node
    public static (bool, Value?) VisitNode(ASTNode node, Dictionary<string, Value> symbolTable)
    {
        switch (node.type)
        {
            case NodeType.Block: return VisitBlockNode(node, symbolTable);
            case NodeType.If: return VisitIfNode(node, symbolTable);
            case NodeType.ForTo: return VisitForToNode(node, symbolTable);
            case NodeType.While: return VisitWhileNode(node, symbolTable);
            case NodeType.FuncCall: return VisitFuncCallNode(node, symbolTable);
            case NodeType.FuncDef: return VisitFuncDefNode(node, symbolTable);
            case NodeType.Return: return VisitReturnNode(node, symbolTable);
            case NodeType.BinOp: return VisitBinOpNode(node, symbolTable);
            case NodeType.Literal: return VisitLiteralNode(node, symbolTable);
            case NodeType.Identifier: return VisitIdentifierNode(node, symbolTable);
            case NodeType.Array: return VisitArrayNode(node, symbolTable);
            case NodeType.ElementAccess: return VisitElementAccessNode(node, symbolTable);
            default:
                break;
        }
        return (false, null);
    }

    // visit return node
    static (bool, Value?) VisitReturnNode(ASTNode node, Dictionary<string, Value> symbolTable)
    {
        // ReturnNode structure
        // ReturnNode
        // -- value

        return (true, VisitNode(node.children[0], symbolTable).Item2);
    }

    // visit block node 
    public static (bool, Value?) VisitBlockNode(ASTNode node, Dictionary<string, Value> symbolTable, Dictionary<string, Value>? parentSymbolTable = null)
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
        return (false, null);
    }

    // visit for..to node
    static (bool, Value?) VisitForToNode(ASTNode node, Dictionary<string, Value> symbolTable)
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

        string iteratorName = node.children[0].value ?? "";

        ASTNode toNode = node.children[2];

        ASTNode forBlock = node.children[3];

        double value = ((Number)VisitNode(node.children[1], symbolTable).Item2).value;
        double toValue = ((Number)VisitNode(toNode, symbolTable).Item2).value;

        int step = value < toValue ? 1 : -1;

        SetSymbol(localSymbolTable, iteratorName, new Number(node.filename,node.line,node.pos, value));

        while (value != toValue)
        {
            VisitBlockNode(forBlock, localSymbolTable, symbolTable);
            value += step;
            SetSymbol(localSymbolTable, iteratorName, new Number(node.filename, node.line, node.pos, value));
        }

        return (false, null);
    }

    // visit while node
    static (bool, Value?) VisitWhileNode(ASTNode node, Dictionary<string, Value> symbolTable)
    {
        // WhileNode structure
        // WhileNode
        // -- condition
        // -- whileBlock

        // visit Condition
        Boolean? condition = (Boolean?)VisitNode(node.children[0], symbolTable).Item2;
        while (Convert.ToBoolean(condition.value))
        {
            // visit the body of the loop
            VisitBlockNode(node.children[1], new(symbolTable), symbolTable);

            // update the condition
            condition = (Boolean?)VisitNode(node.children[0], symbolTable).Item2;
        }
        // return nothing
        return (false, null);
    }

    // visit if node
    static (bool, Value?) VisitIfNode(ASTNode node, Dictionary<string, Value> symbolTable)
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
        Boolean? condition = (Boolean?)VisitNode(node.children[0], symbolTable).Item2;

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
            return (false, null);
        }
    }

    // visit function call node
    static (bool, Value?) VisitFuncCallNode(ASTNode node, Dictionary<string, Value> symbolTable)
    {
        // FuncCallNode structure:
        // FuncCallNode : <function name>
        // -- arg1
        // -- arg2
        // -- ...

        // if the symbol we're calling is a function (it might be something else, like a variable)
        if (GetSymbol(symbolTable, node.value,node) is BaseFunction) // BaseFunction is the class both custom and built-in functions extend from
        {
            List <Value> args = []; // prepare a list for args
            foreach (ASTNode child in node.children) //
            {
                args.Add(VisitNode(child, symbolTable).Item2);
            }

            return ((BaseFunction)symbolTable[node.value]).Execute(args,node.line,node.pos,node.filename);
        }
        else
            throw new EigerError(node.filename,node.line,node.pos, $"{node.value} is not a function");
    }

    // Visit function definition node
    static (bool, Value?) VisitFuncDefNode(ASTNode node, Dictionary<string, Value> symbolTable)
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
        symbolTable[funcName] = new Function(node,funcName, argnames, root, symbolTable);

        // return nothing
        return (false, null);
    }

    // visit binary operator node
    static (bool, Value) VisitBinOpNode(ASTNode node, Dictionary<string, Value> symbolTable)
    {
        // prepare a variable for return value
        Value retVal;
        switch (node.value)
        {
            // check each available binary operator
            case "+": retVal = VisitNode(node.children[0], symbolTable).Item2.AddedTo(VisitNode(node.children[1], symbolTable).Item2); break;
            case "-": retVal = VisitNode(node.children[0], symbolTable).Item2.SubbedBy(VisitNode(node.children[1], symbolTable).Item2); break;
            case "*": retVal = VisitNode(node.children[0], symbolTable).Item2.MultedBy(VisitNode(node.children[1], symbolTable).Item2); break;
            case "/": retVal = VisitNode(node.children[0], symbolTable).Item2.DivedBy(VisitNode(node.children[1], symbolTable).Item2); break;
            case "?=": retVal = VisitNode(node.children[0], symbolTable).Item2.ComparisonEqeq(VisitNode(node.children[1], symbolTable).Item2); break;
            case "<": retVal = VisitNode(node.children[0], symbolTable).Item2.ComparisonLT(VisitNode(node.children[1], symbolTable).Item2); break;
            case ">": retVal = VisitNode(node.children[0], symbolTable).Item2.ComparisonGT(VisitNode(node.children[1], symbolTable).Item2); break;
            case "<=": retVal = VisitNode(node.children[0], symbolTable).Item2.ComparisonLTE(VisitNode(node.children[1], symbolTable).Item2); break;
            case ">=": retVal = VisitNode(node.children[0], symbolTable).Item2.ComparisonGTE(VisitNode(node.children[1], symbolTable).Item2); break;
            case "!=": retVal = VisitNode(node.children[0], symbolTable).Item2.ComparisonNeqeq(VisitNode(node.children[1], symbolTable).Item2); break;
            // assignment operator (assign a value to a variable and return it)
            case "=":
                {
                    Value rightSide = VisitNode(node.children[1], symbolTable).Item2;
                    SetSymbol(symbolTable, node.children[0].value, rightSide);
                    retVal = rightSide;
                }
                break;
            // compound assignment operators (change the variable's value and return the new value)
            case "+=":
                {
                    Value rightSide = VisitNode(node.children[1], symbolTable).Item2;
                    SetSymbol(symbolTable, node.children[0].value, GetSymbol(symbolTable, node.children[0].value, node.children[0]) + rightSide);
                    retVal = rightSide;
                }
                break;
            case "-=":
                {
                    Value rightSide = VisitNode(node.children[1], symbolTable).Item2;
                    SetSymbol(symbolTable, node.children[0].value, GetSymbol(symbolTable, node.children[0].value, node.children[0]) - rightSide);
                    retVal = rightSide;
                }
                break;
            case "*=":
                {
                    Value rightSide = VisitNode(node.children[1], symbolTable).Item2;
                    SetSymbol(symbolTable, node.children[0].value, GetSymbol(symbolTable, node.children[0].value, node.children[0]) * rightSide);
                    retVal = rightSide;
                }
                break;
            case "/=":
                {
                    Value rightSide = VisitNode(node.children[1], symbolTable).Item2;
                    SetSymbol(symbolTable, node.children[0].value, GetSymbol(symbolTable, node.children[0].value, node.children[0]) / rightSide);
                    retVal = rightSide;
                }
                break;
            default: throw new EigerError(node.filename,node.line,node.pos, "Invalid Binary Operator");
        }
        return (false, retVal);
    }

    // visit literal node (pretty self-explanatory)
    static (bool, Value) VisitLiteralNode(ASTNode node, Dictionary<string, Value> symbolTable)
    {
        return (false, Value.ToEigerValue(node.filename,node.line,node.pos,node.value));
    }

    // visit identifier node (pretty self-explanatory too)
    static (bool, Value) VisitIdentifierNode(ASTNode node, Dictionary<string, Value> symbolTable)
    {
        return (false, GetSymbol(symbolTable, node.value,node));
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
        foreach (dynamic item in node.children)
        {
            list.Add(VisitNode(item, symbolTable).Item2);
        }
        return (false,new BuiltInTypes.Array(node.filename,node.line,node.pos,[.. list]));
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
            int idx = Convert.ToInt32(value.Item2);
            return (false, iter.GetIndex(idx));
        }
        catch(IndexOutOfRangeException)
        {
            throw new EigerError(node.filename, node.line, node.pos, "Index out of bounds");
        }
        catch(RuntimeBinderException)
        {
            throw new EigerError(node.filename, node.line, node.pos, "Not iterable");
        }
    }

    // get symbol from a symboltable with error handling
    static Value GetSymbol(Dictionary<string, Value> symbolTable, string key,ASTNode node)
    {
        try
        {
            return symbolTable[key];
        }
        catch (KeyNotFoundException)
        {
            throw new EigerError(node.filename,node.line,node.pos, $"`{key}` is undefined");
        }
    }

    // set symbol from a symboltable with error handling
    static void SetSymbol(Dictionary<string, Value> symbolTable, string key, Value value)
    {
        symbolTable[key] = value;
    }
}
/*
 * EIGERLANG INTERPRETER
 * WRITTEN BY VARDAN PETROSYAN
*/

using EigerLang.Errors;
using EigerLang.Execution.BuiltInTypes;
using EigerLang.Parsing;
using Microsoft.CSharp.RuntimeBinder;

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
    public static Dictionary<string, dynamic?> globalSymbolTable = new() {
        {"emit",emitFunction},
        {"emitln",emitlnFunction},
        {"in",inFunction},
        {"inchar",incharFunction},
        {"cls",clsFunction}
    };

    // function for visiting a node
    public static (bool, dynamic?) VisitNode(ASTNode node, Dictionary<string, dynamic?> symbolTable)
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
    static (bool, dynamic?) VisitReturnNode(ASTNode node, Dictionary<string, dynamic?> symbolTable)
    {
        // ReturnNode structure
        // ReturnNode
        // -- value

        return (true, VisitNode(node.children[0], symbolTable).Item2);
    }

    // visit block node 
    public static (bool, dynamic?) VisitBlockNode(ASTNode node, Dictionary<string, dynamic?> symbolTable, Dictionary<string, dynamic?>? parentSymbolTable = null)
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
            (bool didReturn, dynamic? value) = VisitNode(child, symbolTable);

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
    static (bool,dynamic?) VisitForToNode(ASTNode node, Dictionary<string, dynamic?> symbolTable)
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

        Dictionary<string, dynamic?> localSymbolTable = new(symbolTable);

        string iteratorName = node.children[0].value ?? "";

        ASTNode toNode = node.children[2];

        ASTNode forBlock = node.children[3];

        dynamic value = VisitNode(node.children[1], symbolTable).Item2 ?? 0;
        dynamic toValue = VisitNode(toNode, symbolTable).Item2 ?? 0;

        int step = value < toValue ? 1 : -1;

        SetSymbol(localSymbolTable, iteratorName, value);

        while (value != toValue)
        {
            VisitBlockNode(forBlock, localSymbolTable, symbolTable);
            value += step;
            SetSymbol(localSymbolTable, iteratorName, value);
        }

        return (false, null);
    }

    // visit while node
    static (bool, dynamic?) VisitWhileNode(ASTNode node, Dictionary<string, dynamic?> symbolTable)
    {
        // WhileNode structure
        // WhileNode
        // -- condition
        // -- whileBlock

        // visit Condition
        dynamic condition = VisitNode(node.children[0], symbolTable);
        while (Convert.ToBoolean(condition.Item2))
        {
            // visit the body of the loop
            VisitBlockNode(node.children[1], new(symbolTable), symbolTable);

            // update the condition
            condition = VisitNode(node.children[0], symbolTable);
        }
        // return nothing
        return (false, null);
    }

    // visit if node
    static (bool, dynamic?) VisitIfNode(ASTNode node, Dictionary<string, dynamic?> symbolTable)
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
        dynamic condition = VisitNode(node.children[0], symbolTable);

        // if the condition is true
        if (Convert.ToBoolean(condition.Item2))
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
    static (bool, dynamic?) VisitFuncCallNode(ASTNode node, Dictionary<string, dynamic?> symbolTable)
    {
        // FuncCallNode structure:
        // FuncCallNode : <function name>
        // -- arg1
        // -- arg2
        // -- ...

        // if the symbol we're calling is a function (it might be something else, like a variable)
        if (GetSymbol(symbolTable, node.value,node) is BaseFunction) // BaseFunction is the class both custom and built-in functions extend from
        {
            List<dynamic> args = []; // prepare a list for args
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
    static (bool, dynamic?) VisitFuncDefNode(ASTNode node, Dictionary<string, dynamic?> symbolTable)
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
        symbolTable[funcName] = new Function(funcName, argnames, root, symbolTable);

        // return nothing
        return (false, null);
    }

    // visit binary operator node
    static (bool, dynamic?) VisitBinOpNode(ASTNode node, Dictionary<string, dynamic?> symbolTable)
    {
        // prepare a variable for return value
        dynamic? retVal;
        switch (node.value)
        {
            // check each available binary operator
            case "+": retVal = VisitNode(node.children[0], symbolTable).Item2 + VisitNode(node.children[1], symbolTable).Item2; break;
            case "-": retVal = VisitNode(node.children[0], symbolTable).Item2 - VisitNode(node.children[1], symbolTable).Item2; break;
            case "*": retVal = VisitNode(node.children[0], symbolTable).Item2 * VisitNode(node.children[1], symbolTable).Item2; break;
            case "/": retVal = VisitNode(node.children[0], symbolTable).Item2 / VisitNode(node.children[1], symbolTable).Item2; break;
            case "?=": retVal = VisitNode(node.children[0], symbolTable).Item2 == VisitNode(node.children[1], symbolTable).Item2; break;
            case "<": retVal = VisitNode(node.children[0], symbolTable).Item2 < VisitNode(node.children[1], symbolTable).Item2; break;
            case ">": retVal = VisitNode(node.children[0], symbolTable).Item2 > VisitNode(node.children[1], symbolTable).Item2; break;
            case "<=": retVal = VisitNode(node.children[0], symbolTable).Item2 <= VisitNode(node.children[1], symbolTable).Item2; break;
            case ">=": retVal = VisitNode(node.children[0], symbolTable).Item2 >= VisitNode(node.children[1], symbolTable).Item2; break;
            case "!=": retVal = VisitNode(node.children[0], symbolTable).Item2 != VisitNode(node.children[1], symbolTable).Item2; break;
            // assignment operator (assign a value to a variable and return it)
            case "=":
                {
                    dynamic? rightSide = VisitNode(node.children[1], symbolTable).Item2;
                    SetSymbol(symbolTable, node.children[0].value, rightSide);
                    retVal = rightSide;
                }
                break;
            // compound assignment operators (change the variable's value and return the new value)
            case "+=":
                {
                    dynamic? rightSide = VisitNode(node.children[1], symbolTable).Item2;
                    SetSymbol(symbolTable, node.children[0].value, GetSymbol(symbolTable, node.children[0].value, node.children[0]) + rightSide);
                    retVal = rightSide;
                }
                break;
            case "-=":
                {
                    dynamic? rightSide = VisitNode(node.children[1], symbolTable).Item2;
                    SetSymbol(symbolTable, node.children[0].value, GetSymbol(symbolTable, node.children[0].value, node.children[0]) - rightSide);
                    retVal = rightSide;
                }
                break;
            case "*=":
                {
                    dynamic? rightSide = VisitNode(node.children[1], symbolTable).Item2;
                    SetSymbol(symbolTable, node.children[0].value, GetSymbol(symbolTable, node.children[0].value, node.children[0]) * rightSide);
                    retVal = rightSide;
                }
                break;
            case "/=":
                {
                    dynamic? rightSide = VisitNode(node.children[1], symbolTable).Item2;
                    SetSymbol(symbolTable, node.children[0].value, GetSymbol(symbolTable, node.children[0].value, node.children[0]) / rightSide);
                    retVal = rightSide;
                }
                break;
            default: throw new EigerError(node.filename,node.line,node.pos, "Invalid Binary Operator");
        }
        return (false, retVal);
    }

    // visit literal node (pretty self-explanatory)
    static (bool, dynamic?) VisitLiteralNode(ASTNode node, Dictionary<string, dynamic?> symbolTable)
    {
        return (false, node.value);
    }

    // visit identifier node (pretty self-explanatory too)
    static (bool, dynamic?) VisitIdentifierNode(ASTNode node, Dictionary<string, dynamic?> symbolTable)
    {
        return (false, GetSymbol(symbolTable, node.value,node));
    }

    // visit array node
    static (bool, dynamic?) VisitArrayNode(ASTNode node, Dictionary<string, dynamic?> symbolTable)
    {
        // ArrayNode structure
        // ArrayNode
        // -- element1
        // -- element2
        // -- ...

        List<dynamic?> list = [];
        foreach (dynamic item in node.children)
        {
            list.Add(VisitNode(item, symbolTable).Item2);
        }
        return (false,new BuiltInTypes.Array([.. list]));
    }

    private static (bool, dynamic) VisitElementAccessNode(ASTNode node, Dictionary<string, dynamic?> symbolTable)
    {
        dynamic? iter = VisitNode(node.children[0], symbolTable).Item2;
        (bool, dynamic) value = VisitNode(node.children[1], symbolTable);
        try
        {
            int idx = Convert.ToInt32(value.Item2);
            if (iter is BuiltInTypes.Array)
                return (false, iter.array[idx]);
            else
                return (false, iter[idx]);
        }
        catch(IndexOutOfRangeException)
        {
            throw new EigerError(node.filename, node.line, node.pos, "Index outside of bounds");
        }
        catch(RuntimeBinderException)
        {
            throw new EigerError(node.filename, node.line, node.pos, "Object is not iterable");
        }
    }

    // get symbol from a symboltable with error handling
    static dynamic GetSymbol(Dictionary<string, dynamic?> symbolTable, string key,ASTNode node)
    {
        try
        {
            return symbolTable[key] ?? 0;
        }
        catch (KeyNotFoundException)
        {
            throw new EigerError(node.filename,node.line,node.pos, $"`{key}` is undefined");
        }
    }

    // set symbol from a symboltable with error handling
    static void SetSymbol(Dictionary<string, dynamic?> symbolTable, string key, dynamic value)
    {
        symbolTable[key] = value;
    }
}
using EigerLang.Errors;
using EigerLang.Parsing;

namespace EigerLang.Execution;

class Interpreter
{
    public static BuiltInFunctions.EmitFunction emitFunction = new();
    public static BuiltInFunctions.EmitlnFunction emitlnFunction = new();
    public static BuiltInFunctions.InFunction inFunction = new();
    public static BuiltInFunctions.IncharFunction incharFunction = new();
    public static BuiltInFunctions.ClsFunction clsFunction = new();

    public static Dictionary<string, dynamic?> globalSymbolTable = new() {
        {"emit",emitFunction},
        {"emitln",emitlnFunction},
        {"in",inFunction},
        {"inchar",incharFunction},
        {"cls",clsFunction }
    };

    public static (bool, dynamic?) VisitNode(ASTNode node, Dictionary<string, dynamic?> symbolTable)
    {
        switch (node.type)
        {
            case NodeType.Block: return VisitBlockNode(node, symbolTable);
            case NodeType.If: return VisitIfNode(node, symbolTable);
            case NodeType.While: return VisitWhileNode(node, symbolTable);
            case NodeType.FuncCall: return VisitFuncCallNode(node, symbolTable);
            case NodeType.FuncDef: return VisitFuncDefNode(node, symbolTable);
            case NodeType.Return: return VisitReturnNode(node, symbolTable);
            case NodeType.BinOp: return VisitBinOpNode(node, symbolTable);
            case NodeType.Literal: return VisitLiteralNode(node, symbolTable);
            case NodeType.Identifier: return VisitIdentifierNode(node, symbolTable);
            default:
                break;
        }
        return (false, null);
    }

    static (bool, dynamic?) VisitReturnNode(ASTNode node, Dictionary<string, dynamic?> symbolTable)
    {
        return (true, VisitNode(node.children[0], symbolTable).Item2);
    }

    public static (bool, dynamic?) VisitBlockNode(ASTNode node, Dictionary<string, dynamic?> symbolTable, Dictionary<string, dynamic?>? parentSymbolTable = null)
    {
        foreach (var child in node.children)
        {
            (bool didReturn,dynamic? value) = VisitNode(child, symbolTable); 

            if(parentSymbolTable != null)
            foreach (var symbol in parentSymbolTable.Keys)
            {
                    if(symbolTable.ContainsKey(symbol))
                    {
                        parentSymbolTable[symbol] = symbolTable[symbol];
                    }
            }

            if (didReturn)
            {
                return (true,value);
            }
        }
        return (false,null);
    }

    static (bool, dynamic?) VisitWhileNode(ASTNode node, Dictionary<string, dynamic?> symbolTable)
    {
        dynamic condition = VisitNode(node.children[0], symbolTable);
        while (Convert.ToBoolean(condition.Item2))
        {
            VisitBlockNode(node.children[1], symbolTable);
            condition = VisitNode(node.children[0], symbolTable);
        }
        return (false,null);
    }

    static (bool, dynamic?) VisitIfNode(ASTNode node, Dictionary<string, dynamic?> symbolTable)
    {
        bool hasElseBlock = node.children.Count == 3;
        dynamic condition = VisitNode(node.children[0], symbolTable);
        if (Convert.ToBoolean(condition.Item2))
        {
            return VisitBlockNode(node.children[1], new(symbolTable),symbolTable);
        }
        else if (hasElseBlock)
        {
            return VisitBlockNode(node.children[2], new(symbolTable), symbolTable);
        }
        else
        {
            return (false,null);
        }
    }

    static (bool, dynamic?) VisitFuncCallNode(ASTNode node, Dictionary<string, dynamic?> symbolTable)
    {
        try
        {
            if (symbolTable[node.value] is BaseFunction)
            {
                List<dynamic> args = [];
                foreach (ASTNode child in node.children)
                {
                    args.Add(VisitNode(child, symbolTable).Item2);
                }

                return ((BaseFunction)symbolTable[node.value]).Execute(args);
            }
            else
                throw new EigerError($"{node.value} is not a function");
        }
        catch (KeyNotFoundException)
        {
            throw new EigerError($"Function {node.value} undefined");
        }
    }

    static (bool, dynamic?) VisitFuncDefNode(ASTNode node, Dictionary<string, dynamic?> symbolTable)
    {
        string funcName = node.value ?? "";

        ASTNode root = node.children[0];
        List<string> argnames = new();

        for (int i = 1; i < node.children.Count; i++)
        {
            argnames.Add(node.children[i].value);
        }

        symbolTable[funcName] = new Function(funcName, argnames, root, symbolTable);

        return (false, null);
    }

    static (bool, dynamic?) VisitBinOpNode(ASTNode node, Dictionary<string, dynamic?> symbolTable)
    {
        dynamic? retVal = null;
        switch (node.value)
        {
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
            case "=":
                dynamic rightSide = VisitNode(node.children[1], symbolTable).Item2 ?? 0;
                symbolTable[node.children[0].value] = rightSide;
                retVal =  rightSide;
                break;
            default: throw new EigerError("Invalid Binary Operator");
        }
        return (false, retVal);
    }

    static (bool, dynamic?) VisitLiteralNode(ASTNode node, Dictionary<string, dynamic?> symbolTable)
    {
        return (false, node.value);
    }

    static (bool, dynamic?) VisitIdentifierNode(ASTNode node, Dictionary<string, dynamic?> symbolTable)
    {
        try
        {
            return (false, symbolTable[node.value]);
        }
        catch (KeyNotFoundException)
        {
            throw new EigerError($"Variable {node.value} undefined");
        }
    }
}
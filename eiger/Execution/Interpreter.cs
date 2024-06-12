using EigerLang.Errors;
using EigerLang.Parsing;
using System.Net;


namespace EigerLang.Execution;

class Interpreter
{
    public static BuiltInFunctions.EmitFunction emitFunction = new();
    public static BuiltInFunctions.EmitlnFunction emitlnFunction = new();
    public static BuiltInFunctions.InFunction inFunction = new();
    public static BuiltInFunctions.IncharFunction incharFunction = new();

    public static Dictionary<string, dynamic?> globalSymbolTable = new() {
        {"emit",emitFunction},
        {"emitln",emitlnFunction},
        {"in",inFunction},
        {"inchar",incharFunction},
    };

    public static dynamic VisitNode(ASTNode node, Dictionary<string, dynamic?> symbolTable)
    {
        switch (node.type)
        {
            case NodeType.Block: return VisitBlockNode(node, new(symbolTable) );
            case NodeType.If: return VisitIfNode(node, symbolTable);
            case NodeType.FuncCall: return VisitFuncCallNode(node, symbolTable);
            case NodeType.FuncDef: return VisitFuncDefNode(node, symbolTable);
            case NodeType.Return: throw new EigerError("`ret` can only be used in a function");
            case NodeType.BinOp: return VisitBinOpNode(node, symbolTable);
            case NodeType.Literal: return VisitLiteralNode(node, symbolTable);
            case NodeType.Identifier: return VisitIdentifierNode(node, symbolTable);
            default:
                break;
        }
        return 0;
    }


    public static dynamic VisitBlockNode(ASTNode node,Dictionary<string,dynamic?> symbolTable)
    {
        foreach(var child in node.children)
        {
            VisitNode(child,symbolTable);
        }
        return 0;
    }

    public static dynamic VisitBlockNode(ASTNode node, Dictionary<string, dynamic?> symbolTable, out dynamic retVal)
    {
        foreach (var child in node.children)
        {
            if(child.type == NodeType.Return)
            {
                retVal = VisitNode(child.children[0],symbolTable);
                return retVal;
            }
            VisitNode(child, symbolTable);
        }
        retVal = 0;
        return 0;
    }

    static dynamic VisitIfNode(ASTNode node, Dictionary<string, dynamic?> symbolTable)
    {
        bool hasElseBlock = node.children.Count == 3;
        dynamic condition = VisitNode(node.children[0], symbolTable);
        if(Convert.ToBoolean(condition))
        {
            return VisitNode(node.children[1], symbolTable);
        }
        else if(hasElseBlock)
        {
            return VisitNode(node.children[2], symbolTable);
        }
        else
        {
            return 0;
        }
    }

    static dynamic VisitFuncCallNode(ASTNode node, Dictionary<string, dynamic?> symbolTable)
    {
        try
        {
            if(symbolTable[node.value] is BaseFunction)
            {
                List<dynamic> args = new List<dynamic>();
                foreach(ASTNode child in node.children)
                {
                    args.Add(VisitNode(child,symbolTable));
                }

                return symbolTable[node.value].Execute(args);
            }
            else
                throw new EigerError($"{node.value} is not a function");
        }
        catch (KeyNotFoundException)
        {
            throw new EigerError($"Function {node.value} undefined");
        }
    }

    static dynamic VisitFuncDefNode(ASTNode node, Dictionary<string, dynamic?> symbolTable)
    {
        string funcName = node.value ?? "";

        ASTNode root = node.children[0];
        List<string> argnames = new();

        for(int i =1;i<node.children.Count;i++)
        {
            argnames.Add(node.children[i].value);
        }

        symbolTable[funcName] = new Function(funcName, argnames, root,symbolTable);

        return 0;
    }

    static dynamic VisitBinOpNode(ASTNode node, Dictionary<string, dynamic?> symbolTable)
    {
        switch (node.value)
        {
            case "+": return VisitNode(node.children[0],symbolTable) + VisitNode(node.children[1], symbolTable);
            case "-": return VisitNode(node.children[0], symbolTable) - VisitNode(node.children[1], symbolTable);
            case "*": return VisitNode(node.children[0], symbolTable) * VisitNode(node.children[1], symbolTable);
            case "/": return VisitNode(node.children[0], symbolTable) / VisitNode(node.children[1], symbolTable);
            case "?=": return VisitNode(node.children[0], symbolTable) == VisitNode(node.children[1], symbolTable);
            case "<": return VisitNode(node.children[0], symbolTable) < VisitNode(node.children[1], symbolTable);
            case ">": return VisitNode(node.children[0], symbolTable) > VisitNode(node.children[1], symbolTable);
            case "<=": return VisitNode(node.children[0], symbolTable) <= VisitNode(node.children[1], symbolTable);
            case ">=": return VisitNode(node.children[0], symbolTable) >= VisitNode(node.children[1], symbolTable);
            case "!=": return VisitNode(node.children[0], symbolTable) != VisitNode(node.children[1], symbolTable);
            case "=":
                dynamic rightSide = VisitNode(node.children[1], symbolTable);
                symbolTable[node.children[0].value] = rightSide;
                return rightSide;
        }
        throw new EigerError("Invalid Binary Operator");
    }

    static dynamic VisitLiteralNode(ASTNode node, Dictionary<string, dynamic?> symbolTable)
    {
        return node.value ?? 0;
    }

    static dynamic VisitIdentifierNode(ASTNode node, Dictionary<string, dynamic?> symbolTable)
    {
        try
        {
            return symbolTable[node.value] ?? 0;
        }
        catch (KeyNotFoundException)
        {
            throw new EigerError($"Variable {node.value} undefined");
        }
    }
}
/*
 * EIGERLANG INTERPRETER
 * WRITTEN BY VARDAN PETROSYAN
*/

using EigerLang.Errors;
using EigerLang.Execution.BuiltInTypes;
using EigerLang.Parsing;
using EigerLang.Tokenization;
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
    public static BuiltInFunctions.DoubleFunction doubleFunction = new();
    public static BuiltInFunctions.IntFunction intFunction = new();

    // global symbol table
    public static Dictionary<string, Value> globalSymbolTable = new() {
        {"emit",emitFunction},
        {"emitln",emitlnFunction},
        {"in",inFunction},
        {"inchar",incharFunction},
        {"cls",clsFunction},
        {"int",intFunction},
        {"double",doubleFunction},
    };

    // if the interpreter interprets code written in the shell
    bool inShell = false;

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
            case NodeType.Dataclass: return VisitDataclassNode(node, symbolTable);
            case NodeType.Return: return VisitReturnNode(node, symbolTable);
            case NodeType.BinOp: return VisitBinOpNode(node, symbolTable);
            case NodeType.UnaryOp: return VisitUnaryOpNode(node, symbolTable);
            case NodeType.Literal: return VisitLiteralNode(node, symbolTable);
            case NodeType.Identifier: return VisitIdentifierNode(node, symbolTable);
            case NodeType.Array: return VisitArrayNode(node, symbolTable);
            case NodeType.ElementAccess: return VisitElementAccessNode(node, symbolTable);
            case NodeType.AttrAccess: return VisitAttrAccessNode(node, symbolTable);
            case NodeType.Include: return VisitIncludeNode(node, symbolTable);
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
                    if (symbolTable.ContainsKey(symbol))
                        parentSymbolTable[symbol] = symbolTable[symbol];

            // if the current statement returned a value, return the value and stop block execution
            if (didReturn)
                return (true, value);
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
            (bool didReturn,Value v) = VisitBlockNode(forBlock, localSymbolTable, symbolTable);

            if(didReturn)
                return (true, v);

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
        // FuncCallNode
        // -- functionRef
        // -- arg1
        // -- arg2
        // -- ...

        Value func = VisitNode(node.children[0], symbolTable).Item2;

        // if the symbol we're calling is a function (it might be something else, like a variable)
        if (func is BaseFunction f) // BaseFunction is the class both custom and built-in functions extend from
        {
            List<Value> args = []; // prepare a list for args
            for(int i = 1 ;i < node.children.Count;i++)
            {
                Value val = VisitNode(node.children[i], symbolTable).Item2 ?? throw new EigerError(node.filename, node.line, node.pos, Globals.ArgumentErrorStr,EigerError.ErrorType.ArgumentError);
                args.Add(val);
            }

            return f.Execute(args, node.line, node.pos, node.filename);
        }
        else if(func is Class c) // we're calling a class constructor
        {
            List<Value> args = []; // prepare a list for args
            for (int i = 1; i < node.children.Count; i++)
            {
                Value val = VisitNode(node.children[i], symbolTable).Item2 ?? throw new EigerError(node.filename, node.line, node.pos, Globals.ArgumentErrorStr, EigerError.ErrorType.ArgumentError);
                args.Add(val);
            }

            return c.Execute(args);
        }
        else
            throw new EigerError(node.filename, node.line, node.pos, $"{node.value} is not a function", EigerError.ErrorType.RuntimeError);
    }

    // Visit dataclass definition node
    static (bool, Value) VisitDataclassNode(ASTNode node, Dictionary<string, Value> symbolTable)
    {
        // DataclassNode structure
        // DataclassNode : <class name>
        // -- block
        // ---- definition1
        // ---- definition2
        // ---- ...

        Dataclass classdef = new(node.filename, node.line, node.pos, node.value, new Dictionary<string, Value>(symbolTable), node.children[0]);

        SetSymbol(symbolTable, node.value, classdef);

        return (false, classdef);
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

    // visit unary operator node
    static(bool,Value) VisitUnaryOpNode(ASTNode node, Dictionary<string, Value> symbolTable)
    {
        // prepare a variable for return value
        Value retVal;

        // get unary operator
        string unaryOp = node.value ?? throw new EigerError(node.filename,node.line,node.pos,"Invalid Unary Operator",EigerError.ErrorType.RuntimeError);

        // get right side without unary operator
        Value rightSide = VisitNode(node.children[0], symbolTable).Item2;

        // switch for every unary operator
        switch(unaryOp)
        {
            case "-": retVal = rightSide.Negative(); break;
            case "not": retVal = rightSide.Notted(); break;
            default:
                throw new EigerError(node.filename, node.line, node.pos, "Invalid Unary Operator", EigerError.ErrorType.RuntimeError);
        }

        return (false, retVal);
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
            case "%": retVal = leftSide.ModdedBy(rightSide); break;
            case "^": retVal = leftSide.ExpedBy(rightSide); break;
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
                    Value newVal = GetSymbol(symbolTable, node.children[0]).AddedTo(rightSide);
                    SetSymbol(symbolTable, node.children[0], newVal);
                    retVal = newVal;
                }
                break;
            case "-=":
                {
                    Value newVal = GetSymbol(symbolTable, node.children[0]).SubbedBy(rightSide);
                    SetSymbol(symbolTable, node.children[0], newVal);
                    retVal = newVal;
                }
                break;
            case "*=":
                {
                    Value newVal = GetSymbol(symbolTable, node.children[0]).MultedBy(rightSide);
                    SetSymbol(symbolTable, node.children[0], newVal);
                    retVal = newVal;
                }
                break;
            case "/=":
                {
                    Value newVal = GetSymbol(symbolTable, node.children[0]).DivedBy(rightSide);
                    SetSymbol(symbolTable, node.children[0], newVal);
                    retVal = newVal;
                }
                break;
            default: throw new EigerError(node.filename, node.line, node.pos, $"{Globals.InvalidOperationStr}: {node.value}", EigerError.ErrorType.InvalidOperationError);
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

        Value v;
        ASTNode currentNode = node.children[1];

        while(true)
        {
            if (currentNode.type != NodeType.AttrAccess)
            {
                v = VisitNode(node.children[0], symbolTable).Item2.GetAttr(node.children[1]);
                break;
            }
            currentNode = currentNode.children[1];
        }

        if (v is BaseFunction f && currentNode.type == NodeType.FuncCall)
        {
            List<Value> args = []; // prepare a list for args
            foreach (ASTNode child in currentNode.children)
            {
                Value val = VisitNode(child, symbolTable).Item2 ?? throw new EigerError(node.filename, node.line, node.pos, Globals.ArgumentErrorStr, EigerError.ErrorType.ArgumentError);
                args.Add(val);
            }
            return (false, f.Execute(args, node.line, node.pos, node.filename).Item2);
        }
        else if (v is Class c && currentNode.type == NodeType.FuncCall)
        {
            List<Value> args = []; // prepare a list for args
            foreach (ASTNode child in currentNode.children)
            {
                Value val = VisitNode(child, symbolTable).Item2 ?? throw new EigerError(node.filename, node.line, node.pos, Globals.ArgumentErrorStr, EigerError.ErrorType.ArgumentError);
                args.Add(val);
            }
            return (false, c.Execute(args).Item2);
        }
        else
            return (false, v);
    }

    private static (bool, Value) VisitElementAccessNode(ASTNode node, Dictionary<string, Value> symbolTable)
    {
        // ElementAccessNode structure
        // ElementAccessNode
        // -- value
        // -- idx

        Value iter = VisitNode(node.children[0], symbolTable).Item2;
        Value value = VisitNode(node.children[1], symbolTable).Item2;

        try
        {
            int idx = Convert.ToInt32(((Number)(value ?? throw new EigerError(node.filename, node.line, node.pos, "Index is unknown",EigerError.ErrorType.ParserError))).value);
            return (false, iter.GetIndex(idx));
        }
        catch (IndexOutOfRangeException)
        {
            throw new EigerError(node.filename, node.line, node.pos, Globals.IndexErrorStr,EigerError.ErrorType.IndexError);
        }
    }

    private static (bool, Value) VisitIncludeNode(ASTNode node, Dictionary<string, Value> symbolTable)
    {
        // IncludeNode structure
        // IncludeNode
        // -- path

        string path;

        if (node.children[0].type == NodeType.Identifier)
        {
            path = $"./stdlibs/{node.children[0].value}";
        }
        else if (node.children[0].type == NodeType.Literal && node.children[0].value is string s)
        {
            path = s;
        }
        else
        {
            throw new EigerError(node.filename, node.line, node.pos, "Invalid path", EigerError.ErrorType.RuntimeError);
        }

        if (!path.EndsWith(Globals.fileExtension))
            path += Globals.fileExtension;

        if (Path.GetExtension(path) != Globals.fileExtension) // check extension
        {
            throw new EigerError(node.filename, node.line, node.pos, $"Not a {Globals.fileExtension} file", EigerError.ErrorType.RuntimeError);
        }
        string content;
        try
        {
            content = File.ReadAllText(path); // try reading the file
        }
        catch (IOException)
        {
            throw new EigerError(node.filename, node.line, node.pos, "Failed to read file", EigerError.ErrorType.RuntimeError);
        }

        try
        {
            Lexer lex = new(content, path);
            List<Token> tokens = lex.Tokenize();
            Parser parser = new(tokens);
            ASTNode root = parser.Parse(path);
            VisitBlockNode(root, symbolTable);
        }
        catch (EigerError e)
        {
            Console.WriteLine(e.Message);
        }
        catch (OverflowException e)
        {
            Console.WriteLine($"[INTERNAL] Overflow: {e.Message}");
        }
        catch (Exception e) { Console.WriteLine(e); }

        return (false, new Nix(node.filename, node.line, node.pos));
    }

    // get symbol from a symboltable with error handling
    public static Value GetSymbol(Dictionary<string, Value> symbolTable, ASTNode key)
    {
        string err_key = "";
        try
        {
            if (key.type == NodeType.Identifier)
            {
                err_key = key.value ?? "";
                return symbolTable[key.value];
            }
            else if(key.type == NodeType.FuncCall)
            {
                err_key = key.children[0].value ?? "";
                return symbolTable[key.children[0].value];
            }
            else if (key.type == NodeType.ElementAccess)
            {
                ASTNode listNode = key.children[0];
                ASTNode idxNode = key.children[1];

                if (listNode.type != NodeType.Identifier)
                    throw new EigerError(key.filename, key.line, key.pos, "Must be identifier", EigerError.ErrorType.RuntimeError);

                string listKey = key.children[0].value ?? throw new EigerError(key.filename, key.line, key.pos, "Identifier value not found",EigerError.ErrorType.ParserError);
                int idx = (int)((Number)(VisitNode(idxNode, symbolTable).Item2)).value;

                err_key = listKey;

                return symbolTable[listKey].GetIndex(idx);
            }
            else if(key.type == NodeType.AttrAccess)
            {
                ASTNode leftNode = key.children[0];
                ASTNode rightNode = key.children[1];

                err_key = leftNode.value ?? "";

                return symbolTable[leftNode.value].GetAttr(rightNode);
            }
            else throw new EigerError(key.filename, key.line, key.pos, $"Invalid Node {key.type}", EigerError.ErrorType.ParserError);
        }
        catch (KeyNotFoundException)
        {
            throw new EigerError(key.filename, key.line, key.pos, $"{err_key} is undefined",EigerError.ErrorType.RuntimeError);
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
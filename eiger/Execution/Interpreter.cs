/*
 * EIGERLANG INTERPRETER
*/

using EigerLang.Errors;
using EigerLang.Execution.BuiltInTypes;
using EigerLang.Parsing;
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
    public static BuiltInFunctions.FreadFunction freadFunction = new();
    public static BuiltInFunctions.RandFunction randFunction = new();
    public static BuiltInFunctions.ColorFunction colorFunction = new();
    public static BuiltInFunctions.AsciiFunction asciiFunction = new();
    public static BuiltInFunctions.TimeFunction timeFunction = new();
    public static Number fgColor = new("<std>", 0, 0, (int)ConsoleColor.Gray)
    {
        isReadonly = true
    };
    public static Number bgColor = new("<std>", 0, 0, (int)ConsoleColor.Black)
    {
        isReadonly = true
    };

    // global symbol table
    public static Dictionary<string, Value> globalSymbolTable = new() {
        {"emit",emitFunction},
        {"emitln",emitlnFunction},
        {"in",inFunction},
        {"inchar",incharFunction},
        {"cls",clsFunction},
        {"int",intFunction},
        {"double",doubleFunction},
        {"fread",freadFunction},
        {"rand",randFunction},
        {"color",colorFunction},
        {"color_fg",fgColor},
        {"color_bg",bgColor},
        {"ascii",asciiFunction},
        {"time", timeFunction},
    };

    // function for visiting a node
    public static (bool, bool, Value) VisitNode(ASTNode node, Dictionary<string, Value> symbolTable)
    {
        switch (node.type)
        {
            case NodeType.Block: return VisitBlockNode(node, symbolTable);
            case NodeType.If: return VisitIfNode(node, symbolTable);
            case NodeType.ForTo: return VisitForToNode(node, symbolTable);
            case NodeType.While: return VisitWhileNode(node, symbolTable);
            case NodeType.FuncCall: return VisitFuncCallNode(node, symbolTable);
            case NodeType.FuncDef: return VisitFuncDefNode(node, symbolTable);
            case NodeType.FuncDefInline: return VisitFuncDefInlineNode(node, symbolTable);
            case NodeType.Class: return VisitClassNode(node, symbolTable);
            case NodeType.Dataclass: return VisitDataclassNode(node, symbolTable);
            case NodeType.Return: return VisitReturnNode(node, symbolTable);
            case NodeType.Break: return (true, false, new Nix(node.filename, node.line, node.pos));
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
        return (false, false, new Nix(node.filename, node.line, node.pos));
    }

    // visit return node
    static (bool, bool, Value) VisitReturnNode(ASTNode node, Dictionary<string, Value> symbolTable)
    {
        // ReturnNode structure
        // ReturnNode
        // -- value

        return (false, true, VisitNode(node.children[0], symbolTable).Item3);
    }

    // visit block node 
    public static (bool, bool, Value) VisitBlockNode(ASTNode node, Dictionary<string, Value> symbolTable, Dictionary<string, Value>? parentSymbolTable = null)
    {
        // BlockNode structure
        // BlockNode
        // -- statement1
        // -- statement2
        // -- ...

        // loop through each statement
        foreach (var child in node.children)
        {
            (bool shouldBreak, bool didReturn, Value value) = VisitNode(child, symbolTable);

            // Sync local and parent symbol tables
            // basically syncing the variables in this scope with the ones in the parent scope
            // to enable changing global variables from a child scope (like an if statement or a function)
            // this is not a very optimized solution but I can't come up with a better one
            if (parentSymbolTable != null)
                foreach (var symbol in parentSymbolTable.Keys)
                    if (symbolTable.ContainsKey(symbol))
                        parentSymbolTable[symbol] = symbolTable[symbol];

            // if the current statement returned a value, return the value and stop block execution
            if (didReturn || shouldBreak)
                return (shouldBreak, didReturn, value);
        }
        // return nothing
        return (false, false, new Nix(node.filename, node.line, node.pos));
    }

    // visit for..to node
    static (bool, bool, Value) VisitForToNode(ASTNode node, Dictionary<string, Value> symbolTable)
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

        double value = ((Number)VisitNode(node.children[1], symbolTable).Item3).value;
        double toValue = ((Number)VisitNode(toNode, symbolTable).Item3).value;

        int step = value < toValue ? 1 : -1;

        SetSymbol(localSymbolTable, node.children[0], new Number(node.filename, node.line, node.pos, value));

        while (value != toValue)
        {
            (bool shouldBreak, bool didReturn, Value v) = VisitBlockNode(forBlock, localSymbolTable, symbolTable);

            if (didReturn || shouldBreak)
                return (shouldBreak, didReturn, v);

            value += step;
            SetSymbol(localSymbolTable, node.children[0], new Number(node.filename, node.line, node.pos, value));
        }

        return (false, false, new Nix(node.filename, node.line, node.pos));
    }

    // visit while node
    static (bool, bool, Value) VisitWhileNode(ASTNode node, Dictionary<string, Value> symbolTable)
    {
        // WhileNode structure
        // WhileNode
        // -- condition
        // -- whileBlock

        // visit Condition
        Boolean condition = (Boolean)VisitNode(node.children[0], symbolTable).Item3;
        while (Convert.ToBoolean(condition.value))
        {
            // visit the body of the loop
            (bool shouldBreak, bool didReturn, Value v) = VisitBlockNode(node.children[1], new(symbolTable), symbolTable);

            if (didReturn || shouldBreak)
                return (shouldBreak, didReturn, v);

            // update the condition
            condition = (Boolean)VisitNode(node.children[0], symbolTable).Item3;
        }
        // return nothing
        return (false, false, new Nix(node.filename, node.line, node.pos));
    }

    // visit if node
    // visit if node
    static (bool, bool, Value) VisitIfNode(ASTNode node, Dictionary<string, Value> symbolTable)
    {
        // IfNode structure
        // IfNode
        // -- condition
        // -- ifBlock
        // -- [elseIfNode ...]
        // -- elseBlock (if exists)

        // visit condition
        Boolean condition = (Boolean)VisitNode(node.children[0], symbolTable).Item3;

        // if the condition is true
        if (Convert.ToBoolean(condition.value))
        {
            // visit the if block (2nd child node)
            return VisitBlockNode(node.children[1], new(symbolTable), symbolTable);
        }

        // check for else if blocks
        for (int i = 2; i < node.children.Count; i++)
        {
            ASTNode child = node.children[i];
            if (child.type == NodeType.If)
            {
                // visit else if condition
                Boolean elseIfCondition = (Boolean)VisitNode(child.children[0], symbolTable).Item3;
                if (Convert.ToBoolean(elseIfCondition.value))
                {
                    // visit the else if block (2nd child of else if node)
                    return VisitBlockNode(child.children[1], new(symbolTable), symbolTable);
                }
            }
            else
            {
                // assume this is the else block
                // visit the else block
                return VisitBlockNode(child, new(symbolTable), symbolTable);
            }
        }

        // if no condition is true and there's no else block, do nothing
        return (false, false, new Nix(node.filename, node.line, node.pos));
    }


    // visit function call node
    static (bool, bool, Value) VisitFuncCallNode(ASTNode node, Dictionary<string, Value> symbolTable)
    {
        // FuncCallNode structure:
        // FuncCallNode
        // -- functionRef
        // -- arg1
        // -- arg2
        // -- ...

        Value func = VisitNode(node.children[0], symbolTable).Item3;

        // if the symbol we're calling is a function or a class (it might be something else, like a variable)
        List<Value> args = PrepareArguments(node, symbolTable);

        if (func is BaseFunction f)
            return f.Execute(args, node.line, node.pos, node.filename);
        else if (func is Class c)
            return c.Execute(args);
        else
            throw new EigerError(node.filename, node.line, node.pos, $"{node.value} is not a function", EigerError.ErrorType.RuntimeError);
    }

    // Visit dataclass definition node
    static (bool, bool, Value) VisitDataclassNode(ASTNode node, Dictionary<string, Value> symbolTable)
    {
        // DataclassNode structure
        // DataclassNode : <class name>
        // -- block
        // ---- definition1
        // ---- definition2
        // ---- ...

        Dataclass classdef = new(node.filename, node.line, node.pos, node.value, new Dictionary<string, Value>(symbolTable), node.children[0]);

        SetSymbol(symbolTable, node.value, classdef);

        return (false, false, classdef);
    }

    // Visit class definition node
    static (bool, bool, Value) VisitClassNode(ASTNode node, Dictionary<string, Value> symbolTable)
    {
        // ClassNode structure
        // ClassNode : <class name>
        // -- block
        // ---- statement1
        // ---- statement2
        // ---- ...

        Class classdef = new(node.filename, node.line, node.pos, node.value, symbolTable, node.children[0]);

        SetSymbol(symbolTable, node.value, classdef);

        return (false, false, classdef);
    }

    // Visit function definition node
    static (bool, bool, Value) VisitFuncDefNode(ASTNode node, Dictionary<string, Value> symbolTable)
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
        if (funcName != "")
            symbolTable[funcName] = new Function(node, funcName, argnames, root, symbolTable);

        return (false, false, new Function(node, funcName, argnames, root, symbolTable));
    }

    // Visit inline function definition node
    static (bool, bool, Value) VisitFuncDefInlineNode(ASTNode node, Dictionary<string, Value> symbolTable)
    {
        // VisitFuncDefInlineNode structure
        // VisitFuncDefInlineNode : <function name>
        // -- funcExpr
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

        // return the function
        return (false, false, new InlineFunction(node, funcName, argnames, root, symbolTable));
    }

    // visit unary operator node
    static (bool, bool, Value) VisitUnaryOpNode(ASTNode node, Dictionary<string, Value> symbolTable)
    {
        // get unary operator
        string unaryOp = node.value ?? throw new EigerError(node.filename, node.line, node.pos, "Invalid Unary Operator", EigerError.ErrorType.RuntimeError);

        // get right side without unary operator
        Value rightSide = VisitNode(node.children[0], symbolTable).Item3;
        // prepare a variable for return value
        Value retVal = unaryOp switch
        {
            "-" => rightSide.Negative(),
            "not" => rightSide.Notted(),
            _ => throw new EigerError(node.filename, node.line, node.pos, "Invalid Unary Operator", EigerError.ErrorType.RuntimeError),
        };
        return (false, false, retVal);
    }

    // visit binary operator node
    static (bool, bool, Value) VisitBinOpNode(ASTNode node, Dictionary<string, Value> symbolTable)
    {
        Value leftSide = new Nix(node.filename, node.line, node.pos);

        if (node.value != "=")
            leftSide = VisitNode(node.children[0], symbolTable).Item3;

        Value rightSide = VisitNode(node.children[1], symbolTable).Item3;

        // prepare a variable for return value
        Value retVal = node.value switch
        {
            // check each available binary operator
            "+" => leftSide.AddedTo(rightSide),
            "-" => leftSide.SubbedBy(rightSide),
            "*" => leftSide.MultedBy(rightSide),
            "/" => leftSide.DivedBy(rightSide),
            "%" => leftSide.ModdedBy(rightSide),
            "^" => leftSide.ExpedBy(rightSide),
            "?=" => leftSide.ComparisonEqeq(rightSide),
            "<" => leftSide.ComparisonLT(rightSide),
            ">" => leftSide.ComparisonGT(rightSide),
            "<=" => leftSide.ComparisonLTE(rightSide),
            ">=" => leftSide.ComparisonGTE(rightSide),
            "!=" => leftSide.ComparisonNeqeq(rightSide),
            "=" => HandleAssignment(node, rightSide, symbolTable),
            "+=" => HandleCompoundAssignment(node, rightSide, symbolTable, (left, right) => left.AddedTo(right)),
            "-=" => HandleCompoundAssignment(node, rightSide, symbolTable, (left, right) => left.SubbedBy(right)),
            "*=" => HandleCompoundAssignment(node, rightSide, symbolTable, (left, right) => left.MultedBy(right)),
            "/=" => HandleCompoundAssignment(node, rightSide, symbolTable, (left, right) => left.DivedBy(right)),
            _ => throw new EigerError(node.filename, node.line, node.pos, $"{Globals.InvalidOperationStr}: {node.value}", EigerError.ErrorType.InvalidOperationError),
        };
        return (false, false, retVal);
    }

    private static Value HandleAssignment(ASTNode node, Value rightSide, Dictionary<string, Value> symbolTable)
    {
        try
        {
            Value leftValue = GetSymbol(symbolTable, node.children[0], true);
            if (leftValue.isReadonly)
                throw new EigerError(leftValue.filename, leftValue.line, leftValue.pos, "variable is read-only", EigerError.ErrorType.RuntimeError);
        }
        catch (KeyNotFoundException) { }
        SetSymbol(symbolTable, node.children[0], rightSide);
        return rightSide;
    }

    private static Value HandleCompoundAssignment(ASTNode node, Value rightSide, Dictionary<string, Value> symbolTable, Func<Value, Value, Value> operation)
    {
        Value leftValue = GetSymbol(symbolTable, node.children[0]);
        Value newValue = operation(leftValue, rightSide);
        if (leftValue.isReadonly)
            throw new EigerError(leftValue.filename, leftValue.line, leftValue.pos, $"{leftValue} is read-only", EigerError.ErrorType.RuntimeError);
        else
            SetSymbol(symbolTable, node.children[0], newValue);
        return newValue;
    }


    // visit literal node (pretty self-explanatory)
    static (bool, bool, Value) VisitLiteralNode(ASTNode node, Dictionary<string, Value> symbolTable)
    {
        if (node.value is string && node.value == "true")
            return (false, false, new Boolean(node.filename, node.line, node.pos, true));
        else if (node.value is string && node.value == "false")
            return (false, false, new Boolean(node.filename, node.line, node.pos, false));
        else if (node.value is string && node.value == "nix")
            return (false, false, new Nix(node.filename, node.line, node.pos));

        return (false, false, Value.ToEigerValue(node.filename, node.line, node.pos, node.value));
    }

    // visit identifier node (pretty self-explanatory too)
    static (bool, bool, Value) VisitIdentifierNode(ASTNode node, Dictionary<string, Value> symbolTable)
    {
        return (false, false, GetSymbol(symbolTable, node));
    }

    // visit array node
    static (bool, bool, Value) VisitArrayNode(ASTNode node, Dictionary<string, Value> symbolTable)
    {
        // ArrayNode structure
        // ArrayNode
        // -- element1
        // -- element2
        // -- ...

        List<Value> list = [];
        foreach (ASTNode item in node.children)
        {
            list.Add(VisitNode(item, symbolTable).Item3);
        }
        return (false, false, new Array(node.filename, node.line, node.pos, [.. list]));
    }

    private static (bool, bool, Value) VisitAttrAccessNode(ASTNode node, Dictionary<string, Value> symbolTable)
    {
        // AttrAccessNode structure
        // AttrAccessNode
        // -- left
        // -- right

        Value v;
        ASTNode currentNode = node.children[1];

        while (true)
        {
            if (currentNode.type != NodeType.AttrAccess)
            {
                v = VisitNode(node.children[0], symbolTable).Item3.GetAttr(node.children[1]);
                break;
            }
            currentNode = currentNode.children[1];
        }

        if (v is BaseFunction f && currentNode.type == NodeType.FuncCall)
        {
            List<Value> args = []; // prepare a list for args
            foreach (ASTNode child in currentNode.children)
            {
                Value val = VisitNode(child, symbolTable).Item3 ?? throw new EigerError(node.filename, node.line, node.pos, Globals.ArgumentErrorStr, EigerError.ErrorType.ArgumentError);
                args.Add(val);
            }
            return (false, false, f.Execute(args, node.line, node.pos, node.filename).Item3);
        }
        else if (v is Class c && currentNode.type == NodeType.FuncCall)
        {
            List<Value> args = []; // prepare a list for args
            foreach (ASTNode child in currentNode.children)
            {
                Value val = VisitNode(child, symbolTable).Item3 ?? throw new EigerError(node.filename, node.line, node.pos, Globals.ArgumentErrorStr, EigerError.ErrorType.ArgumentError);
                args.Add(val);
            }
            return (false, false, c.Execute(args).Item3);
        }
        else
            return (false, false, v);
    }

    private static (bool, bool, Value) VisitElementAccessNode(ASTNode node, Dictionary<string, Value> symbolTable)
    {
        // ElementAccessNode structure
        // ElementAccessNode
        // -- value
        // -- idx

        Value iter = VisitNode(node.children[0], symbolTable).Item3;
        Value value = VisitNode(node.children[1], symbolTable).Item3;

        try
        {
            int idx = Convert.ToInt32(((Number)(value ?? throw new EigerError(node.filename, node.line, node.pos, "Index is unknown", EigerError.ErrorType.ParserError))).value);
            return (false, false, iter.GetIndex(idx));
        }
        catch (IndexOutOfRangeException)
        {
            throw new EigerError(node.filename, node.line, node.pos, Globals.IndexErrorStr, EigerError.ErrorType.IndexError);
        }
    }

    private static (bool, bool, Value) VisitIncludeNode(ASTNode node, Dictionary<string, Value> symbolTable)
    {
        // IncludeNode structure
        // IncludeNode
        // -- path

        string path;

        if (node.children[0].type == NodeType.Identifier)
        {
            path = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "stdlibs", node.children[0].value + Globals.fileExtension);
        }
        else if (node.children[0].type == NodeType.Literal && node.children[0].value is string s)
        {
            path = Path.Join(Directory.GetCurrentDirectory(), s);
        }
        else
        {
            throw new EigerError(node.filename, node.line, node.pos, "Invalid path", EigerError.ErrorType.RuntimeError);
        }

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

        Program.Execute(content, path, false);

        return (false, false, new Nix(node.filename, node.line, node.pos));
    }

    // get symbol from a symboltable with error handling
    public static Value GetSymbol(Dictionary<string, Value> symbolTable, ASTNode key, bool returnKeyError = false)
    {
        string err_key = "";
        try
        {
            if (key.type == NodeType.Identifier)
            {
                err_key = key.value ?? "";
                return symbolTable[key.value];
            }
            else if (key.type == NodeType.FuncCall)
            {
                err_key = key.children[0].value ?? "";
                return symbolTable[key.children[0].value];
            }
            else if (key.type == NodeType.ElementAccess)
            {
                ASTNode listNode = key.children[0];
                ASTNode idxNode = key.children[1];

                Value list = GetSymbol(symbolTable, listNode);

                int idx = (int)((Number)VisitNode(idxNode, symbolTable).Item3).value;

                return list.GetIndex(idx);
            }
            else if (key.type == NodeType.AttrAccess)
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
            if (returnKeyError)
                throw new KeyNotFoundException();
            else
                throw new EigerError(key.filename, key.line, key.pos, $"{err_key} is undefined", EigerError.ErrorType.RuntimeError);
        }
    }

    static List<Value> PrepareArguments(ASTNode node, Dictionary<string, Value> symbolTable)
    {
        List<Value> args = new List<Value>();
        for (int i = 1; i < node.children.Count; i++)
        {
            Value val = VisitNode(node.children[i], symbolTable).Item3 ?? throw new EigerError(node.filename, node.line, node.pos, Globals.ArgumentErrorStr, EigerError.ErrorType.ArgumentError);
            args.Add(val);
        }
        return args;
    }

    public static Dictionary<string, Value> GetDictionaryDifference(Dictionary<string, Value> dict1, Dictionary<string, Value> dict2)
    {
        var diff = new Dictionary<string, Value>();

        foreach (var kvp in dict2)
        {
            if (!dict1.ContainsKey(kvp.Key) || dict1[kvp.Key] != kvp.Value)
            {
                diff[kvp.Key] = kvp.Value;
            }
        }

        return diff;
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
                Value list = GetSymbol(symbolTable, listNode);
                int idx = (int)((Number)VisitNode(idxNode, symbolTable).Item3).value;
                list.SetIndex(idx, value);
            }
            else if (key.type == NodeType.Identifier)
            {
                symbolTable[key.value] = value;
            }
            else if (key.type == NodeType.AttrAccess)
            {
                ASTNode leftNode = key.children[0];
                ASTNode rightNode = key.children[1];
                symbolTable[leftNode.value].SetAttr(rightNode, value);
            }
            else
            {
                throw new EigerError(key.filename, key.line, key.pos, "Assignee must be identifier", EigerError.ErrorType.RuntimeError);
            }
        }
        catch (KeyNotFoundException)
        {
            throw new EigerError(key.filename, key.line, key.pos, $"Variable is undefined", EigerError.ErrorType.RuntimeError);
        }
    }
}
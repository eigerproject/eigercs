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
    public static BuiltInFunctions.ExitFunction exitFunction = new();
    public static BuiltInFunctions.FmtFunction fmtFunction = new();
    public static Number fgColor = new("<std>", 0, 0, (int)ConsoleColor.Gray)
    {
        modifiers = ["readonly"]
    };
    public static Number bgColor = new("<std>", 0, 0, (int)ConsoleColor.Black)
    {
        modifiers = ["readonly"]
    };

    // global symbol table
    public static SymbolTable globalSymbolTable = new(null, new() {
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
        {"exit", exitFunction},
        {"fmt", fmtFunction},
    });

    public static SymbolTable defaultGlobalSymbolTable = new(null, new() {
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
        {"exit", exitFunction},
    });

    // to reset the symbol table (used in tests)
    public static void ResetSymbolTable() =>
        globalSymbolTable = new(defaultGlobalSymbolTable);

    // function for visiting a node
    public static ReturnResult VisitNode(ASTNode node, SymbolTable symbolTable)
    {
        switch (node.type)
        {
            case NodeType.Block: return VisitBlockNode(node, new(symbolTable));
            case NodeType.Let: return VisitLetNode(node, symbolTable);
            case NodeType.If: return VisitIfNode(node, symbolTable);
            case NodeType.ForTo: return VisitForToNode(node, symbolTable);
            case NodeType.While: return VisitWhileNode(node, symbolTable);
            case NodeType.FuncCall: return VisitFuncCallNode(node, symbolTable);
            case NodeType.FuncDef: return VisitFuncDefNode(node, symbolTable);
            case NodeType.FuncDefInline: return VisitFuncDefInlineNode(node, symbolTable);
            case NodeType.Class: return VisitClassNode(node, symbolTable);
            case NodeType.Namespace: return VisitNamespaceNode(node, symbolTable);
            case NodeType.Return: return VisitReturnNode(node, symbolTable);
            case NodeType.Break:
                return new()
                {
                    result = new Nix(node.filename, node.line, node.pos),
                    shouldBreak = true
                };
            case NodeType.Continue:
                return new()
                {
                    result = new Nix(node.filename, node.line, node.pos),
                    shouldContinue = true
                };
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
        return new()
        {
            result = new Nix(node.filename, node.line, node.pos)
        };
    }

    // visit return node
    static ReturnResult VisitReturnNode(ASTNode node, SymbolTable symbolTable)
    {
        // ReturnNode structure
        // ReturnNode
        // -- value

        return new()
        {
            result = VisitNode(node.children[0], symbolTable).result,
            shouldReturn = true
        };
    }

    // visit block node 
    public static ReturnResult VisitBlockNode(ASTNode node, SymbolTable symbolTable)
    {
        // BlockNode structure
        // BlockNode
        // -- statement1
        // -- statement2
        // -- ...

        // loop through each statement
        foreach (var child in node.children)
        {
            ReturnResult r = VisitNode(child, symbolTable);

            // if the current statement returned a value, return the value and stop block execution
            if (r.shouldReturn || r.shouldBreak || r.shouldContinue)
                return r;
        }
        // return nothing
        return new()
        {
            result = new Nix(node.filename, node.line, node.pos)
        };
    }

    // visit for..to node
    static ReturnResult VisitForToNode(ASTNode node, SymbolTable symbolTable)
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


        ASTNode toNode = node.children[2];

        ASTNode forBlock = node.children[3];

        double value = ((Number)VisitNode(node.children[1], symbolTable).result).value;
        double toValue = ((Number)VisitNode(toNode, symbolTable).result).value;

        int step = value < toValue ? 1 : -1;


        while (value != toValue)
        {
            SymbolTable localSymbolTable = new(symbolTable);

            localSymbolTable.CreateSymbol(node.children[0].value, new Number(node.filename, node.line, node.pos, value), node.filename, node.line, node.pos);

            ReturnResult r = VisitNode(forBlock, localSymbolTable);

            if (r.shouldBreak) break;
            if (r.shouldReturn) return r;

            value += step;
        }

        return new()
        {
            result = new Nix(node.filename, node.line, node.pos)
        };
    }

    // visit while node
    static ReturnResult VisitWhileNode(ASTNode node, SymbolTable symbolTable)
    {
        // WhileNode structure
        // WhileNode
        // -- condition
        // -- whileBlock

        // visit Condition
        Boolean condition = (Boolean)VisitNode(node.children[0], symbolTable).result;
        while (Convert.ToBoolean(condition.value))
        {
            // visit the body of the loop
            // (bool shouldBreak, bool didReturn, Value v)
            ReturnResult r = VisitNode(node.children[1], new(symbolTable));

            if (r.shouldBreak) break;
            if (r.shouldReturn) return r;

            // update the condition
            condition = (Boolean)VisitNode(node.children[0], symbolTable).result;
        }
        // return nothing
        return new()
        {
            result = new Nix(node.filename, node.line, node.pos)
        };
    }

    // visit if node
    // visit if node
    static ReturnResult VisitIfNode(ASTNode node, SymbolTable symbolTable)
    {
        // IfNode structure
        // IfNode
        // -- condition
        // -- ifBlock
        // -- [elseIfNode ...]
        // -- elseBlock (if exists)

        // visit condition
        Boolean condition = (Boolean)VisitNode(node.children[0], symbolTable).result;

        // if the condition is true
        if (Convert.ToBoolean(condition.value))
        {
            // visit the if block (2nd child node)
            return VisitNode(node.children[1], new(symbolTable));
        }

        // check for else if blocks
        for (int i = 2; i < node.children.Count; i++)
        {
            ASTNode child = node.children[i];
            if (child.type == NodeType.If)
            {
                // visit else if condition
                Boolean elseIfCondition = (Boolean)VisitNode(child.children[0], symbolTable).result;
                if (Convert.ToBoolean(elseIfCondition.value))
                {
                    // visit the else if block (2nd child of else if node)
                    return VisitNode(child.children[1], new(symbolTable));
                }
            }
            else
            {
                // assume this is the else block
                // visit the else block
                return VisitNode(child, new(symbolTable));
            }
        }

        // if no condition is true and there's no else block, do nothing
        return new()
        {
            result = new Nix(node.filename, node.line, node.pos)
        };
    }

    // visit let variable declaration node
    static ReturnResult VisitLetNode(ASTNode node, SymbolTable symbolTable)
    {
        // LetNode structure:
        // LetNode: variableName
        // -- initialValue (optional)

        dynamic?[] pack = node.value ?? new dynamic?[2];
        string varName = pack[1] ?? "";
        List<string> modifiers = pack[0] ?? new List<string>();

        if (symbolTable.HasSymbol(varName))
            throw new EigerError(node.filename, node.line, node.pos, $"Variable {varName} already declared", EigerError.ErrorType.RuntimeError);

        Value v = node.children.Count == 1
            ? VisitNode(node.children[0], symbolTable).result
            : new Nix(node.filename, node.line, node.pos);

        v.modifiers = modifiers;

        symbolTable.CreateSymbol(varName, v, node.filename, node.line, node.pos);

        return new()
        {
            result = v
        };
    }

    // visit function call node
    static ReturnResult VisitFuncCallNode(ASTNode node, SymbolTable symbolTable)
    {
        // FuncCallNode structure:
        // FuncCallNode
        // -- functionRef
        // -- arg1
        // -- arg2
        // -- ...

        Value func = VisitNode(node.children[0], symbolTable).result;

        bool variadic = false;
        int fncount = 0;

        bool isClass = false;

        if (func is BaseFunction f) {
            variadic = f.isVariadic;
            fncount = f.arg_n.Count;
        } else if (func is Class c) isClass = true;
        else
            throw new EigerError(node.filename, node.line, node.pos, $"{node.value} is not a function", EigerError.ErrorType.RuntimeError);

        List<Value> args = PrepareArguments(node, symbolTable, variadic, fncount);

        // if the symbol we're calling is a function or a class (it might be something else, like a variable)
        if (!isClass)
            return (func as BaseFunction).Execute(args, node.line, node.pos, node.filename);
        else 
            return (func as Class).Execute(args, symbolTable);
    }

    // Visit namespace definition node
    static ReturnResult VisitNamespaceNode(ASTNode node, SymbolTable symbolTable)
    {
        // NamespaceNode structure
        // NamespaceNode : <name>
        // -- block
        // ---- definition1
        // ---- definition2
        // ---- ...

        if (symbolTable.HasSymbol(node.value)) {
            if(symbolTable.GetSymbol(node.value, node.filename, node.line, node.pos) is not Namespace)
                throw new EigerError(node.filename, node.line, node.pos, $"{node.value} already declared", EigerError.ErrorType.RuntimeError);
            else {
                Namespace ns = (Namespace)(symbolTable.GetSymbol(node.value, node.filename, node.line, node.pos));
                ns.AddDefinition(node.children[0]);

                return new() { result = ns };
            }
        } else {
            Namespace newNs = new(node.filename, node.line, node.pos, node.value, symbolTable, node.children[0]);

            symbolTable.CreateSymbol(node.value, newNs, node.filename, node.line, node.pos);

            return new() { result = newNs };    
        }
    }

    // Visit class definition node
    static ReturnResult VisitClassNode(ASTNode node, SymbolTable symbolTable)
    {
        // ClassNode structure
        // ClassNode : <class name>
        // -- block
        // ---- statement1
        // ---- statement2
        // ---- ...

        if (symbolTable.HasSymbol(node.value))
            throw new EigerError(node.filename, node.line, node.pos, $"{node.value} already declared", EigerError.ErrorType.RuntimeError);

        Class classdef = new(node.filename, node.line, node.pos, node.value, node.children[0]);

        symbolTable.CreateSymbol(node.value, classdef, node.filename, node.line, node.pos);

        return new() { result = classdef };
    }

    // Visit function definition node
    static ReturnResult VisitFuncDefNode(ASTNode node, SymbolTable symbolTable)
    {
        // FuncDefNode structure
        // FuncDefNode : <function name>
        // -- funcBody
        // -- argName1
        // -- argName2
        // -- ...

        // get function name
        dynamic?[] pack = node.value ?? new dynamic?[2];
        string funcName = pack[1] ?? "";
        List<string> modifiers = pack[0] ?? new List<string>();

        // a symbol with that name already exists
        if (symbolTable.HasSymbol(funcName) && funcName != "")
            throw new EigerError(node.filename, node.line, node.pos, $"{funcName} already declared", EigerError.ErrorType.RuntimeError);

        // get funciton body
        ASTNode root = node.children[0];

        bool variadic = false;

        // prepare list for argnames
        List<string> argnames = new();

        // loop through each arg (starting from 2nd child node
        for (int i = 1; i < node.children.Count; i++)
        {
            // assuming the node is guaranteed to be an identifier, we can get it's name and add to the name
            // this will be useful later when calling the function, we will add each given argument with that
            // name in the function's local scope
            if(node.children[i].value is char && node.children[i].value == '+') {
                argnames.Add(node.children[i + 1].value);
                variadic = true;
                break;
            }
            argnames.Add(node.children[i].value);
        }

        Function result = new Function(node, funcName, argnames, root, symbolTable)
        {
            modifiers = modifiers,
            isVariadic = variadic
        };

        // add the function to the current scope
        if (funcName != "")
            symbolTable.CreateSymbol(funcName, result, node.filename, node.line, node.pos);

        return new() { result = result };
    }

    // Visit inline function definition node
    static ReturnResult VisitFuncDefInlineNode(ASTNode node, SymbolTable symbolTable)
    {
        // VisitFuncDefInlineNode structure
        // VisitFuncDefInlineNode : <function name>
        // -- funcExpr
        // -- argName1
        // -- argName2
        // -- ...

        // get function name
        dynamic?[] pack = node.value ?? new dynamic?[2];
        string funcName = pack[1] ?? "";
        List<string> modifiers = pack[0] ?? new List<string>();

        if (symbolTable.HasSymbol(funcName) && funcName != "")
            throw new EigerError(node.filename, node.line, node.pos, $"{funcName} already declared", EigerError.ErrorType.RuntimeError);

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

        InlineFunction f = new InlineFunction(node, funcName, argnames, root, symbolTable)
        {
            modifiers = modifiers
        };

        // add the function to the current scope
        if (funcName != "")
            symbolTable.CreateSymbol(funcName, f, node.filename, node.line, node.pos);

        // return the function
        return new()
        {
            result = f
        };
    }

    // visit unary operator node
    static ReturnResult VisitUnaryOpNode(ASTNode node, SymbolTable symbolTable)
    {
        // get unary operator
        string unaryOp = node.value ?? throw new EigerError(node.filename, node.line, node.pos, "Invalid Unary Operator", EigerError.ErrorType.RuntimeError);

        // get right side without unary operator
        Value rightSide = VisitNode(node.children[0], symbolTable).result;
        // prepare a variable for return value
        Value retVal = unaryOp switch
        {
            "-" => rightSide.Negative(),
            "not" => rightSide.Notted(),
            _ => throw new EigerError(node.filename, node.line, node.pos, "Invalid Unary Operator", EigerError.ErrorType.RuntimeError),
        };
        return new()
        {
            result = retVal
        };
    }

    // visit binary operator node
    static ReturnResult VisitBinOpNode(ASTNode node, SymbolTable symbolTable)
    {
        Value leftSide = new Nix(node.filename, node.line, node.pos);

        if (node.value != "=")
            leftSide = VisitNode(node.children[0], symbolTable).result;

        Value rightSide = VisitNode(node.children[1], symbolTable).result;

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
            "@" => leftSide.CreateArray(rightSide),
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

        return new()
        {
            result = retVal
        };
    }

    private static Value HandleAssignment(ASTNode node, Value rightSide, SymbolTable symbolTable)
    {
        Value leftValue = symbolTable.GetSymbol(node.children[0]);
        if (leftValue.modifiers.Contains("readonly"))
            throw new EigerError(leftValue.filename, leftValue.line, leftValue.pos, $"{leftValue} is read-only!", EigerError.ErrorType.RuntimeError);

        symbolTable.SetSymbol(node.children[0], rightSide);
        return rightSide;
    }

    private static Value HandleCompoundAssignment(ASTNode node, Value rightSide, SymbolTable symbolTable, Func<Value, Value, Value> operation)
    {
        Value leftValue = symbolTable.GetSymbol(node.children[0]);
        Value newValue = operation(leftValue, rightSide);
        if (leftValue.modifiers.Contains("readonly"))
            throw new EigerError(leftValue.filename, leftValue.line, leftValue.pos, $"{leftValue} is read-only!", EigerError.ErrorType.RuntimeError);
        else
            symbolTable.SetSymbol(node.children[0], newValue);
        return newValue;
    }


    // visit literal node (pretty self-explanatory)
    static ReturnResult VisitLiteralNode(ASTNode node, SymbolTable symbolTable)
    {
        Value v;
        if (node.value is string && node.value == "true")
            v = new Boolean(node.filename, node.line, node.pos, true);
        else if (node.value is string && node.value == "false")
            v = new Boolean(node.filename, node.line, node.pos, false);
        else if (node.value is string && node.value == "nix")
            v = new Nix(node.filename, node.line, node.pos);
        else v = Value.ToEigerValue(node.filename, node.line, node.pos, node.value);

        return new()
        {
            result = v
        };
    }

    // visit identifier node (pretty self-explanatory too)
    static ReturnResult VisitIdentifierNode(ASTNode node, SymbolTable symbolTable)
    {
        return new()
        {
            result = symbolTable.GetSymbol(node)
        };
    }

    // visit array node
    static ReturnResult VisitArrayNode(ASTNode node, SymbolTable symbolTable)
    {
        // ArrayNode structure
        // ArrayNode
        // -- element1
        // -- element2
        // -- ...

        List<Value> list = [];
        foreach (ASTNode item in node.children)
        {
            list.Add(VisitNode(item, symbolTable).result);
        }
        return new()
        {
            result = new Array(node.filename, node.line, node.pos, [.. list])
        };
    }

    private static ReturnResult VisitAttrAccessNode(ASTNode node, SymbolTable symbolTable)
    {
        // AttrAccessNode structure
        // AttrAccessNode
        // -- left
        // -- right

        Value v;
        ASTNode currentNode = node.children[1];
        bool isInSelf = node.children[0].type == NodeType.Identifier && node.children[0].value == "this";

        while (true)
        {
            if (currentNode.type != NodeType.AttrAccess)
            {
                v = VisitNode(node.children[0], symbolTable).result.GetAttr(node.children[1]);
                break;
            }
            currentNode = currentNode.children[1];
        }

        if (v.modifiers.Contains("private") && !isInSelf)
            throw new EigerError(node.filename, node.line, node.pos, "Attribute is private", EigerError.ErrorType.RuntimeError);

        if (v is BaseFunction f && currentNode.type == NodeType.FuncCall)
        {
            List<Value> args = []; // prepare a list for args
            foreach (ASTNode child in currentNode.children)
            {
                Value val = VisitNode(child, symbolTable).result ?? throw new EigerError(node.filename, node.line, node.pos, Globals.ArgumentErrorStr, EigerError.ErrorType.ArgumentError);
                args.Add(val);
            }
            return new()
            {
                result = f.Execute(args, node.line, node.pos, node.filename).result
            };
        }
        else if (v is Class c && currentNode.type == NodeType.FuncCall)
        {
            List<Value> args = []; // prepare a list for args
            foreach (ASTNode child in currentNode.children)
            {
                Value val = VisitNode(child, symbolTable).result ?? throw new EigerError(node.filename, node.line, node.pos, Globals.ArgumentErrorStr, EigerError.ErrorType.ArgumentError);
                args.Add(val);
            }
            return new()
            {
                result = c.Execute(args, symbolTable).result
            };
        }
        else
            return new()
            {
                result = v
            };
    }

    private static ReturnResult VisitElementAccessNode(ASTNode node, SymbolTable symbolTable)
    {
        // ElementAccessNode structure
        // ElementAccessNode
        // -- value
        // -- idx

        Value iter = VisitNode(node.children[0], symbolTable).result;
        Value value = VisitNode(node.children[1], symbolTable).result;

        try
        {
            int idx = Convert.ToInt32(((Number)(value ?? throw new EigerError(node.filename, node.line, node.pos, "Index is unknown", EigerError.ErrorType.ParserError))).value);
            return new()
            {
                result = iter.GetIndex(idx)
            };
        }
        catch (IndexOutOfRangeException)
        {
            throw new EigerError(node.filename, node.line, node.pos, Globals.IndexErrorStr, EigerError.ErrorType.IndexError);
        }
    }

    private static ReturnResult VisitIncludeNode(ASTNode node, SymbolTable symbolTable)
    {
        // IncludeNode structure
        // IncludeNode
        // -- path

        string path;

        if (node.children[0].type == NodeType.Identifier)
        {
            path = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "modules", node.children[0].value + Globals.fileExtension);
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

        return new()
        {
            result = new Nix(node.filename, node.line, node.pos)
        };
    }

    static List<Value> PrepareArguments(ASTNode node, SymbolTable symbolTable, bool variadic, int fncount)
    {
        List<Value> args = new List<Value>();

        if(variadic) {
            if(fncount > node.children.Count) 
                throw new EigerError(node.filename, node.line, node.pos, "Not enough arguments in variadic function", EigerError.ErrorType.ArgumentError);

            for (int i = 1; i < fncount; i++) {
                Value val = VisitNode(node.children[i], symbolTable).result ?? throw new EigerError(node.filename, node.line, node.pos, Globals.ArgumentErrorStr, EigerError.ErrorType.ArgumentError);
                val.modifiers.Clear();
                args.Add(val);
            }

            List<Value> varArgs = new();
            for(int i = fncount; i < node.children.Count; ++i) {
                Value val = VisitNode(node.children[i], symbolTable).result ?? throw new EigerError(node.filename, node.line, node.pos, Globals.ArgumentErrorStr, EigerError.ErrorType.ArgumentError);
                val.modifiers.Clear();
                varArgs.Add(val);
            }

            EigerLang.Execution.BuiltInTypes.Array varArgsSymbol = new("<varargs>", -1, -1, varArgs);
            args.Add(varArgsSymbol);   
        } else {
            for (int i = 1; i < node.children.Count; i++) {
                Value val = VisitNode(node.children[i], symbolTable).result ?? throw new EigerError(node.filename, node.line, node.pos, Globals.ArgumentErrorStr, EigerError.ErrorType.ArgumentError);
                val.modifiers.Clear();
                args.Add(val);
            }
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
}
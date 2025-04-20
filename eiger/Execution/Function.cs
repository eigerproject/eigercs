/*
 * EIGERLANG FUNCTION CLASSES
*/

using EigerLang.Errors;
using EigerLang.Execution.BuiltInTypes;
using EigerLang.Execution;
using EigerLang.Parsing;

using Boolean = EigerLang.Execution.BuiltInTypes.Boolean;

namespace EigerLang.Execution;

// a base function from which both custom and built-in functions will extend
abstract class BaseFunction(ASTNode node, string name, List<string> arg_n, SymbolTable symbolTable) : Value(node.filename, node.line, node.pos)
{
    public string name { get; } = name; // name
    public List<string> arg_n { get; } = arg_n; // argument names

    public SymbolTable symbolTable = symbolTable; // symbol table

    public void CheckArgs(string path, int line, int pos, int argCount)
    {
        // if the count of the given args and the required args are not equal
        if (argCount != arg_n.Count)
            throw new EigerError(path, line, pos, $"Function {name} takes {arg_n.Count} arguments, got {argCount}", EigerError.ErrorType.ArgumentError);
    }

    public abstract ReturnResult Execute(List<Value> args, int line, int pos, string path); // abstract execute function

    public override Boolean ComparisonEqeq(object other) {
        if(other is BaseFunction f)
            return new Boolean(filename, line, pos, name == f.name && arg_n.SequenceEqual(f.arg_n) && symbolTable == f.symbolTable);
        return new Boolean(filename, line, pos, false);
    }

    public override Boolean ComparisonNeqeq(object other) {
         if(other is BaseFunction f)
            return new Boolean(filename, line, pos, !(name == f.name && arg_n.SequenceEqual(f.arg_n) && symbolTable == f.symbolTable));
        return new Boolean(filename, line, pos, true);
    }
}

// custom functions
class Function : BaseFunction
{
    // function body
    ASTNode root;

    public Function(ASTNode node, string name, List<string> arg_n, ASTNode root, SymbolTable symbolTable) : base(node, name, arg_n, symbolTable)
    {
        this.root = root;
    }

    public override ReturnResult Execute(List<Value> args, int line, int pos, string path)
    {
        CheckArgs(path, line, pos, args.Count);

        // create local symbol table
        SymbolTable localSymbolTable = new(symbolTable);

        // add args to that local symbol table
        for (int i = 0; i < args.Count; i++)
            localSymbolTable.CreateSymbol(arg_n[i], args[i], path, line,pos);

        // visit the body and return the result
        return Interpreter.VisitBlockNode(root, localSymbolTable);
    }

    public override string ToString()
    {
        if (name == "") return "[function]";
        return $"[function {name}]";
    }
}

class InlineFunction : BaseFunction
{
    // function body
    ASTNode root;

    public InlineFunction(ASTNode node, string name, List<string> arg_n, ASTNode root, SymbolTable symbolTable) : base(node, name, arg_n, symbolTable)
    {
        this.root = root;
    }

    public override ReturnResult Execute(List<Value> args, int line, int pos, string path)
    {
        CheckArgs(path, line, pos, args.Count);

        // create local symbol table
        SymbolTable localSymbolTable = new(symbolTable);

        // add args to that local symbol table
        for (int i = 0; i < args.Count; i++)
            localSymbolTable.CreateSymbol(arg_n[i], args[i], path, line,pos);

        // visit the body and return the result
        ReturnResult r = Interpreter.VisitNode(root, localSymbolTable);
        r.shouldReturn = true;
        return r;
    }

    public override string ToString()
    {
        if (name == "") return "[inline function]";
        return $"[inline function {name}]";
    }
}

// built-in functions
abstract class BuiltInFunction : BaseFunction
{
    public BuiltInFunction(string name, List<string> arg_n) : base(new(NodeType.Block, 0, 0, 0, "<unset>"), name, arg_n, Interpreter.globalSymbolTable) { }

    public override ReturnResult Execute(List<Value> args, int line, int pos, string file) { return new() { result = new Nix(file, line, pos) }; }

    public override string ToString()
    {
        return $"[built-in function {name}]";
    }
}
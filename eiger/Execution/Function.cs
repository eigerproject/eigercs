using EigerLang.Errors;
using EigerLang.Parsing;

namespace EigerLang.Execution;

abstract class BaseFunction(string name, List<string> arg_n, Dictionary<string,dynamic?> symbolTable)
{
    public string name { get; } = name;
    public List<string> arg_n { get; } = arg_n;

    public Dictionary<string, dynamic?> symbolTable = symbolTable;

    public abstract dynamic Execute(List<dynamic> args);
}

class Function : BaseFunction
{
    ASTNode root;

    public Function(string name, List<string> arg_n, ASTNode root, Dictionary<string, dynamic?> symbolTable) : base(name,arg_n,symbolTable)
    {
        this.root = root;
    }

    public override dynamic Execute(List<dynamic> args)
    {
        if(args.Count != arg_n.Count)
        {
            throw new EigerError($"Function {name} takes {arg_n.Count} arguments, got {args.Count}");
        }

        Dictionary<string, dynamic?> localSymbolTable = new(symbolTable);

        for (int i = 0; i < args.Count;i++)
        {
            localSymbolTable[arg_n[i]] = args[i];
        }

        Interpreter.VisitBlockNode(root, localSymbolTable, out dynamic ret,out bool set);

        return ret;
    }

    public override string ToString()
    {
        return $"[function {name}]";
    }
}

abstract class BuiltInFunction : BaseFunction
{
    public BuiltInFunction(string name, List<string> arg_n) : base(name, arg_n,Interpreter.globalSymbolTable) {}

    public override dynamic Execute(List<dynamic> args) { return 0; }

    public override string ToString()
    {
        return $"[built-in function {name}]";
    }
}
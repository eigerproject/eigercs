/*
 * EIGERLANG FUNCTION CLASSES
 * WRITTEN BY VARDAN PETROSYAN
*/

using EigerLang.Errors;
using EigerLang.Parsing;

namespace EigerLang.Execution;

// a base function from which both custom and built-in functions will extend
abstract class BaseFunction(string name, List<string> arg_n, Dictionary<string,dynamic?> symbolTable)
{
    public string name { get; } = name; // name
    public List<string> arg_n { get; } = arg_n; // argument names

    public Dictionary<string, dynamic?> symbolTable = symbolTable; // symbol table

    public void CheckArgs(string path,int line,int pos,int argCount)
    {
        // if the count of the given args and the required args are not equal
        if (argCount != arg_n.Count)
            throw new EigerError(path, line, pos, $"Function {name} takes {arg_n.Count} arguments, got {argCount}");
    }

    public abstract (bool,dynamic?) Execute(List<dynamic> args,int line,int pos, string path); // abstract execute function
}

// custom functions
class Function : BaseFunction
{
    // function body
    ASTNode root;

    public Function(string name, List<string> arg_n, ASTNode root, Dictionary<string, dynamic?> symbolTable) : base(name,arg_n,symbolTable)
    {
        this.root = root;
    }

    public override (bool, dynamic?) Execute(List<dynamic> args,int line,int pos, string path)
    {
        CheckArgs(path,line,pos,args.Count);

        // create local symbol table
        Dictionary<string, dynamic?> localSymbolTable = new(symbolTable);

        // add args to that local symbol table
        for (int i = 0; i < args.Count;i++)
        {
            localSymbolTable[arg_n[i]] = args[i];
        }

        // visit the body and return the result
        return Interpreter.VisitBlockNode(root, localSymbolTable,symbolTable);
    }

    public override string ToString()
    {
        return $"[function {name}]";
    }
}

// built-in functions
abstract class BuiltInFunction : BaseFunction
{
    public BuiltInFunction(string name, List<string> arg_n) : base(name, arg_n,Interpreter.globalSymbolTable) {}

    public override (bool, dynamic?) Execute(List<dynamic> args,int line,int pos,string file) { return (false,null); }

    public override string ToString()
    {
        return $"[built-in function {name}]";
    }
}
using EigerLang.Parsing;
using EigerLang.Execution;

namespace EigerLang.Execution.BuiltInTypes;

public class Class : Value
{
    public readonly dynamic name;
    ASTNode blockNode;

    public Class(string filename, int line, int pos, string name, ASTNode blockNode) : base(filename, line, pos)
    {
        if (name == "new")
            throw new EigerLang.Errors.EigerError(filename, line, pos, "`new` is an invalid name for a class", Errors.EigerError.ErrorType.RuntimeError);
        this.name = name;
        this.blockNode = blockNode;
    }

    public ReturnResult Execute(List<Value> args, SymbolTable symbolTable)
    {
        // create local symbol table
        SymbolTable localSymbolTable = new(symbolTable);

        Instance inst = new(filename, line, pos, this, localSymbolTable);

        localSymbolTable.CreateSymbol("this", inst, "<setting `this`>", 0, 0);

        Interpreter.VisitBlockNode(blockNode, localSymbolTable);

        if (localSymbolTable.HasSymbol("new")) {
            Value value = localSymbolTable.GetSymbol("new", filename, line, pos);
            if (value is Function constructorFunc)
                constructorFunc.Execute(args, line, pos, filename);
        }

        return new() { result = inst };
    }

    public override Value GetAttr(ASTNode attr)
    {
        if (attr.value == "type")
        {
            return new String(filename, line, pos, "class");
        }
        return base.GetAttr(attr);
    }

    public override string ToString()
    {
        return $"[class {name}]";
    }

}
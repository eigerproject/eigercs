using EigerLang.Parsing;

namespace EigerLang.Execution.BuiltInTypes;

public class Class : Value
{
    public readonly dynamic name;
    public Dictionary<string, Value> symbolTable;
    ASTNode blockNode;

    public Class(string filename, int line, int pos, string name, Dictionary<string, Value> symbolTable, ASTNode blockNode) : base(filename, line, pos)
    {
        if (name == "new")
            throw new EigerLang.Errors.EigerError(filename, line, pos, "`new` is an invalid name for a class", Errors.EigerError.ErrorType.RuntimeError);
        this.filename = filename;
        this.line = line;
        this.pos = pos;
        this.name = name;
        this.symbolTable = symbolTable;
        this.blockNode = blockNode;
    }

    public (bool, bool, Instance) Execute(List<Value> args)
    {
        // create local symbol table
        Dictionary<string, Value> localSymbolTable = new(symbolTable);

        Instance inst = new(filename, line, pos, this, localSymbolTable, symbolTable);

        localSymbolTable["this"] = inst;

        Interpreter.VisitBlockNode(blockNode, localSymbolTable);

        localSymbolTable = Interpreter.GetDictionaryDifference(symbolTable, localSymbolTable);

        if (localSymbolTable.TryGetValue("new", out Value value))
            if (value is Function constructorFunc)
                constructorFunc.Execute(args, line, pos, filename);

        return (false, false, inst);
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
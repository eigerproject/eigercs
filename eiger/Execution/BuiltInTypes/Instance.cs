using EigerLang.Parsing;

namespace EigerLang.Execution.BuiltInTypes;

public class Instance : Value
{
    public readonly dynamic? value = null;
    string name;
    Class createdFrom;
    Dictionary<string, Value> symbolTable;
    Dictionary<string, Value> parentSymbolTable;

    public Instance(string filename, int line, int pos, Class createdFrom, Dictionary<string, Value> symbolTable, Dictionary<string, Value> parentSymbolTable) : base(filename, line, pos)
    {
        this.filename = filename;
        this.line = line;
        this.pos = pos;
        this.name = createdFrom.name;
        this.createdFrom = createdFrom;
        this.symbolTable = symbolTable;
        this.parentSymbolTable = parentSymbolTable;
    }

    public override Value GetAttr(ASTNode attr)
    {
        if (attr.value == "type")
        {
            return new String(filename, line, pos, "instance");
        }
        else if (attr.value == "baseclass")
        {
            return createdFrom;
        }
        return Interpreter.GetSymbol(symbolTable, attr);
    }

    public override void SetAttr(ASTNode attr, Value val)
    {
        Interpreter.SetSymbol(symbolTable, attr, val);
    }

    public override string ToString()
    {
        return $"[instance of class {name}]";
    }

}
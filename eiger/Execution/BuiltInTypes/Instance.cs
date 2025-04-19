using EigerLang.Parsing;

namespace EigerLang.Execution.BuiltInTypes;

public class Instance : Value
{
    public readonly dynamic? value = null;
    string name;
    Class createdFrom;
    SymbolTable symbolTable;

    public Instance(string filename, int line, int pos, Class createdFrom, SymbolTable symbolTable) : base(filename, line, pos)
    {
        this.filename = filename;
        this.line = line;
        this.pos = pos;
        this.name = createdFrom.name;
        this.createdFrom = createdFrom;
        this.symbolTable = symbolTable;
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
        return symbolTable.GetSymbol(attr);
    }

    public override void SetAttr(ASTNode attr, Value val)
    {
        symbolTable.SetSymbol(attr, val);
    }

    public override string ToString()
    {
        return $"[instance of class {name}]";
    }

}
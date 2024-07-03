using EigerLang.Parsing;

namespace EigerLang.Execution.BuiltInTypes;

public class Dataclass : Value
{
    public readonly dynamic name;
    public string filename;
    public int line, pos;
    public Dictionary<string, Value> symbolTable;

    public Dataclass(string filename, int line, int pos, string name, Dictionary<string, Value> symbolTable, ASTNode blockNode) : base(filename, line, pos)
    {
        this.filename = filename;
        this.line = line;
        this.pos = pos;
        this.name = name;


        // create local symbol table
        Dictionary<string, Value> localSymbolTable = new(symbolTable);

        localSymbolTable["this"] = this;

        this.symbolTable = localSymbolTable;

        Interpreter.VisitBlockNode(blockNode, localSymbolTable);
    }

    public override Value GetAttr(ASTNode attr)
    {
        if (attr.value == "type")
        {
            return new String(filename, line, pos, "dataclass");
        }
        return Interpreter.GetSymbol(symbolTable, attr);
    }

    public override void SetAttr(ASTNode attr, Value val)
    {
        Interpreter.SetSymbol(symbolTable, attr, val);
    }

    public override string ToString()
    {
        return $"[dataclass {name}]";
    }

}
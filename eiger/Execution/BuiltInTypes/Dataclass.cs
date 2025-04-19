using EigerLang.Parsing;

namespace EigerLang.Execution.BuiltInTypes;

public class Dataclass : Value
{
    public readonly dynamic name;
    public SymbolTable symbolTable;

    public Dataclass(string filename, int line, int pos, string name, SymbolTable symbolTable, ASTNode blockNode) : base(filename, line, pos)
    {
        this.filename = filename;
        this.line = line;
        this.pos = pos;
        this.name = name;

        // create local symbol table
        SymbolTable localSymbolTable = new(symbolTable);
        localSymbolTable.CreateSymbol("this", this, "<setting this>", 0, 0);

        this.symbolTable = localSymbolTable;

        Interpreter.VisitBlockNode(blockNode, localSymbolTable);
    }

    public override Value GetAttr(ASTNode attr)
    {
        if (attr.value == "type")
            return new String(filename, line, pos, "dataclass");

        return symbolTable.GetSymbol(attr);
    }

    public override void SetAttr(ASTNode attr, Value val)
    {
        symbolTable.SetSymbol(attr, val);
    }

    public override string ToString()
    {
        return $"[dataclass {name}]";
    }

}
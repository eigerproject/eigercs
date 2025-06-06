using EigerLang.Parsing;

namespace EigerLang.Execution.BuiltInTypes;

public class Namespace : Value
{
    public readonly dynamic name;
    public SymbolTable symbolTable;

    public Namespace(string filename, int line, int pos, string name, SymbolTable symbolTable, ASTNode blockNode) : base(filename, line, pos)
    {
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
            return new String(filename, line, pos, "namespace");

        return symbolTable.GetSymbol(attr, false);
    }

    public override void SetAttr(ASTNode attr, Value val)
    {
        symbolTable.SetSymbol(attr, val, false);
    }

    public override string ToString()
    {
        return $"[namespace {name}]";
    }

    public void AddDefinition(ASTNode root) {
        Interpreter.VisitBlockNode(root, symbolTable);
    }

}
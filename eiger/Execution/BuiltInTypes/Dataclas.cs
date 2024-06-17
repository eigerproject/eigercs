using EigerLang.Errors;
using EigerLang.Parsing;
using System;
using System.IO;

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
        this.symbolTable = symbolTable;

        Interpreter.VisitBlockNode(blockNode, symbolTable);
    }

    public override Value GetAttr(ASTNode attr)
    {
        if (attr.value == "type")
        {
            return new String(filename, line, pos, "[type Dataclass]");
        }
        return Interpreter.GetSymbol(symbolTable, attr);
    }

    public override string ToString()
    {
        return $"[dataclass {name}]";
    }

}
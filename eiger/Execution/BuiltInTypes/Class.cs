using EigerLang.Errors;
using EigerLang.Parsing;
using System;
using System.IO;

namespace EigerLang.Execution.BuiltInTypes;

public class Class : Value
{
    public readonly dynamic name;
    public string filename;
    public int line, pos;
    public Dictionary<string, Value> symbolTable;
    ASTNode blockNode;

    public Class(string filename, int line, int pos, string name, Dictionary<string, Value> symbolTable, ASTNode blockNode) : base(filename, line, pos)
    {
        this.filename = filename;
        this.line = line;
        this.pos = pos;
        this.name = name;
        this.symbolTable = symbolTable;
        this.blockNode = blockNode;
    }

    public (bool, Instance) Execute(List<Value> args)
    {
        // create local symbol table
        Dictionary<string, Value> localSymbolTable = new(symbolTable);

        Instance inst = new(filename, line, pos, this, localSymbolTable,symbolTable);

        localSymbolTable["this"] = inst;

        Interpreter.VisitBlockNode(blockNode, localSymbolTable);

        if (localSymbolTable.TryGetValue(name, out Value value))
            if (value is Function constructorFunc)
                constructorFunc.Execute(args, line, pos, filename);

        return (false, inst);
    }

    public override Value GetAttr(ASTNode attr)
    {
        if (attr.value == "type")
        {
            return new String(filename, line, pos, "[type Class]");
        }
        return base.GetAttr(attr);
    }

    public override string ToString()
    {
        return $"[class {name}]";
    }

}
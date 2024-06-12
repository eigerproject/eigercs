namespace EigerLang.Parsing;

public enum NodeType
{
    Block,
    If,
    While,
    Return,
    FuncCall,
    FuncDef,
    BinOp,
    Literal,
    Identifier
}
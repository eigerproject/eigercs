/*
 * EIGERLANG NODETYPE ENUM
*/

namespace EigerLang.Parsing;

public enum NodeType
{
    Block,
    Let,
    If,
    While,
    ForTo,
    Return,
    FuncCall,
    FuncDef,
    FuncDefInline,
    BinOp,
    Literal,
    Identifier,
    Array,
    ElementAccess,
    Class,
    AttrAccess,
    Namespace,
    Include,
    UnaryOp,
    Break,
    Continue
}
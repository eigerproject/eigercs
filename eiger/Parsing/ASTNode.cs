/*
 * EIGERLANG ASTNODE CLASS
 * WRITTEN BY VARDAN PETROSYAN
*/

namespace EigerLang.Parsing;

public class ASTNode(NodeType type, dynamic? value)
{
    public NodeType type = type;
    public List<ASTNode> children = new();
    public dynamic? value = value;

    public void AddChild(ASTNode node)
    {
        children.Add(node);
    }

    public void Print(int indent = 0)
    {
        Console.Write("├──");
        for (int i = 0; i < indent; ++i) Console.Write("──");
        Console.Write(" ");
        if (value != null)
            Console.WriteLine($"{type} : `{value}`");
        else
            Console.WriteLine($"{type}");
        foreach (ASTNode child in children)
        {
            child.Print(indent + 1);
        }
    }
}
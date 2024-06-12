using EigerLang.Errors;
using EigerLang.Tokenization;

namespace EigerLang.Parsing;

public class Parser(List<Token> tokens)
{
    int current;

    HashSet<TokenType> termOps = new HashSet<TokenType>
    {
        TokenType.MUL, TokenType.DIV, TokenType.EQEQ, TokenType.NEQEQ,
        TokenType.GT, TokenType.LT, TokenType.GTE, TokenType.LTE,
    };

    HashSet<TokenType> exprOps = new HashSet<TokenType>
    {
        TokenType.PLUS, TokenType.MINUS
    };


    public ASTNode Parse()
    {
        return StatementList();
    }

    Token Advance()
    {
        if (current < tokens.Count)
        {
            return tokens[current++];
        }
        throw new EigerError("<stdin>", tokens[tokens.Count - 1].line, tokens[tokens.Count - 1].pos, "Unexpected End of Input");
    }

    Token Peek()
    {
        if (current < tokens.Count)
        {
            return tokens[current];
        }
        return new Token(-1, -1, TokenType.UNDEFINED);
    }

    Token? PeekNext()
    {
        if (current + 1 < tokens.Count)
        {
            return tokens[current + 1];
        }
        return null;
    }

    void Match(TokenType type)
    {
        if (Peek().type == type)
        {
            Advance();
        }
        else
        {
            throw new EigerError("<stdin>", Peek().line, Peek().pos, $"Unexpected token `{Peek().value}`");
        }
    }

    void Match(TokenType type, dynamic expected)
    {
        if (Peek().type == type && Peek().value == expected)
        {
            Advance();
        }
        else
        {
            throw new EigerError("<stdin>", Peek().line, Peek().pos, $"Unexpected token `{Peek().value}`");
        }
    }

    ASTNode StatementList()
    {
        ASTNode root = new ASTNode(NodeType.Block, null);
        while (current < tokens.Count && Peek().value.ToString() != "end" && Peek().value.ToString() != "else")
        {
            root.AddChild(Statement());
        }
        return root;
    }

    ASTNode Statement()
    {
        Token peeked = Peek();
        switch (peeked.value)
        {
            case "if": return IfStatement();
            case "func": return FuncDefStatement();
            default: return Expr();
        }
    }

    ASTNode FuncDefStatement()
    {
        Match(TokenType.IDENTIFIER, "func");
        string funcName = Advance().value ?? "";

        ASTNode node = new(NodeType.FuncDef, funcName);
        List<ASTNode> args = [];

        Match(TokenType.LPAREN);

        if (Peek().type == TokenType.RPAREN) { Match(TokenType.RPAREN); }
        else
        {
            do
            {
                if (Peek().type == TokenType.COMMA) Advance();

                args.Add(Factor());

            } while (Peek().type == TokenType.COMMA);
        }

        Match(TokenType.RPAREN);

        ASTNode rootNode = StatementList();

        node.AddChild(rootNode);

        foreach(var arg in args)
        {
            node.AddChild(arg);
        }

        return node;
    }

    ASTNode VarAccessStatement()
    {
        if (PeekNext() != null)
            if (PeekNext().type == TokenType.LPAREN)
                return FunctionCallStatement();

        return new ASTNode(NodeType.Identifier, Advance().value);
    }

    ASTNode FunctionCallStatement()
    {
        Token funcName = Advance();
        Match(TokenType.LPAREN);
        ASTNode node = new(NodeType.FuncCall, funcName.value);

        if (Peek().type == TokenType.RPAREN) { Match(TokenType.RPAREN); return node; }

        do
        {
            if (Peek().type == TokenType.COMMA) Advance();

            node.AddChild(Expr());

        } while (Peek().type == TokenType.COMMA);

        Match(TokenType.RPAREN);

        return node;
    }

    ASTNode IfStatement()
    {
        Match(TokenType.IDENTIFIER, "if");
        ASTNode condition = Expr();
        Match(TokenType.IDENTIFIER, "then");
        ASTNode thenBranch = StatementList();
        ASTNode? elseBranch = null;
        if (Peek().value == "else")
        {
            Match(TokenType.IDENTIFIER, "else");
            elseBranch = StatementList();
        }
        Match(TokenType.IDENTIFIER, "end");

        ASTNode ifNode = new ASTNode(NodeType.If, null);
        ifNode.AddChild(condition);
        ifNode.AddChild(thenBranch);
        if (elseBranch != null)
        {
            ifNode.AddChild(elseBranch);
        }
        return ifNode;
    }

    ASTNode EmitStatement()
    {
        Match(TokenType.IDENTIFIER, "emit");
        Match(TokenType.LPAREN);
        ASTNode arg = Expr();
        Match(TokenType.RPAREN);

        ASTNode printNode = new ASTNode(NodeType.FuncCall, "emit");
        printNode.AddChild(arg);
        return printNode;
    }

    ASTNode Expr()
    {
        ASTNode node = Term();
        while (true)
        {
            TokenType tokenType = Peek().type;

            // Handle assignment operators
            if (tokenType == TokenType.EQ)
            {
                Token op = Advance();
                ASTNode right = Expr();
                ASTNode binOpNode = new ASTNode(NodeType.BinOp, op.value);
                binOpNode.AddChild(node);
                binOpNode.AddChild(right);
                node = binOpNode;
            }
            else if (exprOps.Contains(tokenType))
            {
                Token op = Advance();
                ASTNode right = Term();
                ASTNode binOpNode = new ASTNode(NodeType.BinOp, op.value);
                binOpNode.AddChild(node);
                binOpNode.AddChild(right);
                node = binOpNode;
            }
            else
            {
                break;
            }
        }
        return node;
    }

    ASTNode Term()
    {
        ASTNode node = Factor();
        while (termOps.Contains(Peek().type))
        {
            Token op = Advance();
            ASTNode right = Factor();
            ASTNode binOpNode = new ASTNode(NodeType.BinOp, op.value);
            binOpNode.AddChild(node);
            binOpNode.AddChild(right);
            node = binOpNode;
        }
        return node;
    }

    ASTNode Factor()
    {
        if (Peek().type == TokenType.NUMBER)
        {
            return new ASTNode(NodeType.Literal, Advance().value);
        }
        else if (Peek().type == TokenType.IDENTIFIER)
        {
            if (PeekNext() != null && PeekNext().type == TokenType.LPAREN)
            {
                return FunctionCallStatement();
            }
            else
            {
                return new ASTNode(NodeType.Identifier, Advance().value);
            }
        }
        else if (Peek().type == TokenType.STRING)
        {
            return new ASTNode(NodeType.Literal, Advance().value);
        }
        else if (Peek().type == TokenType.LPAREN)
        {
            Match(TokenType.LPAREN);
            ASTNode node = Expr();
            Match(TokenType.RPAREN);
            return node;
        }
        else
        {
            throw new EigerError("<stdin>", Peek().line, Peek().pos, $"Unexpected Token: {Peek()}");
        }
    }
}
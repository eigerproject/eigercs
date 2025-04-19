using EigerLang.Execution.BuiltInTypes;
using EigerLang.Execution;
using EigerLang.Parsing;
using EigerLang.Errors;

namespace EigerLang.Execution
{
    public class SymbolTable
    {
        Dictionary<string, Value> values;
        SymbolTable parent;

        public SymbolTable(SymbolTable _parent)
        {
            this.parent = _parent;
            values = new();
        }

        public SymbolTable(SymbolTable _parent, Dictionary<string, Value> _values)
        {
            this.parent = _parent;
            this.values = _values;
        }

        public bool HasSymbol(string key) {
            if (values.ContainsKey(key)) return true;
            else if (parent != null) return parent.HasSymbol(key);
            else return false;
        }

        public void CreateSymbol(string key, Value val, string filename, int line, int pos) {
            //if(HasSymbol(key))
            //    throw new EigerError(filename, line, pos, $"{key} is already declared", EigerError.ErrorType.RuntimeError);
            /*else*/ values[key] = val;
        }

        public void SetSymbol(ASTNode key, Value value) {
            try
            {
                if (key.type == NodeType.ElementAccess)
                {
                    ASTNode listNode = key.children[0];
                    ASTNode idxNode = key.children[1];
                    Value list = GetSymbol(listNode);
                    int idx = (int)((Number)Interpreter.VisitNode(idxNode, this).result).value;
                    list.SetIndex(idx, value);
                }
                else if (key.type == NodeType.Identifier)
                {
                    value.modifiers = values[key.value].modifiers;
                    values[key.value] = value;
                }
                else if (key.type == NodeType.AttrAccess)
                {
                    ASTNode leftNode = key.children[0];
                    ASTNode rightNode = key.children[1];
                    values[leftNode.value].SetAttr(rightNode, value);
                }
                else
                {
                    throw new EigerError(key.filename, key.line, key.pos, $"Left side of assignment is invalid node of type {key.type}", EigerError.ErrorType.RuntimeError);
                }
            }
            catch (KeyNotFoundException)
            {
                if(parent != null) parent.SetSymbol(key, value);
                else throw new EigerError(key.filename, key.line, key.pos, $"Setting to undefined symbol", EigerError.ErrorType.RuntimeError);
            }
        }

        public void SetSymbol(string key, Value value, string filename, int line, int pos) {
            if(values.ContainsKey(key))
                values[key] = value;
            else if (parent != null)
                parent.SetSymbol(key, value, filename, line, pos);
            else throw new EigerError(filename, line, pos, $"Setting to undefined symbol {key}", EigerError.ErrorType.RuntimeError); 
        }

        public Value GetSymbol(string key, string filename, int line, int pos) {
            if(values.ContainsKey(key)) return values[key];
            else if (parent != null) return parent.GetSymbol(key, filename, line, pos);
            else throw new EigerError(filename, line, pos, $"{key} is undefined", EigerError.ErrorType.RuntimeError); 
        }

        public Value GetSymbol(ASTNode key)
        {
            string err_key = "";
            try
            {
                if (key.type == NodeType.Identifier)
                {
                    err_key = key.value ?? "";
                    return values[key.value];
                }
                else if (key.type == NodeType.FuncCall)
                {
                    err_key = key.children[0].value ?? "";
                    return values[key.children[0].value];
                }
                else if (key.type == NodeType.ElementAccess)
                {
                    ASTNode listNode = key.children[0];
                    ASTNode idxNode = key.children[1];

                    Value list = GetSymbol(listNode);

                    int idx = (int)((Number)Interpreter.VisitNode(idxNode, this).result).value;

                    return list.GetIndex(idx);
                }
                else if (key.type == NodeType.AttrAccess)
                {
                    ASTNode leftNode = key.children[0];
                    ASTNode rightNode = key.children[1];

                    err_key = leftNode.value ?? "";

                    return values[leftNode.value].GetAttr(rightNode);
                }
                else throw new EigerError(key.filename, key.line, key.pos, $"Invalid Node {key.type}", EigerError.ErrorType.ParserError);
            }
            catch (KeyNotFoundException)
            {
                if(parent != null) return parent.GetSymbol(key);
                else
                throw new EigerError(key.filename, key.line, key.pos, $"{err_key} is undefined", EigerError.ErrorType.RuntimeError);
            }
        }
    }
}

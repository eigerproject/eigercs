using EigerLang.Execution.BuiltInTypes;
using EigerLang.Errors;
using EigerLang.Parsing;

namespace EigerLang.Execution
{
    public class SymbolTable
    {
        private readonly Dictionary<string, Value> values;
        private readonly SymbolTable? parent;
        private readonly Dictionary<string, SymbolTable>? lookupCache;

        public SymbolTable(SymbolTable? _parent)
        {
            parent = _parent;
            values = new();

            if (_parent != null) lookupCache = new();
        }

        public SymbolTable(SymbolTable? _parent, Dictionary<string, Value> _values)
        {
            parent = _parent;
            values = _values;

            if (_parent != null) lookupCache = new();
        }

        public bool HasSymbol(string key)
        {
            for (SymbolTable? current = this; current != null; current = current.parent)
                if (current.values.ContainsKey(key)) return true;

            return false;
        }

        public void CreateSymbol(string key, Value val, string filename, int line, int pos)
        {
            if (values.ContainsKey(key))
                throw new EigerError(filename, line, pos, $"{key} is already declared", EigerError.ErrorType.RuntimeError);

            values[key] = val;
            lookupCache?.Remove(key); // If caching, reset cache
        }

        public void SetSymbol(ASTNode key, Value value, bool checkParents = true)
        {
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
                    SymbolTable? targetScope = ResolveSymbolTable(key.value, checkParents);
                    if (targetScope == null) throw new KeyNotFoundException();

                    value.modifiers = targetScope.values[key.value].modifiers;
                    targetScope.values[key.value] = value;
                }
                else if (key.type == NodeType.AttrAccess)
                {
                    ASTNode leftNode = key.children[0];
                    Value leftVal = GetSymbol(leftNode);
                    ASTNode rightNode = key.children[1];
                    leftVal.SetAttr(rightNode, value);
                }
                else
                    throw new EigerError(key.filename, key.line, key.pos, $"Left side of assignment is invalid node of type {key.type}", EigerError.ErrorType.RuntimeError);
            }
            catch (KeyNotFoundException)
            {
                if (parent != null && checkParents) parent.SetSymbol(key, value);
                else throw new EigerError(key.filename, key.line, key.pos, $"Setting to undefined symbol", EigerError.ErrorType.RuntimeError);
            }
        }

        public void SetSymbol(string key, Value value, string filename, int line, int pos)
        {
            SymbolTable? targetScope = ResolveSymbolTable(key);
            if (targetScope != null)
                targetScope.values[key] = value;
            else
                throw new EigerError(filename, line, pos, $"Setting to undefined symbol {key}", EigerError.ErrorType.RuntimeError);
        }

        public Value GetSymbol(string key, string filename, int line, int pos)
        {
            SymbolTable? targetScope = ResolveSymbolTable(key);
            if (targetScope != null)
                return targetScope.values[key];
            else
                throw new EigerError(filename, line, pos, $"{key} is undefined", EigerError.ErrorType.RuntimeError);
        }

        public Value GetSymbol(ASTNode key, bool checkParents = true)
        {
            string err_key = "";

            try
            {
                if (key.type == NodeType.Identifier)
                {
                    err_key = key.value ?? "";
                    SymbolTable? targetScope = ResolveSymbolTable(key.value, checkParents);
                    if (targetScope != null)
                        return targetScope.values[key.value];

                    throw new KeyNotFoundException();
                }
                else if (key.type == NodeType.FuncCall)
                {
                    err_key = key.children[0].value ?? "";
                    SymbolTable? targetScope = ResolveSymbolTable(key.children[0].value, checkParents);
                    if (targetScope != null)
                        return targetScope.values[key.children[0].value];

                    throw new KeyNotFoundException();
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
                    Value leftVal = GetSymbol(leftNode);
                    ASTNode rightNode = key.children[1];

                    return leftVal.GetAttr(rightNode);
                }
                else
                    throw new EigerError(key.filename, key.line, key.pos, $"Invalid Node {key.type}", EigerError.ErrorType.ParserError);
            }
            catch (KeyNotFoundException)
            {
                if (parent != null && checkParents) return parent.GetSymbol(key);
                throw new EigerError(key.filename, key.line, key.pos, $"{err_key} is undefined", EigerError.ErrorType.RuntimeError);
            }
        }

        private SymbolTable? ResolveSymbolTable(string key, bool checkParents = true)
        {
            if (lookupCache != null && lookupCache.TryGetValue(key, out var cached))
                return cached;

            for (SymbolTable? current = this; current != null; current = checkParents ? current.parent : null)
                if (current.values.ContainsKey(key))
                {
                    lookupCache?.Add(key, current);
                    return current;
                }
            return null;
        }
    }
}

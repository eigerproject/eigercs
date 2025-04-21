using EigerLang.Errors;
using EigerLang.Parsing;

namespace EigerLang.Execution.BuiltInTypes;

public class Value(string _filename, int _line, int _pos)
{
    private List<string>? _modifiers;
    public List<string> modifiers
    {
        get => _modifiers ??= new List<string>();
        set => _modifiers = value;
    }

    public string filename = _filename;
    public int line = _line, pos = _pos;

    public virtual Value AddedTo(object other)
    {
        throw new EigerError(filename, line, pos, $"{Globals.InvalidOperationStr}: {this.GetType().Name} + {other.GetType().Name}", EigerError.ErrorType.InvalidOperationError);
    }

    public virtual Value SubbedBy(object other)
    {
        throw new EigerError(filename, line, pos, $"{Globals.InvalidOperationStr}: {this.GetType().Name} - {other.GetType().Name}", EigerError.ErrorType.InvalidOperationError);
    }
    public virtual Value MultedBy(object other)
    {
        throw new EigerError(filename, line, pos, $"{Globals.InvalidOperationStr}: {this.GetType().Name} * {other.GetType().Name}", EigerError.ErrorType.InvalidOperationError);
    }

    public virtual Value DivedBy(object other)
    {
        throw new EigerError(filename, line, pos, $"{Globals.InvalidOperationStr}: {this.GetType().Name} / {other.GetType().Name}", EigerError.ErrorType.InvalidOperationError);
    }

    public virtual Value ModdedBy(object other)
    {
        throw new EigerError(filename, line, pos, $"{Globals.InvalidOperationStr}: {this.GetType().Name} % {other.GetType().Name}", EigerError.ErrorType.InvalidOperationError);
    }

    public virtual Value CreateArray(object other)
    {
        throw new EigerError(filename, line, pos, $"{Globals.InvalidOperationStr}: {this.GetType().Name} @ {other.GetType().Name}", EigerError.ErrorType.InvalidOperationError);
    }

    public virtual Value ExpedBy(object other)
    {
        throw new EigerError(filename, line, pos, $"{Globals.InvalidOperationStr}: {this.GetType().Name} ^ {other.GetType().Name}", EigerError.ErrorType.InvalidOperationError);
    }

    public virtual Value Notted()
    {
        throw new EigerError(filename, line, pos, $"{Globals.InvalidOperationStr}: not {this.GetType().Name}", EigerError.ErrorType.InvalidOperationError);
    }

    public virtual Value Negative()
    {
        throw new EigerError(filename, line, pos, $"{Globals.InvalidOperationStr}: -{this.GetType().Name}", EigerError.ErrorType.InvalidOperationError);
    }

    public virtual Boolean ComparisonEqeq(object other)
    {
        if (other is Nix)
            return new Boolean(filename, line, pos, this is Nix);

        throw new EigerError(filename, line, pos, $"{Globals.InvalidOperationStr}: {this.GetType().Name} ?= {other.GetType().Name}", EigerError.ErrorType.InvalidOperationError);
    }

    public virtual Boolean ComparisonNeqeq(object other)
    {
        if (other is Nix)
            return new Boolean(filename, line, pos, this is not Nix);

        throw new EigerError(filename, line, pos, $"{Globals.InvalidOperationStr}: {this.GetType().Name} != {other.GetType().Name}", EigerError.ErrorType.InvalidOperationError);
    }

    public virtual Boolean ComparisonGT(object other)
    {
        throw new EigerError(filename, line, pos, $"{Globals.InvalidOperationStr}: {this.GetType().Name} > {other.GetType().Name}", EigerError.ErrorType.InvalidOperationError);
    }

    public virtual Boolean ComparisonLTE(object other)
    {
        throw new EigerError(filename, line, pos, $"{Globals.InvalidOperationStr}: {this.GetType().Name} <= {other.GetType().Name}", EigerError.ErrorType.InvalidOperationError);
    }

    public virtual Boolean ComparisonGTE(object other)
    {
        throw new EigerError(filename, line, pos, $"{Globals.InvalidOperationStr}: {this.GetType().Name} >= {other.GetType().Name}", EigerError.ErrorType.InvalidOperationError);
    }

    public virtual Boolean ComparisonLT(object other)
    {
        throw new EigerError(filename, line, pos, $"{Globals.InvalidOperationStr}: {this.GetType().Name} < {other.GetType().Name}", EigerError.ErrorType.InvalidOperationError);
    }

    public virtual Value GetIndex(int idx)
    {
        throw new EigerError(filename, line, pos, $"Object of type {this.GetType().Name} is not iterable", EigerError.ErrorType.RuntimeError);
    }

    public virtual void SetIndex(int idx, Value val)
    {
        throw new EigerError(filename, line, pos, $"Object of type {this.GetType().Name} is not iterable", EigerError.ErrorType.RuntimeError);
    }

    public virtual Value GetLength()
    {
        throw new EigerError(filename, line, pos, "Object of type {this.GetType().Name} has no length", EigerError.ErrorType.RuntimeError);
    }

    public virtual Value GetAttr(ASTNode attr)
    {
        Value retVal;
        if (attr.type == NodeType.AttrAccess)
            retVal = GetAttr(attr.children[0]).GetAttr(attr.children[1]);
        else if (attr.value == "asString")
            retVal = new String(filename, line, pos, ToString() ?? "");
        else if (attr.value == "length")
            retVal = GetLength();
        else
            throw new EigerError(filename, line, pos, "Attribute not found", EigerError.ErrorType.RuntimeError);

        if (retVal.modifiers.Contains("private"))
            throw new EigerError(filename, line, pos, "Attribute is private", EigerError.ErrorType.RuntimeError);

        return retVal;
    }

    public virtual void SetAttr(ASTNode attr, Value val)
    {
        throw new EigerError(filename, line, pos, "Attribute not found", EigerError.ErrorType.RuntimeError);
    }

    public static Value ToEigerValue(string filename, int line, int pos, dynamic val)
    {
        if (val is double || val is int)
        {
            return new Number(filename, line, pos, val);
        }
        else if (val is string)
        {
            return new String(filename, line, pos, val);
        }
        throw new EigerError(filename, line, pos, "Invalid Data type", EigerError.ErrorType.ParserError);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        if (obj is null || obj.GetType() != this.GetType()) return false;
        return ComparisonEqeq(obj).value;
    }

    public override int GetHashCode() => HashCode.Combine(filename, line, pos);
}
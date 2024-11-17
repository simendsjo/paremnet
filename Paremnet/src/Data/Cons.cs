using Paremnet.Error;
using System.Collections.Generic;

namespace Paremnet.Data;

/// <summary>
/// Cons cell, contains first and rest (car and cdr) elements.
/// </summary>
public class Cons(Val first, Val rest)
{
    /// <summary> First value of this cons cell </summary>
    public Val First = first;

    /// <summary> Second value of this cons cell </summary>
    public Val Rest = rest;

    // scheme-like accessors

    /// <summary> Shorthand for the second element of the list </summary>
    public Val Second => GetNthCons(1).First;
    /// <summary> Shorthand for the third element of the list </summary>
    public Val Third => GetNthCons(2).First;
    /// <summary> Shorthand for the third element of the list </summary>
    public Val Fourth => GetNthCons(3).First;

    /// <summary> Shorthand for cdr, the sublist after the first element (so second element and beyond) </summary>
    public Val AfterFirst => Rest;
    /// <summary> Shorthand for the sublist after the second element (so third element and beyond) </summary>
    public Val AfterSecond => GetNthCons(1).Rest;
    /// <summary> Shorthand for the sublist after the third element (so fourth element and beyond) </summary>
    public Val AfterThird => GetNthCons(2).Rest;


    /// <summary> Retrieves Nth cons cell in the list, 0-indexed, as an O(N) operation. 
    /// List must have enough elements, otherwise an error will be thrown. </summary>
    public Cons GetNthCons(int n)
    {
        Cons cons = this;
        while (n-- > 0)
        {
            cons = cons.Rest.AsConsOrNull;
            if (cons == null) { throw new LanguageError("List operation out of bounds"); }
        }
        return cons;
    }

    /// <summary> Retrieves Nth element in the list, 0-indexed, as an O(N) operation. 
    /// List must have enough elements, otherwise an error will be thrown. </summary>
    public Val GetNth(int n) => GetNthCons(n).First;

    /// <summary> Retrieves tail of the Nth cons cell in the list, 0-indexed, as an O(N) operation. 
    /// List must have enough elements, otherwise an error will be thrown. </summary>
    public Val GetNthTail(int n) => GetNthCons(n).Rest;

    /// <summary> Helper function: converts a cons list into a native list </summary>
    public List<Val> ToNativeList() => ToNativeList(new Val(this));


    /// <summary> 
    /// Helper function: converts an array of arguments to a cons list.
    /// Whether it's null-terminated or not depends on the existence of a "." in the penultimate position.
    /// </summary>
    public static Val MakeList(List<Val> values)
    {
        int len = values.Count;
        bool dotted = (len >= 3 && values[len - 2].AsSymbolOrNull?.FullName == ".");

        // the tail should be either the last value, or a cons containing the last value
        Val result =
            dotted ? values[len - 1] :
            len >= 1 ? new Cons(values[len - 1], Val.NIL) :
            Val.NIL;

        int iterlen = dotted ? len - 3 : len - 2;
        for (int i = iterlen; i >= 0; i--)
        {
            result = new Cons(values[i], result);
        }
        return result;
    }

    /// <summary> 
    /// Helper function: converts an enumeration of native values to a proper (nil-terminated) cons list
    /// </summary>
    public static Val MakeListFromNative<T>(IEnumerable<T> values)
    {
        Cons first = null, last = null;
        foreach (T value in values)
        {
            Cons newcell = new(new Val(value), Val.NIL);
            if (first == null)
            {
                first = newcell;
            }
            else
            {
                last.Rest = newcell;
            }
            last = newcell;
        }

        return first ?? Val.NIL;
    }

    /// <summary> Helper function: converts a single value to a cons list. </summary>
    public static Cons MakeList(Val first) => new(first, Val.NIL);

    /// <summary> Helper function: converts two values to a cons list. </summary>
    public static Cons MakeList(Val first, Val second) => new(first, new Cons(second, Val.NIL));

    /// <summary> Helper function: converts three values to a cons list. </summary>
    public static Cons MakeList(Val first, Val second, Val third) => new(first, new Cons(second, new Cons(third, Val.NIL)));

    /// <summary> Helper function: converts a cons list into a native list </summary>
    public static List<Val> ToNativeList(Val element)
    {
        List<Val> results = [];
        while (element.IsNotNil)
        {
            if (element.IsAtom)
            {
                throw new LanguageError("Only null-terminated lists of cons cells can be converted to arrays!");
            }
            Cons cons = element.AsCons;
            results.Add(cons.First);
            element = cons.Rest;
        }
        return results;
    }

    /// <summary> Returns true if the value is a cons cell </summary>
    public static bool IsCons(Val value) => value.IsCons;

    /// <summary> Returns true if the value is an atom, ie. not a cons cell </summary>
    public static bool IsAtom(Val value) => !value.IsCons;

    /// <summary> Returns true if the value is nil </summary>
    public static bool IsNil(Val value) => value.IsNil;

    /// <summary> Returns true if the value is a properly nil-terminated cons list </summary>
    public static bool IsList(Val value)
    {
        if (value.IsNil) { return true; }

        Cons cons = value.AsConsOrNull;
        while (cons != null)
        {
            if (cons.Rest.IsNil) { return true; } // found our terminating NIL
            cons = cons.Rest.AsConsOrNull;
        }
        return false;
    }

    /// <summary> Returns the number of cons cells in the list, starting at value. O(n) operation. </summary>
    public static int Length(Val value) => Length(value.AsConsOrNull);

    /// <summary> Returns the number of cons cells in the list, starting at value. O(n) operation. </summary>
    public static int Length(Cons cons)
    {
        int result = 0;
        while (cons != null)
        {
            result++;
            cons = cons.Rest.AsConsOrNull;
        }
        return result;
    }
}

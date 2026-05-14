using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace BlazOrbit.Abstractions;

public sealed class SelectionTypeInfo
{
    private readonly Func<IEnumerable<object>, object?> _createValue;
    private readonly Func<object?, IEnumerable<object>> _extractValues;

    /// <param name="valueType">Concrete CLR type of the selection value. May be a scalar
    /// (single-select), an array (<c>T[]</c>) or a generic collection
    /// (<see cref="List{T}"/>, <see cref="HashSet{T}"/>). For multi-select collection types
    /// the ctor needs the parameterless constructor and an <c>Add</c> method preserved on
    /// <paramref name="valueType"/>.</param>
    [RequiresUnreferencedCode("SelectionTypeInfo activates the supplied collection type at runtime (Activator.CreateInstance + GetMethod(\"Add\")); the trimmer must preserve its parameterless constructor and Add method.")]
    [RequiresDynamicCode("Constructing a closed generic List<T>/HashSet<T> at runtime via Activator.CreateInstance is unsupported by AOT when the element type was not statically observed.")]
    public SelectionTypeInfo(Type valueType)
    {
        ValueType = valueType;

        if (valueType.IsArray)
        {
            IsMultiple = true;
            ElementType = valueType.GetElementType()!;
        }
        else if (valueType.IsGenericType &&
                 typeof(IEnumerable).IsAssignableFrom(valueType) &&
                 valueType != typeof(string))
        {
            IsMultiple = true;
            ElementType = valueType.GetGenericArguments()[0];
        }
        else
        {
            IsMultiple = false;
            ElementType = valueType;
        }

        _extractValues = BuildExtractValuesFunc();
        _createValue = BuildCreateValueFunc();
    }

    public Type ElementType { get; }
    public bool IsMultiple { get; }
    public Type ValueType { get; }

    public bool ContainsValue(object? collection, object? value) => collection != null && value != null &&
                                                                    (!IsMultiple
                                                                        ? ValuesEqual(collection, value)
                                                                        : ExtractValues(collection)
                                                                            .Any(v => ValuesEqual(v, value)));

    public TValue CreateValue<TValue>(IEnumerable<object> values)
        => (TValue)_createValue(values)!;

    public IEnumerable<object> ExtractValues(object? value)
        => _extractValues(value);

    public bool ValuesEqual(object? a, object? b) =>
        (a == null && b == null) || (a != null && b != null && a.Equals(b));

    private Func<IEnumerable<object>, object?> BuildCreateValueFunc()
    {
        if (!IsMultiple)
        {
            return values =>
            {
                List<object> list = values.ToList();
                return list.Count > 0 ? list[0] : null;
            };
        }

        if (ValueType.IsArray)
        {
            return values =>
            {
                List<object> list = values.ToList();
                Array array = Array.CreateInstance(ElementType, list.Count);
                for (int i = 0; i < list.Count; i++)
                {
                    array.SetValue(list[i], i);
                }

                return array;
            };
        }

        if (ValueType.IsGenericType)
        {
            Type genericDef = ValueType.GetGenericTypeDefinition();

            if (genericDef == typeof(List<>))
            {
                return values =>
                {
                    IList list = (IList)Activator.CreateInstance(ValueType)!;
                    foreach (object value in values)
                    {
                        list.Add(value);
                    }

                    return list;
                };
            }

            if (genericDef == typeof(HashSet<>))
            {
                System.Reflection.MethodInfo? addMethod = ValueType.GetMethod("Add");
                return values =>
                {
                    object set = Activator.CreateInstance(ValueType)!;
                    foreach (object value in values)
                    {
                        addMethod?.Invoke(set, [value]);
                    }

                    return set;
                };
            }
        }

        return values =>
        {
            List<object> list = values.ToList();
            Array array = Array.CreateInstance(ElementType, list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                array.SetValue(list[i], i);
            }

            return array;
        };
    }

    private Func<object?, IEnumerable<object>> BuildExtractValuesFunc()
    {
        return value => value == null
            ? []
            : IsMultiple && value is IEnumerable enumerable and not string
                ? enumerable.Cast<object>()
                : [value];
    }
}
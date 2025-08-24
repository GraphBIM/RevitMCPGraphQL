using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using GraphQLParser.AST;
using GraphQL.Types;

namespace RevitMCPGraphQL.GraphQL.Types;

// Simple scalar that passes through IDictionary<string,string> values for JSON serialization
public sealed class StringMapScalarType : ScalarGraphType
{
    public StringMapScalarType()
    {
        Name = "StringMap";
        Description = "A key-value map of strings to strings.";
    }

    public override object? Serialize(object? value)
    {
        if (value is IDictionary dict)
            return dict;

        if (value is IDictionary<string, string> s2s)
            return s2s;

        if (value is IDictionary<string, object?> s2o)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in s2o)
                result[kv.Key] = kv.Value?.ToString() ?? string.Empty;
            return result;
        }

        return null;
    }

    public override object? ParseValue(object? value) => value;

    public override object? ParseLiteral(GraphQLValue value)
    {
        if (value is GraphQLObjectValue ov)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var fields = ov.Fields;
            if (fields == null)
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var field in fields)
            {
                var key = field.Name.StringValue;
                string? str = null;
                var v = field.Value;
                if (v is GraphQLStringValue sv) str = sv.Value.ToString();
                else if (v is GraphQLIntValue iv) str = iv.Value.ToString();
                else if (v is GraphQLFloatValue fv) str = fv.Value.ToString();
                else if (v is GraphQLBooleanValue bv) str = bv.Value.ToString();
                else if (v is GraphQLNullValue) str = null;
                else str = v?.ToString();
                dict[key] = str ?? string.Empty;
            }
            return dict;
        }
        return null;
    }
}

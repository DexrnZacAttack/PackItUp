using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;

namespace PackItUp;

public static class JsonSchemaOptions
{
    // https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/extract-schema#transform-the-generated-schema
    public static JsonNode TransformAddDescriptions(JsonSchemaExporterContext context, JsonNode schema)
    {
        // Determine if a type or property and extract the relevant attribute provider.
        ICustomAttributeProvider? attributeProvider = context.PropertyInfo is not null
                                                          ? context.PropertyInfo.AttributeProvider
                                                          : context.TypeInfo.Type;

        // Look up any description attributes.
        DescriptionAttribute? descriptionAttr = attributeProvider?
                                               .GetCustomAttributes(inherit: true)
                                               .Select(attr => attr as DescriptionAttribute)
                                               .FirstOrDefault(attr => attr is not null);

        // Apply description attribute to the generated schema.
        if (descriptionAttr != null)
        {
            if (schema is not JsonObject jObj)
            {
                // Handle the case where the schema is a Boolean.
                JsonValueKind valueKind = schema.GetValueKind();
                Debug.Assert(valueKind is JsonValueKind.True or JsonValueKind.False);
                schema = jObj = new JsonObject();
                if (valueKind is JsonValueKind.False)
                {
                    jObj.Add("not", true);
                }
            }

            jObj.Insert(0, "description", descriptionAttr.Description);
        }

        return schema;
    }
}
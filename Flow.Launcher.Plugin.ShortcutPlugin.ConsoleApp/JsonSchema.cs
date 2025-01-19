using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;

namespace Flow.Launcher.Plugin.ShortcutPlugin.ConsoleApp;
internal class JsonSchema
{
    private static readonly JsonSchemaExporterOptions ExporterOptions = new()
    {
        TransformSchemaNode = (context, schema) =>
        {
            // Determine if a type or property and extract the relevant attribute provider.
            var attributeProvider = context.PropertyInfo is not null
                ? context.PropertyInfo.AttributeProvider
                : context.TypeInfo.Type;

            // Look up any description attributes.
            var descriptionAttr = attributeProvider?
                .GetCustomAttributes(inherit: true)
                .Select(attr => attr as DescriptionAttribute)
                .FirstOrDefault(attr => attr is not null);

            // Apply description attribute to the generated schema.
            if (descriptionAttr == null)
            {
                return schema;
            }

            if (schema is not JsonObject jObj)
            {
                // Handle the case where the schema is a Boolean.
                var valueKind = schema.GetValueKind();

                Debug.Assert(valueKind is JsonValueKind.True or JsonValueKind.False);

                schema = jObj = [];

                if (valueKind is JsonValueKind.False)
                {
                    jObj.Add("not", true);
                }
            }

            jObj.Insert(0, "description", descriptionAttr.Description);

            return schema;
        }
    };

    public static string GenerateShortcutSchema()
    {
        var options = JsonSerializerOptions.Default;
        var schema = options.GetJsonSchemaAsNode(typeof(ShortcutsJsonType), ExporterOptions);

        return schema.ToString();
    }
}

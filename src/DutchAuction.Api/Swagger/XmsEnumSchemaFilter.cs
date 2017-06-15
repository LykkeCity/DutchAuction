using System.Linq;
using System.Reflection;
using Swashbuckle.Swagger.Model;
using Swashbuckle.SwaggerGen.Generator;

namespace DutchAuction.Api.Swagger
{
    public class XmsEnumSchemaFilter : ISchemaFilter
    {
        private readonly XmsEnumExtensionsOptions _options;

        public XmsEnumSchemaFilter(XmsEnumExtensionsOptions options)
        {
            _options = options;
        }
        
        public void Apply(Schema model, SchemaFilterContext context)
        {
            if (model.Properties == null)
            {
                return;
            }

            foreach (var property in model.Properties.Where(x => x.Value.Enum != null))
            {
                var typeProperty = context.SystemType.GetProperty(property.Key, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public).PropertyType;

                property.Value.Extensions.Add("x-ms-enum", new
                {
                    name = typeProperty.Name,
                    modelAsString = _options != XmsEnumExtensionsOptions.UseEnums
                });
            }
        }
    }
}
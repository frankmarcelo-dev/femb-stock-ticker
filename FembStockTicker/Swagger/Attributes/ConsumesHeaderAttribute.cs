using System.Diagnostics.CodeAnalysis;

namespace FembStockTicker.Swagger.Attributes
{
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ConsumesHeaderAttribute : Attribute
    {
        public ConsumesHeaderAttribute(string name, string type)
        {
            Name = name;
            Type = type;
            Description = string.Empty;
            Required = false;
        }

        public string Name { get; }
        public string Type { get; }
        public string Description { get; set; }
        public bool Required { get; set; }
    }
}

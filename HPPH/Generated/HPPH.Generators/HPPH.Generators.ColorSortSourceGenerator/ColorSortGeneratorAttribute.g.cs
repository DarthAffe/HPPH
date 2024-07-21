#nullable enable

namespace HPPH;

[AttributeUsage(AttributeTargets.Method)]
internal class ColorSortGeneratorAttribute(string dataTypeName, string sortValueName) : Attribute
{
    public string DataTypeName { get; } = dataTypeName;
    public string SortValueName { get; } = sortValueName;
}
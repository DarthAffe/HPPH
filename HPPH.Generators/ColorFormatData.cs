using System.Text;

namespace HPPH.Generators;

internal readonly struct ColorFormatData(string typeName, int bpp, char firstEntry, char secondEntry, char thirdEntry, char fourthEntry)
{
    public readonly string TypeName = typeName;
    public readonly int Bpp = bpp;
    public readonly string FirstEntry = firstEntry.ToString().ToLowerInvariant();
    public readonly string SecondEntry = secondEntry.ToString().ToLowerInvariant();
    public readonly string ThirdEntry = thirdEntry.ToString().ToLowerInvariant();
    public readonly string FourthEntry = fourthEntry.ToString().ToLowerInvariant();

    public string FirstEntryName => GetEntryName(FirstEntry);
    public string SecondEntryName => GetEntryName(SecondEntry);
    public string ThirdEntryName => GetEntryName(ThirdEntry);
    public string FourthEntryName => GetEntryName(FourthEntry);

    public string Format
    {
        get
        {
            StringBuilder sb = new();

            if (!string.IsNullOrWhiteSpace(FirstEntry))
                sb.Append(FirstEntry);

            if (!string.IsNullOrWhiteSpace(SecondEntry))
                sb.Append(SecondEntry);

            if (!string.IsNullOrWhiteSpace(ThirdEntry))
                sb.Append(ThirdEntry);

            if (!string.IsNullOrWhiteSpace(FourthEntry))
                sb.Append(FourthEntry);

            return sb.ToString().ToUpper();
        }
    }

    private static string GetEntryName(string entry)
        => entry switch
        {
            "a" => "Alpha",
            "r" => "Red",
            "g" => "Green",
            "b" => "Blue",
            _ => string.Empty
        };
}
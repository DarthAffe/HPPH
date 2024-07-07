using System;
using System.Text;

namespace HPPH.Generators;

internal readonly struct ColorFormatData(string typeName, int bpp, char firstEntry, char secondEntry, char thirdEntry, char fourthEntry)
{
    #region Properties & Fields

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

    public string ByteMapping => CreateByteMapping();

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

    #endregion
    
    #region Methods

    private string CreateByteMapping()
    {
        string[] mapping = new string[Bpp];
        if (Bpp > 0)
        {
            mapping[0] = GetByteMappingIndex(FirstEntry).ToString();

            if (Bpp > 1)
            {
                mapping[1] = GetByteMappingIndex(SecondEntry).ToString();

                if (Bpp > 2)
                {
                    mapping[2] = GetByteMappingIndex(ThirdEntry).ToString();

                    if (Bpp > 3)
                    {
                        mapping[3] = GetByteMappingIndex(FourthEntry).ToString();
                    }
                }
            }
        }

        return string.Join(", ", mapping);
    }

    private static string GetEntryName(string entry)
        => entry switch
        {
            "r" => "Red",
            "g" => "Green",
            "b" => "Blue",
            "a" => "Alpha",
            _ => string.Empty
        };

    private static int GetByteMappingIndex(string entry)
        => entry switch
        {
            "r" => 0,
            "g" => 1,
            "b" => 2,
            "a" => 3,
            _ => throw new IndexOutOfRangeException()
        };

    #endregion
}
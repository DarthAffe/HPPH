using System.Text;

namespace HPPH.Generators;

internal readonly record struct ColorFormatData
{
    #region Properties & Fields

    public readonly string TypeName;
    public readonly int Bpp;
    public readonly string FirstEntry;
    public readonly string SecondEntry;
    public readonly string ThirdEntry;
    public readonly string FourthEntry;

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

    #region Constructors

    public ColorFormatData(string typeName, int bpp, char firstEntry, char secondEntry, char thirdEntry, char fourthEntry)
    {
        this.TypeName = typeName;
        this.Bpp = bpp;
        this.FirstEntry = firstEntry.ToString().ToLowerInvariant();
        this.SecondEntry = secondEntry.ToString().ToLowerInvariant();
        this.ThirdEntry = thirdEntry.ToString().ToLowerInvariant();
        this.FourthEntry = fourthEntry.ToString().ToLowerInvariant();
    }

    #endregion

    #region Methods

    private string CreateByteMapping()
    {
        string[] mapping = new string[Bpp];
        if (Bpp > 0)
        {
            mapping[0] = "Color." + FirstEntry.ToUpper();

            if (Bpp > 1)
            {
                mapping[1] = "Color." + SecondEntry.ToUpper();

                if (Bpp > 2)
                {
                    mapping[2] = "Color." + ThirdEntry.ToUpper();

                    if (Bpp > 3)
                    {
                        mapping[3] = "Color." + FourthEntry.ToUpper();
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

    #endregion
}
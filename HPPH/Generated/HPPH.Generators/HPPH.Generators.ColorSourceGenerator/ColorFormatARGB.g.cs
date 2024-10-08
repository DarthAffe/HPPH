﻿#nullable enable

namespace HPPH;

public sealed partial class ColorFormatARGB : IColorFormat
{
    #region Properties & Fields

    public static ColorFormatARGB Instance { get; } = new();

    public int BytesPerPixel => 4;
    
    public string Name => "ARGB";

    ReadOnlySpan<byte> IColorFormat.ByteMapping => [Color.A, Color.R, Color.G, Color.B];

    #endregion

    #region Constructors

    private ColorFormatARGB() {}

    #endregion
    
    #region Methods
    
    public IColor CreateColor(byte r, byte g, byte b, byte a) => ColorARGB.Create(r, g, b, a);
    
    #endregion
}
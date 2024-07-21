#nullable enable

// ReSharper disable ConvertToAutoProperty
// ReSharper disable ReplaceWithPrimaryConstructorParameter

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace HPPH;

[DebuggerDisplay("[A: {AlphaMin}-{AlphaMax}, R: {RedMin}-{RedMax}, G: {GreenMin}-{GreenMax}, B: {BlueMin}-{BlueMax}]")]
[StructLayout(LayoutKind.Sequential)]
public readonly partial struct MinMaxABGR(byte aMin, byte aMax, byte bMin, byte bMax, byte gMin, byte gMax, byte rMin, byte rMax) : IMinMax
{
    #region Properties & Fields

    private readonly byte _aMin = aMin;
    private readonly byte _aMax = aMax;
    
    private readonly byte _bMin = bMin;
    private readonly byte _bMax = bMax;
    
    private readonly byte _gMin = gMin;
    private readonly byte _gMax = gMax;

    private readonly byte _rMin = rMin;
    private readonly byte _rMax = rMax;

    public byte RedMin => _rMin;
    public byte RedMax => _rMax;
    
    public byte GreenMin => _gMin;
    public byte GreenMax => _gMax;
    
    public byte BlueMin => _bMin;
    public byte BlueMax => _bMax;
    
    public byte AlphaMin => _aMin;
    public byte AlphaMax => _aMax;
    
    public byte RedRange => (byte)(_rMax - _rMin);
    public byte GreenRange => (byte)(_gMax - _gMin);
    public byte BlueRange => (byte)(_bMax - _bMin);
    public byte AlphaRange => (byte)(_aMax - _aMin);
    
    #endregion

    #region Methods

    /// <inheritdoc />
    public override string ToString() => $"[A: {AlphaMin}-{AlphaMax}, R: {RedMin}-{RedMax}, G: {GreenMin}-{GreenMax}, B: {BlueMin}-{BlueMax}]";

    #endregion
}

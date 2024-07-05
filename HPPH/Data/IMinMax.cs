namespace HPPH;

public interface IMinMax
{
    public byte RedMin { get; }
    public byte RedMax { get; }

    public byte GreenMin { get; }
    public byte GreenMax { get; }

    public byte BlueMin { get; }
    public byte BlueMax { get; }

    public byte AlphaMin { get; }
    public byte AlphaMax { get; }

    public byte RedRange { get; }
    public byte GreenRange { get; }
    public byte BlueRange { get; }
    public byte AlphaRange { get; }
}
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace HPPH.Test.Infrastructure;

/// <summary>
/// Native memory with an inaccessible page directly before and after it.
/// The data either starts right after the leading guard page or ends right at the trailing one,
/// so a SIMD kernel that reads or writes even one byte outside the data crashes with an AccessViolationException.
/// Managed arrays can't catch this, because overreads land in the neighbouring heap memory.
/// </summary>
internal sealed unsafe partial class GuardedBuffer : IDisposable
{
    #region Constants

    private const int PAGE_SIZE = 4096;

    private const uint MEM_COMMIT = 0x1000;
    private const uint MEM_RESERVE = 0x2000;
    private const uint MEM_RELEASE = 0x8000;
    private const uint PAGE_NOACCESS = 0x01;
    private const uint PAGE_READWRITE = 0x04;

    #endregion

    #region Properties & Fields

    private readonly byte* _allocation;
    private readonly byte* _data;

    public int Length { get; }
    public Span<byte> Bytes => new(_data, Length);

    #endregion

    #region Constructors

    private GuardedBuffer(int length, bool alignToEnd)
    {
        this.Length = length;

        nuint dataPages = (nuint)((length + PAGE_SIZE - 1) / PAGE_SIZE);
        nuint total = (dataPages + 2) * PAGE_SIZE;

        _allocation = (byte*)VirtualAlloc(null, total, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
        if (_allocation == null) throw new Win32Exception(Marshal.GetLastWin32Error());

        Protect(_allocation);
        Protect(_allocation + ((dataPages + 1) * PAGE_SIZE));

        _data = alignToEnd
            ? _allocation + ((dataPages + 1) * PAGE_SIZE) - (nuint)length
            : _allocation + PAGE_SIZE;
    }

    ~GuardedBuffer() => VirtualFree(_allocation, 0, MEM_RELEASE);

    #endregion

    #region Methods

    /// <summary>Data ends exactly at the trailing guard page (catches overreads and overwrites).</summary>
    public static GuardedBuffer AtEnd(int length) => new(length, true);

    /// <summary>Data starts exactly after the leading guard page (catches underreads).</summary>
    public static GuardedBuffer AtStart(int length) => new(length, false);

    public Span<T> As<T>() where T : struct => MemoryMarshal.Cast<byte, T>(Bytes);

    public void Fill(int seed) => new Random(seed).NextBytes(Bytes);

    public void Dispose()
    {
        VirtualFree(_allocation, 0, MEM_RELEASE);
        GC.SuppressFinalize(this);
    }

    private static void Protect(byte* page)
    {
        if (!VirtualProtect(page, PAGE_SIZE, PAGE_NOACCESS, out _))
            throw new Win32Exception(Marshal.GetLastWin32Error());
    }

    #endregion

    #region DLL-Imports

    [LibraryImport("kernel32", SetLastError = true)]
    private static partial void* VirtualAlloc(void* address, nuint size, uint allocationType, uint protect);

    [LibraryImport("kernel32", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool VirtualProtect(void* address, nuint size, uint newProtect, out uint oldProtect);

    [LibraryImport("kernel32", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool VirtualFree(void* address, nuint size, uint freeType);

    #endregion
}

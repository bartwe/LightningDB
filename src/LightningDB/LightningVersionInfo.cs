using System.Runtime.InteropServices;
using static LightningDB.Native.Lmdb;

namespace LightningDB;

/// <summary>
///     Represents lmdb version information.
/// </summary>
public struct LightningVersionInfo {
    internal static LightningVersionInfo Get() {
        var version = mdb_version(out var major, out var minor, out var patch);
        return new() {
            Version = Marshal.PtrToStringUTF8(version) ?? "",
            Major = major,
            Minor = minor,
            Patch = patch,
        };
    }

    /// <summary>
    ///     Major version number.
    /// </summary>
    public int Major { get; private set; }

    /// <summary>
    ///     Minor version number.
    /// </summary>
    public int Minor { get; private set; }

    /// <summary>
    ///     Patch version number.
    /// </summary>
    public int Patch { get; private set; }

    /// <summary>
    ///     Version string.
    /// </summary>
    public string Version { get; private set; }
}

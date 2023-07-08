using System.Diagnostics.CodeAnalysis;

namespace IL2ShakerDriver;

[SuppressMessage("ReSharper", "InconsistentNaming")]
internal enum SimSpeed
{
    Paused = 0,
    x1_32  = 1,
    x1_16  = 2,
    x1_8   = 3,
    x1_4   = 4,
    x1_2   = 5,
    x1     = 6,
    x2     = 7,
    x4     = 8,
    x8     = 9
}
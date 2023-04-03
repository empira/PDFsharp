// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

#pragma warning disable 1591

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Represents the type of the special character.
    /// </summary>
    public enum SymbolName : uint
    {
        // \space(...)
        Blank = 0xF1000001,
        En = 0xF1000002,
        Em = 0xF1000003,
        EmQuarter = 0xF1000004,
        Em4 = EmQuarter,

        // Used to serialize as \tab, \linebreak
        Tab = 0xF2000001,
        LineBreak = 0xF4000001,

        // for internal use only .
        ParaBreak = 0xF4000007,
        //MarginBreak       = 0xF4000002,

        // \symbol(...)
        Euro = 0xF8000001,
        Copyright = 0xF8000002,
        Trademark = 0xF8000003,
        RegisteredTrademark = 0xF8000004,
        Bullet = 0xF8000005,
        Not = 0xF8000006,
        EmDash = 0xF8000007,
        EnDash = 0xF8000008,
        NonBreakableBlank = 0xF8000009,
        HardBlank = NonBreakableBlank,
    }
}

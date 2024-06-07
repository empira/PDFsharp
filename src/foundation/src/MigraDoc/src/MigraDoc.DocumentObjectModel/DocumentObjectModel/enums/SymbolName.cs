// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Represents the type of the special character.
    /// </summary>
    public enum SymbolName : uint
    {
        // ===== \space(...) =====
        /// <summary>
        /// A regular blank.
        /// </summary>
        Blank = 0xF1000001,

        /// <summary>
        /// An n-width blank.
        /// </summary>
        En = 0xF1000002,

        /// <summary>
        /// An em-width blank.
        /// </summary>
        Em = 0xF1000003,

        /// <summary>
        /// A blank that is a quarter of em wide.
        /// </summary>
        EmQuarter = 0xF1000004,

        /// <summary>
        /// A blank that is a quarter of em wide.
        /// </summary>
        Em4 = EmQuarter,

        // ===== Used to serialize as \tab, \linebreak =====
        /// <summary>
        /// A tabulator.
        /// </summary>
        Tab = 0xF2000001,

        /// <summary>
        /// A line break.
        /// </summary>
        LineBreak = 0xF4000001,

        // ===== For internal use only =====
        /// <summary>
        /// A paragraph break.
        /// </summary>
        ParaBreak = 0xF4000007,
        //MarginBreak       = 0xF4000002,

        // ===== \symbol(...) =====
        /// <summary>
        /// The Euro symbol €.
        /// </summary>
        Euro = 0xF8000001,

        /// <summary>
        /// The copyright symbol ©.
        /// </summary>
        Copyright = 0xF8000002,

        /// <summary>
        /// The trademark symbol ™.
        /// </summary>
        Trademark = 0xF8000003,

        /// <summary>
        /// The registered trademark symbol ®.
        /// </summary>
        RegisteredTrademark = 0xF8000004,

        /// <summary>
        /// The bullet symbol •.
        /// </summary>
        Bullet = 0xF8000005,

        /// <summary>
        /// The not symbol ¬.
        /// </summary>
        Not = 0xF8000006,

        /// <summary>
        /// The em dash —.
        /// </summary>
        EmDash = 0xF8000007,

        /// <summary>
        /// The en dash –.
        /// </summary>
        EnDash = 0xF8000008,

        /// <summary>
        /// A no-break space.
        /// </summary>
        NonBreakableBlank = 0xF8000009,

        /// <summary>
        /// A no-break space.
        /// </summary>
        HardBlank = NonBreakableBlank,
    }
}

// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// v7.0.0 Ready

namespace PdfSharp.Pdf.Forms
{
    /// <summary>
    /// Specifies the flags of interactive form (AcroForm) fields.
    /// </summary>
    [Flags]
    public enum PdfFormFieldFlags
    {
        // Reference 2.0: Table 227 — Field flags common to all field types / Page 532

        // ----- Common to all fields -----------------------------------------------------------------------

        /// <summary>
        /// If set, an interactive PDF processor shall not allow a user to change the value of the
        /// field. Additionally, any associated widget annotations should not interact with the user;
        /// that is, they should not respond to mouse clicks nor change their appearance in response
        /// to mouse motions.
        /// NOTE: This flag is useful for fields whose values are computed or imported from a database.
        /// </summary>
        ReadOnly = 1 << (1 - 1),

        /// <summary>
        /// If set, the field shall have a value at the time it is exported by a submit-form action.
        /// </summary>
        Required = 1 << (2 - 1),

        /// <summary>
        /// If set, the field shall not be exported by a submit-form action.
        /// </summary>
        NoExport = 1 << (3 - 1),

        // ----- Specific to button fields ------------------------------------------------------------------

        // Reference 2.0: Table 229 — Field flags specific to button fields / Page 535

        /// <summary>
        /// (Radio buttons only) If set, exactly one radio button shall be selected at all times;
        /// selecting the currently selected button has no effect. If clear, clicking the selected
        /// button deselects it, leaving no button selected.
        /// </summary>
        NoToggleToOff = 1 << (15 - 1),

        /// <summary>
        /// If set, the field is a set of radio buttons; if clear, the field is a check box.
        /// This flag may be set only if the Pushbutton flag is clear.
        /// </summary>
        Radio = 1 << (16 - 1),

        /// <summary>
        /// If set, the field is a push-button that does not retain a permanent value.
        /// </summary>
        Pushbutton = 1 << (17 - 1),

        /// <summary>
        /// (PDF 1.5) If set, a group of radio buttons within a radio button field that use the same value
        /// for the on state will turn on and off in unison; that is if one is checked, they are all checked.
        /// If clear, the buttons are mutually exclusive (the same behaviour as HTML radio buttons).
        /// </summary>
        RadiosInUnison = 1 << (26 - 1),

        //// ----- Specific to text fields ------------------------------------------------------------------

        // Reference 2.0: Table 231 — Field flags specific to text fields / Page 539

        /// <summary>
        /// If set, the field may contain multiple lines of text; if clear, the field’s text shall be
        /// restricted to a single line.
        /// </summary>
        Multiline = 1 << (13 - 1),

        /// <summary>
        /// If set, the field is intended for entering a secure password that should not be echoed
        /// visibly to the screen. Characters typed from the keyboard shall instead be echoed in some
        /// unreadable form, such as asterisks or bullet characters.
        /// NOTE
        /// To protect password confidentiality, it is imperative that PDF processors never store
        /// the value of the text field in the PDF file if this flag is set.
        /// </summary>
        Password = 1 << (14 - 1),

        /// <summary>
        /// (PDF 1.4) If set, the text entered in the field represents the pathname of a file whose
        /// contents shall be submitted as the value of the field.
        /// </summary>
        FileSelect = 1 << (21 - 1),

        /// <summary>
        /// (PDF 1.4) If set, text entered in the field shall not be spell-checked.
        /// </summary>
        DoNotSpellCheckTextField = 1 << (23 - 1),

        /// <summary>
        /// (PDF 1.4) If set, the field shall not scroll (horizontally for single-line fields, vertically
        /// for multiple-line fields) to accommodate more text than fits within its annotation rectangle.
        /// Once the field is full, no further text shall be accepted for interactive form filling;
        /// for non-interactive form filling, the filler should take care not to add more character than
        /// will visibly fit in the defined area.
        /// </summary>
        DoNotScroll = 1 << (24 - 1),

        /// <summary>
        /// (PDF 1.5) May be set only if the MaxLen entry is present in the text field dictionary and if the
        /// Multiline, Password, and FileSelect flags are clear. If set, the field shall be automatically
        /// divided into as many equally spaced positions, or combs, as the value of MaxLen, and the text
        /// is laid out into those combs.
        /// </summary>
        CombTextField = 1 << (25 - 1),

        /// <summary>
        /// (PDF 1.5) If set, the value of this field shall be a rich text string. If the field has a value,
        /// the RV entry of the field dictionary shall specify the rich text string.
        /// </summary>
        RichTextTextField = 1 << (26 - 1),

        // ----- Specific to choice fields ------------------------------------------------------------------

        // Reference 2.0: Table 233 — Field flags specific to choice fields / Page 542

        /// <summary>
        /// If set, the field is a combo box; if clear, the field is a list box.
        /// </summary>
        Combo = 1 << (18 - 1),

        /// <summary>
        /// If set, the combo box shall include an editable text box as well as a drop-down list; if clear,
        /// it shall include only a drop-down list. This flag shall be used only if the Combo flag is set.
        /// </summary>
        Edit = 1 << (19 - 1),

        /// <summary>
        /// If set, the field’s option items shall be sorted alphabetically. This flag is intended for use
        /// by PDF writers, not by PDF readers. PDF readers shall display the options in the order in which
        /// they occur in the Opt array.
        /// </summary>
        Sort = 1 << (20 - 1),

        /// <summary>
        /// (PDF 1.4) If set, more than one of the field’s option items may be selected simultaneously;
        /// if clear, at most one item shall be selected.
        /// </summary>
        MultiSelect = 1 << (22 - 1),

        /// <summary>
        /// (PDF 1.4) If set, text entered in the field shall not be spell-checked. This flag shall not be
        /// used unless the Combo and Edit flags are both set.
        /// </summary>
        DoNotSpellCheckChoiceField = 1 << (23 - 1),

        /// <summary>
        /// (PDF 1.5) If set, the new value shall be committed as soon as a selection is made (commonly with
        /// the pointing device). In this case, supplying a value for a field involves three actions:
        /// selecting the field for fill-in, selecting a choice for the fill-in value, and leaving that field,
        /// which finalizes or "commits" the data choice and triggers any actions associated with the entry
        /// or changing of this data. If this flag is on, then processing does not wait for leaving the field
        /// action to occur, but immediately proceeds to the third step.
        /// This option enables applications to perform an action once a selection is made, without requiring
        /// the user to exit the field. If clear, the new value is not committed until the user exits the field.
        /// </summary>
        CommitOnSelChangeChoiceField = 1 << (27 - 1),
    }
}

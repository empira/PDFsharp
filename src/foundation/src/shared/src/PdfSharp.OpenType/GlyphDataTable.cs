// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

#define VERBOSE_

using PdfSharp.Internal.Threading;

//using PdfSharp.Internal;

//using Fixed = System.Int32;
//using FWord = System.Int16;
//using UFWord = System.UInt16;

// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

namespace PdfSharp.Internal.OpenType
{
    [Flags]
    enum SimpleGlyphFlags : byte
    {
        /// <summary>
        /// Bit 0: If set, the point is on the curve; otherwise, it is off the curve.
        /// </summary>
        ON_CURVE_POINT = 0x01,

        /// <summary>
        /// Bit 1: If set, the corresponding x-coordinate is 1 byte long, and the sign is determined
        /// by the X_IS_SAME_OR_POSITIVE_X_SHORT_VECTOR flag. If not set, its interpretation depends
        /// on the X_IS_SAME_OR_POSITIVE_X_SHORT_VECTOR flag: If that other flag is set, the x-coordinate
        /// is the same as the previous x-coordinate, and no element is added to the xCoordinates
        /// array. If both flags are not set, the corresponding element in the xCoordinates array is
        /// two bytes and interpreted as a signed integer. See the description of the
        /// X_IS_SAME_OR_POSITIVE_X_SHORT_VECTOR flag for additional information.
        /// </summary>
        X_SHORT_VECTOR = 0x02,

        /// <summary>
        /// Bit 2: If set, the corresponding y-coordinate is 1 byte long, and the sign is determined
        /// by the Y_IS_SAME_OR_POSITIVE_Y_SHORT_VECTOR flag. If not set, its interpretation depends
        /// on the Y_IS_SAME_OR_POSITIVE_Y_SHORT_VECTOR flag: If that other flag is set, the y-coordinate
        /// is the same as the previous y-coordinate, and no element is added to the yCoordinates
        /// array. If both flags are not set, the corresponding element in the yCoordinates array is
        /// two bytes and interpreted as a signed integer. See the description of the
        /// Y_IS_SAME_OR_POSITIVE_Y_SHORT_VECTOR flag for additional information.
        /// </summary>
        Y_SHORT_VECTOR = 0x04,

        /// <summary>
        /// Bit 3: If set, the next byte (read as unsigned) specifies the number of additional times
        /// this flag byte is to be repeated in the logical flags array — that is, the number of
        /// additional logical flag entries inserted after this entry. (In the expanded logical
        /// array, this bit is ignored.) In this way, the number of flags listed can be smaller
        /// than the number of points in the glyph description.
        /// </summary>
        REPEAT_FLAG = 0x08,

        /// <summary>
        /// Bit 4: This flag has two meanings, depending on how the X_SHORT_VECTOR flag is set.
        /// If X_SHORT_VECTOR is set, this bit describes the sign of the value, with 1 equaling
        /// positive and 0 negative. If X_SHORT_VECTOR is not set and this bit is set, then the
        /// current x-coordinate is the same as the previous x-coordinate. If X_SHORT_VECTOR is
        /// not set and this bit is also not set, the current x-coordinate is a signed 16-bit
        /// delta vector.
        /// </summary>
        X_IS_SAME_OR_POSITIVE_X_SHORT_VECTOR = 0x10,

        /// <summary>
        /// Bit 5: This flag has two meanings, depending on how the Y_SHORT_VECTOR flag is set.
        /// If Y_SHORT_VECTOR is set, this bit describes the sign of the value, with 1 equaling
        /// positive and 0 negative. If Y_SHORT_VECTOR is not set and this bit is set, then the
        /// current y-coordinate is the same as the previous y-coordinate. If Y_SHORT_VECTOR is
        /// not set and this bit is also not set, the current y-coordinate is a signed 16-bit
        /// delta vector.
        /// </summary>
        Y_IS_SAME_OR_POSITIVE_Y_SHORT_VECTOR = 0x20,

        /// <summary>
        /// Bit 6: If set, contours in the glyph description could overlap. Use of this flag is
        /// not required — that is, contours may overlap without having this flag set. When used,
        /// it must be set on the first flag byte for the glyph. See additional details below.
        /// </summary>
        OVERLAP_SIMPLE = 0x40,

        /// <summary>
        /// Bit 7 is reserved: set to zero.
        /// </summary>
        Reserved = 0x80
    }

    [Flags]
    enum ComponentGlyphFlags : ushort
    {
        /// <summary>
        /// Bit 0: If this is set, the arguments are 16-bit (uint16 or int16);
        /// otherwise, they are bytes (uint8 or int8).
        /// </summary>
        ARG_1_AND_2_ARE_WORDS = 0x0001,

        /// <summary>
        /// Bit 1: If this is set, the arguments are signed xy values;
        /// otherwise, they are unsigned point numbers.
        /// </summary>
        ARGS_ARE_XY_VALUES = 0x0002,

        /// <summary>
        /// Bit 2: If set and ARGS_ARE_XY_VALUES is also set, the xy values are rounded
        /// to the nearest grid line. Ignored if ARGS_ARE_XY_VALUES is not set.
        /// </summary>
        ROUND_XY_TO_GRID = 0x0004,

        /// <summary>
        /// Bit 3: This indicates that there is a simple scale for the component.
        /// Otherwise, scale = 1.0.
        /// </summary>
        WE_HAVE_A_SCALE = 0x0008,

        /// <summary>
        /// Bit 5: Indicates at least one more glyph after this one.
        /// </summary>
        MORE_COMPONENTS = 0x0020,

        /// <summary>
        /// Bit 6: The x direction will use a different scale from the y direction.
        /// </summary>
        WE_HAVE_AN_X_AND_Y_SCALE = 0x0040,

        /// <summary>
        /// Bit 7: There is a 2 by 2 transformation that will be used to scale the component.
        /// </summary>
        WE_HAVE_A_TWO_BY_TWO = 0x0080,

        /// <summary>
        /// Bit 8: Following the last component are instructions for the composite glyph.
        /// </summary>
        WE_HAVE_INSTRUCTIONS = 0x0100,

        /// <summary>
        /// Bit 9: If set, this forces the aw and lsb (and rsb) for the composite to be equal
        /// to those from this component glyph. This works for hinted and unhinted glyphs.
        /// </summary>
        USE_MY_METRICS = 0x0200,

        /// <summary>
        /// Bit 10: If set, the components of the compound glyph overlap. Use of this flag is not
        /// required — that is, component glyphs may overlap without having this flag set. When used,
        /// it must be set on the flag word for the first component. Some rasterizer implementations
        /// may require fonts to use this flag to obtain correct behavior — see additional remarks,
        /// above, for the similar OVERLAP_SIMPLE flag used in simple-glyph descriptions.
        /// </summary>
        OVERLAP_COMPOUND = 0x0400,

        /// <summary>
        /// Bit 11: The composite is designed to have the component offset scaled.
        /// Ignored if ARGS_ARE_XY_VALUES is not set.
        /// </summary>
        SCALED_COMPONENT_OFFSET = 0x0800,

        /// <summary>
        /// Bit 12: The composite is designed not to have the component offset scaled.
        /// Ignored if ARGS_ARE_XY_VALUES is not set.
        /// </summary>
        UNSCALED_COMPONENT_OFFSET = 0x1000,

        /// <summary>
        /// Bits 4, 13, 14 and 15 are reserved: set to 0.
        /// </summary>
        Reserved = 0xE010
    }

    ////// Re/Sharper restore InconsistentNaming
    ////// Re/Sharper restore UnusedMember.Global

    /// <summary>
    /// This table contains information that describes the glyphs in the font in the TrueType outline format.
    /// Information regarding the rasterizer (scaler) refers to the TrueType rasterizer. 
    /// http://www.microsoft.com/typography/otspec/glyf.htm
    /// </summary>
    public class GlyphDataTable : OpenTypeFontTable
    {
        public const string Tag = TableTagNames.Glyf;

        internal byte[] GlyphTable = null!;


        public GlyphDataTable()
            : base(null, Tag)
        {
            DirectoryEntry.Tag = TableTagNames.Glyf;
        }

        public GlyphDataTable(OpenTypeFontFace fontData)
            : base(fontData, Tag)
        {
            DirectoryEntry.Tag = TableTagNames.Glyf;
            Read();
        }

        /// <summary>
        /// Converts the bytes in a handy representation.
        /// </summary>
        public void Read()
        {
            try
            {
                // Not yet needed...
            }
            // ReSharper disable once RedundantCatchClause
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Gets the data of the specified glyph.
        /// </summary>
        public byte[] GetGlyphData(int glyph)
        {
            //var loca = FontData!.loca;
            int start = GetOffset(glyph);
            int next = GetOffset(glyph + 1);
            int count = next - start;
            byte[] bytes = new byte[count];
            Buffer.BlockCopy(FontData!.OTFontSource!.Bytes, start, bytes, 0, count);
            return bytes;
        }

        /// <summary>
        /// Gets the size of the byte array that defines the glyph.
        /// </summary>
        public int GetGlyphSize(int glyph)
        {
            //var loca = FontData.loca;
            return GetOffset(glyph + 1) - GetOffset(glyph);
        }

        /// <summary>
        /// Gets the offset of the specified glyph relative to the first byte of the font image.
        /// </summary>
        public int GetOffset(int glyph)
        {
            return DirectoryEntry.Offset + FontData!.loca.LocaTable[glyph];
        }

        /// <summary>
        /// Adds for all composite glyphs, the glyphs the composite one is made of.
        /// </summary>
        public void CompleteGlyphClosure(Dictionary<ushort, object?> glyphs)
        {
            int count = glyphs.Count;
            ushort[] glyphArray = new ushort[glyphs.Count];
            glyphs.Keys.CopyTo(glyphArray, 0);
            // ReSharper disable once CanSimplifyDictionaryLookupWithTryAdd because of .NET Framework
            if (!glyphs.ContainsKey(0))
                glyphs.Add(0, null);
            // #NFM
            // Ensure no other threads can alter the Position property of this OpenTypeFontFace instance,
            // see https://forum.pdfsharp.net/viewtopic.php?f=2&t=2248#p10378
            try
            {
                Locks.EnterFontManagement();
                for (int idx = 0; idx < count; idx++)
                    AddCompositeGlyphs(glyphs, glyphArray[idx]);
            }
            finally { Locks.ExitFontManagement(); }
        }

        /// <summary>
        /// If the specified glyph is a composite glyph add the glyphs it is made of to the glyph table.
        /// </summary>
        void AddCompositeGlyphs(Dictionary<ushort, object?> glyphs, int glyph)
        {
            //int start = fontData.loca.GetOffset(glyph);
            int start = GetOffset(glyph);
            // Has no contour?
            if (start == GetOffset(glyph + 1))
                return;
            FontData!.Position = start;
            int numContours = FontData.ReadShort();
            // Isn’t a composite glyph?
            if (numContours >= 0)
                return;
            FontData.SeekOffset(8);
            for (; ; )
            {
                ComponentGlyphFlags flags = (ComponentGlyphFlags)FontData.ReadUFWord();
                ushort cGlyph = FontData.ReadUFWord();
                ////if (!glyphs.ContainsKey(cGlyph))
                ////    glyphs.Add(cGlyph, null);
                glyphs.TryAdd(cGlyph, null);
                if ((flags & ComponentGlyphFlags.MORE_COMPONENTS) == 0)
                    return;
                int offset = (flags & ComponentGlyphFlags.ARG_1_AND_2_ARE_WORDS) == 0 ? 2 : 4;
                if ((flags & ComponentGlyphFlags.WE_HAVE_A_SCALE) != 0)
                    offset += 2;
                else if ((flags & ComponentGlyphFlags.WE_HAVE_AN_X_AND_Y_SCALE) != 0)
                    offset += 4;
                if ((flags & ComponentGlyphFlags.WE_HAVE_A_TWO_BY_TWO) != 0)
                    offset += 8;
                FontData.SeekOffset(offset);
            }
        }

        /// <summary>
        /// Gets the glyph header for the specified glyph.
        /// Used in PDFsharp Graphics to calculate right and bottom side bearings.
        /// </summary>
        public GlyphHeader GetGlyphHeader(ushort glyphIndex)
        {
            FontData!.Position = GetOffset(glyphIndex);
            var glyphHeader = new GlyphHeader
            {
                numberOfContours = FontData.ReadShort(),
                xMin = FontData.ReadShort(),
                yMin = FontData.ReadShort(),
                xMax = FontData.ReadShort(),
                yMax = FontData.ReadShort()
            };
            return glyphHeader;
        }

        /// <summary>
        /// Prepares the font table to be compiled into its binary representation.
        /// </summary>
        public override void PrepareForCompilation()
        {
            base.PrepareForCompilation();

            if (DirectoryEntry.Length == 0)
                DirectoryEntry.Length = GlyphTable.Length;
            DirectoryEntry.CheckSum = CalcChecksum(GlyphTable);
        }

        /// <summary>
        /// Converts the font into its binary representation.
        /// </summary>
        public override void Write(OpenTypeFontWriter writer)
        {
            writer.Write(GlyphTable, 0, DirectoryEntry.PaddedLength);
        }

        //// Constants from OpenType spec.
        //const int ARG_1_AND_2_ARE_WORDS = 1;
        //const int WE_HAVE_A_SCALE = 8;
        //const int MORE_COMPONENTS = 32;
        //const int WE_HAVE_AN_X_AND_Y_SCALE = 64;
        //const int WE_HAVE_A_TWO_BY_TWO = 128;
    }

    public struct GlyphHeader
    {
        /// <summary>
        /// If the number of contours is greater than or equal to zero, this is a simple glyph.
        /// If negative, this is a composite glyph — the value -1 should be used for composite glyphs.
        /// </summary>
        public short numberOfContours;

        public short xMin;  // Minimum x for coordinate data.

        public short yMin;  // Minimum y for coordinate data.

        public short xMax;  // Maximum x for coordinate data.

        public short yMax;  // Maximum y for coordinate data.
    }
}

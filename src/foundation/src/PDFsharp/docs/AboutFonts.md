# Fonts

**This document is under construction**

## TODOs
* History: GDI+ 
* Font resolver
    * PlatformFontresolver
    * ~~DefaultFontResolver~~
    * FailsafeFontResolver


## Terms

This section define terms used in PDFsharp. For a general introduction on typography see 
[Microsoft Typography documentation](https://learn.microsoft.com/en-us/typography/).

## em size
The em size specifies the size of a font.
It is the size of the [em quad](https://en.wikipedia.org/wiki/Quad_(typography)) used for the font.

### Font
Generally in typography a font is a complete character set in a particular point size, in a particular typeface.
A computer font is a file that contains a set of characters in a particular style of a typeface. E.g in Windows
the file 'ARIALBD.TTF' contains a large set of glyphs of the font **Arial** with the style bold.

### Font face
A font face is the manifestation of a particular typeface. A font file may contain more than one font face.
Example: The user describes the typeface he wants to get, e.g. Segoe UI Semibold Italic the system gets him
the content of the file `SEGUISBLI.TTF`.

### Font family
A font family is a collection of fonts that share particular design features within a specific style of typeface.
A font family shares common properties of all its fonts like line spacing. E.g. the font family **Segoe UI** contains 12 font faces,
while **Segoe UI Emoji** contains only one face.

### Typeface
A Typeface refers to a complete set of characters that are unified by a common design ethos.

(It seems in English it is *typeface* but *font face*. Looks strange to me as a German. TODO: Change source code and file names.)

### Typeface vs font
Typeface and font or font face are often used similarly. In general, a typeface is something more abstract about
the visual appearance while a font is a physical representaion of a typeface. A typeface is what a font designer
cerates in his mind. A font is what was created from the typeface to actually print text. Ancient fonts were made
of wood or lead (plumbum), while todays computer fonts are mostly OpenType fonts with e.g. TrueType outlines.
The following example may clarify the difference absed on **Arial**.

*A lot of font designers consider **Arial** a poor typeface, because it is (or at least looks like as) just an unambitious copy of **Helvetica**.
On the other hand Arial is a high value font, because it comes with Windows in 9 different styles with more than 50,000 glyphs
for a lot of languages.*

To be clear we use the terms typeface or font face, never font.

## Font specifig X-classes

Font related classes with the `X` prefix are used by developers to specifiy fonts in PDFsharp
used to draw text.

### XFont

The class **XFont** defines an object that is ultimately used to draw text.
The concept was taken from GDI+.
The **XFont** exists independently of a PDF docuemnt.
You can think of an XFont as a moniker to a specific font face plus a specific font size.
It is a fascade to XGlyphTypeface.
Holds a reference to **OpenTypeDescriptor** to get all information about a the font face.
This is possible because once loaded font faces persists in memory.
Holds a reference to **XGlyphTypeface**. That keeps the reference to the font face.
In contrast to an **XGlyphTypeface** a font holds the following additional information.
* The font em-size used for rendering the font.
* Additional decorator styles like underline or strikethough.
* An optional type used for encoding the text within the PDF document. Either ANSI or Unicode.

### XFontStyleEx

The class **XFontStyleEx** (prior to PDFsharp 6 named **XFontStyle**) specifies style information applied to text.
The concept was taken from GDI+.

### XFontStyle

The concept was taken from WPF.

### XFontStyles

The concept was taken from WPF.

### XFontWeight

The concept was taken from WPF.

### XFontWeights

The concept was taken from WPF.

### XFontStretch

The concept was taken from WPF.

### XFontStretches

The concept was taken from WPF.

### XTypeface
The class **XTypeface** represents a combination of **XFontFamily**, **XFontWeight**, **XFontStyle**, and **XFontStretch**.
It is used to describe a particular typeface of a font family.
The concept was taken from WPF.


### XGlyphTypeface
The class **XGlyphTypeface** specifies a physical font face that corresponds to a font file on the disk or in memory.
While a **XFont** or a **XTypeface** specifiy what you want to get from the font resolver, a **XGlypeTypeface** identifies
what you actually get. For instance, when you specify you want to get an italic typeface of a particular font family, you may
actually get a font face of a non-italic physical font, but the **XGlyphTypeface** specifies that the italic style is simulated
by skewing characters from the font face by 20�.
The concept was taken from WPF.


### XFontSource
The class **XFontSource** represents the bytes of font file in memory. The concept comes from Silverlight.
PDFsharp only supports TrueType font files. TrueType font collections and OpenType files with TrueType Outlines may be on the todo list.


### XStyleSimulations
The class **XStyleSimulation** describes the simulation style of a font.
Bold, Italic, or both can be simulated for a font that doesn�t have these faces physically.
The concept was taken from WPF.
PDFsharp only simules the italic face by sloping the font 20 degrees to the right.

### XPrivateFontCollection
The class **XPrivateFontCollection** is a collection of all fonts used by PDFsharp.
The concept comes from GDI+.
PDFsharp initializes the collection depending on the platform.
Developers can customize this collection until it is used the first time.

### Font Resolver
The GDI and WPF build of PDFsharp uses the underlying Windows framework (`System.Drawing` / `System.Windows` respectively) to
translate a request for a font from a description of a typeface to a particular font face. In the core build of PDFsharp
a font resolver does this job.

## PDF specfific font relatated classes

Font related classes that start with the `Pdf` prefix represents objects used by PDFsharp to create
PDF files. Each instance of such a class belongs to one and only one **PdfDocumnet** object.

### PdfFont : PdfDictionary

The class **PdfFont** is the base class for PDF specific font objects.
Holds a reference ...

### PdfCIDFont : PdfFont

The class **PdfCIDFont** represents character encoded PDF fonts.
It is used together with **PdfType0Font** to represent text in a content by a sequence of the 
glyph ids of the Unicode code points of the original UTF-32 encoded text.

### PdfTrueTypeFont : PdfFont

The class **PdPdfTrueTypeFontfFont** is used by PDFsharp to represent TrueType fonts in a PDF
document that use the WinAnsiEncoding string representation.
It uses lesser space for text in PDF file than the CID (character id) glyph encoded text representation,
but the character set is limited to (roughly) ANSI characters (codepage 1252).

### PdfType0Font : PdfFont

The class **PdfType0Font** represents a PDF font dictionary derived from **PdfCIDFont**.
Together with **PdfCIDFont** t is used to represent text in a content stream by a sequende of 
glyph ids of the Unicode code points of the original text. This class hold the reference to the
ToUnicode map.

### sealed PdfFontDescriptor : PdfDictionary

The class **PdfFontDescriptor** holds document specific information about a font face used in this document.
Each PDF document owns one PdfFontDescriptor for each font face used in the document.
Both PDF fonts objects used in PDFsharp (**PdfTrueTypeFont** and **PdfCIDFont**) shares, when both are crated,
the same **PdfFontDescriptor** and consequently the same font subset in **PdfFontProgram**.
Microsoft Word creates two font descripotrs.
~~A **PdfFontDescriptor** object holds the reference to the dictionary containing the font subset
stream.~~
* Hold a collection of each code point used in the document.

## Public font related classes
### IFontResolver
### IFontResolver2
### PlatformFontResolver
TODO: Does it match with SystemFontResolver?
### FontResolverInfo
The class **FontResolverInfo** is returnd by a font resolver to inform PDFsharp about the reult
of a font face request.

## Internal font related classes

Font related classes that are used by PDFsharp to handle font files has no specific prefix.
They are internal

### FontDescriptor
The class **FontDescriptor** is the base class for font face specific font descriptors.
The only derived class in PDFsharp is **OpenTypeDescriptor**
### FontFamilyInternal
The class **FontFamilyInternal** is the cached implementation class of **XFontFamily**
### OpenTypeDescriptor : FontDescriptor
The class **OpenTypeDescriptor** holds information about a font face and has a one to one
relationship to **OpenTypeFontFace**.
The **OpenTypeDescriptor** is shared between PDF documents.
### CMapInfo
The class **CMapInfo** holds information about the used characters of a font face in a
particular PDF document. When drawing text with a particualr **XFont** using an **XGraphics** instance
all used code points are collected in an **CMapInfo** within the **PdfFontDescriptor** for the
underlying font face that belongs to this document.
This information is used by the **PdfFontDescriptor** to compile a appropriate font subset
out of the original font face when the PDF file is created.
### OpenTypeFontFace
The class **OpenTypeFontFace** represents a font face in memory.
It holds a decompiled representation of all the OpenType font tables used by PDFsharp.
PDFsharp supports only OpenType fonts with PostScirpt or TrueType outline.
You can think of an **OpenTypeFontFace** object as the internal representation of an **XFontSource** object.
An **OpenTypeFontFace** may belong to more that one **XGlyphTypeface** because **XGlyphTypeface** takes
style simulation into account.
### OpenTypeFontTable
The class **OpenTypeFontTable** is the base class of currently 22 derived classes that represent the
decompiled content of the OpenType font tables PDFsharp uses from a font face.
### FontResolverInfo
### PlatformFontResolverInfo : FontResolverInfo

## Font caches and keys

### FontFactory
The class **FontFactory** maps the request for a typeface to a physical font face in memory.

### OpenTypeFontFaceCache
The class **OpenTypeFontFaceCache** caches **OpenTypeFontFace**
### PdfFontTable
The class **PdfFontTable** maps **XFont** objects to **PdfFont** objects.
Key: {facename}{ () | (b)old | (i)talic | (bi)bolditalic }/{ (A)NSI | (U)nicode }
### FontDescriptorCache
The class **FontDescriptorCache** caches **OpenTypeFontdescriptor**.
Key format: {name}/{ () | (b)old | (i)talic | (bi)bolditalic }
Example:    arial/b
### FontFamilyCache
The class **FontFamilyCache** caches **FontFamilyInternal**.
### GlyphTypefaceCache
The class **GlyphTypefaceCache** caches **XGlyphTypeface**.
Key format: {family-name}/{(N)ormal | (O)blique | (I)talic}/{weight}/{stretch}|{(B)old|not (b)old}/{(I)talic|not (i)talic}:tk  
Example:     arial/I/400/500|B/i:tk  

## Development
### FontsDevHelper

## PdfSharp.Snippets


### FailsafeFontResolver

This font resolver maps each request to a valid FontSource.
If a typeface cannot be resolved by the PlatformFontResolver, it is substituted by a SegoeWP font.


## References

[WPF Font Selectionasdf Model](https://assets.pdfsharp.net/docs/WPF%20Font%20Selection%20Model.pdf)
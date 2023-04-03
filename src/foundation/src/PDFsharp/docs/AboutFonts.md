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

### Typeface vs font
Typeface and font or font face are often used similarly. In general, a typeface is something more abstract about
the visual appearance while a font is a physical representaion of a typeface. A typeface is what a font designer
cerates in his mind. A font is what was created from the typeface to actually print text. Ancient fonts were made
of wood or lead (plumbum), while todays computer fonts are mostly OpenType fonts with e.g. TrueType outlines.
The following examples may clarify the difference with **Arial** as an example.
A lot of font desingers consider **Arial** a poor typeface, because it is (or at least looks like as) just an unambitious copy of **Helvetica**.
On the other hand Arial is a high value font, because it comes with Windows in 9 different styles with more than 50,000 glyphs
for a lot of languages.







## Font classes

### XFont

The class **Font** defines an object that is ultimately used to draw text.
The concept was taken from GDI+.

### XFontStyleEx

The class **XFontStyleEx** (former name **XFontStyle**) specifies style information applied to text.
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

The class **XTypeface** represents a combination of **XFontFamily**, **XFontWeight**, **XFontStyleEx**, and **XFontStretch**.
It is used to describe a particular typeface of a font family.
The concept was taken from WPF.


### XGlyphTypeface

The class **XGlyphTypeface** specifies a physical font face that corresponds to a font file on the disk or in memory.

The concept was taken from WPF.


### XFontSource

The class **XFontSource** represents the bytes of font file in memory. The concept comes from Silverlight.
PDFsharp only supports only TrueType font files. TrueType font collections and OpenType files with TrueType Outlines may be on the todo list.


### XStyleSimulations

The class **XStyleSimulation** describes the simulation style of a font.
Bold, Italic, or both can be simulated for a font that don't have this faces.
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

## PdfSharp.Snippets


### FailsafeFontResolver

This font resolver maps each request to a valid FontSource.
If a typeface cannot be resolved by the PlatformFontResolver, it is substituted by a SegoeWP font.

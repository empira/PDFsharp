# Font selection

Font selection is done by a **FontResolver**.

## Windows platforms

The GDI+ and the WPF builds use GDI+ / WPF functionality to select a font
if no Custom FontResolver is set. If a Custom FontResolver is set, it is always used.
The behavior is the same for .NET and .NET Framework.

The Core build for .NET Framework runs only under Windows and therefore it uses the
WindowsFontResolver (see below) if no Custom FontResolver is set.

The Core build for .NET executed under Windows uses the
WindowsFontResolver (see below) if no Custom FontResolver is set.

### The Windows Font

## Non-Windows platforms

The Core build for .NET always needs a Custom FontResolver.

### WSL2

PDFsharp prior to version 6.2 uses the `C:\Windows\Fonts` folder.
We removed this behavior.

### Desktop Linux

A Linux distribution with UI has fonts installed, but there is no (or at least no easy)
way to locate them on a particular distribution.

### Mac

We have no Macs at empira Software.
You must write your own FontResolver.

### Linux Docker Container

A Linux distribution for a Docker Container may not provide fonts.
You must write your own FontResolver.

### Mobile devices, Rasperry Pi, etc.

You must write your own FontResolver.

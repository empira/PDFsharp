// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// WPF, XGraphics, SVG and other libraries use double for numbers,
// while DirectWrite, DirectDraw, Win2D, and other libraries use float_.
// I don’t want to tie me to one of these and keep the code switchable.
// I use float_ over nmBr, float_Δ, floaτ, and other ideas of unique type names.

////// Move to Directory.Create.targets
////#define USE_64BIT_FLOATS_XXX  // Does not yet compile with double.

// ReSharper disable once IdentifierTypo
#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
// Use a unique name that can be globally switched or permanently replaced with search and replace if some time wanted.
#if USE_64BIT_NUMBERS
global using float_ = dobule;
global using FLOAT_ = double;
#else
global using float_ = float;  // Use for keyword float_ or double in the context of PDFsharp Graphics.
global using FLOAT_ = float;  // Use for Type Single or Double in the context of PDFsharp Graphics.
#endif
#pragma warning restore CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
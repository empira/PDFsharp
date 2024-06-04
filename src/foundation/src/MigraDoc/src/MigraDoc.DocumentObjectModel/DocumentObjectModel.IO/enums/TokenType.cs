// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.IO
{
    /// <summary>
    /// The tokens used by DdlScanner/DdlParser.
    /// </summary>
    enum TokenType
    {
        /// <summary>
        /// White space or comment.
        /// </summary>
        None,

        /// <summary>
        /// Same as identifiers in C#, but not case sensitive.
        /// </summary>
        Identifier,

        /// <summary>
        /// Both «true» and «\bold» are keywords, case sensitive.
        /// </summary>
        KeyWord,

        /// <summary>
        /// Sample: «42»
        /// </summary>
        IntegerLiteral,

        /// <summary>
        /// Samples: «42.0», «42.», «.42»,...
        /// </summary>
        RealLiteral,

        /// <summary>
        /// Not used.
        /// </summary>
        CharacterLiteral,

        /// <summary>
        /// Both «"text"» and «@"text with ""quotes"""».
        /// </summary>
        StringLiteral,

        /// <summary>
        /// Samples: «.», «{», «+=»,...
        /// </summary>
        OperatorOrPunctuator,

        /// <summary>
        /// Plain text. Possible after ReadText.
        /// </summary>
        Text,
    }
}

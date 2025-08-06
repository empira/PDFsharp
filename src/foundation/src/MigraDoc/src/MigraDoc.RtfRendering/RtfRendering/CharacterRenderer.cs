﻿// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using MigraDoc.DocumentObjectModel;

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Renders a special character to RTF.
    /// </summary>
    class CharacterRenderer : RendererBase
    {
        /// <summary>
        /// Creates a new instance of the CharacterRenderer class.
        /// </summary>
        internal CharacterRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
            : base(domObj, docRenderer)
        {
            _character = (Character)domObj;
        }

        /// <summary>
        /// Renders a character to rtf.
        /// </summary>
        internal override void Render()
        {
            if (_character.Char != '\0')
            {
                _rtfWriter.WriteHex((uint)_character.Char);
            }
            else
            {
                int count = _character.Values.Count is null ? 1 : _character.Count;
                switch (_character.SymbolName)
                {
                    case SymbolName.Blank:
                        for (int idx = 0; idx < count; idx++)
                            _rtfWriter.WriteBlank();
                        //WriteText wouldn’t work if there was a control before.
                        break;

                    case SymbolName.Bullet:
                        for (int idx = 0; idx < count; idx++)
                            _rtfWriter.WriteControl("bullet");
                        break;

                    case SymbolName.Copyright:
                        for (int idx = 0; idx < count; idx++)
                            _rtfWriter.WriteHex(0xa9);
                        break;

                    case SymbolName.Em:
                        for (int idx = 0; idx < count; idx++)
                        {
                            _rtfWriter.WriteControl("u", "8195");
                            //I don’t know why, but it works:
                            _rtfWriter.WriteHex(0x20);
                        }
                        break;

                    case SymbolName.Em4:
                        for (int idx = 0; idx < count; idx++)
                        {
                            _rtfWriter.WriteControl("u", "8197");
                            //I don’t know why, but it works:
                            _rtfWriter.WriteHex(0x20);
                        }
                        break;

                    case SymbolName.En:
                        for (int idx = 0; idx < count; idx++)
                        {
                            _rtfWriter.WriteControl("u", "8194");
                            //I don’t know why, but it works:
                            _rtfWriter.WriteHex(0x20);
                        }
                        break;

                    case SymbolName.EmDash:
                        for (int idx = 0; idx < count; idx++)
                            _rtfWriter.WriteControl("emdash");
                        break;

                    case SymbolName.EnDash:
                        for (int idx = 0; idx < count; idx++)
                            _rtfWriter.WriteControl("endash");
                        break;

                    case SymbolName.Euro:
                        for (int idx = 0; idx < count; idx++)
                            _rtfWriter.WriteHex(0x80);
                        break;

                    case SymbolName.NonBreakableBlank:
                        for (int idx = 0; idx < count; idx++)
                            // Must not have a blank after "\~", so pass false as first parameter.
                            _rtfWriter.WriteControl(false, "~");
                        break;

                    case SymbolName.LineBreak:
                        for (int idx = 0; idx < count; idx++)
                            _rtfWriter.WriteControl("line");
                        break;

                    case SymbolName.Not:
                        for (int idx = 0; idx < count; idx++)
                            _rtfWriter.WriteHex(0xac);
                        break;

                    case SymbolName.ParaBreak:
                        for (int idx = 0; idx < count; idx++)
                            _rtfWriter.WriteControl("par");
                        break;

                    case SymbolName.RegisteredTrademark:
                        for (int idx = 0; idx < count; idx++)
                            _rtfWriter.WriteHex(0xae);
                        break;

                    case SymbolName.Tab:
                        for (int idx = 0; idx < count; idx++)
                            _rtfWriter.WriteControl("tab");
                        break;

                    case SymbolName.Trademark:
                        for (int idx = 0; idx < count; idx++)
                            _rtfWriter.WriteHex(0x99);
                        break;
                }
            }
        }

        readonly Character _character;
    }
}
// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Collections.Generic;
using System.Diagnostics;
using MigraDoc.DocumentObjectModel;

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Renders a ListInfo in the \listoverridetable control.
    /// </summary>
    class ListInfoOverrideRenderer : RendererBase
    {
        public ListInfoOverrideRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
            : base(domObj, docRenderer)
        {
            _listInfo = (ListInfo)domObj;
        }

        public static void Clear()
        {
            _numberList.Clear();
            _listNumber = 1;
        }

        /// <summary>
        /// Renders a ListInfo to RTF for the \listoverridetable.
        /// </summary>
        internal override void Render()
        {
            int id = ListInfoRenderer.GetListID(_listInfo);
            if (id > -1)
            {
                _rtfWriter.StartContent();
                _rtfWriter.WriteControl("listoverride");
                _rtfWriter.WriteControl("listid", id);
                _rtfWriter.WriteControl("listoverridecount", 0);
                _rtfWriter.WriteControl("ls", _listNumber);
                _rtfWriter.EndContent();
                _numberList.Add(_listInfo, _listNumber);
                ++_listNumber;
            }
        }

        readonly ListInfo _listInfo;
        static int _listNumber = 1;
        static readonly Dictionary<ListInfo, int> _numberList = new Dictionary<ListInfo, int>();

        internal static int GetListNumber(ListInfo li)
        {
            if (_numberList.ContainsKey(li))
                return _numberList[li];

            return -1;
        }
    }
}

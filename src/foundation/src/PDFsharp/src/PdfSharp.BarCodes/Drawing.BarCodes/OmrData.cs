// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Drawing.BarCodes
{
#if true_
    /// <summary>
    /// Represents the data coded within the OMR code.
    /// </summary>
    class OmrData
    {
        private OmrData()
        { }

        public static OmrData ForTesting
        {
            get
            {
                OmrData data = new OmrData();
                data.AddMarkDescription("LK");
                data.AddMarkDescription("DGR");
                data.AddMarkDescription("GM1");
                data.AddMarkDescription("GM2");
                data.AddMarkDescription("GM4");
                data.AddMarkDescription("GM8");
                data.AddMarkDescription("GM16");
                data.AddMarkDescription("GM32");
                data.AddMarkDescription("ZS1");
                data.AddMarkDescription("ZS2");
                data.AddMarkDescription("ZS3");
                data.AddMarkDescription("ZS4");
                data.AddMarkDescription("ZS5");
                data.InitMarks();
                return data;
            }
        }

        ///// <summary>
        ///// NYI: Get OMR description read from text file.
        ///// </summary>
        ///// <returns>An OmrData object.</returns>
        //public static OmrData FromDescriptionFile(string filename)
        //{
        //  throw new NotImplementedException();
        //}

        /// <summary>
        /// Adds a mark description by name.
        /// </summary>
        /// <param name="name">The name to for setting or unsetting the mark.</param>
        private void AddMarkDescription(string name)
        {
            if (_marksInitialized)
                throw new InvalidOperationException(BcgSR.OmrAlreadyInitialized);

            _nameToIndex[name] = AddedDescriptions;
            ++AddedDescriptions;
        }

        private void InitMarks()
        {
            if (AddedDescriptions == 0)
                throw new InvalidOperationException();

            _marks = new bool[AddedDescriptions];
            _marks.Initialize();
            _marksInitialized = true;
        }

        private int FindIndex(string name)
        {
            if (!_marksInitialized)
                InitMarks();

            if (!_nameToIndex.Contains(name))
                throw new ArgumentException(BcgSR.InvalidMarkName(name));

            return (int)_nameToIndex[name];
        }

        public void SetMark(string name)
        {
            int idx = FindIndex(name);
            _marks[idx] = true;
        }

        public void UnsetMark(string name)
        {
            int idx = FindIndex(name);
            _marks[idx] = false;
        }

        public bool[] Marks
        {
            get { return _marks; }
        }
        System.Collections.Hash_table nameToIndex = new Hash_table();
        bool[] marks;
        int addedDescriptions = 0;
        bool marksInitialized = false;
    }
#endif
}

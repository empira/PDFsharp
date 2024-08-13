// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.ComponentModel;

namespace PdfSharp.Charting
{
    /// <summary>
    /// Represents a DataLabel of a Series
    /// </summary>
    public class DataLabel : DocumentObject
    {
        /// <summary>
        /// Initializes a new instance of the DataLabel class.
        /// </summary>
        public DataLabel()
        { }

        /// <summary>
        /// Initializes a new instance of the DataLabel class with the specified parent.
        /// </summary>
        internal DataLabel(DocumentObject parent) : base(parent) { }

        #region Methods
        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new DataLabel Clone() 
            => (DataLabel)DeepCopy();

        /// <summary>
        /// Implements the deep copy of the object.
        /// </summary>
        protected override object DeepCopy()
        {
            var dataLabel = (DataLabel)base.DeepCopy();
            if (dataLabel._font != null)
            {
                dataLabel._font = dataLabel._font.Clone();
                dataLabel._font.Parent = dataLabel;
            }
            return dataLabel;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets a numeric format string for the DataLabel.
        /// </summary>
        public string Format { get; set; } = "";

        /// <summary>
        /// Gets the Font for the DataLabel.
        /// </summary>
        public Font Font 
            => _font ??= new Font(this);
        // ReSharper disable once InconsistentNaming because this is old code
        internal Font? _font;

        /// <summary>
        /// Gets or sets the position of the DataLabel.
        /// </summary>
        public DataLabelPosition Position
        {
            get => (DataLabelPosition)_position;
            set
            {
                if (!Enum.IsDefined(typeof(DataLabelPosition), value))
                    throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(DataLabelPosition));

                _position = value;
                _positionInitialized = true;
            }
        }
        // ReSharper disable once InconsistentNaming because this is old code
        internal DataLabelPosition _position;
        // ReSharper disable once InconsistentNaming because this is old code
        internal bool _positionInitialized;

        /// <summary>
        /// Gets or sets the type of the DataLabel.
        /// </summary>
        public DataLabelType Type
        {
            get => _type;
            set
            {
                if (!Enum.IsDefined(typeof(DataLabelType), value))
                    throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(DataLabelType));

                _type = value;
                _typeInitialized = true;
            }
        }
        internal DataLabelType _type;
        // ReSharper disable once InconsistentNaming because this is old code
        internal bool _typeInitialized;
        #endregion
    }
}

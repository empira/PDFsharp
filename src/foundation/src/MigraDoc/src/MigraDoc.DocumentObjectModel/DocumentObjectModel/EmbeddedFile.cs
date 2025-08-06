// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// EmbeddedFile is used for PDF Documents that shall be embedded in another PDF Document. EmbeddedFile is only supported in PDF.
    /// </summary>
    public class EmbeddedFile : DocumentObject
    {
        /// <summary>
        /// Initializes a new instance of the EmbeddedFile class.
        /// </summary>
        public EmbeddedFile()
        {
            BaseValues = new EmbeddedFileValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the EmbeddedFile class.
        /// </summary>
        /// <param name="name">The name used to refer and to entitle the embedded file.</param>
        /// <param name="path">The path of the file to embed.</param>
        public EmbeddedFile(string name, string path)
        {
            BaseValues = new EmbeddedFileValues(this);
            Name = name;
            Path = path;
        }

        /// <summary>
        /// Gets or sets the name used to refer and to entitle the embedded file.
        /// </summary>
        public string Name
        {
            get => Values.Name ?? "";
            set => Values.Name = value;
        }

        /// <summary>
        /// Gets or sets the path to the file to embed.
        /// </summary>
        public string Path
        {
            get => Values.Path ?? "";
            set => Values.Path = value;
        }

        /// <summary>
        /// Converts Section into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            serializer.WriteLine("\\EmbeddedFile");
            serializer.BeginAttributes();

            if (!String.IsNullOrEmpty(Values.Name))
                serializer.WriteSimpleAttribute("Name", Name);

            if (!String.IsNullOrEmpty(Values.Path))
                serializer.WriteSimpleAttribute("Path", Path);

            serializer.EndAttributes();
        }

        /// <summary>
        /// Returns the metaobject of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(EmbeddedFile));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public EmbeddedFileValues Values => (EmbeddedFileValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class EmbeddedFileValues : Values
        {
            internal EmbeddedFileValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? Name { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? Path { get; set; }
        }
    }
}

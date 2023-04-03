// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Represents the collection of embedded files.
    /// </summary>
    public class EmbeddedFiles : DocumentObjectCollection
    {
        /// <summary>
        /// Initializes a new instance of the EmbeddedFiles class.
        /// </summary>
        public EmbeddedFiles()
        {
            BaseValues = new EmbeddedFilesValues(this);
        }

        /// <summary>
        /// Gets an embedded file by its index. First embedded file has index 0.
        /// </summary>
        public new EmbeddedFile this[int index] => (base[index] as EmbeddedFile)!; // BUG??? Exception?

        /// <summary>
        /// Adds a new EmbeddedFile.
        /// </summary>
        /// <param name="name">The name used to refer and to entitle the embedded file.</param>
        /// <param name="path">The path of the file to embed.</param>
        public EmbeddedFile Add(string name, string path)
        {
            var embeddedFile = new EmbeddedFile(name, path);
            Add(embeddedFile);
            return embeddedFile;
        }

        /// <summary>
        /// Converts Sections into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            var count = Count;
            for (var index = 0; index < count; ++index)
            {
                var embeddedFile = this[index];
                embeddedFile.Serialize(serializer);
            }
        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(EmbeddedFiles));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public EmbeddedFilesValues Values => (EmbeddedFilesValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class EmbeddedFilesValues : Values
        {
            internal EmbeddedFilesValues(DocumentObject owner) : base(owner)
            { }
        }
    }
}

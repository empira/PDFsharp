// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel.Shapes
{
    /// <summary>
    /// Represents an image in the document or paragraph.
    /// </summary>
    public class Image : Shape
    {
        /// <summary>
        /// Initializes a new instance of the Image class.
        /// </summary>
        public Image()
        {
            BaseValues = new ImageValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the Image class with the specified parent.
        /// </summary>
        internal Image(DocumentObject parent) : base(parent)
        {
            BaseValues = new ImageValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the Image class from the specified (file) name.
        /// </summary>
        public Image(string name) : this()
        {
            Name = name;
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new Image Clone()
            => (Image)DeepCopy();

        /// <summary>
        /// Implements the deep copy of the object.
        /// </summary>
        protected override object DeepCopy()
        {
            Image image = (Image)base.DeepCopy();
            if (image.Values.PictureFormat != null)
            {
                image.Values.PictureFormat = image.Values.PictureFormat.Clone();
                image.Values.PictureFormat.Parent = image;
            }
            return image;
        }
        //#endregion

        //#region Properties
        /// <summary>
        /// Gets or sets the name of the image.
        /// </summary>
        public string Name
        {
            get => Values.Name ?? "";
            set => Values.Name = value;
        }

        /// <summary>
        /// Gets or sets the ScaleWidth of the image.
        /// If the Width is set, too, the resulting image width is ScaleWidth * Width.
        /// </summary>
        public double ScaleWidth
        {
            get => Values.ScaleWidth ?? 0.0;
            set => Values.ScaleWidth = value;
        }

        /// <summary>
        /// Gets or sets the ScaleHeight of the image.
        /// If the Height is set, too, the resulting image height is ScaleHeight * Height.
        /// </summary>
        public double ScaleHeight
        {
            get => Values.ScaleHeight ?? 0.0;
            set => Values.ScaleHeight = value;
        }

        /// <summary>
        /// Gets or sets whether the AspectRatio of the image is kept unchanged.
        /// If both Width and Height are set, this property is ignored.
        /// </summary>
        public bool LockAspectRatio
        {
            get => Values.LockAspectRatio ?? false;
            set => Values.LockAspectRatio = value;
        }

        /// <summary>
        /// Gets or sets the PictureFormat for the image.
        /// </summary>
        public PictureFormat PictureFormat
        {
            get => Values.PictureFormat ??= new(this);
            set
            {
                SetParent(value);
                Values.PictureFormat = value;
            }
        }

        /// <summary>
        /// Gets or sets a user defined resolution for the image in dots per inch.
        /// </summary>
        public double Resolution
        {
            get => Values.Resolution ?? 0.0;
            set => Values.Resolution = value;
        }
        //#endregion

        /// <summary>
        /// Converts Image into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            serializer.WriteLine("\\image(\"" + Values.Name?.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\")");

            int pos = serializer.BeginAttributes();

            base.Serialize(serializer);
            if (Values.ScaleWidth is not null)
                serializer.WriteSimpleAttribute("ScaleWidth", ScaleWidth);
            if (Values.ScaleHeight is not null)
                serializer.WriteSimpleAttribute("ScaleHeight", ScaleHeight);
            if (Values.LockAspectRatio is not null)
                serializer.WriteSimpleAttribute("LockAspectRatio", LockAspectRatio);
            if (Values.Resolution is not null)
                serializer.WriteSimpleAttribute("Resolution", Resolution);
            Values.PictureFormat?.Serialize(serializer);

            serializer.EndAttributes(pos);
        }

        /// <summary>
        /// Gets the concrete image path, taking into account the DOM document's DdlFile and
        /// ImagePath properties as well as the given working directory (which can be null).
        /// </summary>
        public string GetFilePath(string workingDir)
        {
            if (Name.StartsWith("base64:", StringComparison.Ordinal)) // The file is stored in the string here, so we don't have to add a path.
                return Name;

            string filePath;

            if (!String.IsNullOrEmpty(workingDir))
                filePath = workingDir;
            else
                filePath = Directory.GetCurrentDirectory() + "\\";

            if (!Document.Values.ImagePath.IsValueNullOrEmpty())
            {
                string? foundfile = ImageHelper.GetImageName(filePath, Name, Document.ImagePath);
                if (foundfile != null)
                    filePath = foundfile;
                else
                    filePath = Path.Combine(filePath, Name);
            }
            else
                filePath = Path.Combine(filePath, Name);

            return filePath;
        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(Image));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public new ImageValues Values => (ImageValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class ImageValues : ShapeValues
        {
            internal ImageValues(DocumentObject owner) : base(owner)
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
            public double? ScaleWidth { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public double? ScaleHeight { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public bool? LockAspectRatio { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public PictureFormat? PictureFormat { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public double? Resolution { get; set; }
        }
    }
}

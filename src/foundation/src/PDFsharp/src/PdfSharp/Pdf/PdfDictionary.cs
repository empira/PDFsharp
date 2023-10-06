// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Collections;
using System.Reflection;
using System.Text;
using PdfSharp.Drawing;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Filters;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Internal;
using System.Diagnostics.CodeAnalysis;

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Value creation flags. Specifies whether and how a value that does not exist is created.
    /// </summary>
    // ReSharper disable InconsistentNaming
    public enum VCF
    // ReSharper restore InconsistentNaming
    {
        /// <summary>
        /// Don't create the value.
        /// </summary>
        None,

        /// <summary>
        /// Create the value as direct object.
        /// </summary>
        Create,

        /// <summary>
        /// Create the value as indirect object.
        /// </summary>
        CreateIndirect,
    }

    /// <summary>
    /// Represents a PDF dictionary object.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public class PdfDictionary : PdfObject, IEnumerable<KeyValuePair<string, PdfItem?>>
    {
        // Reference: 3.2.6  Dictionary Objects / Page 59

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfDictionary"/> class.
        /// </summary>
        public PdfDictionary()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfDictionary"/> class.
        /// </summary>
        /// <param name="document">The document.</param>
        public PdfDictionary(PdfDocument document)
            : base(document)
        { }

        /// <summary>
        /// Initializes a new instance from an existing dictionary. Used for object type transformation.
        /// </summary>
        protected PdfDictionary(PdfDictionary dict)
            : base(dict)
        {
            if (dict._elements != null)
                dict._elements.ChangeOwner(this);
            if (dict.Stream != null)
                dict.Stream.ChangeOwner(this);
        }

        /// <summary>
        /// Creates a copy of this dictionary. Direct values are deep copied. Indirect references are not
        /// modified.
        /// </summary>
        public new PdfDictionary Clone()
            => (PdfDictionary)Copy();

        /// <summary>
        /// This function is useful for importing objects from external documents. The returned object is not
        /// yet complete. irefs refer to external objects and directed objects are cloned but their document
        /// property is null. A cloned dictionary or array needs a 'fix-up' to be a valid object.
        /// </summary>
        protected override object Copy()
        {
            var dict = (PdfDictionary)base.Copy();
            if (dict._elements != null)
            {
                dict._elements = dict._elements.Clone();
                dict._elements.ChangeOwner(dict);
                var names = dict._elements.KeyNames;
                foreach (var name in names)
                {
                    if (dict._elements[name] is PdfObject obj)
                    {
                        obj = obj.Clone();
                        // Recall that obj.Document is now null.
                        dict._elements[name] = obj;
                    }
                }
            }
            if (dict.Stream != null!)
            {
                dict.Stream = dict.Stream.Clone();
                dict.Stream.ChangeOwner(dict);
            }
            return dict;
        }

        /// <summary>
        /// Gets the dictionary containing the elements of this dictionary.
        /// </summary>
        public DictionaryElements Elements => _elements ??= new DictionaryElements(this);

        /// <summary>
        /// The elements of the dictionary.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        internal DictionaryElements? _elements;

        /// <summary>
        /// Returns an enumerator that iterates through the dictionary elements.
        /// </summary>
        public IEnumerator<KeyValuePair<string, PdfItem?>> GetEnumerator()
            => Elements.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        /// <summary>
        /// Returns a string with the content of this object in a readable form. Useful for debugging purposes only.
        /// </summary>
        public override string ToString()
        {
            // Get keys and sort.
            PdfName[] keys = Elements.KeyNames;
            List<PdfName> list = new List<PdfName>(keys);
            list.Sort(PdfName.Comparer);
            list.CopyTo(keys, 0);

            var pdf = new StringBuilder();
            pdf.Append("<< ");
            foreach (PdfName key in keys)
                pdf.Append(key + " " + Elements[key] + " ");
            pdf.Append(">>");

            return pdf.ToString();
        }

        internal override void WriteObject(PdfWriter writer)
        {
            writer.WriteBeginObject(this);
            //int count = Elements.Count;
            PdfName[] keys = Elements.KeyNames;

#if DEBUG
            // TODO: automatically set length
            if (Stream != null)
                Debug.Assert(Elements.ContainsKey("/Length"), "Dictionary has a stream but no length is set.");
#endif

            if (_stream is not null && writer.SecurityHandler != null)
            {
                // Encryption could change the size of the stream.
                // Encrypt the bytes before writing the dictionary to get and update the actual size.
                var bytes = (byte[])_stream.Value.Clone();
                writer.SecurityHandler.EncryptStream(ref bytes, this);
                _stream.Value = bytes;
                Elements[PdfStream.Keys.Length] = new PdfInteger(_stream?.Length ?? 0);
            }

#if DEBUG
            // Sort keys for debugging purposes. Comparing PDF files with for example programs like
            // Araxis Merge is easier with sorted keys.
            if (writer.Layout == PdfWriterLayout.Verbose)
            {
                var list = new List<PdfName>(keys);
                list.Sort(PdfName.Comparer);
                list.CopyTo(keys, 0);
            }
#endif

            foreach (var key in keys)
                WriteDictionaryElement(writer, key);
            if (Stream != null)
                WriteDictionaryStream(writer);
            writer.WriteEndObject();
        }

        /// <summary>
        /// Writes a key/value pair of this dictionary. This function is intended to be overridden
        /// in derived classes.
        /// </summary>
        internal virtual void WriteDictionaryElement(PdfWriter writer, PdfName key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            var item = Elements[key];
#if DEBUG
            // TODO: simplify PDFsharp
            if (item is PdfObject { IsIndirect: true } pdfObject)
            {
                // Replace an indirect object by its Reference.
                item = pdfObject.Reference;
                Debug.Assert(false, "Check when we come here.");
            }
#endif
            key.WriteObject(writer);
            item?.WriteObject(writer);
            writer.NewLine();
        }

        /// <summary>
        /// Writes the stream of this dictionary. This function is intended to be overridden
        /// in a derived class.
        /// </summary>
        internal virtual void WriteDictionaryStream(PdfWriter writer)
        {
            writer.WriteStream(this, (writer.Options & PdfWriterOptions.OmitStream) == PdfWriterOptions.OmitStream);
        }

        /// <summary>
        /// Gets or sets the PDF stream belonging to this dictionary. Returns null if the dictionary has
        /// no stream. To create the stream, call the CreateStream function.
        /// </summary>
        public PdfStream Stream
        {
            get => _stream!; // ?? NRT.ThrowOnNull<PdfStream>(); // Can be null.
            set => _stream = value;
        }
        PdfStream? _stream;

        /// <summary>
        /// Creates the stream of this dictionary and initializes it with the specified byte array.
        /// The function must not be called if the dictionary already has a stream.
        /// </summary>
        public PdfStream CreateStream(byte[] value)
        {
            if (_stream != null)
                throw new InvalidOperationException("The dictionary already has a stream.");

            Stream = new PdfStream(value, this);
            // Always set the length.
            Elements["/Length"] = new PdfInteger(Stream.Length);
            return Stream;
        }

        /// <summary>
        /// When overridden in a derived class, gets the KeysMeta of this dictionary type.
        /// </summary>
        internal virtual DictionaryMeta Meta => null!;

        /// <summary>
        /// Represents the interface to the elements of a PDF dictionary.
        /// </summary>
        [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
        public sealed class DictionaryElements : IDictionary<string, PdfItem?>, ICloneable
        {
            internal DictionaryElements(PdfDictionary ownerDictionary)
            {
                _elements = new Dictionary<string, PdfItem?>();
                _ownerDictionary = ownerDictionary;
            }

            object ICloneable.Clone()
            {
                var dictionaryElements = (DictionaryElements)MemberwiseClone();
                dictionaryElements._elements = new Dictionary<string, PdfItem?>(dictionaryElements._elements);
                dictionaryElements._ownerDictionary = null!;
                return dictionaryElements;
            }

            /// <summary>
            /// Creates a shallow copy of this object. The clone is not owned by a dictionary anymore.
            /// </summary>
            public DictionaryElements Clone()
                => (DictionaryElements)((ICloneable)this).Clone();

            /// <summary>
            /// Moves this instance to another dictionary during object type transformation.
            /// </summary>
            internal void ChangeOwner(PdfDictionary ownerDictionary)
            {
                if (_ownerDictionary != null!)
                {
                    // ???
                }

                // Set new owner.
                _ownerDictionary = ownerDictionary;

                // Set owners elements to this.
                ownerDictionary._elements = this;
            }

            /// <summary>
            /// Gets the dictionary to which this elements object belongs to.
            /// </summary>
            internal PdfDictionary Owner => _ownerDictionary ?? NRT.ThrowOnNull<PdfDictionary>();

            /// <summary>
            /// Converts the specified value to boolean.
            /// If the value does not exist, the function returns false.
            /// If the value is not convertible, the function throws an InvalidCastException.
            /// </summary>
            public bool GetBoolean(string key, bool create)
            {
                object? obj = this[key];
                if (obj == null)
                {
                    if (create)
                        this[key] = new PdfBoolean();
                    return false;
                }

                //if (obj is PdfReference reference)
                //    obj = reference.Value;
                PdfReference.Dereference(ref obj);

                return obj switch
                {
                    PdfBoolean boolean => boolean.Value,
                    PdfBooleanObject booleanObject => booleanObject.Value,
                    _ => throw new InvalidCastException("GetBoolean: Object is not a boolean.")
                };
            }

            /// <summary>
            /// Converts the specified value to boolean.
            /// If the value does not exist, the function returns false.
            /// If the value is not convertible, the function throws an InvalidCastException.
            /// </summary>
            public bool GetBoolean(string key)
                => GetBoolean(key, false);

            /// <summary>
            /// Sets the entry to a direct boolean value.
            /// </summary>
            public void SetBoolean(string key, bool value)
                => this[key] = new PdfBoolean(value);

            /// <summary>
            /// Converts the specified value to integer.
            /// If the value does not exist, the function returns 0.
            /// If the value is not convertible, the function throws an InvalidCastException.
            /// </summary>
            public int GetInteger(string key, bool create)
            {
                object? obj = this[key];
                if (obj == null)
                {
                    if (create)
                        this[key] = new PdfInteger();
                    return 0;
                }

                if (obj is PdfNull)
                    return 0;

                if (obj is PdfReference reference)
                    obj = reference.Value;

                return obj switch
                {
                    PdfInteger integer => integer.Value,
                    PdfIntegerObject integerObject => integerObject.Value,
                    _ => throw new InvalidCastException("GetInteger: Object is not an integer.")
                };
            }

            /// <summary>
            /// Converts the specified value to integer.
            /// If the value does not exist, the function returns 0.
            /// If the value is not convertible, the function throws an InvalidCastException.
            /// </summary>
            public int GetInteger(string key)
                => GetInteger(key, false);

            /// <summary>
            /// Sets the entry to a direct integer value.
            /// </summary>
            public void SetInteger(string key, int value)
                => this[key] = new PdfInteger(value);

            /// <summary>
            /// Converts the specified value to double.
            /// If the value does not exist, the function returns 0.
            /// If the value is not convertible, the function throws an InvalidCastException.
            /// </summary>
            public double GetReal(string key, bool create)
            {
                object? obj = this[key];
                if (obj == null)
                {
                    if (create)
                        this[key] = new PdfReal();
                    return 0;
                }

                if (obj is PdfReference reference)
                    obj = reference.Value;

                return obj switch
                {
                    PdfReal real => real.Value,
                    PdfRealObject realObject => realObject.Value,
                    PdfInteger integer => integer.Value,
                    PdfIntegerObject integerObject => integerObject.Value,
                    _ => throw new InvalidCastException("GetReal: Object is not a number.")
                };
            }

            /// <summary>
            /// Converts the specified value to double.
            /// If the value does not exist, the function returns 0.
            /// If the value is not convertible, the function throws an InvalidCastException.
            /// </summary>
            public double GetReal(string key)
                => GetReal(key, false);

            /// <summary>
            /// Sets the entry to a direct double value.
            /// </summary>
            public void SetReal(string key, double value)
                => this[key] = new PdfReal(value);

            /// <summary>
            /// Converts the specified value to String.
            /// If the value does not exist, the function returns the empty string.
            /// </summary>
            public string GetString(string key, bool create)
            {
                object? obj = this[key];
                if (obj == null)
                {
                    if (create)
                        this[key] = new PdfString();
                    return "";
                }

                if (obj is PdfReference reference)
                    obj = reference.Value;

                return obj switch
                {
                    PdfString str => str.Value,
                    PdfStringObject strObject => strObject.Value,
                    PdfName name => name.Value,
                    PdfNameObject nameObject => nameObject.Value,
                    _ => throw new InvalidCastException("GetString: Object is not a string.")
                };
            }

            /// <summary>
            /// Converts the specified value to String.
            /// If the value does not exist, the function returns the empty string.
            /// </summary>
            public string GetString(string key)
                => GetString(key, false);

            /// <summary>
            /// Tries to get the string. TODO: more TryGet...
            /// </summary>
            public bool TryGetString(string key, /*[MaybeNullWhen(false)]*/ out string value)
            {
                value = null;
                var obj = this[key];
                if (obj == null)
                    return false;

                if (obj is PdfReference reference)
                    obj = reference.Value;

                switch (obj)
                {
                    case PdfString str:
                        value = str.Value;
                        return true;
                    case PdfStringObject strObject:
                        value = strObject.Value;
                        return true;
                    case PdfName name:
                        value = name.Value;
                        return true;
                    case PdfNameObject nameObject:
                        value = nameObject.Value;
                        return true;
                    default:
                        return false;
                }
            }

            /// <summary>
            /// Sets the entry to a direct string value.
            /// </summary>
            public void SetString(string key, string value)
                => this[key] = new PdfString(value);

            /// <summary>
            /// Converts the specified value to a name.
            /// If the value does not exist, the function returns the empty string.
            /// </summary>
            public string GetName(string key)
            {
                var obj = this[key];
                if (obj == null)
                {
                    //if (create)
                    //  this[key] = new Pdf();
                    return String.Empty;
                }

                if (obj is PdfReference reference)
                    obj = reference.Value;

                return obj switch
                {
                    PdfName name => name.Value,
                    PdfNameObject nameObject => nameObject.Value,
                    _ => throw new InvalidCastException("GetName: Object is not a name.")
                };
            }

            /// <summary>
            /// Sets the specified name value.
            /// If the value doesn't start with a slash, it is added automatically.
            /// </summary>
            public void SetName(string key, string value)
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                if (value.Length == 0 || value[0] != '/')
                    value = "/" + value;

                this[key] = new PdfName(value);
            }

            /// <summary>
            /// Converts the specified value to PdfRectangle.
            /// If the value does not exist, the function returns an empty rectangle.
            /// If the value is not convertible, the function throws an InvalidCastException.
            /// </summary>
            public PdfRectangle GetRectangle(string key, bool create)
            {
                var value = new PdfRectangle();
                var obj = this[key];
                if (obj == null)
                {
                    if (create)
                        this[key] = value = new PdfRectangle();
                    return value;
                }
                if (obj is PdfReference reference)
                    obj = reference.Value;

                if (obj is PdfArray { Elements.Count: 4 } array)
                {
                    value = new PdfRectangle(array.Elements.GetReal(0), array.Elements.GetReal(1),
                      array.Elements.GetReal(2), array.Elements.GetReal(3));
                    this[key] = value;
                }
                else
                    value = (PdfRectangle)obj;
                return value;
            }

            /// <summary>
            /// Converts the specified value to PdfRectangle.
            /// If the value does not exist, the function returns an empty rectangle.
            /// If the value is not convertible, the function throws an InvalidCastException.
            /// </summary>
            public PdfRectangle GetRectangle(string key)
                => GetRectangle(key, false);

            /// <summary>
            /// Sets the entry to a direct rectangle value, represented by an array with four values.
            /// </summary>
            public void SetRectangle(string key, PdfRectangle rect)
                => _elements[key] = rect;

            /// Converts the specified value to XMatrix.
            /// If the value does not exist, the function returns an identity matrix.
            /// If the value is not convertible, the function throws an InvalidCastException.
            public XMatrix GetMatrix(string key, bool create)
            {
                var value = new XMatrix();
                var obj = this[key];
                if (obj == null)
                {
                    if (create)
                        this[key] = new PdfLiteral("[1 0 0 1 0 0]");  // cannot be parsed, implement a PdfMatrix...
                    return value;
                }

                if (obj is PdfReference reference)
                    obj = reference.Value;

                value = obj switch
                {
                    PdfArray { Elements.Count: 6 } array
                        => new XMatrix(array.Elements.GetReal(0),
                        array.Elements.GetReal(1), array.Elements.GetReal(2), array.Elements.GetReal(3),
                        array.Elements.GetReal(4), array.Elements.GetReal(5)),
                    PdfLiteral => throw new NotImplementedException("Parsing matrix from literal."),
                    _ => throw new InvalidCastException("Element is not an array with 6 values.")
                };
                return value;
            }

            /// Converts the specified value to XMatrix.
            /// If the value does not exist, the function returns an identity matrix.
            /// If the value is not convertible, the function throws an InvalidCastException.
            public XMatrix GetMatrix(string key)
                => GetMatrix(key, false);

            /// <summary>
            /// Sets the entry to a direct matrix value, represented by an array with six values.
            /// </summary>
            public void SetMatrix(string key, XMatrix matrix)
                => _elements[key] = PdfLiteral.FromMatrix(matrix);

            /// <summary>
            /// Converts the specified value to DateTime.
            /// If the value does not exist, the function returns the specified default value.
            /// If the value is not convertible, the function throws an InvalidCastException.
            /// </summary>
            public DateTime GetDateTime(string key, DateTime defaultValue)
            {
                var obj = this[key];
                if (obj == null)
                    return defaultValue;

                // TODO obj = PdfReference.GetValueIfReference(obj)
                if (obj is PdfReference reference)
                    obj = reference.Value;

                if (obj is PdfDate date)
                    return date.Value;

                string strDate;
                if (obj is PdfString pdfString)
                    strDate = pdfString.Value;
                else
                {
                    if (obj is PdfStringObject stringObject)
                        strDate = stringObject.Value;
                    else
                        throw new InvalidCastException("GetName: Object is not a name.");
                }

                if (strDate != "")
                {
                    try
                    {
                        defaultValue = Parser.ParseDateTime(strDate, defaultValue);
                    }
                    // ReSharper disable EmptyGeneralCatchClause
                    catch { }
                    // ReSharper restore EmptyGeneralCatchClause
                }
                return defaultValue;
            }

            /// <summary>
            /// Sets the entry to a direct datetime value.
            /// </summary>
            public void SetDateTime(string key, DateTime value)
                => _elements[key] = new PdfDate(value);

            internal int GetEnumFromName(string key, object defaultValue, bool create)
            {
                if (defaultValue is not Enum)
                    throw new ArgumentException(nameof(defaultValue));

                var obj = this[key];
                if (obj == null)
                {
                    if (create)
                        this[key] = new PdfName(defaultValue.ToString()!);

                    // ReSharper disable once PossibleInvalidCastException because Enum objects can always be cast to int.
                    return (int)defaultValue;
                }
                //Debug.Assert(obj is Enum);  // BUG This always fails.
                return (int)Enum.Parse(defaultValue.GetType(), obj.ToString()?.Substring(1)/*[1..]*/ ?? "", false);
            }

            internal int GetEnumFromName(string key, object defaultValue)
                => GetEnumFromName(key, defaultValue, false);

            internal void SetEnumAsName(string key, object value)
            {
                if (value is not Enum)
                    throw new ArgumentException(nameof(value));
                _elements[key] = new PdfName("/" + value);
            }

            /// <summary>
            /// Gets the value for the specified key. If the value does not exist, it is optionally created.
            /// </summary>
            public PdfItem? GetValue(string key, VCF options)
            {
                // PdfDictionary? dict;
                // PdfArray? array;
                var value = this[key];
                //if (value == null ||
                //    value is PdfNull ||
                //    value is PdfReference && ((PdfReference)value).Value is PdfNullObject)
                if (value is null or PdfNull or PdfReference { Value: PdfNullObject })
                {
                    if (options != VCF.None)
                    {
#if DEBUG_
                        if (key == "/Resources")
                            Debug-Break.Break();
#endif
                        var type = GetValueType(key);
                        if (type != null)
                        {
                            Debug.Assert(typeof(PdfItem).IsAssignableFrom(type), "Type not allowed.");
                            PdfObject? obj;
                            if (typeof(PdfDictionary).IsAssignableFrom(type))
                            {
                                value = obj = CreateDictionary(type, null);
                            }
                            else if (typeof(PdfArray).IsAssignableFrom(type))
                            {
                                value = obj = CreateArray(type, null);
                            }
                            else
                                throw new NotImplementedException("Type other than array or dictionary.");

                            if (options == VCF.CreateIndirect)
                            {
                                _ownerDictionary?.Owner.IrefTable.Add(obj);
                                this[key] = obj.Reference;
                            }
                            else
                                this[key] = obj;
                        }
                        else
                            throw new NotImplementedException("Cannot create value for key: " + key);
                    }
                }
                else
                {
                    // The value exists and can be returned. But for imported documents check for necessary
                    // object type transformation.
                    PdfReference? iref;
                    if ((iref = value as PdfReference) != null)
                    {
                        // Case: value is an indirect reference.
                        value = iref.Value;
                        if (value == null)
                        {
                            // If we come here PDF file is corrupted.
                            throw new InvalidOperationException("Indirect reference without value.");
                        }

                        if (true) // || _owner.Document.IsImported)
                        {
                            var type = GetValueType(key);
                            Debug.Assert(type != null, "No value type specified in meta information. Please send this file to PDFsharp support.");

                            if (type != null! && type != value.GetType())
                            {
                                if (typeof(PdfDictionary).IsAssignableFrom(type))
                                {
                                    Debug.Assert(value is PdfDictionary, "Bug in PDFsharp. Please send this file to PDFsharp support.");
                                    value = CreateDictionary(type, (PdfDictionary)value);
                                }
                                else if (typeof(PdfArray).IsAssignableFrom(type))
                                {
                                    Debug.Assert(value is PdfArray, "Bug in PDFsharp. Please send this file to PDFsharp support.");
                                    value = CreateArray(type, (PdfArray)value);
                                }
                                else
                                    throw new NotImplementedException("Type other than array or dictionary.");
                            }
                        }
                        return value;
                    }

                    // Transformation is only possible after PDF import.
                    if (true) // || _owner.Document.IsImported)
                    {
                        // Case: value is a direct object
                        //if ((dict = value as PdfDictionary) != null)
                        if (value is PdfDictionary dict)
                        {
                            Debug.Assert(!dict.IsIndirect);

                            var type = GetValueType(key);
                            Debug.Assert(type != null, "No value type specified in meta information. Please send this file to PDFsharp support.");
                            if (dict.GetType() != type)
                                dict = CreateDictionary(type, dict);
                            return dict;
                        }

                        //if ((array = value as PdfArray) != null)
                        if (value is PdfArray array)
                        {
                            Debug.Assert(!array.IsIndirect);

                            var type = GetValueType(key);
                            // This is more complicated. If type is null do nothing
                            //Debug.Assert(type != null, "No value type specified in meta information. Please send this file to PDFsharp support.");
                            if (type != null && type != array.GetType())
                                array = CreateArray(type, array);
                            return array;
                        }
                    }
                }
                return value;
            }

            /// <summary>
            /// Short cut for GetValue(key, VCF.None).
            /// </summary>
            public PdfItem? GetValue(string key)
                => GetValue(key, VCF.None);

            /// <summary>
            /// Returns the type of the object to be created as value of the specified key.
            /// </summary>
            Type? GetValueType(string key)  // TODO: move to PdfObject
            {
                Type? type = null;
                var meta = _ownerDictionary.Meta;
                if (meta != null!)
                {
                    var kd = meta[key];
                    if (kd != null)
                        type = kd.GetValueType();
                    //else
                    //    Debug.WriteLine("Warning: Key not descriptor table: " + key);  // TODO: check what this means...
                }
                //else
                //    Debug.WriteLine("Warning: No meta provided for type: " + _owner.GetType().Name);  // TODO: check what this means...
                return type;
            }

            PdfArray CreateArray(Type type, PdfArray? oldArray)
            {
#if true
                //ConstructorInfo ctorInfo;
                PdfArray? array;
                if (oldArray == null)
                {
                    // Use constructor with signature 'Ctor(PdfDocument owner)'.
                    var ctorInfo = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                          null, new[] { typeof(PdfDocument) }, null);
                    Debug.Assert(ctorInfo != null, "No appropriate constructor found for type: " + type.Name);
                    //array = ctorInfo.Invoke(new object[] { _ownerDictionary.Owner }) as PdfArray;
                    array = ctorInfo.Invoke(new object[] { _ownerDictionary.Owner }) as PdfArray;
                }
                else
                {
                    // Use constructor with signature 'Ctor(PdfDictionary dict)'.
                    var ctorInfo = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                                null, new[] { typeof(PdfArray) }, null);
                    Debug.Assert(ctorInfo != null, "No appropriate constructor found for type: " + type.Name);
                    //array = ctorInfo.Invoke(new object[] { oldArray }) as PdfArray;
                    array = ctorInfo.Invoke(new object[] { oldArray }) as PdfArray;
                }
                return array ?? NRT.ThrowOnNull<PdfArray>();
#else
                // Rewritten Win_RT style.
                PdfArray array = null;
                if (oldArray == null)
                {
                    // Use constructor with signature 'Ctor(PdfDocument owner)'.
                    var ctorInfos = type.GetTypeInfo().DeclaredConstructors; //.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    //null, new Type[] { typeof(PdfDocument) }, null);
                    foreach (var ctorInfo in ctorInfos)
                    {
                        var parameters = ctorInfo.GetParameters();
                        if (parameters.Length == 1 && parameters[0].ParameterType == typeof(PdfDocument))
                        {
                            array = ctorInfo.Invoke(new object[] { _ownerDictionary.Owner }) as PdfArray;
                            break;
                        }
                    }
                    Debug.Assert(array != null, "No appropriate constructor found for type: " + type.Name);
                }
                else
                {
                    // Use constructor with signature 'Ctor(PdfDictionary dict)'.
                    var ctorInfos = type.GetTypeInfo().DeclaredConstructors; // .GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    //null, new Type[] { typeof(PdfArray) }, null);
                    foreach (var ctorInfo in ctorInfos)
                    {
                        var parameters = ctorInfo.GetParameters();
                        if (parameters.Length == 1 && parameters[0].ParameterType == typeof(PdfArray))
                        {
                            array = ctorInfo.Invoke(new object[] { oldArray }) as PdfArray;
                            break;
                        }
                    }
                    Debug.Assert(array != null, "No appropriate constructor found for type: " + type.Name);
                }
                return array;
#endif
            }

            PdfDictionary CreateDictionary(Type type, PdfDictionary? oldDictionary)
            {
#if true
                ConstructorInfo? ctorInfo;
                PdfDictionary? dict;
                if (oldDictionary == null)
                {
                    // Use constructor with signature 'Ctor(PdfDocument owner)'.
                    ctorInfo = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null, new[] { typeof(PdfDocument) }, null);
                    Debug.Assert(ctorInfo != null, "No appropriate constructor found for type: " + type.Name);
                    dict = ctorInfo.Invoke(new object[] { _ownerDictionary.Owner }) as PdfDictionary;
                }
                else
                {
                    // Use constructor with signature 'Ctor(PdfDictionary dict)'.
                    ctorInfo = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                      null, new[] { typeof(PdfDictionary) }, null);
                    Debug.Assert(ctorInfo != null, "No appropriate constructor found for type: " + type.Name);
                    dict = ctorInfo.Invoke(new object[] { oldDictionary }) as PdfDictionary;
                }
                return dict ?? NRT.ThrowOnNull<PdfDictionary>();
#else
                // Rewritten Win_RT style.
                PdfDictionary dict = null;
                if (oldDictionary == null)
                {
                    // Use constructor with signature 'Ctor(PdfDocument owner)'.
                    var ctorInfos = type.GetTypeInfo().DeclaredConstructors; //GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    //null, new Type[] { typeof(PdfDocument) }, null);
                    foreach (var ctorInfo in ctorInfos)
                    {
                        var parameters = ctorInfo.GetParameters();
                        if (parameters.Length == 1 && parameters[0].ParameterType == typeof(PdfDocument))
                        {
                            dict = ctorInfo.Invoke(new object[] { _ownerDictionary.Owner }) as PdfDictionary;
                            break;
                        }
                    }
                    Debug.Assert(dict != null, "No appropriate constructor found for type: " + type.Name);
                }
                else
                {
                    var ctorInfos = type.GetTypeInfo().DeclaredConstructors; // GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(PdfDictionary) }, null);
                    foreach (var ctorInfo in ctorInfos)
                    {
                        var parameters = ctorInfo.GetParameters();
                        if (parameters.Length == 1 && parameters[0].ParameterType == typeof(PdfDictionary))
                        {
                            dict = ctorInfo.Invoke(new object[] { _ownerDictionary.Owner }) as PdfDictionary;
                            break;
                        }
                    }
                    Debug.Assert(dict != null, "No appropriate constructor found for type: " + type.Name);
                }
                return dict;
#endif
            }

            PdfItem CreateValue(Type type, PdfDictionary? oldValue)
            {
#if true
                var ctorInfo = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null, new Type[] { typeof(PdfDocument) }, null);
                var obj = ctorInfo!.Invoke(new object[] { _ownerDictionary.Owner }) as PdfObject;
                if (oldValue != null)
                {
                    obj!.Reference = oldValue.Reference;
                    if (obj.Reference != null)
                    {
                        obj.Reference.Value = obj;
                        if (obj is PdfDictionary dict)
                            dict._elements = oldValue._elements;
                    }
                }
                return obj ?? NRT.ThrowOnNull<PdfItem>();
#else
                // Rewritten Win_RT style.
                PdfObject obj = null;
                var ctorInfos = type.GetTypeInfo().DeclaredConstructors; // GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(PdfDocument) }, null);
                foreach (var ctorInfo in ctorInfos)
                {
                    var parameters = ctorInfo.GetParameters();
                    if (parameters.Length == 1 && parameters[0].ParameterType == typeof(PdfDocument))
                    {
                        obj = ctorInfo.Invoke(new object[] { _ownerDictionary.Owner }) as PdfObject;
                        break;
                    }
                }
                Debug.Assert(obj != null, "No appropriate constructor found for type: " + type.Name);
                if (oldValue != null)
                {
                    obj.Reference = oldValue.Reference;
                    obj.Reference.Value = obj;
                    if (obj is PdfDictionary)
                    {
                        PdfDictionary dict = (PdfDictionary)obj;
                        dict._elements = oldValue._elements;
                    }
                }
                return obj;
#endif
            }

            /// <summary>
            /// Sets the entry with the specified value. DON'T USE THIS FUNCTION - IT MAY BE REMOVED.
            /// </summary>
            public void SetValue(string key, PdfItem value)
            {
                //Debug.Assert((value is PdfObject && ((PdfObject)value).Reference == null) | !(value is PdfObject),
                Debug.Assert(value is PdfObject { Reference: null } or not PdfObject,
                "You try to set an indirect object directly into a dictionary.");

                // HACK?
                _elements[key] = value;
            }
            
            /// <summary>
            /// Gets the PdfObject with the specified key, or null if no such object exists. If the key refers to
            /// a reference, the referenced PdfObject is returned.
            /// </summary>
            public PdfObject? GetObject(string key)
            {
                var item = this[key];
                if (item is PdfReference reference)
                    return reference.Value;
                return item as PdfObject;
            }

            /// <summary>
            /// Gets the PdfDictionary with the specified key, or null if no such object exists. If the key refers to
            /// a reference, the referenced PdfDictionary is returned.
            /// </summary>
            public PdfDictionary? GetDictionary(string key)
                => GetObject(key) as PdfDictionary;

            /// <summary>
            /// Gets the PdfArray with the specified key, or null if no such object exists. If the key refers to
            /// a reference, the referenced PdfArray is returned.
            /// </summary>
            public PdfArray? GetArray(string key)
                => GetObject(key) as PdfArray;

            /// <summary>
            /// Gets the PdfReference with the specified key, or null if no such object exists.
            /// </summary>
            public PdfReference? GetReference(string key)
            {
                var item = this[key];
                return item as PdfReference;
            }

            /// <summary>
            /// Sets the entry to the specified object. The object must not be an indirect object,
            /// otherwise an exception is raised.
            /// </summary>
            public void SetObject(string key, PdfObject obj)
            {
                if (obj.Reference is not null)
                    throw new ArgumentException("PdfObject must not be an indirect object.", nameof(obj));
                this[key] = obj;
            }

            /// <summary>
            /// Sets the entry as a reference to the specified object. The object must be an indirect object,
            /// otherwise an exception is raised.
            /// </summary>
            public void SetReference(string key, PdfObject obj)
            {
                if (obj.Reference is null)
                    throw new ArgumentException("PdfObject must be an indirect object.", nameof(obj));
                this[key] = obj.Reference;
            }

            /// <summary>
            /// Sets the entry as a reference to the specified iref.
            /// </summary>
            public void SetReference(string key, PdfReference? iref)
            {
                if (iref is null)
                    throw new ArgumentNullException(nameof(iref));
                this[key] = iref;
            }

            #region IDictionary Members

            /// <summary>
            /// Gets a value indicating whether the <see cref="T:System.Collections.IDictionary"></see> object is read-only.
            /// </summary>
            public bool IsReadOnly => false;

            /// <summary>
            /// Returns an <see cref="T:System.Collections.IDictionaryEnumerator"></see> object for the <see cref="T:System.Collections.IDictionary"></see> object.
            /// </summary>
            public IEnumerator<KeyValuePair<string, PdfItem?>> GetEnumerator()
            {
                return _elements.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((ICollection)_elements).GetEnumerator();
            }

            /// <summary>
            /// Gets or sets an entry in the dictionary. The specified key must be a valid PDF name
            /// starting with a slash '/'. This property provides full access to the elements of the
            /// PDF dictionary. Wrong use can lead to errors or corrupt PDF files.
            /// </summary>
            public PdfItem? this[string key]
            {
                get
                {
                    _elements.TryGetValue(key, out var item);
                    return item;
                }
                set
                {
                    if (value == null)
                        throw new ArgumentNullException(nameof(value));
#if DEBUG_
                    if (key == "/MediaBox")
                        key.GetType();

                    //if (value is PdfObject)
                    //{
                    //  PdfObject obj = (PdfObject)value;
                    //  if (obj.Reference != null)
                    //    throw new ArgumentException("An object with an indirect reference cannot be a direct value. Try to set an indirect reference.");
                    //}
                    if (value is PdfDictionary)
                    {
                        PdfDictionary dict = (PdfDictionary)value;
                        if (dict._stream != null)
                            throw new ArgumentException("A dictionary with stream cannot be a direct value.");
                    }
#endif
                    if (value is PdfObject { IsIndirect: true } obj)
                        value = obj.Reference;
                    _elements[key] = value;
                }
            }

            /// <summary>
            /// Gets or sets an entry in the dictionary identified by a PdfName object.
            /// </summary>
            public PdfItem? this[PdfName key]
            {
                get => this[key.Value];
                set
                {
                    if (value == null)
                        throw new ArgumentNullException(nameof(value));

#if DEBUG
                    if (value is PdfDictionary dictionary)
                    {
                        var dict = dictionary;
                        if (dict.Stream != null)
                            throw new ArgumentException("A dictionary with stream cannot be a direct value.");
                    }
#endif

                    if (value is PdfObject { IsIndirect: true } obj)
                        value = obj.Reference;
                    _elements[key.Value] = value;
                }
            }

            /// <summary>
            /// Removes the value with the specified key.
            /// </summary>
            public bool Remove(string key)
            {
                return _elements.Remove(key);
            }

            /// <summary>
            /// Removes the value with the specified key.
            /// </summary>
            public bool Remove(KeyValuePair<string, PdfItem?> item)
            {
                throw new NotImplementedException();
            }

            ///// <summary>
            ///// Determines whether the dictionary contains the specified name.
            ///// </summary>
            //[Obsolete("Use ContainsKey.")]
            //public bool Contains(string key)
            //{
            //    return _elements.ContainsKey(key);
            //}

            /// <summary>
            /// Determines whether the dictionary contains the specified name.
            /// </summary>
            public bool ContainsKey(string key)
            {
                return _elements.ContainsKey(key);
            }

            /// <summary>
            /// Determines whether the dictionary contains a specific value.
            /// </summary>
            public bool Contains(KeyValuePair<string, PdfItem?> item)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Removes all elements from the dictionary.
            /// </summary>
            public void Clear()
            {
                _elements.Clear();
            }

            /// <summary>
            /// Adds the specified value to the dictionary.
            /// </summary>
            public void Add(string key, PdfItem? value)
            {
                if (String.IsNullOrEmpty(key))
                    throw new ArgumentNullException(nameof(key));

                if (key[0] != '/')
                    throw new ArgumentException("The key must start with a slash '/'.");

                // If object is indirect automatically convert value to reference.
                if (value is PdfObject { IsIndirect: true } obj)
                    value = obj.Reference;

                _elements.Add(key, value);
            }

            /// <summary>
            /// Adds an item to the dictionary.
            /// </summary>
            public void Add(KeyValuePair<string, PdfItem?> item)
            {
                Add(item.Key, item.Value);
            }

            /// <summary>
            /// Gets all keys currently in use in this dictionary as an array of PdfName objects.
            /// </summary>
            public PdfName[] KeyNames
            {
                get
                {
                    ICollection values = _elements.Keys;
                    int count = values.Count;
                    string[] strings = new string[count];
                    values.CopyTo(strings, 0);
                    PdfName[] names = new PdfName[count];
                    for (int idx = 0; idx < count; idx++)
                        names[idx] = new PdfName(strings[idx]);
                    return names;
                }
            }

            /// <summary>
            /// Get all keys currently in use in this dictionary as an array of string objects.
            /// </summary>
            public ICollection<string> Keys
            {
                // It is by design not to return _elements.Keys, but a copy.
                get
                {
                    ICollection values = _elements.Keys;
                    int count = values.Count;
                    string[] keys = new string[count];
                    values.CopyTo(keys, 0);
                    return keys;
                }
            }

            /// <summary>
            /// Gets the value associated with the specified key.
            /// </summary>
            public bool TryGetValue(string key, out PdfItem? value)
            {
                return _elements.TryGetValue(key, out value);
            }

            /// <summary>
            /// Gets all values currently in use in this dictionary as an array of PdfItem objects.
            /// </summary>
            //public ICollection<PdfItem> Values
            public ICollection<PdfItem?> Values
            {
                // It is by design not to return _elements.Values, but a copy.
                get
                {
                    ICollection values = _elements.Values;
                    var items = new PdfItem[values.Count];
                    values.CopyTo(items, 0);
                    return items;
                }
            }

            /// <summary>
            /// Return false.
            /// </summary>
            public bool IsFixedSize => false;

            #endregion

            #region ICollection Members

            /// <summary>
            /// Return false.
            /// </summary>
            public bool IsSynchronized => false;

            /// <summary>
            /// Gets the number of elements contained in the dictionary.
            /// </summary>
            public int Count => _elements.Count;

            /// <summary>
            /// Copies the elements of the dictionary to an array, starting at a particular index.
            /// </summary>
            public void CopyTo(KeyValuePair<string, PdfItem?>[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// The current implementation returns null.
            /// </summary>
            public object SyncRoot => null!;

            #endregion

            /// <summary>
            /// Access a key that may contain an array or a single item for working with its value(s).
            /// </summary>
            public ArrayOrSingleItemHelper ArrayOrSingleItem => new(this); // TODO PDFsharp6: Naming.
            
            /// <summary>
            /// Gets the DebuggerDisplayAttribute text.
            /// </summary>
            // ReSharper disable UnusedMember.Local
            internal string DebuggerDisplay
            // ReSharper restore UnusedMember.Local
            {
                get
                {
                    var sb = new StringBuilder();
                    sb.AppendFormat(CultureInfo.InvariantCulture, "key={0}:(", _elements.Count);
                    var addSpace = false;
                    ICollection<string> keys = _elements.Keys;
                    foreach (var key in keys)
                    {
                        if (addSpace)
                            sb.Append(' ');
                        addSpace = true;
                        sb.Append(key);
                    }
                    sb.Append(')');
                    return sb.ToString();
                }
            }

            /// <summary>
            /// The elements of the dictionary with a string as key.
            /// Because the string is a name it starts always with a '/'.
            /// </summary>
            Dictionary<string, PdfItem?> _elements;

            /// <summary>
            /// The dictionary this object belongs to.
            /// </summary>
            PdfDictionary _ownerDictionary;
        }

        /// <summary>
        /// The PDF stream objects.
        /// </summary>
        public sealed class PdfStream
        {
            internal PdfStream(PdfDictionary ownerDictionary)
            {
                _ownerDictionary = ownerDictionary ?? throw new ArgumentNullException(nameof(ownerDictionary));
            }

            /// <summary>
            /// A .NET string can contain char(0) as a valid character.
            /// </summary>
            internal PdfStream(byte[] value, PdfDictionary owner)
                : this(owner)
            {
                _value = value;
            }

            /// <summary>
            /// Clones this stream by creating a deep copy.
            /// </summary>
            public PdfStream Clone()
            {
                var stream = (PdfStream)MemberwiseClone();
                stream._ownerDictionary = null!;
                if (stream._value != null)
                {
                    stream._value = new byte[stream._value.Length];
                    _value!.CopyTo(stream._value, 0);
                }
                return stream;
            }

            /// <summary>
            /// Moves this instance to another dictionary during object type transformation.
            /// </summary>
            internal void ChangeOwner(PdfDictionary dict)
            {
                if (_ownerDictionary != null!)
                {
                    // ???
                }

                // Set new owner.
                _ownerDictionary = dict;

                // Set owners stream to this.
                _ownerDictionary.Stream = this;
            }

            /// <summary>
            /// The dictionary the stream belongs to.
            /// </summary>
            PdfDictionary _ownerDictionary;

            /// <summary>
            /// Gets the length of the stream, i.e. the actual number of bytes in the stream.
            /// </summary>
            public int Length => _value?.Length ?? 0;

            /// <summary>
            /// Get or sets the bytes of the stream as they are, i.e. if one or more filters exist the bytes are
            /// not unfiltered.
            /// </summary>
            public byte[] Value
            {
                get => _value ??= Array.Empty<byte>();
                set
                {
                    if (value == null!)
                        throw new ArgumentNullException(nameof(value));
                    _value = value;
                    _ownerDictionary.Elements.SetInteger("/Length", value.Length);
                }
            }
            byte[]? _value;

            /// <summary>
            /// Gets the value of the stream unfiltered. The stream content is not modified by this operation.
            /// </summary>
            public byte[] UnfilteredValue
            {
                get
                {
                    byte[]? bytes = null;
                    if (_value != null)
                    {
                        var filter = _ownerDictionary.Elements["/Filter"];
                        if (filter != null)
                        {
                            var decodeParms = _ownerDictionary.Elements[Keys.DecodeParms];
                            bytes = Filtering.Decode(_value, filter, decodeParms);
                            if (bytes == null!)
                            {
                                string message = $"«Cannot decode filter '{filter}'»";
                                bytes = PdfEncoders.RawEncoding.GetBytes(message);
                            }
                        }
                        else
                        {
                            bytes = new byte[_value.Length];
                            _value.CopyTo(bytes, 0);
                        }
                    }
                    return bytes ?? Array.Empty<byte>();
                }
            }

            /// <summary>
            /// Tries to unfilter the bytes of the stream. If the stream is filtered and PDFsharp knows the filter
            /// algorithm, the stream content is replaced by its unfiltered value and the function returns true.
            /// Otherwise, the content remains untouched and the function returns false.
            /// The function is useful for analyzing existing PDF files.
            /// </summary>
            public bool TryUnfilter()
            {
                if (_value != null)
                {
                    var filter = _ownerDictionary.Elements["/Filter"];
                    if (filter != null)
                    {
                        // PDFsharp can only uncompress streams that are compressed with the ZIP or LZH algorithm.
                        var decodeParms = _ownerDictionary.Elements[Keys.DecodeParms];
                        var bytes = Filtering.Decode(_value, filter, decodeParms);
                        if (bytes != null)
                        {
                            _ownerDictionary.Elements.Remove("/Filter");
                            _ownerDictionary.Elements.Remove(Keys.DecodeParms);
                            Value = bytes;
                        }
                        else
                            return false;
                    }
                }
                return true;
            }

            /// <summary>
            /// Compresses the stream with the FlateDecode filter.
            /// If a filter is already defined, the function has no effect.
            /// </summary>
            public void Zip()
            {
                if (_value == null)
                    return;

                if (!_ownerDictionary.Elements.ContainsKey("/Filter"))
                {
                    _value = Filtering.FlateDecode.Encode(_value, _ownerDictionary._document.Options.FlateEncodeMode);
                    _ownerDictionary.Elements["/Filter"] = new PdfName("/FlateDecode");
                    _ownerDictionary.Elements["/Length"] = new PdfInteger(_value.Length);
                }
            }

            /// <summary>
            /// Returns the stream content as a raw string.
            /// </summary>
            public override string ToString()
            {
                if (_value == null)
                    return "«null»";

                string stream;
                var filter = _ownerDictionary.Elements["/Filter"];
                if (filter != null)
                {
#if true
                    var decodeParms = _ownerDictionary.Elements[Keys.DecodeParms];
                    var bytes = Filtering.Decode(_value, filter, decodeParms);
                    if (bytes != null)
                        stream = PdfEncoders.RawEncoding.GetString(bytes, 0, bytes.Length);
#else

                    if (_owner.Elements.GetString("/Filter") == "/FlateDecode")
                    {
                        stream = Filtering.FlateDecode.DecodeToString(_value);
                    }
#endif
                    else
                        throw new NotImplementedException("Unknown filter");
                }
                else
                    stream = PdfEncoders.RawEncoding.GetString(_value, 0, _value.Length);

                return stream;
            }

            //internal void WriteObject_(Stream stream)
            //{
            //    if (_value != null)
            //        stream.Write(_value, 0, value.Length);
            //}

            ///// <summary>
            ///// Converts a raw encoded string into a byte array.
            ///// </summary>
            //public static byte[] RawEncode(string content)
            //{
            //    return PdfEncoders.RawEncoding.GetBytes(content);
            //}

            /// <summary>
            /// Common keys for all streams.
            /// </summary>
            public class Keys : KeysBase
            {
                // ReSharper disable InconsistentNaming

                /// <summary>
                /// (Required) The number of bytes from the beginning of the line following the keyword
                /// stream to the last byte just before the keyword endstream. (There may be an additional
                /// EOL marker, preceding endstream, that is not included in the count and is not logically
                /// part of the stream data.)
                /// </summary>
                [KeyInfo(KeyType.Integer | KeyType.Required)]
                public const string Length = "/Length";

                /// <summary>
                /// (Optional) The name of a filter to be applied in processing the stream data found between
                /// the keywords stream and endstream, or an array of such names. Multiple filters should be
                /// specified in the order in which they are to be applied.
                /// </summary>
                [KeyInfo(KeyType.NameOrArray | KeyType.Optional)]
                public const string Filter = "/Filter";

                /// <summary>
                /// (Optional) A parameter dictionary or an array of such dictionaries, used by the filters
                /// specified by Filter. If there is only one filter and that filter has parameters, DecodeParms
                /// must be set to the filter’s parameter dictionary unless all the filter’s parameters have
                /// their default values, in which case the DecodeParms entry may be omitted. If there are 
                /// multiple filters and any of the filters has parameters set to nondefault values, DecodeParms
                /// must be an array with one entry for each filter: either the parameter dictionary for that
                /// filter, or the null object if that filter has no parameters (or if all of its parameters have
                /// their default values). If none of the filters have parameters, or if all their parameters
                /// have default values, the DecodeParms entry may be omitted.
                /// </summary>
                [KeyInfo(KeyType.ArrayOrDictionary | KeyType.Optional)]
                public const string DecodeParms = "/DecodeParms";

                /// <summary>
                /// (Optional; PDF 1.2) The file containing the stream data. If this entry is present, the bytes
                /// between stream and endstream are ignored, the filters are specified by FFilter rather than
                /// Filter, and the filter parameters are specified by FDecodeParms rather than DecodeParms.
                /// However, the Length entry should still specify the number of those bytes. (Usually, there are
                /// no bytes and Length is 0.)
                /// </summary>
                [KeyInfo("1.2", KeyType.String | KeyType.Optional)]
                public const string F = "/F";

                /// <summary>
                /// (Optional; PDF 1.2) The name of a filter to be applied in processing the data found in the
                /// stream’s external file, or an array of such names. The same rules apply as for Filter.
                /// </summary>
                [KeyInfo("1.2", KeyType.NameOrArray | KeyType.Optional)]
                public const string FFilter = "/FFilter";

                /// <summary>
                /// (Optional; PDF 1.2) A parameter dictionary, or an array of such dictionaries, used by the
                /// filters specified by FFilter. The same rules apply as for DecodeParms.
                /// </summary>
                [KeyInfo("1.2", KeyType.ArrayOrDictionary | KeyType.Optional)]
                public const string FDecodeParms = "/FDecodeParms";

                /// <summary>
                /// Optional; PDF 1.5) A non-negative integer representing the number of bytes in the decoded
                /// (defiltered) stream. It can be used to determine, for example, whether enough disk space is
                /// available to write a stream to a file.
                /// This value should be considered a hint only; for some stream filters, it may not be possible
                /// to determine this value precisely.
                /// </summary>
                [KeyInfo("1.5", KeyType.Integer | KeyType.Optional)]
                public const string DL = "/DL";

                // ReSharper restore InconsistentNaming
            }
        }

        /// <summary>
        /// Gets the DebuggerDisplayAttribute text.
        /// </summary>
        // ReSharper disable UnusedMember.Local
        string DebuggerDisplay
        // ReSharper restore UnusedMember.Local
        {
            get
            {
#if true
                return String.Format(CultureInfo.InvariantCulture, "dictionary({0},[{1}])={2}",
                    ObjectID.DebuggerDisplay,
                    Elements.Count,
                    _elements?.DebuggerDisplay);
#else
                return String.Format(CultureInfo.InvariantCulture, "dictionary({0},[{1}])=", ObjectID.DebuggerDisplay, _elements.DebuggerDisplay);
#endif
            }
        }
    }
}

// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Runtime.CompilerServices;
using System.Collections;
using System.Text;
using Microsoft.Extensions.Logging;
using PdfSharp.Internal;
using PdfSharp.Logging;
using PdfSharp.Drawing;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Filters;
using PdfSharp.Pdf.Internal;
using PdfSharp.Pdf.IO;

// TODO REMOVE
#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Represents a PDF dictionary object.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public class PdfDictionary : PdfContainer, IEnumerable<KeyValuePair<string, PdfItem>>  // TODO: explain why not PdfItem? anymore
    {
        // Reference 1.7: 3.2.6  Dictionary Objects / Page 59
        // Reference 2.0: 7.3.7  Dictionary objects / Page 30

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfDictionary"/> class.
        /// </summary>
        public PdfDictionary()
        {
            // Direct object.
            ItemFlags |= ItemFlags.IsDictionary;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfDictionary"/> class.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="createIndirect">If true creates an indirect object.</param>
        public PdfDictionary(PdfDocument document, bool createIndirect = false)
            : base(document, createIndirect)
        {
            ItemFlags |= ItemFlags.IsDictionary;
        }

        /// <summary>
        /// Initializes a new instance from an existing dictionary.
        /// Used for object type transformation.
        /// </summary>
        protected PdfDictionary(PdfDictionary dict)
            : base(dict)
        {
#if DEBUG
            // Protect against unintended invocation.
            var oldType = dict.GetType();
            if (oldType != typeof(PdfDictionary))
            {
                var newType = GetType();
                if (oldType == newType)
                {
                    throw new InvalidOperationException($"You try to convert a PDF dictionary into type '{newType.FullName}', " +
                                                        $"but the dictionary is already of this type.");
                }

                if (!oldType.IsAssignableFrom(newType))
                {
                    throw new InvalidOperationException($"You try to convert type '{oldType.FullName}' into type '{newType.FullName}', " +
                                                        $"but '{newType.Name}' is not derived from '{oldType.Name}'.");
                }
            }
#endif
            // Move ownership of elements and stream to this instance.
            dict._elements?.ChangeOwner(this);
            dict.Stream?.ChangeOwner(this);
            dict.SetDead();
            ItemFlags |= ItemFlags.IsDictionary | ItemFlags.IsTransformed;
        }

        /// <summary>
        /// Creates a copy of this dictionary.
        /// Direct values are deep-copied. Indirect references are not modified.
        /// </summary>
        public new PdfDictionary Clone()
            => (PdfDictionary)Copy();

        /// <summary>  // TODO check
        /// This function is useful for importing objects from external documents. The returned object is not
        /// yet complete. irefs refer to external objects and directed objects are cloned but their document
        /// property is null. A cloned dictionary or array needs a 'fix-up' to be a valid object.
        /// </summary>
        protected override object Copy()
        {
            // Clone dictionary.
            var dict = (PdfDictionary)base.Copy();
            var elements = dict._elements;
            if (elements != null)
            {
                elements = elements.Clone();
                dict._elements = elements;
                elements.ChangeOwner(dict);
            }

            var stream = dict.Stream;
            if (stream != null)
            {
                stream = stream.Clone();
                dict.Stream = stream;
                stream.ChangeOwner(dict);
            }
            return dict;
        }

        /// <summary>
        /// Gets the dictionary containing the elements of this dictionary.
        /// </summary>
        public DictionaryElements Elements
        {
            get
            {
                EnsureAlive();
                return _elements ??= new(this);
            }
            set
            {
                if (_elements != null && value != null)
                    throw new InvalidOperationException("Elements cannot be set if already created.");
                _elements = value;
            }
        }

        /// <summary>
        /// The elements of the dictionary.
        /// </summary>
        DictionaryElements? _elements;

        /// <summary>
        /// Returns an enumerator that iterates through the dictionary elements.
        /// </summary>
        public IEnumerator<KeyValuePair<string, PdfItem>> GetEnumerator()
            => Elements.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        /// <summary>
        /// Returns a string with the content of this object in a readable form.
        /// Useful for debugging purposes only.
        /// </summary>
        public override string ToString()
        {
            // Get keys and sort.
            PdfName[] keys = Elements.KeyNames;
            List<PdfName> list = [.. keys];
            list.Sort(/*PdfName.*/Comparer);
            list.CopyTo(keys, 0);

            var text = new StringBuilder();
            text.Append("<<");
            foreach (var key in keys)
                text.Append(key + " " + Elements[key] + " ");  // Need PdfReference here.
            text.Append(">>");

            return text.ToString();
        }

        internal override void WriteObject(PdfWriter writer)
        {
#if DEBUG
            if (ObjectID.ObjectNumber == 1)
                _ = typeof(int);
#endif
            writer.WriteBeginObject(this);
            //int count = Elements.Count;
            PdfName[] keys = Elements.KeyNames;

#if DEBUG
            // TODO_OLD: automatically set length
            if (Stream != null)
                Debug.Assert(Elements.ContainsKey("/Length"), "Dictionary has a stream but no length is set.");
#endif
            if (_stream is not null && writer.EffectiveSecurityHandler != null)
            {
                // Encryption could change the size of the stream.
                // Encrypt the bytes before writing the dictionary to get and update the actual size.
                var bytes = (byte[])_stream.Value.Clone();
                writer.EffectiveSecurityHandler.EncryptStream(ref bytes, this);
                _stream.Value = bytes;
                Elements[PdfStream.Keys.Length] = new PdfInteger(_stream?.Length ?? 0);
            }

            // Sort keys for debugging purposes. Comparing PDF files with for example programs like
            // Araxis Merge is easier with sorted keys.
            if (writer.IsVerboseLayout)
            {
                var list = new List<PdfName>(keys);
                list.Sort(/*PdfName.*/Comparer);
                list.CopyTo(keys, 0);
            }

            foreach (var key in keys)
                WriteDictionaryElement(writer, key);
            if (Stream != null)
                WriteDictionaryStream(writer);
            writer.WriteEndObject();
        }

        /// <summary>
        /// Writes a key-value pair of this dictionary.
        /// This function is intended to be overridden in derived classes.
        /// </summary>
        internal void WriteDictionaryElement(PdfWriter writer, PdfName key)
        {
            Debug.Assert(key != null);
            var item = Elements[key]!; // We need references here. GetValue is harmful and leads to broken files. // #US373
            key.WriteObject(writer);
            //if (writer.Layout == PdfWriterLayout.Verbose)
            //    writer.WriteSpace();
            item.WriteObject(writer);
            if (writer.IsVerboseLayout)
                writer.NewLine();
        }

        /// <summary>
        /// Writes the stream of this dictionary.
        /// This function is intended to be overridden in a derived class.
        /// </summary>
        internal virtual void WriteDictionaryStream(PdfWriter writer)
        {
            if (!IsIndirect)
                throw new InvalidOperationException("Direct PDF dictionary must not have a PDF stream.");

            writer.WriteStream(this, (writer.Options & PdfWriterOptions.OmitStream) == PdfWriterOptions.OmitStream);
        }

        /// <summary>
        /// Gets the PDF stream belonging to this dictionary. Returns null if the dictionary has
        /// no stream. To create the stream, call the CreateStream function.
        /// </summary>
        public PdfStream? Stream
        {
            get
            {
                EnsureAlive();
                return _stream;
            }

            internal set
            {
                EnsureAlive();
                _stream = value;
            }
        }
        PdfStream? _stream;

        /// <summary>
        /// Creates the stream of this dictionary and initializes it with the specified byte array.
        /// The function must not be called if the dictionary already has a stream.
        /// </summary>
        public PdfStream CreateStream(byte[] value)
        {
            EnsureAlive();

            // Ensure that the object is an indirect object.

            if (_stream != null)
                throw new InvalidOperationException("The dictionary already has a stream.");

            // OK, you can create a PDF stream for a direct object. But it must become indirect before saving.
            //if (!IsIndirect)
            //    throw new InvalidOperationException("Cannot create a stream for a direct PDF dictionary.");

            Stream = new(value, this);
            // Always set the length.
            Elements["/Length"] = new PdfInteger(Stream.Length);
            return Stream;
        }

        /// <summary>
        /// Clones the elements of the specified PDF dictionary.
        /// </summary>
        internal void CloneElementsOf(PdfDictionary dic)
        {
            _elements = dic.Elements.Clone();
            // Note that ParentInfo is still null for each item.
            _elements.OwningContainer = this;
        }

        /// <summary>
        /// When overridden in a derived class, gets the KeysMeta of this dictionary type.
        /// </summary>
        internal virtual DictionaryMeta Meta => null!;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        new void EnsureAlive()
        {
            if (IsDead)
            {
                throw new InvalidOperationException(
                    "This dictionary cannot be used anymore, because its content is now owned by an object of a derived class.");
            }
        }

        /// <summary>
        /// Represents the interface to the elements of a PDF dictionary.
        /// </summary>
        [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
        public sealed class DictionaryElements : ElementsBase, IDictionary<string, PdfItem>, ICloneable
        {
            // Note that the value type of IDictionary is not nullable anymore.
            // This is because the value of a dictionary element is always a PdfItem and cannot be null.
            // However, the index still returns PdfItem? because it returns null if the key does not exist.

            internal DictionaryElements(PdfDictionary owningDictionary)
                : base(owningDictionary)
            {
                Debug.Assert(ReferenceEquals(OwningDictionary, owningDictionary));
            }

            object ICloneable.Clone()
            {
                // Shallow clone the Dictionary elements.
                var dictionaryElements = (DictionaryElements)MemberwiseClone();
                var elements = new Dictionary<string, PdfItem>(_elements);
                dictionaryElements._elements = elements;
                dictionaryElements.OwningContainer = null!;

                // Clone all direct objects.
                var names = _elements.Keys; // Take original dictionary…
                foreach (var name in names)
                {
                    var item = elements[name];
                    if (item is PdfObject obj)
                    {
                        Debug.Assert(obj.IsIndirect is false);
                        Debug.Assert(obj.ParentInfo is not null);
                        obj = obj.Clone();
                        Debug.Assert(obj.ParentInfo is null);
                        obj.SetStructureParent(dictionaryElements, name);
                        elements[name] = obj; // … because we change the clone during iteration.
                    }
                    else if (item is PdfReference reference)
                    {
                        reference.AddRef();
                    }
                    else
                    {
                        // Nothing to do for PDF primitives.
                        //_ = typeof(int);
                    }
                }

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
            internal void ChangeOwner(PdfDictionary newOwningDictionary)
            {
                if (OwningDictionary != null!)
                {
                    // Can this assertion really fail?
                    Debug.Assert(ReferenceEquals(this, OwningDictionary._elements)); //TODO: Check why it fails.

                    // Disconnect old owner from this DictionaryElements.
                    OwningDictionary._elements = null;
                }

                // Set new owner.
                OwningContainer = newOwningDictionary;

                // Set owners elements to this.
                newOwningDictionary._elements = this;
            }

            /// <summary>
            /// Determines whether the specified key has a value.
            /// </summary>
            public bool HasValue(string key)
                => _elements.ContainsKey(key);

            // ===== PdfBoolean =====

            /// <summary>
            /// Gets the boolean value that corresponds to the specified key.
            /// If the key does not exist and create is false, the function returns the defaultValue.
            /// If the key does not exist and create is true,
            /// a direct value will be created using the defaultValue.
            /// If the key exists but the value is neither a boolean nor a PDF reference to a boolean,
            /// the function throws an InvalidOperationException.
            /// </summary>
            public bool GetBoolean(string key, bool create = false, bool defaultValue = false)
            {
                TryGetBooleanInternal(key, out var result, create, defaultValue, true);
                return result;
            }

            /// <summary>
            /// Tries to get the boolean value of the PDF object with the specified key.
            /// Returns true on success, false otherwise.
            /// </summary>
            public bool TryGetBoolean(string key, out bool result)
                => TryGetBooleanInternal(key, out result, false, false, false);

            // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            bool TryGetBooleanInternal(string key, out bool result, bool create, bool defaultValue,
                bool throwOnTypeMismatch)
            {
                Name.EnsureName(key);
                //var value = this[key];
                var value = GetValue(key); // #US373
                if (value == null)
                {
                    result = defaultValue;
                    if (create)
                    {
                        SetValueInternal(key, new PdfBoolean(defaultValue));
                        return true;
                    }
                    return false;
                }

                // PdfReference.Dereference(ref value); // #US373
                bool success = true;
                result = value switch
                {
                    PdfBoolean boolean => boolean.Value,
                    PdfBooleanObject boolean => boolean.Value,
                    _ => Fail()
                };
                return success;

                bool Fail()
                {
                    if (throwOnTypeMismatch)
                    {
                        throw new InvalidOperationException(
                            SyMsgs.DictionaryEntryIsOfWrongType(key, typeof(PdfBoolean), value.GetType()).Message);
                    }

                    success = false;
                    return defaultValue;
                }
            }

            /// <summary>
            /// Sets the entry to a direct boolean value.
            /// </summary>
            public void SetBoolean(string key, bool value)
                => SetValueInternal(key, new PdfBoolean(value));

            // ===== PdfInteger =====

            /// <summary>
            /// Gets the integer value that corresponds to the specified key.
            /// If the key does not exist and create is false, the function returns the defaultValue.
            /// If the key does not exist and create is true,
            /// a direct value will be created using the defaultValue.
            /// If the key exists but the value is neither an integer nor a PDF reference to an integer,
            /// the function throws an InvalidOperationException.
            /// </summary>
            public int GetInteger(string key, bool create = false, int defaultValue = 0)
            {
                TryGetIntegerInternal(key, out int result, create, defaultValue, true);
                return result;
            }

            /// <summary>
            /// Tries to get the integer value for the specified key.
            /// Returns true on success, false if value is of wrong type.
            /// </summary>
            public bool TryGetInteger(string key, out int result)
                => TryGetIntegerInternal(key, out result, false, 0, false);

            // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            bool TryGetIntegerInternal(string key, out int result, bool create, int defaultValue,
                bool throwOnTypeMismatch)
            {
                Name.EnsureName(key);
                //var value = this[key];
                var value = GetValue(key, VCF.NoTransform); // #US373
                if (value == null)
                {
                    result = defaultValue;
                    if (create)
                    {
                        SetValueInternal(key, new PdfInteger(defaultValue));
                        return true;
                    }
                    return false;
                }

                // PdfReference.Dereference(ref value); // #US373
                bool success = true;
                result = value switch
                {
                    PdfInteger integer => integer.Value,
                    PdfIntegerObject integer => integer.Value,
                    PdfLongInteger integer => LongInteger(integer.Value),
                    PdfLongIntegerObject integer => LongInteger(integer.Value),
                    _ => Fail()
                };
                return success;

                int LongInteger(long l)
                {
                    if (l is >= Int32.MinValue and <= Int32.MaxValue)
                        return (int)l;
                    return Fail();
                }

                int Fail()
                {
                    if (throwOnTypeMismatch)
                    {
                        throw new InvalidOperationException(
                            SyMsgs.DictionaryEntryIsOfWrongType(key, typeof(PdfInteger), value.GetType()).Message);
                    }

                    success = false;
                    return defaultValue;
                }
            }

            /// <summary>
            /// Sets the entry to a direct integer value.
            /// </summary>
            public void SetInteger(string key, int value)
                => SetValueInternal(key, new PdfInteger(value));

            /// <summary>
            /// Sets the entry to a direct integer value that is used as a flag.
            /// </summary>
            public void SetIntegerFlag(string key, int value)
                => SetValueInternal(key, new PdfInteger(value, true));

            // ===== PdfInteger as unsigned integer =====

            /// <summary>
            /// Gets the unsigned integer value that corresponds to the specified key.
            /// If the key does not exist and create is false, the function returns the defaultValue.
            /// If the key does not exist and create is true,
            /// a direct value will be created using the defaultValue.
            /// If the key exists but the value is neither an integer nor a PDF reference to an integer,
            /// the function throws an InvalidOperationException.
            /// </summary>
            public uint GetUnsignedInteger(string key, bool create = false, uint defaultValue = 0)  // TOD
            {
                TryGetUnsignedIntegerInternal(key, out uint result, create, defaultValue, true);
                return result;
            }

            /// <summary>
            /// Tries to get the unsigned integer value for the specified key.
            /// Returns true on success, false if value is of wrong type.
            /// </summary>
            public bool TryGetUnsignedInteger(string key, out uint result)
#if true
                => TryGetUnsignedIntegerInternal(key, out result, false, 0, false);
#else
            {
                bool success = TryGetLongIntegerInternal(key, out long longResult, false, 0, false);
                if (!success || longResult < Int32.MinValue || longResult > UInt32.MaxValue)
                {
                    result = 0;
                    return false;
                }

                result = unchecked((uint)longResult);
                return true;
            }
#endif
            bool TryGetUnsignedIntegerInternal(string key, out uint result, bool create, uint defaultValue, bool throwOnTypeMismatch)
            {
                // Background: PDF treats flags (like permissions) as a 32-bit unsigned integer.
                // If the most significant bit is set, PDFsharp writes it as a negative integer.
                // But some producer apps write it as a positive value in the range [int.MaxValue+1..uint.MaxValue].
                // PDFsharp parses such a number as PdfLongInteger.
                // This code handles that case.

                Name.EnsureName(key);
                var value = this[key]; // #US373 Also use GetValue here?
                if (value == null)
                {
                    result = defaultValue;
                    if (create)
                    {
                        SetValueInternal(key, new PdfInteger(unchecked((int)defaultValue)));
                        return true;
                    }

                    return false;
                }

                PdfReference.Dereference(ref value);
                bool success = true;
                result = value switch
                {
                    PdfInteger integer => FromInteger(integer.Value),
                    PdfIntegerObject integer => FromInteger(integer.Value),
                    PdfLongInteger longInteger => FromLongInteger(longInteger.Value),
                    PdfLongIntegerObject longInteger => FromLongInteger(longInteger.Value),
                    _ => Fail()
                };
                return success;

                uint FromInteger(int value)
                {
                    return unchecked((uint)value);
                }

                uint FromLongInteger(long value)
                {
                    // if (value is >= 0 and <= UInt32.MaxValue)
                    if (value is >= Int32.MinValue and <= UInt32.MaxValue)
                        return unchecked((uint)value);
                    return Fail();
                }

                uint Fail()
                {
                    if (throwOnTypeMismatch)
                    {
                        throw new InvalidOperationException(
                            SyMsgs.DictionaryEntryIsOfWrongType(key, typeof(PdfInteger), value.GetType()).Message);
                    }

                    success = false;
                    return defaultValue;
                }
            }

            // ===== PdfLongInteger =====

            /// <summary>
            /// Gets the integer or long integer value that corresponds to the specified key.
            /// If the key does not exist and create is false, the function returns the defaultValue.
            /// If the key does not exist and create is true,
            /// a direct value will be created using the defaultValue.
            /// If the key exists but the value is neither an integer or a long integer
            /// nor a PDF reference to an integer or a long integer,
            /// the function throws an InvalidOperationException.
            /// </summary>
            public long GetLongInteger(string key, bool create = false, long defaultValue = 0)
            {
                TryGetLongIntegerInternal(key, out long result, create, defaultValue, true);
                return result;
            }

            /// <summary>
            /// Tries to get the long integer value for the specified key.
            /// Returns true on success, false if value is of wrong type.
            /// </summary>
            public bool TryGetLongInteger(string key, out long result)
                => TryGetLongIntegerInternal(key, out result, false, 0, false);

            // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            bool TryGetLongIntegerInternal(string key, out long result, bool create, long defaultValue,
                bool throwOnTypeMismatch)
            {
                Name.EnsureName(key);
                var value = this[key];
                if (value == null)
                {
                    result = defaultValue;
                    if (create)
                    {
                        SetValueInternal(key, new PdfLongInteger(defaultValue));
                        return true;
                    }

                    return false;
                }

                PdfReference.Dereference(ref value);
                bool success = true;
                result = value switch
                {
                    PdfInteger integer => integer.Value,
                    PdfIntegerObject integer => integer.Value,
                    PdfLongInteger longInteger => longInteger.Value,
                    PdfLongIntegerObject longInteger => longInteger.Value,
                    _ => Fail()
                };
                return success;

                long Fail()
                {
                    if (throwOnTypeMismatch)
                    {
                        throw new InvalidOperationException(
                            SyMsgs.DictionaryEntryIsOfWrongType(key, typeof(PdfLongInteger), value.GetType()).Message);
                    }

                    success = false;
                    return defaultValue;
                }
            }

            /// <summary>
            /// Sets the entry to a direct long integer value.
            /// </summary>
            public void SetLongInteger(string key, long value)
                => SetValueInternal(key, new PdfLongInteger(value));

            // ===== PdfBoolean =====

            /// <summary>
            /// Gets the real or integer value that corresponds to the specified key.
            /// If the key does not exist and create is false, the function returns the defaultValue.
            /// If the key does not exist and create is true,
            /// a direct value will be created using the defaultValue.
            /// If the key exists but the value is neither a real or an integer
            /// nor a PDF reference to a real or an integer, the function throws an InvalidOperationException.
            /// </summary>
            public double GetReal(string key, bool create = false, double defaultValue = 0.0)
            {
                TryGetRealInternal(key, out double result, create, defaultValue, true);
                return result;
            }

            /// <summary>
            /// Tries to get the real value for the specified key.
            /// Returns true on success, false if value is of wrong type.
            /// </summary>
            public bool TryGetReal(string key, out double result)
                => TryGetRealInternal(key, out result, false, 0, false);

            // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            bool TryGetRealInternal(string key, out double result, bool create, double defaultValue,
                bool throwOnTypeMismatch)
            {
                Name.EnsureName(key);
                var value = this[key];
                if (value == null)
                {
                    result = defaultValue;
                    if (create)
                    {
                        SetValueInternal(key, new PdfReal(defaultValue));
                        return true;
                    }

                    return false;
                }

                PdfReference.Dereference(ref value);
                bool success = true;
                result = value switch
                {
                    PdfReal real => real.Value,
                    PdfRealObject realObject => realObject.Value,
                    PdfInteger integer => integer.Value,
                    PdfIntegerObject integer => integer.Value,
                    PdfLongInteger longInteger => longInteger.Value,
                    PdfLongIntegerObject longInteger => longInteger.Value,
                    _ => Fail()
                };
                return success;

                double Fail()
                {
                    if (throwOnTypeMismatch)
                    {
                        throw new InvalidOperationException(
                            SyMsgs.DictionaryEntryIsOfWrongType(key, typeof(PdfReal), value.GetType()).Message);
                    }

                    success = false;
                    return defaultValue;
                }
            }

            /// <summary>
            /// Sets the entry to a direct double value.
            /// </summary>
            public void SetReal(string key, double value)
                => SetValueInternal(key, new PdfReal(value));

            // No SetNullableReal because there is no use case.

            // ===== PdfString =====

            /// <summary>
            /// Gets the string value that corresponds to the specified key.
            /// If the key does not exist and create is false, the function returns the defaultValue.
            /// If the key does not exist and create is true,
            /// a direct value will be created using the defaultValue.
            /// If the key exists but the value is neither a string or name
            /// nor a PDF reference to a string or a name, the function throws an InvalidOperationException.
            /// </summary>
            public string GetString(string key, bool create = false, string defaultValue = "")
            {
                TryGetStringInternal(key, out string result, create, defaultValue, true);
                return result;
            }

            /// <summary>
            /// Tries to get the name value for the specified key.
            /// Returns true on success, false if value is of wrong type.
            /// </summary>
            public bool TryGetString(string key, [MaybeNullWhen(false)] out string result)
                => TryGetStringInternal(key, out result, false, "", false);

            // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            bool TryGetStringInternal(string key, out string result, bool create,
                string defaultValue, bool throwOnTypeMismatch)
            {
                Name.EnsureName(key);
                var value = this[key];
                if (value == null)
                {
                    result = defaultValue;
                    if (create)
                    {
                        SetValueInternal(key, new PdfString(defaultValue));
                        return true;
                    }

                    return false;
                }

                PdfReference.Dereference(ref value);
                bool success = true;
                result = value switch
                {
                    PdfString str => str.Value,
                    PdfStringObject strObject => strObject.Value,
                    PdfName name => name.Value,
                    PdfNameObject nameObject => nameObject.Value,
                    _ => Fail()
                };
                return success;

                string Fail()
                {
                    if (throwOnTypeMismatch)
                    {
                        throw new InvalidOperationException(
                            SyMsgs.DictionaryEntryIsOfWrongType(key, typeof(PdfString), value.GetType()).Message);
                    }

                    success = false;
                    return defaultValue;
                }
            }

            /// <summary>
            /// Sets the entry to a direct string value.
            /// </summary>
            public void SetString(string key, string value)
                => SetValueInternal(key, new PdfString(value));

            // ===== PdfName =====

            /// <summary>
            /// Gets the name value that corresponds to the specified key.
            /// If the key does not exist and create is false, the function returns the defaultValue.
            /// If the key does not exist and create is true,
            /// a direct value will be created using the defaultValue.
            /// If the key exists but the value is neither a name nor a PDF reference to a name,
            /// the function throws an InvalidOperationException.
            /// Note that the default defaultValue is the empty name ("/"), not the empty string.
            /// </summary>
            public string GetName(string key, bool create = false, string defaultValue = "/")
            {
                TryGetNameInternal(key, out string result, create, defaultValue, true);
                return result;
            }

            /// <summary>
            /// Tries to get the name value for the specified key.
            /// Returns true on success, false if value is of wrong type.
            /// </summary>
            public bool TryGetName(string key, out string result)
                => TryGetNameInternal(key, out result, false, "/", false);

            // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            bool TryGetNameInternal(string key, out string result, bool create,
                string defaultValue, bool throwOnTypeMismatch)
            {
                Name.EnsureName(key);
                var value = this[key];
                if (value == null)
                {
                    // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
                    result = defaultValue ?? "/";
                    if (create)
                    {
                        SetValueInternal(key, new PdfName(result)); // Fail in Name if defaultValue is empty.
                        return true;
                    }

                    return false;
                }

                PdfReference.Dereference(ref value);
                bool success = true;
                result = value switch
                {
                    PdfName name => name.Value,
                    PdfNameObject name => name.Value,
                    _ => Fail()
                };
                return success;

                string Fail()
                {
                    if (throwOnTypeMismatch)
                    {
                        throw new InvalidOperationException(
                            SyMsgs.DictionaryEntryIsOfWrongType(key, typeof(PdfName), value.GetType()).Message);
                    }

                    success = false;
                    // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
                    return defaultValue ?? "/";
                }
            }

            /// <summary>
            /// Sets a PDF name at the specified key.
            /// If the value doesn’t start with a slash, it is added automatically,
            /// but starting with PDFsharp version 6.3 a warning is logged.
            /// </summary>
            public void SetName(string key, string value)
            {
                Name.EnsureName(key);

                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                // Ensure that value already starts with a slash.
                if (value.Length == 0 || value[0] != '/')
                {
                    PdfSharpLogHost.Logger.LogWarning("A PDF name must start with a '/'.");
                    value = String.Concat("/", value);
                }

                SetValueInternal(key, new PdfName(value));
            }

            public void SetName<T>(string key, T value) where T : Enum
            {
                SetName(key, Name.FromEnum(value).Value);
            }

            // ===== PdfRectangle =====

            /// <summary>
            /// Gets the rectangle value that corresponds to the specified key.
            /// If the key does not exist and create is false, the function returns the defaultValue.
            /// If the key does not exist and create is true,
            /// a direct value will be created using the defaultValue.
            /// If the key exists but the value is neither a rectangle or an array with four elements
            /// nor a PDF reference to a rectangle or an array with four elements,
            /// the function throws an InvalidOperationException.
            /// </summary>
            public PdfRectangle? GetRectangle(string key, bool create = false, PdfRectangle? defaultValue = null)
            {
                TryGetRectangleInternal(key, out var result, create, defaultValue, true);
                return result;
            }

            public PdfRectangle GetRequiredRectangle(string key, bool create = false, PdfRectangle? defaultValue = null)
            {
                if (create && defaultValue == null)
                    throw new InvalidOperationException("Cannot create a rectangle if the default value is null.");

                if (TryGetRectangleInternal(key, out var result, create, defaultValue, true)
                    || result != null)
                    return result;
                throw ExceptionOnNull(key);
            }

            /// <summary>
            /// Tries to get the rectangle value for the specified key.
            /// Returns true on success, false if value is of wrong type.
            /// </summary>
            public bool TryGetRectangle(string key, [MaybeNullWhen(false)] out PdfRectangle result)
                => TryGetRectangleInternal(key, out result, false, null, false);

            // Re/Sharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            bool TryGetRectangleInternal(string key, [MaybeNullWhen(false)] out PdfRectangle result, bool create,
                PdfRectangle? defaultValue, bool throwOnTypeMismatch)
            {
                Name.EnsureName(key);
                var value = this[key];
                if (value == null)
                {
                    result = defaultValue;
                    if (create)
                    {
                        if (result == null)
                            throw new InvalidOperationException("You cannot create a PdfRectangle if the default value is null.");
                        SetValueInternal(key, result);
                        return true;
                    }
                    return false;
                }

                PdfReference.Dereference(ref value);
                bool success = true;
                result = value switch
                {
                    // PdfRectangle is replaced by PdfArray in SetValueInternal.
                    PdfRectangle rect => throw new InvalidOperationException("Should not come here anymore"),
                    PdfArray array => FromArray(array),
                    _ => Fail()
                };
                return success;

                //PdfRectangle FromRectangle(PdfRectangle rc)  // DELETE
                //{
                //    // PdfRectangle objects should be replaced by PdfArray.
                //    throw new InvalidOperationException("Should not come here anymore");
                //}

                PdfRectangle? FromArray(PdfArray array)
                {
                    if (array.Elements.Count == 4)
                    {
                        var rectangle = new PdfRectangle(array.Elements.GetReal(0), array.Elements.GetReal(1),
                            array.Elements.GetReal(2), array.Elements.GetReal(3));

                        return rectangle;
                    }
                    return Fail("A PdfRectangle expects a PdfArray with 4 real values.");
                }

                PdfRectangle? Fail(string? message = "")
                {
                    if (throwOnTypeMismatch)
                    {
                        message ??= SyMsgs.DictionaryEntryIsOfWrongType(key, typeof(PdfArray), value.GetType()).Message;
                        throw new InvalidOperationException(message);
                    }
                    success = false;
                    return defaultValue; //?? new PdfRectangle(); TODO test
                }
            }

            /// <summary>
            /// Sets the entry to a direct rectangle value, represented by an array with four values.
            /// </summary>
            public void SetRectangle(string key, PdfRectangle rect)
            {
                // Setting PdfRectangle is handled as a special case in SetValueInternal.
                // This is because dict.SetValue(key, rect) also must be handled correctly.
                SetValueInternal(key, rect);
            }

            // ===== XMatrix =====

            // IMPROVE: Should be extension method to separate XGraphics from PDF Core.
            /// Converts the specified value to XMatrix.
            /// If the value does not exist, the function returns an identity matrix.
            /// If the value is not convertible, the function throws an InvalidCastException.
            public XMatrix GetMatrix(string key, bool create = false, XMatrix? defaultMatrix = null)
            {
                //var item = this[key];
                var item = GetValue(key); // #US373
                if (item == null)
                {
                    if (create)
                    {
                        PdfArray array;
                        if (defaultMatrix is null)
                        {
                            array = new(new PdfInteger(1), new PdfInteger(0),
                                        new PdfInteger(0), new PdfInteger(1),
                                        new PdfInteger(0), new PdfInteger(0));
                        }
                        else
                        {
                            array = new(new PdfReal(defaultMatrix.Value.M11), new PdfReal(defaultMatrix.Value.M12),
                                        new PdfReal(defaultMatrix.Value.M21), new PdfReal(defaultMatrix.Value.M22),
                                        new PdfReal(defaultMatrix.Value.OffsetX), new PdfReal(defaultMatrix.Value.OffsetY));
                        }
                        SetValueInternal(key, array);
                    }
                    return defaultMatrix ?? XMatrix.Identity;
                }

                //PdfReference.Dereference(ref item);
                return item switch
                {
                    PdfArray { Elements.Count: 6 } array =>
                        new(array.Elements.GetReal(0), array.Elements.GetReal(1),
                            array.Elements.GetReal(2), array.Elements.GetReal(3),
                            array.Elements.GetReal(4), array.Elements.GetReal(5)),
                    PdfLiteral => throw new NotImplementedException("Parsing matrix from literal."),
                    _ => throw new InvalidCastException("Element is not an array with 6 values.")
                };
            }

            //public bool TryGetMatrix(string key, out XMatrix result)
            //{
            //    //TODO?
            //    throw new NotImplementedException();
            //}

            //public bool TryGetMatrixInternal(string key, out XMatrix result)
            //{
            //    //TODO?
            //    throw new NotImplementedException();
            //}

            /// <summary>
            /// Sets the entry to a direct matrix value, represented by an array with six values.
            /// </summary>
            public void SetMatrix(string key, XMatrix matrix)
            {
                SetValueInternal(key, new PdfArray(
                    new PdfReal(matrix.M11), new PdfReal(matrix.M12),
                    new PdfReal(matrix.M21), new PdfReal(matrix.M22),
                    new PdfReal(matrix.OffsetX), new PdfReal(matrix.OffsetY)));
            }

            // ===== PdfDate =====

            /// <summary>
            /// Converts the specified value to DateTimeOffset.
            /// If the value does not exist, the function returns the specified default value.
            /// If the value is not convertible, the function throws an InvalidOperationException.
            /// </summary>
            public DateTimeOffset? GetDateTime(string key, DateTimeOffset? defaultValue = null, bool create = false)
            {
                TryGetDateTimeInternal(key, out DateTimeOffset? result, create, defaultValue, true);
                return result;
            }

            public DateTimeOffset GetRequiredDateTime(string key, DateTimeOffset? defaultValue = null, bool create = false)
            {
                TryGetDateTimeInternal(key, out DateTimeOffset? result, create, defaultValue, true);
                if (result == null)
                    throw ExceptionOnNull(key);
                return result.Value;
            }

            /// <summary>
            /// Tries to get the date value for the specified key.
            /// Returns true on success, false if value is of wrong type.
            /// </summary>
            public bool TryGetDateTime(string key, out DateTimeOffset? result)
            {
                return TryGetDateTimeInternal(key, out result, false, null, false);
            }

            // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            bool TryGetDateTimeInternal(string key, [MaybeNullWhen(false)] out DateTimeOffset? result, bool create,
                DateTimeOffset? defaultValue, bool throwOnTypeMismatch)
            {
                Name.EnsureName(key);

                result = null;
                var value = this[key];
                if (value == null)
                {
                    result = defaultValue;
                    if (create)
                    {
                        if (result == null)
                        {
                            // Cannot create value without default value.
                            return false;
                        }
                        SetValueInternal(key, new PdfDate(result.Value));
                        return true;
                    }
                    return false;
                }

                PdfReference.Dereference(ref value);
                bool success = true;
                result = value switch
                {
                    PdfDate date => date.Value,
                    PdfString date => DateFromString(date.Value),
                    PdfStringObject date => DateFromString(date.Value),
                    _ => Fail()
                };
                return success;

                DateTimeOffset? DateFromString(string pdfDate)
                {
                    if (Parser.TryParseDate(pdfDate, out var dateTime))
                        return dateTime;
                    // TODO Throw exception here, indicating malformed date string?
                    return Fail();
                }

                DateTimeOffset? Fail()
                {
                    if (throwOnTypeMismatch)
                    {
                        throw new InvalidOperationException(
                            SyMsgs.DictionaryEntryIsOfWrongType(key, typeof(PdfDate), value.GetType()).Message);
                    }

                    success = false;
                    return defaultValue;
                }
            }

            /// <summary>
            /// Sets the entry to a direct datetime value.
            /// </summary>
            [Obsolete("Use DateTimeOffset as parameter.")]
            public void SetDateTime(string key, DateTime value)
            {
                Name.EnsureName(key);

                SetValueInternal(key, new PdfDate(value));
            }

            /// <summary>
            /// Sets the entry to a direct datetime value.
            /// </summary>
            public void SetDateTime(string key, DateTimeOffset value)
            {
                Name.EnsureName(key);

                SetValueInternal(key, new PdfDate(value));
            }

            // ===== GetValue =====

            /// <summary>
            /// Gets the value for the specified key.
            /// If the value does not exist, it is optionally created.
            /// If the value is a PDF reference the referenced object is returned, except the itemType
            /// is set to PdfReference.
            /// </summary>
            public PdfItem? GetValue(string key, VCF options = VCF.None,
                [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                            DynamicallyAccessedMemberTypes.NonPublicConstructors)]
                Type? valueType = null)
            {
#if PDFSHARP_DEBUG_
                if (ShouldBreak5)
                    Debugger.Break();
#endif
                Name.EnsureName(key);
                if (valueType != null)
                    EnsureValueType<PdfItem>(valueType);

                return GetValueInternal(key, options, valueType, valueType != null);
            }

            /// <summary>
            /// Gets the result as GetValue if it is not null.
            /// Otherwise, an exception is thrown.
            /// </summary>
            public PdfItem GetRequiredValue(string key, VCF options = VCF.None,
                [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                            DynamicallyAccessedMemberTypes.NonPublicConstructors)]
                Type? valueType = null)
            {
                return GetValue(key, options, valueType)
                       ?? throw ExceptionOnNull(key);
            }

            // For interface IDictionary<string, PdfItem>.TryGetValue(string, out PdfItem)
            public bool TryGetValue(string key, [MaybeNullWhen(false)] out PdfItem value)
                => TryGetValue(key, out value, null);

            public bool TryGetValue(string key, [MaybeNullWhen(false)] out PdfItem value,
                [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                            DynamicallyAccessedMemberTypes.NonPublicConstructors)]
                Type? valueType)
            {
                Name.EnsureName(key);
                if (valueType != null)
                    EnsureValueType<PdfItem>(valueType);

                value = null;
                var result = GetValueInternal(key, VCF.NoTransform, valueType, false);
                if (result != null)
                {
                    value = result;
                    return true;
                }

                return false;
            }

            public T? GetValue<
                    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                                DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>
                (string key, VCF options = VCF.None) where T : PdfItem
            {
                Name.EnsureName(key);

                var value = GetValueInternal(key, options, typeof(T), true) as T;
                return value;
            }

            public T GetRequiredValue<
                    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                                DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>
                (string key, VCF options = VCF.None) where T : PdfItem
            {
                return GetValue<T>(key, options)
                       ?? throw ExceptionOnNull(key);
            }

            public bool TryGetValue<T>(string key, [MaybeNullWhen(false)] out T value) where T : PdfItem
            {
                Name.EnsureName(key);

                value = null;
                if (GetValueInternal(key, VCF.NoTransform, null, false) is T valueOfT)
                {
                    value = valueOfT;
                    return true;
                }

                return false;
            }

            /// <summary>
            /// Sets the entry with the specified value. DON’T USE THIS FUNCTION - IT MAY BE REMOVED.  // PDFsharp/NT
            /// </summary>
            public void SetValue(string key, PdfItem value)
            {
                Name.EnsureName(key);

                SetValueInternal(key, value); // TODO: Check for indirect objects.
            }

            // ===== GetObject =====

            /// <summary>
            /// Gets the PdfObject with the specified key, or null if no such object exists.
            /// If the key refers to a reference, the referenced PdfObject is returned.
            /// * 
            /// </summary>
            public PdfObject? GetObject(string key, VCF options = VCF.None,
                [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                            DynamicallyAccessedMemberTypes.NonPublicConstructors)]
                Type? valueType = null)
            {
                Name.EnsureName(key);
                if (valueType != null)
                    EnsureValueType<PdfObject>(valueType);
                //else
                //    valueType = typeof(PdfObject);

                var value = GetValueInternal(key, options, valueType, false);
                return value as PdfObject;
            }

            public PdfObject GetRequiredObject(string key, VCF options = VCF.None,
                [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                            DynamicallyAccessedMemberTypes.NonPublicConstructors)]
                Type? valueType = null)
            {
                return GetObject(key, options, valueType)
                       ?? throw ExceptionOnNull(key);
            }

            public T? GetObject<
                    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                                DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>
                (string key, VCF options = VCF.None) where T : PdfObject
            {
                Name.EnsureName(key);

                // TODO: Never throw?
                bool throwOnTypeMismatch = typeof(PdfContainer).IsAssignableFrom(typeof(T));
                return GetValueInternal(key, options, typeof(T), throwOnTypeMismatch) as T;
            }

            public T GetRequiredObject<
                    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                                DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>
                (string key, VCF options = VCF.None) where T : PdfObject
            {
                return GetObject<T>(key, options)
                       ?? throw ExceptionOnNull(key);
            }

            /// <summary>
            /// Sets the entry to the specified object.
            /// </summary>
            public void SetObject(string key, PdfObject obj)  // Used in PDFsharp
            {
                Name.EnsureName(key);

                SetValueInternal(key, obj);
            }

            // ===== GetArray =====

            /// <summary>
            /// Gets the PdfArray with the specified key, or null, if no such object exists and should not be created.
            /// If the key refers to a reference, the referenced PdfArray is returned.
            /// If the array exists, but cannot be transformed to type T, an exception is thrown.
            /// </summary>
            /// <param name="key">The key.</param>
            /// <param name="options">The creation options.</param>
            /// <param name="valueType"></param>
            public PdfArray? GetArray(string key, VCF options = VCF.None,
                [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                            DynamicallyAccessedMemberTypes.NonPublicConstructors)]
                Type? valueType = null)
            {
                Name.EnsureName(key);
                if (valueType != null)
                    EnsureValueType<PdfArray>(valueType);

                return GetValueInternal(key, options, valueType, valueType != null) as PdfArray;
            }

            public PdfArray GetRequiredArray(string key, VCF options = VCF.None,
                [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                            DynamicallyAccessedMemberTypes.NonPublicConstructors)]
                Type? valueType = null)

            {
                return GetArray(key, options, valueType)
                       ?? throw ExceptionOnNull(key);
            }

            public bool TryGetArray(string key, [MaybeNullWhen(false)] out PdfArray array)
                => TryGetArray(key, out array, null);


            public bool TryGetArray(string key, [MaybeNullWhen(false)] out PdfArray array,
                [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                            DynamicallyAccessedMemberTypes.NonPublicConstructors)]
                Type? valueType)
            {
                Name.EnsureName(key);
                if (valueType != null)
                    EnsureValueType<PdfArray>(valueType);

                array = null;
                if (GetValueInternal(key, VCF.NoTransform, valueType, false) is PdfArray value)
                {
                    array = value;
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Gets the PdfArray with the specified key, or null, if no such object exists and should not be created.
            /// If the key refers to a reference, the referenced PdfArray is returned.
            /// If the array exists, but cannot be transformed to type T, an exception is thrown.
            /// </summary>
            /// <param name="key">The key.</param>
            /// <param name="options">The creation options.</param>
            public T? GetArray<
                    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                                DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>
                (string key, VCF options = VCF.None) where T : PdfArray
            {
                Name.EnsureName(key);

                var array = GetValueInternal(key, options, typeof(T), true) as T;
                return array;
            }

            /// <summary>
            /// Gets the PdfArray with the specified key, or throws an exception, if no such object exists and
            /// should not be created.
            /// If the key refers to a reference, the referenced PdfArray is returned.
            /// If the array exists, but cannot be transformed to type T, an exception is thrown.
            /// </summary>
            /// <param name="key">The key.</param>
            /// <param name="options">The creation options.</param>
            public T GetRequiredArray<
                    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                                DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>
                (string key, VCF options = VCF.None) where T : PdfArray
            {
                Name.EnsureName(key);

                return GetArray<T>(key, options)
                       ?? throw ExceptionOnNull(key);
            }

            public bool TryGetArray<
                    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                                DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>
                (string key, [MaybeNullWhen(false)] out T array) where T : PdfArray
            {
                Name.EnsureName(key);

                array = null;
                if (GetValueInternal(key, VCF.NoTransform, typeof(T), false) is T valueOfT)
                {
                    array = valueOfT;
                    return true;
                }
                return false;
            }

            // ===== GetDictionary =====

            /// <summary>
            /// Gets the PdfDictionary with the specified key, or null, if no such object exists
            /// and should not be created.
            /// If the key refers to a reference, the referenced PdfDictionary is returned.
            /// </summary>
            public PdfDictionary? GetDictionary(string key, VCF options = VCF.None,
                [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                            DynamicallyAccessedMemberTypes.NonPublicConstructors)]
                Type? valueType = null)
            {
                Name.EnsureName(key);
                if (valueType != null)
                    EnsureValueType<PdfDictionary>(valueType);

                return GetValueInternal(key, options, valueType, valueType != null) as PdfDictionary;
            }

            public PdfDictionary GetRequiredDictionary(string key, VCF options = VCF.None,
                [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                            DynamicallyAccessedMemberTypes.NonPublicConstructors)]
                Type? valueType = null)
            {
                return GetDictionary(key, options, valueType)
                       ?? throw ExceptionOnNull(key);
            }

            public bool TryGetDictionary(string key, [MaybeNullWhen(false)] out PdfDictionary dict)
                => TryGetDictionary(key, out dict, null);

            public bool TryGetDictionary(string key, [MaybeNullWhen(false)] out PdfDictionary dict,
                [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                            DynamicallyAccessedMemberTypes.NonPublicConstructors)]
                Type? valueType)
            {
                Name.EnsureName(key);
                if (valueType != null)
                    EnsureValueType<PdfDictionary>(valueType);

                dict = null;
                if (GetValueInternal(key, VCF.NoTransform, valueType, false) is PdfDictionary value)
                {
                    dict = value;
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Gets the PdfDictionary with the specified key, or null, if no such object exists and
            /// should not be created.
            /// If the key refers to a reference, the referenced PdfDictionary is returned.
            /// If the dictionary exists, but cannot be transformed to type T, an exception is thrown.
            /// </summary>
            /// <typeparam name="T">The type of the PDF dictionary.</typeparam>
            /// <param name="key">The key.</param>
            /// <param name="options">The creation options.</param>
            public T? GetDictionary<
                    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                                DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>
                (string key, VCF options = VCF.None) where T : PdfDictionary
            {
                Name.EnsureName(key);

                var value = GetValueInternal(key, options, typeof(T), true);
                return value as T;
            }

            /// <summary>
            /// Gets the PdfDictionary with the specified key, or throws an exception, if no such object exists and
            /// should not be created.
            /// If the key refers to a reference, the referenced PdfDictionary is returned.
            /// If the dictionary exists, but cannot be transformed to type T, an exception is thrown.
            /// </summary>
            /// <typeparam name="T">The type of the PDF dictionary.</typeparam>
            /// <param name="key">The key.</param>
            /// <param name="options">The creation options.</param>
            public T GetRequiredDictionary<
                    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                                DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>
                (string key, VCF options = VCF.None) where T : PdfDictionary
            {
                return GetDictionary<T>(key, options)
                       ?? throw ExceptionOnNull(key);
            }

            public bool TryGetDictionary<
                    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                                DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>
                (string key, [MaybeNullWhen(false)] out T dict) where T : PdfDictionary
            {
                Name.EnsureName(key);

                dict = null;
                if (GetValueInternal(key, VCF.NoTransform, typeof(T), false) is T valueOfT)
                {
                    dict = valueOfT;
                    return true;
                }
                return false;
            }

            // ===== PdfReference =====

            /// <summary>
            /// Gets the PdfReference with the specified key, or null if no such object exists.
            /// </summary>
            public PdfReference? GetReference(string key)
            {
                Name.EnsureName(key);

                // Always get the raw value.
                if (_elements.TryGetValue(key, out var value))
                    return value as PdfReference;
                return null;
            }

            /// <summary>
            /// Gets the PdfReference with the specified key, or throws an exception,
            /// if no such object exists.
            /// </summary>
            public PdfReference GetRequiredReference(string key)
            {
                Name.EnsureName(key);

                return GetReference(key)
                       ?? throw new InvalidOperationException(SyMsgs.IndirectReferenceMustNotBeNull.Message);
            }

            /// <summary>
            /// Sets the entry to an indirect reference.
            /// The specified object must be an indirect object,
            /// otherwise an exception is raised.
            /// </summary>
            [Obsolete("Use SetObject with an indirect object.")]
            public void SetReference(string key, PdfObject obj)
            {
                Name.EnsureName(key);

                if (obj.Reference == null)
                    throw new ArgumentException("PdfObject is not an indirect object.", nameof(obj));// #SyMsg
                SetValueInternal(key, obj.Reference);
            }

            /// <summary>
            /// Sets the entry to an indirect reference.
            /// </summary>
            public void SetReference(string key, PdfReference iref)
            {
                Name.EnsureName(key);

                if (iref is null)
                    throw new ArgumentNullException(nameof(iref));
                SetValueInternal(key, iref);
            }

            /// <summary>
            /// Access a key that may contain an array or a single item for working with its value(s).
            /// </summary>
            public ArrayOrSingleItemHelper ArrayOrSingleItem => new(this); // TODO_OLD PDFsharp6: Naming.

            // ===== Enum =====

            public int GetEnumFromName(string key, object defaultValue, bool create = false) // TODO: review, object => Enum?
            {
                Name.EnsureName(key);

                var item = this[key];
                if (item == null)
                {
                    if (create)
                        SetValueInternal(key, new PdfName(defaultValue.ToString()!));

                    // ReSharper disable once PossibleInvalidCastException because Enum objects can always be cast to int.
                    return (int)defaultValue;
                }

                PdfReference.Dereference(ref item); // Could be a reference. #US373
                return (int)Enum.Parse(defaultValue.GetType(), item.ToString()?[1..] ?? "", false);
            }

            public void SetEnumAsName(string key, object value)
            {
                Name.EnsureName(key);

                if (value is not Enum)
                    throw new ArgumentException("value is not an Enum.", nameof(value));
                SetValueInternal(key, new PdfName("/" + value));
            }

            public bool TryGetEnum<TEnum>(string key, out TEnum result) where TEnum : struct, Enum
            {
                result = default;

                if (TryGetInteger(key, out var value))
                {
                    result = (TEnum)(value as object);
                    return true;
                }

                if (TryGetName(key, out var name))
                {
                    // Name must always have a leading solidus.
                    Debug.Assert(!String.IsNullOrEmpty(name));

                    result = Enum.Parse<TEnum>(name?[1..] ?? "", false);

                    return true;
                }
                return false;
            }

            public TEnum? GetEnum<TEnum>(string key) where TEnum : struct, Enum
            {
                if (TryGetEnum<TEnum>(key, out var result))
                {
                    return result;
                }
                return null;
            }


            // ===== Internal =====

            /// <summary>  // TODO more docs
            /// Gets the value for the specified key.
            /// If the value does not exist, it is optionally created.
            /// If the value exists, it can be optionally transformed to a derived type.
            /// If the value is a PdfReference, the indirect object is returned.
            /// </summary>
            PdfItem? GetValueInternal(string key, VCF options,
                [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                            DynamicallyAccessedMemberTypes.NonPublicConstructors)]
                Type? valueType, bool throwOnTypeMismatch)
            {
#if DEBUG_
                // Are we reading a PDF document?
                if ((OwningDictionary.Document?.IrefTable ?? null) is { IsUnderConstruction: true })
                {
                    //Debugger.Break();
                    _ = typeof(int);
                }
#endif
#if DEBUG_
                if (typeof(PdfNameDictionary) == valueType)
                    _ = typeof(int);
#endif
                // Name.EnsureName(key) must be done by caller.

                bool valueIsAlreadyOfSuitableType = false;
                if (!_elements.TryGetValue(key, out var value))
                {
                    // Case: Dictionary entry with specified key does not exist.

                    // Create the value if requested.
                    if (options is VCF.Create or VCF.CreateIndirect)
                    {
                        var type = valueType ?? GetValueType(key);
                        if (type != null)
                        {
                            // Case: value is null, but we have a type to create it.

                            //Debug.Assert(typeof(PdfObject).IsAssignableFrom(type), "Type not allowed.");
                            //PdfObject? obj;

                            if (typeof(PdfContainer).IsAssignableFrom(type))
                            {
                                // Case: Create PDF array or dictionary.
                                var obj = CreateContainer(type, null, options is VCF.CreateIndirect);

                                if (options is VCF.CreateIndirect)
                                {
                                    // There are dictionaries (e.g. PdfNameDictionary) that are always created as indirect
                                    // in their constructor.
                                    if (obj.IsIndirect is false) // TODO Check this everywhere
                                        OwningDictionary.Owner.IrefTable.Add(obj);
                                    Debug.Assert(obj.Reference is not null);
                                }
                                else // if (options is VCF.Create) is always true here.
                                {
                                    Debug.Assert(obj.Reference is null);
                                }
                                value = obj;
                            }
                            // Note that the cases below are not real world scenarios, but you cannot force a developer not to write code like this:
                            //   var item = new PdfDictionary().Elements.GetValue<PdfString>("/SomeKey", VCF.Create)
                            // This creates an empty PdfString which is senseless, but valid.
                            else if (typeof(PdfPrimitive).IsAssignableFrom(type))
                            {
                                // Case: Create a PDF primitive. This makes no sense because it is immutable, and we have no default value.

                                // Primitives cannot be indirect objects.
                                if (options is VCF.CreateIndirect)
                                    throw new InvalidOperationException($"Cannot create an indirect object of type '{type.FullName}'.");

                                value = (PdfItem)(Activator.CreateInstance(type)
                                        ?? throw new InvalidOperationException($"Cannot create instance of type '{type.FullName}'."));

                                value.SetTransformed();
                            }
                            else if (typeof(PdfPrimitiveObject).IsAssignableFrom(type))
                            {
                                // Case: Create a PDF primitive object. This makes even less sense and is not implemented.

                                // Primitive objects cannot be direct.
                                if (options is VCF.Create)
                                    throw new InvalidOperationException($"Cannot create a direct object of type '{type.FullName}'.");

                                throw new InvalidOperationException(
                                    $"Cannot create instance of type '{type.FullName}' " +
                                    "because creating of indirect primitive PDF objects makes less sense and is not implemented.");
                            }
                            else if (typeof(PdfReference) == type)
                            {
                                // Case: itemType is PdfReference in combination with Create/CreateIndirect.

                                throw new InvalidOperationException($"Cannot combine type '{type.FullName}' with creation flags.");
                            }
                            else
                            {
                                // Case: We only come here for special types like PdfDocument which are derived from PdfObject but are not containers.
                                // Just throw.

                                throw new InvalidOperationException($"Cannot create instance of type '{type.FullName}' " +
                                                                    "because this is not intended for this PDF type.");
                            }

                            Debug.Assert(value.IsTransformed);

                            SetValueInternal(key, value);

                            Debug.Assert(valueType == null || valueType.IsInstanceOfType(value));
                            valueIsAlreadyOfSuitableType = true;
                            //return value;
                        }
                        else
                        {
                            // Case: We have no type and cannot create an instance.
                            // But check if this is caused by to aggressive trimming the assembly.

                            // /Info is the first type created by object type transformation.
                            // In case somebody trimmed PDFsharp to hard, we come here und try to give a
                            // meaningful explanation.
                            if (key == "/Info")
                            {
                                // We come here if PDFsharp was fully trimmed and meta-data could not be found by reflection.
                                throw new InvalidOperationException(
                                    "PDFsharp relies on reflection and does not work when a fully-trimmed self-contained file is used.\r\n" +
                                    "See https://docs.pdfsharp.net/ for further information.");
                            }

                            // Maybe missing or wrong meta configuration.
                            throw new InvalidOperationException(
                                $"Cannot create value for key '{key}' because no type was found.");
                        }
                    }
                }
                else
                {
                    // Case: The value exists and can be returned.
                    // But /*for imported documents*/ check for object type transformation.

                    if (value is PdfReference iref)
                    {
#if DEBUG_
                        var irefTable = OwningDictionary.Document?.IrefTable ?? null;
                        if (irefTable is { IsUnderConstruction: true })
                        {
                            //Debug.Assert(false, "Should not happen anymore.");
                            _ = typeof(int);
                        }
#endif
                        // Case: value is an indirect object. It can only be a container.

#if true_  // TODO Create US for this case and DELETE the code.
                        // Check a very particular case first that can happen during
                        // the timespan when the IrefTable is not completely read.
                        var irefTable = OwningDictionary.Document?.IrefTable ?? null;
                        if (irefTable is { IsUnderConstruction: true })
                        {
                            // Case: During the import of a PDF document GetValue is called.
                            // This happens only during encryption.
                            // IMPROVE: Prevent from coming here. Check if we need temp irefs anymore.
                            var newIref = irefTable[iref.Value.ObjectID];
                            if (newIref == null)
                            {
                                // Case: Cannot happen. We can have irefs with no value, but not
                                // indirect objects with no reference.
                                _ = typeof(int);
                            }
                            else if (ReferenceEquals(iref, newIref) is false)
                            {
                                // Case: We are reading a PDF document that hast more than one XRef table or xref streams.
                                // A top level object can contain an indirect reference to an object with an ID that does not yet exist.
                                // TODO more checks...
                                if (ReferenceEquals(iref.Value, newIref.Value))
                                {
                                    iref = newIref;
                                    iref.Value.Reference = iref;
                                    UpdateValueInternal(key, iref);
                                }
                            }
                            else
                            {
                                _ = typeof(int);
                            }
                        }
#if DEBUG
                        else if (irefTable != null)
                        {
                            // TO/DO: Check if already required.
                            var altIref = irefTable[iref.Value.ObjectID];
                            if (altIref == null)
                            {
                                // Should not come here.
                                _ = typeof(int);
                            }
                            else if (ReferenceEquals(iref, altIref) is false)
                            {
                                // Should not come here.
                                iref = altIref;
                                UpdateValueInternal(key, iref);
                            }
                        }
#endif
#endif
                        // Is PdfReference explicitly requested?
                        if (valueType == typeof(PdfReference))
                            return value;

                        // In all other cases use the referenced value.
                        value = iref.Value;
                        if (value == null)
                        {
                            // If we come here PDF file is corrupted.
                            // TODO: What if we have a reference to a non-existing object?
                            // A reference without a value can only happen during loading an existing PDF file.
                            throw new InvalidOperationException("Indirect reference without value.");
                        }

                        // The nesting can be simplified, but keep it as it is for better understandability.
                        if (options != VCF.NoTransform)
                        {
                            // Get type from parameter or metadata.
                            var type = valueType ?? GetValueType(key);

                            // Try transformation only once. If it fails, don’t try again.
                            // Should we try transformation?

                            // TODO: What if valueType more derived than metadata type? Transform again?
                            if (value.ShouldTryTransformation
                                || type != null
                                || value.GetType().IsAssignableFrom(type))
                            {
                                // Do we have a type anyway?
                                if (type != null)
                                {
                                    // Case: We have a type and an existing indirect object.

                                    if (type.IsInstanceOfType(value))
                                    {
                                        // Case: The value is already of the appropriate type.

                                        // Set to transformed, but only if the requested type is not one of the base types.
                                        if (type != typeof(PdfDictionary) && type != typeof(PdfArray))
                                            value.SetTransformed();
                                    }
                                    else if (value is PdfContainer cont)
                                    {
                                        // Case: Transform array or dictionary.

                                        // TODO: Test to transform twice.

                                        //Debug.Assert(cont.IsTransformed is false or true);
                                        Debug.Assert(cont.ParentInfo is null);
                                        //if (OwningDictionary.Document.IrefTable.IsUnderConstruction)
                                        value = CreateContainer(type, cont, cont.IsIndirect);
                                        Debug.Assert(cont.IsDead);
                                        Debug.Assert(value.IsTransformed);
                                        Debug.Assert(cont.ParentInfo is null);

                                    }
                                    else if (value is PdfPrimitiveObject)
                                    {
                                        throw new InvalidOperationException(
                                            $"Primitive indirect object of type '{value.GetType().FullName}' cannot be transformed to type '{type.FullName}'.");
                                    }
                                    else
                                    {
                                        // Exotic case: Reference to e.g. PdfDocument.
                                        // Just throw.
                                        throw new InvalidOperationException(
                                            $"Indirect object of type '{value.GetType().FullName}' cannot be transformed to type '{type.FullName}'.");
                                    }
                                    // TODO: Should fail because it is a Reference - test this.
                                    SetValueInternal(key, value);
                                }
                                else
                                {
                                    // TODO: Should be possible to transform later.
                                    value.SetTransformationTried();
                                }
                            }
                        }
                        Debug.Assert(value != null);
                        //return value;
                    }
                    else
                    {
                        // Case: value is a direct object.

                        // Transformation is possible after PDF import or the user
                        // creates a less derived container (e.g. in unit tests).
                        if (options != VCF.NoTransform)
                        {
                            // Try transformation only once. If it fails, don’t try again.
                            // Should we try transformation?
                            if (value.ShouldTryTransformation)
                            {
                                // Get type from parameter or metadata.
                                var type = valueType ?? GetValueType(key);

                                // Do we have a type anyway?
                                if (type != null)
                                {
                                    // Case: We have a type and an existing primitive or direct object.

                                    // Handle special case PdfRectangle first.
                                    if (type == typeof(PdfRectangle)) // TODO in Array and in non-else case above.
                                    {
                                        if (value is PdfArray { Elements.Count: 4 } array)
                                        {
                                            value = new PdfRectangle(array);
                                        }
                                        else
                                        {
                                            if (throwOnTypeMismatch)
                                                throw new InvalidOperationException($"Cannot create PdfRectangle from type '{value.GetType().FullName}'.");
                                            value = null;
                                        }
                                    }
                                    else if (type.IsInstanceOfType(value))
                                    {
                                        // Case: The value is already of the appropriate type.

                                        // Set to transformed, but only if the requested type is not one of the base types.
                                        if (type != typeof(PdfDictionary) && type != typeof(PdfArray))
                                            value.SetTransformed();
                                    }
                                    else if (value is PdfContainer cont)
                                    {
                                        // Case: Transform direct array or dictionary.

                                        Debug.Assert(cont.IsIndirect is false);
                                        Debug.Assert(cont.IsTransformed is false);
                                        Debug.Assert(cont.ParentInfo is not null);

                                        value = CreateContainer(type, cont, false /*cont.IsIndirect*/);
                                        SetValueInternal(key, value);

                                        Debug.Assert(cont.IsDead);
                                        Debug.Assert(value.IsTransformed); // TODO: Can transform twice?
                                        Debug.Assert(cont.ParentInfo is null);
                                        Debug.Assert(((PdfContainer)value).ParentInfo is not null);
                                    }
                                    else if (value is PdfNull nullObject)
                                    {
                                        // TODO Add in Array
                                        _ = typeof(int);
                                    }
                                    // #US373 begin
                                    else if (value is PdfInteger integer &&
                                             type == typeof(PdfReal))
                                    {
                                        value = new PdfReal(integer.Value);
                                        SetValueInternal(key, value);
                                    }
                                    // #US373 end
                                    else
                                    {
                                        // Exotic case: value is not a PDF array or dictionary, but not from the requested type.
                                        // Just throw.
                                        throw new InvalidOperationException(
                                            $"Value of type '{value.GetType().FullName}' cannot be transformed to type '{type.FullName}'.");
                                    }
                                }
                                else
                                {
                                    // TODO: Should be possible to transform later.
                                    value.SetTransformationTried();
                                }
                            }
                        }
                    }
                }

                // Ensure not a type mismatch.
                if (!valueIsAlreadyOfSuitableType && value != null && valueType != null)
                {
                    //if (!valueType.IsAssignableFrom(value.GetType()))
                    if (!valueType.IsInstanceOfType(value))
                    {
                        if (throwOnTypeMismatch)
                            throw ExceptionOnTypeMismatch(key, valueType, value.GetType());
                        return null;
                    }
                }
                return value;
            }

            /// <summary>
            /// Implementation of SetValue.
            /// Handles setting same value, setting indirect object,
            /// and releasing old value.
            /// Keep in sync with PdfArray.
            /// </summary>
            void SetValueInternal(string key, PdfItem value, bool mustNotExist = false)
            {
#if DEBUG_
                // Are we reading a PDF document?
                if ((OwningDictionary.Document?.IrefTable ?? null) is { IsUnderConstruction: true })
                {
                    //Debugger.Break();
                    _ = typeof(int);
                }
#endif
                // Already asserted from caller.
                Debug.Assert(value is not null);

                if (value.IsDead)
                    throw new InvalidOperationException("TODO: Is Dead.");  // /messages/ObjectIsDead.html

                // Special treatment for PdfRectangle.
                // Convert to direct PdfArray.
                if (value is PdfRectangle rect)
                    value = rect.GetAsArrayOfValues();
                else
                    PdfReference.ToReference(ref value);

                // Is value already set?
                if (_elements.TryGetValue(key, out var oldItem))
                {
                    if (mustNotExist)
                        throw new InvalidOperationException($"Key '{key}' already exists.");
                }

                // Check self-assignment because of SetDead.
                if (ReferenceEquals(oldItem, value))
                {
                    LogWarning();
                    return;
                }

                if (value is PdfObject obj)
                {
                    if (obj.Reference != null)
                    {
                        Debug.Assert(false, "Should not come here anymore.");

                        // Case: Indirect object

                        Debug.Assert(obj.ParentInfo is null, "An indirect object must not have a structure parent.");
                        value = obj.Reference;
                        if (ReferenceEquals(oldItem, value))
                        {
                            LogWarning();
                            return;
                        }
                    }
                    else if (obj is PdfPrimitiveObject)
                    {
                        // Case: Direct non-container object

                        FailForDirectPrimitiveObject(obj);
                    }
                    else
                    {
                        // Case: Direct container object
                        Debug.Assert(obj is PdfContainer);

                        if (obj.ParentInfo != null)
                            throw new InvalidOperationException("A direct object can only be added once to a container."); // TODO Error code

                        obj.SetStructureParent(this, key);
                    }
                }
                else
                {
                    // Case: PdfPrimitive or PdfReference
                    Debug.Assert(value is PdfReference or PdfPrimitive);
                }

                _elements[key] = value;

                if (oldItem != null)
                    ReleaseItem(oldItem);

                return;

                void LogWarning()
                {
                    //PdfSharpLogHost.Logger.LogWarning("Setting same value in dictionary.");
                }
            }

            /// <summary>
            /// Returns the type of the object to be created as value of the specified key.
            /// </summary>
            [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                                DynamicallyAccessedMemberTypes.NonPublicConstructors)]
            Type? GetValueType(string key)
            {
                Type? type = null;
                var meta = OwningDictionary.Meta;
                if (meta != null!)
                {
                    var kd = meta[key];
                    if (kd != null)
                        type = kd.GetValueType();
                }
                return type;
            }

            /// <summary>
            /// The dictionary this elements object belongs to.
            /// </summary>
            public PdfDictionary OwningDictionary => (PdfDictionary)OwningContainer;

            #region IDictionary Members

            /// <summary>
            /// Gets a value indicating whether the <see cref="T:System.Collections.IDictionary"></see> object is read-only.
            /// </summary>
            public bool IsReadOnly => false;

            /// <summary>
            /// Returns an <see cref="T:System.Collections.IDictionaryEnumerator"></see> object for the <see cref="T:System.Collections.IDictionary"></see> object.
            /// </summary>
            //IEnumerator<KeyValuePair<string, PdfItem>> IEnumerable<KeyValuePair<String, PdfItem>>.GetEnumerator()
            //    => (IEnumerator<KeyValuePair<string, PdfItem>>)GetEnumerator();

            public IEnumerator<KeyValuePair<string, PdfItem>> GetEnumerator()
                => _elements.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator()
                => ((ICollection)_elements).GetEnumerator();

            PdfItem IDictionary<String, PdfItem>.this[string key]
            {
                [Obsolete("Make sure references are handled correctly.")]
                get => this[key]!; // TODO?? Key not found exception? BREAKING?
                set => this[key] = value;
            }

            /// <summary>
            /// Gets or sets an entry in the dictionary. The specified key must be a valid PDF name
            /// starting with a slash '/'. This property provides full access to the elements of the
            /// PDF dictionary. Wrong use can lead to errors or corrupt PDF files.
            /// TODO: review:
            /// New in 6.3:
            /// Gets or sets an entry in the dictionary.
            /// The getter returns null if no value with the specified key exists.
            /// If the value is a PdfReference this value is returned, i.e. in contrast to GetValue
            /// it is not dereferenced to its indirect object.
            /// The setter throws an ArgumentNullException if the value is null.
            /// If you try to set an indirect object its PdfReference is set instead.
            /// It is not possible to have a .NET reference to an indirect PdfObject.
            /// </summary>
            public PdfItem? this[string key]
            {
                // #US373 [Obsolete("Make sure references are handled correctly.")]
                get
                {
                    Name.EnsureName(key);

                    // Always get the raw (not dereferenced) value.
                    _elements.TryGetValue(key, out var item);
                    return item;
                }
                set
                {
                    if (value == null)
                        throw new ArgumentNullException(nameof(value));

                    SetValueInternal(key, value);
                }
            }

            /// <summary>
            /// Gets or sets an entry in the dictionary identified by a Name.
            /// </summary>
            public PdfItem? this[Name name]
            {
                // #US373 [Obsolete("Make sure references are handled correctly.")]
                get => this[name.Value];
                set => this[name.Value] = value;
            }

            /// <summary>
            /// Gets or sets an entry in the dictionary identified by a PdfName object.
            /// </summary>
            public PdfItem? this[PdfName key]
            {
                // #US373 [Obsolete("Make sure references are handled correctly.")]
                get => this[key.Value];
                set => this[key.Value] = value;
            }

            /// <summary>
            /// Removes the value with the specified key.
            /// </summary>
            public bool Remove(string key)
            {
                // ReSharper disable once InvertIf
                // ReSharper disable once CanSimplifyDictionaryRemovingWithSingleCall because this is not in .NET Standard
                if (_elements.TryGetValue(key, out var oldItem))
                {
                    _elements.Remove(key);
                    ReleaseItem(oldItem);
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Removes the value with the specified key.
            /// </summary>
            public bool Remove(KeyValuePair<string, PdfItem> item)
            {
                // ReSharper disable once InvertIf
                if (_elements.TryGetValue(item.Key, out var value))
                {
                    // ReSharper disable once InvertIf
                    if (ReferenceEquals(item.Value, value))
                    {
                        Remove(item.Key);
                        return true;
                    }
                }
                return false;
            }

            /// <summary>
            /// Determines whether the dictionary contains the specified name.
            /// </summary>
            public bool ContainsKey(string key) => _elements.ContainsKey(key);

            /// <summary>
            /// Determines whether the dictionary contains a specific key.
            /// </summary>
            public bool Contains(KeyValuePair<string, PdfItem> item)
            {
                if (_elements.TryGetValue(item.Key, out var value))
                {
                    return ReferenceEquals(item.Value, value);
                }
                return false;
            }

            /// <summary>
            /// Removes all elements from the dictionary.
            /// </summary>
            public void Clear()
            {
                var oldItems = _elements.Values.ToArray();
                _elements.Clear();
                foreach (var oldItem in oldItems)
                    ReleaseItem(oldItem);
            }

            /// <summary>
            /// Adds the specified value to the dictionary.
            /// If the value is an indirect object, the PdfReference is added instead.
            /// If the value is a direct object that war previously added to another
            /// dictionary or array, an exception is thrown.
            /// </summary>
            //void IDictionary<string, PdfItem?>.Add(string key, PdfItem? value)
            //    => Add(key, value ?? throw new ArgumentNullException(nameof(value)));

            public void Add(string key, PdfItem value)
            {
                //if (String.IsNullOrEmpty(key))
                //    throw new ArgumentNullException(nameof(key));
                //if (key[0] != '/')
                //    throw new ArgumentException("The key must start with a slash '/'.");
                Name.EnsureName(key);

                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                SetValueInternal(key, value);
            }

            /// <summary>
            /// Adds an item to the dictionary.
            /// </summary>
            /// <exception cref="ArgumentNullException">If item.Value is null</exception>
            //void ICollection<KeyValuePair<String, PdfItem?>>.Add(KeyValuePair<string, PdfItem?> item) =>
            //    Add(item.Key, item.Value ?? throw new ArgumentNullException(nameof(item.Value)));

            public void Add(KeyValuePair<string, PdfItem> item) => Add(item.Key, item.Value);

            /// <summary>
            /// Gets all keys currently in use in this dictionary as an array of PdfName objects.
            /// </summary>
            public PdfName[] KeyNames
            {
                get
                {
                    ICollection values = _elements.Keys;
                    int count = values.Count;
                    var strings = new string[count];
                    values.CopyTo(strings, 0);
                    var names = new PdfName[count];
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
            /// Gets all values currently in use in this dictionary as an array of PdfItem objects.
            /// </summary>
            //ICollection<PdfItem?> IDictionary<string, PdfItem?>.Values => (ICollection<PdfItem?>)Values;

            public ICollection<PdfItem> Values
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
            public void CopyTo(KeyValuePair<string, PdfItem>[] array, int arrayIndex)
            {
                foreach (var element in _elements)
                    array[arrayIndex++] = element;
            }

            /// <summary>
            /// The current implementation returns null.
            /// </summary>
            public object SyncRoot => null!;

            #endregion

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
                    sb.AppendFormat(Invariant($"count={_elements.Count} ["));
                    var addSpace = false;
                    ICollection<string> keys = _elements.Keys;
                    foreach (var key in keys)
                    {
                        if (addSpace)
                            sb.Append(' ');
                        addSpace = true;
                        sb.Append(key);
                    }
                    sb.Append(']');
                    return sb.ToString();
                }
            }

            static Exception ExceptionOnNull(string key)
            {
                return new InvalidOperationException($"Value at key '{key}' is null.");
            }

            static Exception ExceptionOnTypeMismatch(string key, Type expected, Type found)
            {
                return new InvalidOperationException($"Value at key '{key}' is expected to be of type {expected.FullName}, but is of type {found.FullName}.");
            }

            /// <summary>
            /// The elements of the dictionary with a string as key.
            /// Because the string is a name it starts always with a '/'.
            /// </summary>
            Dictionary<string, PdfItem> _elements = [];
        }

        /// <summary>
        /// Represents the optional PDF stream of a dictionary.
        /// </summary>
        public sealed class PdfStream
        {
            internal PdfStream(PdfDictionary ownerDictionary)
            {
                _ownerDictionary = ownerDictionary ?? throw new ArgumentNullException(nameof(ownerDictionary));

                // TODO EnsureIndirect()
                if (ownerDictionary.IsIndirect is false)
                {
                    //TODO: Does not work on all tests.
                    // throw new InvalidOperationException("Cannot create a stream for a direct object.");

                    // This happened when we create a stream for a PdfDictionary during its creation.
                    // The Dictionary must have an owner, but is not yet added to the iRef table.
                    ownerDictionary.SetMustBeIndirect();
                }
            }

            /// <summary>
            /// A .NET string can contain char(0) as a valid character.
            /// </summary>
            internal PdfStream(byte[] value, PdfDictionary owner)
                : this(owner)
            {
                // TODO EnsureIndirect()
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
                //if (_ownerDictionary != null!)
                //{
                //    // ???
                //    Debug.Assert(false);
                //}

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
                get => _value ??= [];
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
                        var filter = _ownerDictionary.Elements.GetValue(Keys.Filter);
                        //var filter = _ownerDictionary.Elements[Keys.Filter]; #US373
                        if (filter != null)
                        {
                            var decodeParms = _ownerDictionary.Elements.GetValue(Keys.DecodeParms);
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

                    return bytes ?? [];
                }
            }

            /// <summary>
            /// Tries to unfilter the bytes of the stream. If the stream is filtered and PDFsharp knows the filter
            /// algorithm, the stream content is replaced by its unfiltered value and the function returns true.
            /// Otherwise, the content remains untouched and the function returns false.
            /// The function is useful for analyzing existing PDF files.
            /// </summary>
            [Obsolete("Not correctly implemented. Use the function TryUncompress.")]
            public bool TryUnfilter()
            {
                // Keep old code, do not break existing code.
                if (_value != null)
                {
                    var filter = _ownerDictionary.Elements.GetValue(Keys.Filter/*, VCF.NoTransform*/);
                    if (filter != null)
                    {
                        // PDFsharp can only uncompress streams that are compressed with the ZIP or LZH algorithm.
                        var decodeParms = _ownerDictionary.Elements.GetValue(Keys.DecodeParms/*, VCF.NoTransform*/);
                        var bytes = Filtering.Decode(_value, filter, decodeParms);
                        if (bytes != null!)
                        {
                            _ownerDictionary.Elements.Remove(Keys.Filter);
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
            /// Tries to uncompress the bytes of the stream. If the stream is filtered with the LZWDecode or FlateDecode filter,
            /// the stream content is replaced by its uncompressed value and the function returns true.
            /// Otherwise, the content remains untouched and the function returns false.
            /// The function is useful for analyzing existing PDF files.
            /// </summary>
            public bool TryUncompress()
            {
                if (_value != null)
                {
                    var filter = _ownerDictionary.Elements.GetValue(Keys.Filter);
                    // var filter = _ownerDictionary.Elements[Keys.Filter]; #US373
                    if (filter == null)
                        return false;

                    // filter can be an array. We only try to unzip a single filter name.
                    string filterName;
                    if (filter is PdfArray { Elements.Count: 1 } array)
                    {
                        filterName = array.Elements[0].ToString()!.TrimStart('/');
                    }
                    else
                    {
                        filterName = filter.ToString()!.TrimStart('/');
                    }

                    // PDF 1.7 specs say that the abbreviations are also allowed as filter values.
                    if (filterName is PdfFilterNames.LzwDecode or PdfFilterNames.LzwDecodeAbbreviation
                        or PdfFilterNames.FlateDecode or PdfFilterNames.FlateDecodeAbbreviation)
                    {
                        // PDFsharp can only uncompress streams that are compressed with the ZIP or LZH algorithm.
                        var decodeParms = _ownerDictionary.Elements.GetValue(Keys.DecodeParms/*, VCF.NoTransform*/);
                        var bytes = Filtering.Decode(_value, filter, decodeParms);

                        // Remove the filter and optional decode parameters.
                        _ownerDictionary.Elements.Remove(Keys.Filter);
                        _ownerDictionary.Elements.Remove(Keys.DecodeParms);

                        // Replace original bytes with unzipped version.
                        Value = bytes;
                        return true;
                    }
                }
                return false;
            }

            /// <summary>
            /// Returns true if the dictionary contains the key '/Filter',
            /// false otherwise.
            /// </summary>
            public bool IsFiltered()
            {
                if (_value != null)
                {
                    // TODO #US373: Check documentation. Returns false if Filter is set, but stream is not.
                    return _ownerDictionary.Elements.HasValue(Keys.Filter); // #US373
                }
                return false;
            }

            /// <summary>
            /// Compresses the stream with the FlateDecode filter.
            /// If a filter is already defined, the function has no effect.
            /// </summary>
            public void Zip()
            {
                if (_value == null)
                    return;

                if (!_ownerDictionary.Elements.ContainsKey(Keys.Filter))
                {
                    _value = Filtering.FlateDecode.Encode(_value, _ownerDictionary.Document.Options.FlateEncodeMode);
                    _ownerDictionary.Elements.SetName(Keys.Filter, "/FlateDecode");
                    _ownerDictionary.Elements.SetInteger(Keys.Length, _value.Length);
                }
            }

            /// <summary>
            /// Returns the stream content as a raw string.
            /// </summary>
            public override string ToString()
            {
                if (_value == null)
                    return "«null»";

                string content;
                var filter = _ownerDictionary.Elements.GetValue(Keys.Filter);
                if (filter != null)
                {
                    var decodeParms = _ownerDictionary.Elements.GetValue(Keys.DecodeParms);
                    var bytes = Filtering.Decode(_value, filter, decodeParms);
                    if (bytes != null!)
                        content = PdfEncoders.RawEncoding.GetString(bytes, 0, bytes.Length);
                    else
                        throw new NotImplementedException("Unknown filter");
                }
                else
                    content = PdfEncoders.RawEncoding.GetString(_value, 0, _value.Length);

                return content;
            }

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
        /// Implements a comparer that is used to sort the keys of a dictionary.
        /// It puts /Type and /Subtype at the top.
        /// </summary>
        class KeyComparer : IComparer<PdfName>
        {
            public int Compare(PdfName? l, PdfName? r)
            {
                if (l != null)
                {
                    if (r != null)
                    {
                        if (l.Value.Equals("/Type"))
                            return -1;
                        if (r.Value.Equals("/Type"))
                            return 1;

                        if (l.Value.Equals("/Subtype"))
                            return -1;
                        if (r.Value.Equals("/Subtype"))
                            return 1;

                        return String.Compare(l.Value, r.Value, StringComparison.Ordinal);
                    }
                    return 1;
                }
                return r == null ? 0 : -1;
            }
        }

        static KeyComparer Comparer => _keyComparer ??= new();

        static KeyComparer? _keyComparer;

        /// <summary>
        /// Gets the DebuggerDisplayAttribute text.
        /// </summary>
        // ReSharper disable UnusedMember.Local
        string DebuggerDisplay
            => Invariant($"{GetType().Name}({ObjectID.DebuggerDisplay}, count={_elements?.Count ?? 0}, cont={_elements?.DebuggerDisplay})");
        // ReSharper restore UnusedMember.Local
    }
}

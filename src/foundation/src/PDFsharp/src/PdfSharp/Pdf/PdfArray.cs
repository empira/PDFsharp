// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Logging;
using PdfSharp.Internal;
using PdfSharp.Logging;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Represents a PDF array object.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public class PdfArray : PdfContainer, IEnumerable<PdfItem>
    {
        // Reference 1.7: 3.2.5  Array Objects / Page 58
        // Reference 2.0: 7.3.6  Array objects / Page 29

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfArray"/> class.
        /// </summary>
        public PdfArray()
        {
            // Direct object.
            ItemFlags |= ItemFlags.IsArray;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfArray"/> class.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="createIndirect">If true creates an indirect object.</param>
        public PdfArray(PdfDocument document, bool createIndirect = false)
            : base(document, createIndirect)
        {
            ItemFlags |= ItemFlags.IsArray;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfArray"/> class.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="items">The items.</param>
        public PdfArray(PdfDocument document, params PdfItem[] items)
            : base(document)
        {
            foreach (var item in items)
                Elements.Add(item);

            ItemFlags |= ItemFlags.IsArray;
        }

        public PdfArray(params PdfItem[] items) : this()
        {
            foreach (var item in items)
                Elements.Add(item);
        }

        internal PdfArray(params int[] items) : this()
        {
            foreach (var item in items)
                Elements.Add(new PdfInteger(item));
        }

        internal PdfArray(params double[] items) : this()
        {
            foreach (var item in items)
                Elements.Add(new PdfReal(item));
        }

        /// <summary>
        /// Initializes a new instance from an existing array.
        /// Used for object type transformation.
        /// </summary>
        protected PdfArray(PdfArray array)
            : base(array)
        {
#if DEBUG
            // Protect against unintended invocation of this cont.
            var oldType = array.GetType();
            if (oldType != typeof(PdfArray))
            {
                var newType = GetType();
                if (oldType == newType)
                {
                    throw new InvalidOperationException($"You try to convert a PDF array into type '{newType.FullName}', " +
                                                        $"but the array is already of this type.");
                }

                if (!oldType.IsAssignableFrom(newType))
                {
                    throw new InvalidOperationException($"You try to convert type '{oldType.FullName}' into type '{newType.FullName}', " +
                                                        $"but '{newType.Name}' is not derived from '{oldType.Name}'.");
                }
            }
#endif
            // Move ownership of elements to this instance.
            array._elements?.ChangeOwner(this);
            array.SetDead();
            ItemFlags |= ItemFlags.IsArray | ItemFlags.IsTransformed;
        }

        /// <summary>
        /// Creates a copy of this array. Direct elements are deep-copied.
        /// Indirect references are not modified.
        /// </summary>
        public new PdfArray Clone()
            => (PdfArray)Copy();

        /// <summary>
        /// Implements the copy mechanism.
        /// </summary>
        protected override object Copy()
        {
            // Clone array.
            var array = (PdfArray)base.Copy();
            var elements = array._elements;
            if (elements != null)
            {
                elements = elements.Clone();
                array._elements = elements;
                elements.ChangeOwner(array);
            }
            return array;
        }

        /// <summary>
        /// Gets the collection containing the elements of this array.
        /// </summary>
        public ArrayElements Elements
        {
            get
            {
                EnsureAlive();
                return _elements ??= new(this);
            }
        }

        /// <summary>
        /// The elements of the array.
        /// </summary>
        ArrayElements? _elements;

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        public virtual IEnumerator<PdfItem> GetEnumerator()
            => Elements.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        /// <summary>
        /// Returns a string with the content of this object in a readable form.
        /// Useful for debugging purposes only.
        /// </summary>
        public override string ToString()
        {
            var text = new StringBuilder();
            text.Append('[');
            int count = Elements.Count;
            for (int idx = 0; idx < count; idx++)
            {
                if (idx != 0)
                    text.Append(' ');
                text.Append(Elements[idx]);
            }
            text.Append(']');
            return text.ToString();
        }

        internal override void WriteObject(PdfWriter writer)
        {
            writer.WriteBeginObject(this);
            int count = Elements.Count;
#if TEST_CODE_
            // Ensure that PDFsharp does not use PdfLiterals anymore (PdfMatrix, PdfOutline, ...).
            //if (count == 4 && Elements.GetName(1) == "/XYZ" && Elements.GetInteger(2) == 842)
            if (count is > 3 and <= 5)
            {
                if (ParentInfo?.OwningElements.OwningContainer.ObjectID.ObjectNumber == 9658)
                    Debugger.Break();
                var item0 = Elements[0];
                var item1 = Elements[1];
                var item2 = Elements[2];
                if (item0 is PdfReference iref && iref.ObjectNumber == 29)
                {
                    _ = typeof(int);
                }
                if (item1 is PdfName name && name.Value == "/XYZ")
                //                      &&
                //(item2 is PdfInteger integer && integer.Value == 842))
                {
                    _ = typeof(int);
                }
            }
#endif
            for (int idx = 0; idx < count; idx++)
            {
                var value = Elements[idx];
                value.WriteObject(writer);
            }
            writer.WriteEndObject();
        }

        /// <summary>
        /// Clones the elements of the specified PDF dictionary.
        /// </summary>
        internal void CloneElementsOf(PdfArray array)
        {
            _elements = array.Elements.Clone();
            // Note that ParentInfo is still null for each item.
            _elements.OwningContainer = this;
        }

        // There is no "ArrayMetadata".

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        new void EnsureAlive()
        {
            if (IsDead)
            {
                throw new InvalidOperationException(
                    "This array cannot be used anymore, because its content was now owned by an object of a derived class.");
            }
        }

        /// <summary>
        /// Represents the elements of a PdfArray.
        /// </summary>
        [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
        public sealed class ArrayElements : ElementsBase, IList<PdfItem>, ICloneable
        {
            internal ArrayElements(PdfArray owningArray)
                : base(owningArray)
            {
                Debug.Assert(ReferenceEquals(OwningArray, owningArray));
            }

            object ICloneable.Clone()
            {
                // Shallow clone the list elements.
                var arrayElements = (ArrayElements)MemberwiseClone();
                var elements = new List<PdfItem>(arrayElements._elements);
                arrayElements._elements = elements;
                arrayElements.OwningContainer = null!;

                // Clone all direct objects.
                int count = Count;
                for (int idx = 0; idx < count; idx++)
                {
                    var item = elements[idx];
                    if (item is PdfObject obj)
                    {
                        // Case: item is a direct object.

                        Debug.Assert(obj.IsIndirect is false);
                        Debug.Assert(obj.ParentInfo is not null);
                        obj = obj.Clone();
                        Debug.Assert(obj.ParentInfo is null);
                        obj.SetStructureParent(arrayElements, idx);
                        elements[idx] = obj;
                    }
                    else if (item is PdfReference reference)
                    {
                        reference.AddRef();
                    }
                    else
                    {
                        _ = typeof(int);
                    }
                }
                return arrayElements;
            }

            /// <summary>
            /// Creates a shallow copy of this object.
            /// </summary>
            public ArrayElements Clone()
                => (ArrayElements)((ICloneable)this).Clone();

            /// <summary>
            /// Moves this instance to another array during object type transformation.
            /// </summary>
            internal void ChangeOwner(PdfArray newOwningArray)
            {
                if (OwningArray != null!)
                {
                    // Can this assertion really fail?
                    Debug.Assert(ReferenceEquals(this, OwningArray._elements));

                    // Disconnect old owner from this ArrayElements.
                    OwningArray._elements = null;
                }

                // Set new owner.
                OwningContainer = newOwningArray;

                // Set owners elements to this.
                newOwningArray._elements = this;
            }

            // ===== PdfBoolean =====

            /// <summary>
            /// Gets the boolean value for the specified index.
            /// If the value exists but is neither a boolean nor a PDF reference to a boolean,
            /// the function throws an InvalidOperationException.
            /// </summary>
            public bool GetBoolean(int index)
            {
                TryGetBooleanInternal(index, out bool result, true);
                return result;
            }

            /// <summary>
            /// Tries to get the boolean value for the specified index.
            /// Returns true on success, false if value is of wrong type.
            /// </summary>
            public bool TryGetBoolean(int index, out bool result)
                => TryGetBooleanInternal(index, out result, false);

            // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            bool TryGetBooleanInternal(int index, out bool result, bool throwOnTypeMismatch)
            {
                EnsureIndex(index, true);

                var value = this[index];
                PdfReference.Dereference(ref value);
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
                            SyMsgs.ArrayEntryIsOfWrongType(index, typeof(PdfBoolean), value.GetType()).Message);
                    }
                    success = false;
                    return false;
                }
            }

            /// <summary>
            /// Sets the entry to the specified value.
            /// </summary>
            public void SetBoolean(int index, bool value)
            {
                SetValueInternal(index, new PdfBoolean(value));
            }

            // ===== PdfInteger =====

            /// <summary>
            /// Gets the integer value for the specified index.
            /// If the value exists but is neither an integer nor a PDF reference to an integer,
            /// the function throws an InvalidOperationException.
            /// </summary>
            public int GetInteger(int index)
            {
                TryGetIntegerInternal(index, out int result, true);
                return result;
            }

            /// <summary>
            /// Tries to get the integer value for the specified index.
            /// Returns true on success, false if value is of wrong type.
            /// </summary>
            public bool TryGetInteger(int index, out int result)
                => TryGetIntegerInternal(index, out result, false);

            // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            bool TryGetIntegerInternal(int index, out int result, bool throwOnTypeMismatch)
            {
                EnsureIndex(index, true);

                var value = this[index];
                PdfReference.Dereference(ref value);
                bool success = true;
                result = value switch
                {
                    PdfInteger integer => integer.Value,
                    PdfIntegerObject integer => integer.Value,
                    _ => Fail()
                };
                return success;

                int Fail()
                {
                    if (throwOnTypeMismatch)
                    {
                        throw new InvalidOperationException(
                            SyMsgs.ArrayEntryIsOfWrongType(index, typeof(PdfInteger), value.GetType()).Message);
                    }
                    success = false;
                    return 0;
                }
            }

            /// <summary>
            /// Sets the entry to the specified value.
            /// </summary>
            public void SetInteger(int index, int value)
            {
                EnsureIndex(index, true);

                SetValueInternal(index, new PdfInteger(value));
            }

            // No GetUnsignedInteger or TryGetUnsignedInteger yet.
            // Will be implemented if we find a use case.

            // ===== PdfLongInteger =====

            /// <summary>
            /// Gets the long integer value for the specified index.
            /// If the value exists but is neither an integer or a long integer
            /// nor a PDF reference to an integer or a long integer,
            /// the function throws an InvalidOperationException.
            /// </summary>
            public long GetLongInteger(int index)
            {
                TryGetLongIntegerInternal(index, out long result, true);
                return result;
            }

            /// <summary>
            /// Tries to get the long integer value for the specified index.
            /// Returns true on success, false if value is of wrong type.
            /// </summary>
            public bool TryGetLongInteger(int index, out long result)
                => TryGetLongIntegerInternal(index, out result, false);

            // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            bool TryGetLongIntegerInternal(int index, out long result, bool throwOnTypeMismatch)
            {
                EnsureIndex(index, true);

                var value = this[index];
                PdfReference.Dereference(ref value);
                bool success = true;
                result = value switch
                {
                    PdfInteger integer => integer.Value,
                    PdfIntegerObject integer => integer.Value,
                    PdfLongInteger integer => integer.Value,
                    PdfLongIntegerObject integer => integer.Value,
                    _ => Fail()
                };
                return success;

                long Fail()
                {
                    if (throwOnTypeMismatch)
                    {
                        throw new InvalidOperationException(
                            SyMsgs.ArrayEntryIsOfWrongType(index, typeof(PdfLongInteger), value.GetType()).Message);
                    }
                    success = false;
                    return 0;
                }
            }

            /// <summary>
            /// Sets the entry to the specified value.
            /// </summary>
            public void SetLongInteger(int index, long value)
            {
                SetValueInternal(index, new PdfLongInteger(value));
            }

            // ===== PdfReal =====

            /// <summary>
            /// Gets the real value for the specified index.
            /// If the value exists but is neither an integer nor a real
            /// nor a PDF reference to an integer or a real,
            /// the function throws an InvalidOperationException.
            /// </summary>
            public double GetReal(int index)
            {
                TryGetRealInternal(index, out double result, true);
                return result;
            }

            /// <summary>
            /// Tries to get the real value for the specified index.
            /// Returns true on success, false if value is of wrong type.
            /// </summary>
            public bool TryGetReal(int index, out double result)
                => TryGetRealInternal(index, out result, false);

            // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            bool TryGetRealInternal(int index, out double result, bool throwOnTypeMismatch)
            {
                EnsureIndex(index, true);

                var value = this[index];
                PdfReference.Dereference(ref value);
                bool success = true;
                result = value switch
                {
                    PdfInteger integer => integer.Value,
                    PdfIntegerObject integer => integer.Value,
                    PdfLongInteger integer => integer.Value,
                    PdfLongIntegerObject integer => integer.Value,
                    PdfReal real => real.Value,
                    PdfRealObject real => real.Value,
                    _ => Fail()
                };
                return success;

                double Fail()
                {
                    if (throwOnTypeMismatch)
                    {
                        throw new InvalidOperationException(
                            SyMsgs.ArrayEntryIsOfWrongType(index, typeof(PdfReal), value.GetType()).Message);
                    }
                    success = false;
                    return 0;
                }
            }

            /// <summary>
            /// Sets the entry to the specified value.
            /// </summary>
            public void SetReal(int index, double value)
            {
                SetValueInternal(index, new PdfReal(value));
            }

            // ===== PdfReal as nullable value =====

            /// <summary>
            /// Gets the double? value for the specified index.
            /// If the value exists but is neither a null object nor a real or integer
            /// nor a PDF reference to a null object, a real or integer,
            /// the function throws an InvalidOperationException.
            /// </summary>
            public double? GetNullableReal(int index/*, double? defaultValue=*/)
            {
                // Note that this function exists for arrays only. It makes no sense for dictionaries,
                // because entries like "/SomeNumber null" do not exist.

                TryGetNullableRealInternal(index, out double? result, true);
                return result;
            }

            /// <summary>
            /// Tries to get the double? value for the specified index.
            /// Returns true on success, false if value is of wrong type.
            /// </summary>
            public bool TryGetNullableReal(int index, out double? result)
                => TryGetNullableRealInternal(index, out result, false);

            // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            bool TryGetNullableRealInternal(int index, out double? result, bool throwOnTypeMismatch)
            {
                var value = this[index];
                PdfReference.Dereference(ref value);
                bool success = true;
                result = value switch
                {
                    PdfNull => null,
                    PdfNullObject => null,
                    PdfInteger integer => integer.Value,
                    PdfIntegerObject integer => integer.Value,
                    PdfLongInteger integer => integer.Value,
                    PdfLongIntegerObject integer => integer.Value,
                    PdfReal real => real.Value,
                    PdfRealObject real => real.Value,
                    _ => Fail()
                };
                return success;

                double? Fail()
                {
                    if (throwOnTypeMismatch)
                    {
                        throw new InvalidOperationException(
                            SyMsgs.ArrayEntryIsOfWrongType(index, typeof(PdfReal), value.GetType()).Message);
                    }
                    success = false;
                    return null;
                }
            }

            // No SetNullableReal because it makes no sense.

            // ===== PdfString =====

            /// <summary>
            /// Gets the string value for the specified index.
            /// If the value exists but is neither a string nor a PDF reference to a string, the function throws an InvalidOperationException.
            /// </summary>
            public string GetString(int index)
            {
                TryGetStringInternal(index, out string result, true);
                return result;
            }

            /// <summary>
            /// Tries to get the string value for the specified index.
            /// Returns true on success, false if value is of wrong type.
            /// </summary>
            public bool TryGetString(int index, out string result)
                => TryGetStringInternal(index, out result, false);

            // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            bool TryGetStringInternal(int index, out string result, bool throwOnTypeMismatch)
            {
                var value = this[index];
                PdfReference.Dereference(ref value);
                bool success = true;
                result = value switch
                {
                    PdfString str => str.Value,
                    PdfStringObject str => str.Value,
                    _ => Fail()
                };
                return success;

                string Fail()
                {
                    if (throwOnTypeMismatch)
                    {
                        throw new InvalidOperationException(
                            SyMsgs.ArrayEntryIsOfWrongType(index, typeof(PdfString), value.GetType()).Message);
                    }
                    success = false;
                    return "";
                }
            }

            /// <summary>
            /// Sets the entry to the specified value.
            /// </summary>
            public void SetString(int index, string value)
            {
                SetValueInternal(index, new PdfString(value));
            }

            // ===== PdfName =====

            /// <summary>
            /// Gets the name value for the specified index.
            /// If the value exists but is neither a name nor a PDF reference to a name,
            /// the function throws an InvalidOperationException.
            /// </summary>
            public string GetName(int index)
            {
                TryGetNameInternal(index, out string result, true);
                return result;
            }

            /// <summary>
            /// Tries to get the name value for the specified index.
            /// Returns true on success, false if value is of wrong type.
            /// </summary>
            public bool TryGetName(int index, out string result)
                => TryGetNameInternal(index, out result, false);

            // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            bool TryGetNameInternal(int index, out string result, bool throwOnTypeMismatch)
            {
                var value = this[index];
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
                            SyMsgs.ArrayEntryIsOfWrongType(index, typeof(PdfInteger), value.GetType()).Message);
                    }
                    success = false;
                    return "/";
                }
            }

            /// <summary>
            /// Sets a PDF name at the specified index.
            /// </summary>
            public void SetName(int index, string value)
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                // Ensure that value already starts with a slash.
                if (value.Length == 0 || value[0] != '/')
                {
                    PdfSharpLogHost.Logger.LogWarning("A PDF name must start with a '/'.");
                    value = String.Concat("/", value);
                }
                SetValueInternal(index, new PdfName(value));
            }

            // ===== GetValue =====

            /// <summary>  // TODO
            /// Gets the value for the specified key.
            /// Unsinn: If the value does not exist, it is optionally created.
            /// </summary>
            public PdfItem? GetValue(int index, VCF options = VCF.None,
                [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                            DynamicallyAccessedMemberTypes.NonPublicConstructors)]
                Type? valueType = null)
            {
#if PDFSHARP_DEBUG_
                if (ShouldBreak5)
                    Debugger.Break();
#endif
                EnsureIndex(index, false);
                EnsureNoCreationFlags(options);
                if (valueType != null)
                    EnsureValueType<PdfItem>(valueType);

                return GetValueInternal(index, options, valueType, valueType != null);
            }

            /// <summary>
            /// * throws if valueType does not match
            /// </summary>
            public PdfItem GetRequiredValue(int index, VCF options = VCF.None,
                [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                            DynamicallyAccessedMemberTypes.NonPublicConstructors)]
                Type? valueType = null)
            {
                return GetValue(index, options, valueType)
                       ?? throw ExceptionOnNull(index);
            }

            public bool TryGetValue(int index, [MaybeNullWhen(false)] out PdfItem value)
                => TryGetValue(index, out value, null);

            /// <summary>
            /// * throws if result is not of type T.
            /// </summary>
            public bool TryGetValue(int index, [MaybeNullWhen(false)] out PdfItem value,
                [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                            DynamicallyAccessedMemberTypes.NonPublicConstructors)]
                Type? valueType)
            {
                value = null;
                var result = GetValueInternal(index, VCF.NoTransform, valueType, false);
                if (result != null)
                {
                    value = result;
                    return true;
                }
                return false;

                //value = GetValueInternal(index, VCF.NoTransform, valueType, false);
                //return value != null;
            }

            /// <summary>
            /// * throws if result is not of type T.
            /// </summary>
            public T? GetValue<
                [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                            DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>
                (int index, VCF options = VCF.None) where T : PdfItem
            {
                EnsureIndex(index, false);
                EnsureNoCreationFlags(options);

                var value = (T?)GetValueInternal(index, options, typeof(T), true);
                return value;
            }

            /// <summary>
            /// * throws if result is not of type T.
            /// </summary>
            public T GetRequiredValue<
                    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                                DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>
                (int index, VCF options = VCF.None) where T : PdfItem
            {
                return GetValue<T>(index, options)
                       ?? throw ExceptionOnNull(index);
            }

            public bool TryGetValue<
                    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                                DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>
                (int index, [MaybeNullWhen(false)] out T value) where T : PdfItem
            {
                value = null;
                if (GetValueInternal(index, VCF.NoTransform, typeof(T), false) is T valueOfT)
                {
                    value = valueOfT;
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Sets the entry with the specified value. DON’T USE THIS FUNCTION - IT MAY BE REMOVED.  // PDFsharp/NT
            /// </summary>
            public void SetValue(int index, PdfItem value)
            {
                SetValueInternal(index, value);
            }

            // ===== GetObject =====

            /// <summary>
            /// Gets the PdfObject with the specified index, or null if no such object exists.
            /// If the index refers to a reference, the referenced PdfObject is returned.
            /// </summary>
            public PdfObject? GetObject(int index, VCF options = VCF.None,
                [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                            DynamicallyAccessedMemberTypes.NonPublicConstructors)]
                Type? valueType = null)
            {
                EnsureIndex(index, false);
                EnsureNoCreationFlags(options);
                if (valueType != null)
                    EnsureValueType<PdfObject>(valueType);
                //else
                //    valueType = typeof(PdfObject);

                var value = GetValueInternal(index, options, valueType, false);
                return value as PdfObject;
            }

            public PdfObject GetRequiredObject(int index, VCF options = VCF.None,
                [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                            DynamicallyAccessedMemberTypes.NonPublicConstructors)]
                Type? valueType = null)
            {
                return GetObject(index, options, valueType)
                       ?? throw ExceptionOnNull(index);
            }

            /// <summary>
            /// Gets a PDF object of type T from the specified index.
            /// If the found object is a PDF reference or a PDF container (array or dictionary) but not of type T
            /// it is tried to be transformed into this type and replaces the old object.
            /// If the transformation is not possible, e.g. because T does not match with the found type,
            /// an exception is thrown.
            /// Returns null if the found object is neither a PDF reference nor a PDF container.
            /// </summary>
            /// <typeparam name="T">The type of the PDF object to get.</typeparam>
            /// <param name="index">The 0-based index of the object.</param>
            /// <param name="options"></param>
            public T? GetObject<
                [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                            DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>
                (int index, VCF options = VCF.None) where T : PdfObject
            {
                EnsureIndex(index, false);
                EnsureNoCreationFlags(options);

                // TODO: Never throw?
                bool throwOnTypeMismatch = typeof(PdfContainer).IsAssignableFrom(typeof(T));
                return GetValueInternal(index, options, typeof(T), throwOnTypeMismatch) as T;
            }

            public T GetRequiredObject<
                    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                                DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>
                (int index, VCF options = VCF.None) where T : PdfObject
            {
                return GetObject<T>(index, options)
                       ?? throw ExceptionOnNull(index);
            }

            public void SetObject(int index, PdfObject obj)  // Used in PDFsharp
            {
                EnsureIndex(index, false);

                SetValueInternal(index, obj);
            }

            // ===== GetArray =====

            /// <summary>
            /// Gets the PdfArray with the specified index, or null if no such object exists.
            /// If the index refers to a reference, the referenced PdfArray is returned.
            /// </summary>
            public PdfArray? GetArray(int index, VCF options = VCF.None,
                [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                            DynamicallyAccessedMemberTypes.NonPublicConstructors)]
                Type? valueType = null)
            {
                EnsureIndex(index, false);
                EnsureNoCreationFlags(options);
                if (valueType != null)
                    EnsureValueType<PdfArray>(valueType);

                return GetValueInternal(index, options, valueType, valueType != null) as PdfArray;
            }

            /// <summary>
            /// Gets the PdfArray with the specified index.
            /// An InvalidOperationException is thrown if the object does not exist.
            /// If the index refers to a reference, the referenced PdfArray is returned.
            /// </summary>
            public PdfArray GetRequiredArray(int index, VCF options = VCF.None,
                [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                            DynamicallyAccessedMemberTypes.NonPublicConstructors)]
                Type? valueType = null)
            {
                return GetArray(index, options, valueType)
                       ?? throw ExceptionOnNull(index);
            }

            public bool TryGetArray(int index, [MaybeNullWhen(false)] out PdfArray array)
                => TryGetArray(index, out array, null);

            public bool TryGetArray(int index, [MaybeNullWhen(false)] out PdfArray array,
                [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                            DynamicallyAccessedMemberTypes.NonPublicConstructors)]
                Type? valueType)
            {
                EnsureIndex(index, false);
                if (valueType != null)
                    EnsureValueType<PdfArray>(valueType);

                array = null;
                if (GetValueInternal(index, VCF.NoTransform, valueType, false) is PdfArray value)
                {
                    array = value;
                    return true;
                }
                return false;
            }

            public T? GetArray<
                    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                                DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>
                (int index, VCF options = VCF.None) where T : PdfArray
            {
                EnsureIndex(index, false);
                EnsureNoCreationFlags(options);

                var array = GetValueInternal(index, options, typeof(T), true) as T;
                return array;
            }

            public T GetRequiredArray<
                    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                                DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>
                (int index, VCF options = VCF.None) where T : PdfArray
            {
                return GetArray<T>(index, options)
                       ?? throw ExceptionOnNull(index);
            }

            public bool TryGetArray<
                    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                                DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>
                (int index, [MaybeNullWhen(false)] out T array) where T : PdfArray
            {
                EnsureIndex(index, false);

                array = null;
                if (GetValueInternal(index, VCF.NoTransform, typeof(T), false) is T valueOfT)
                {
                    array = valueOfT;
                    return true;
                }
                return false;
            }

            // ===== GetDictionary =====

            /// <summary>
            /// Gets the PdfDictionary with the specified index, or null if no such object exists.
            /// If the index refers to a reference, the referenced PdfDictionary is returned.
            /// </summary>
            public PdfDictionary? GetDictionary(int index, VCF options = VCF.None,
                [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                            DynamicallyAccessedMemberTypes.NonPublicConstructors)]
                Type? valueType = null)
            {
                EnsureIndex(index, false);
                EnsureNoCreationFlags(options);
                if (valueType != null)
                    EnsureValueType<PdfDictionary>(valueType);

                return GetValueInternal(index, options, valueType, valueType != null) as PdfDictionary;
            }

            public PdfDictionary GetRequiredDictionary(int index, VCF options = VCF.None,
                [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                            DynamicallyAccessedMemberTypes.NonPublicConstructors)]
                Type? valueType = null)
            {
                return GetDictionary(index, options, valueType)
                       ?? throw ExceptionOnNull(index);
            }

            public bool TryGetDictionary(int index, [MaybeNullWhen(false)] out PdfDictionary dict)
                => TryGetDictionary(index, out dict, null);

            public bool TryGetDictionary(int index, [MaybeNullWhen(false)] out PdfDictionary dict,
                [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                            DynamicallyAccessedMemberTypes.NonPublicConstructors)]
                Type? valueType)
            {
                EnsureIndex(index, false);
                if (valueType != null)
                    EnsureValueType<PdfDictionary>(valueType);

                dict = null;
                if (GetValueInternal(index, VCF.NoTransform, valueType, false) is PdfDictionary value)
                {
                    dict = value;
                    return true;
                }
                return false;
            }

            public T? GetDictionary<
                    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                                DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>
                (int index, VCF options = VCF.None) where T : PdfDictionary
            {
                EnsureIndex(index, false);
                EnsureNoCreationFlags(options);

                var value = GetValueInternal(index, options, typeof(T), true);
                return value as T;
            }

            public T GetRequiredDictionary<
                    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                                DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>
                (int index, VCF options = VCF.None) where T : PdfDictionary
            {
                return GetDictionary<T>(index, options)
                       ?? throw ExceptionOnNull(index);
            }

            public bool TryGetDictionary<
                    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                                DynamicallyAccessedMemberTypes.NonPublicConstructors)] T>
                (int index, [MaybeNullWhen(false)] out T dict) where T : PdfDictionary
            {
                EnsureIndex(index, false);

                dict = null;
                if (GetValueInternal(index, VCF.NoTransform, typeof(T), false) is T valueOfT)
                {
                    dict = valueOfT;
                    return true;
                }
                return false;
            }

            // ===== PdfReference =====

            /// <summary>
            /// Gets a PDF reference from the specified index, or null if no such object exists.
            /// </summary>
            public PdfReference? GetReference(int index)
            {
                EnsureIndex(index, false);

                return _elements[index] as PdfReference;
            }

            /// <summary>
            /// Gets a PDF reference from the specified index, or throws an exception,
            /// if no such object exists.
            /// </summary>
            public PdfReference GetRequiredReference(int index)
            {
                EnsureIndex(index, false);

                return _elements[index] as PdfReference
                       ?? throw new InvalidOperationException(SyMsgs.IndirectReferenceMustNotBeNull.Message);
            }

            /// <summary>
            /// Sets the entry to an indirect reference.
            /// </summary>
            public void SetReference(int index, PdfReference iref)
            {
                EnsureIndex(index, true);

                if (iref is null)
                    throw new ArgumentNullException(nameof(iref));
                SetValueInternal(index, iref);
            }

            // ===== PdfItem =====

            /// <summary>
            /// Gets all items of this array.
            /// </summary>
            public PdfItem[] Items => _elements.ToArray();

            // ===== Internal =====

            /// <summary>
            /// Gets the value for the specified index.
            /// There is no create option because a value cannot not exist.
            /// A value can be optionally transformed to a derived type.
            /// </summary>
            PdfItem? GetValueInternal(int index, VCF options,
                [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                            DynamicallyAccessedMemberTypes.NonPublicConstructors)]
                Type? valueType, bool throwOnTypeMismatch)
            {
#if DEBUG_ // KEEP Test in more detail and report a bug.  TODO
                // TODO: Why leads 'var valueTest' to type 'PdfItem?'?
                var valueTest2 = this[index]; // Is of type 'PdfItem' not of type 'PdfItem?'.
                valueTest2 = null;  // No Warning hint.
#endif
#if DEBUG_
                // Are we reading a PDF document?
                if ((OwningArray.Document?.IrefTable ?? null) is { IsUnderConstruction: true })
                {
                    //Debugger.Break();
                    _ = typeof(int);
                }
#endif
                Debug.Assert(options is VCF.Create or VCF.CreateIndirect is false);

                // ReSharper suggested to use var, but this would make the type nullable. ???TODO: Why?
                var valueTest = _elements[index]; // Is always of type PdfItem
                // ReSharper disable once SuggestVarOrType_SimpleTypes because value is of type PdfItem? if I use var.
                PdfItem value = _elements[index]; // Is always of type PdfItem.

                Debug.Assert(value is not null);

                // Case: The value exists and can be returned. But for imported documents check for necessary
                // object type transformation.

                if (value is PdfReference iref)
                {
#if DEBUG_
                    var irefTable = OwningArray.Document?.IrefTable ?? null;
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
                        var irefTable = OwningArray.Document?.IrefTable ?? null;
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
                                    UpdateValueInternal(index, iref);
                                }
                            }
                            else
                            {
                                _ = typeof(int);
                            }
                        }
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
                        throw new InvalidOperationException("Indirect reference without value.");
                    }

                    // The nesting can be simplified, but keep it as it is for better understandability.
                    if (options != VCF.NoTransform)
                    {
                        // //Get type from parameter or metadata.
                        // In contrast to PdfDictionary we have no metadata for arrays.
                        var type = valueType /*?? GetValueType(key)*/;

                        // Try transformation only once. If it fails, don’t try again.
                        // Should we try transformation?
                        if (value.ShouldTryTransformation
                            || type != null
                            || value.GetType().IsAssignableFrom(type))
                        {
                            // Do we have a type anyway?
                            if (type != null)
                            {
                                // Is value already of the expected type?
                                if (type.IsInstanceOfType(value))
                                {
                                    // Case: The value is already of the appropriate type.

                                    // Set to transformed, but only if the requested type is not a base type.
                                    if (type != typeof(PdfDictionary) && type != typeof(PdfArray))
                                        value.SetTransformed();
                                }
                                else if (value is PdfContainer cont)
                                {
                                    // Case: Transform array or dictionary.

                                    // TODO: Test to transform twice.

                                    //Debug.Assert(cont.IsIndirect is false);
                                    //Debug.Assert(cont.IsTransformed is false);
                                    Debug.Assert(cont.ParentInfo is null);
                                    value = CreateContainer(type, cont, cont.IsIndirect);
                                    //Not for indirect objects UpdateValueInternal(index, value);
                                    Debug.Assert(cont.IsDead);
                                    Debug.Assert(value.IsTransformed);
                                    Debug.Assert(cont.ParentInfo is null);
                                    //Debug.Assert(((PdfContainer)value).ParentInfo is not null);
                                }
                                //else
                                //{
                                //    throw new NotImplementedException("Type is not a PDF container.");
                                //}
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
                                // TODO: Do not call if value is same.
                                SetValueInternal(index, value);
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
                            // In contrast to PdfDictionary we have no metadata for arrays.
                            var type = valueType; // There is no 'GetValueType(index)'.

                            // Do we have a type anyway?
                            if (type != null)
                            {
                                // Case: We have a type and an existing primitive or direct object.

                                // Handle special case PdfRectangle first.
                                // Make no sense in PdfArray.
                                if (type == typeof(PdfRectangle))
                                {
                                    throw new InvalidOperationException(
                                        "PdfRectangle is not an appropriate type for an item of a PdfArray.");
                                }
                                if (type.IsInstanceOfType(value))
                                {
                                    // Case: The value is already of the appropriate type.

                                    // Set to transformed, but only if the requested type is not a base type.
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
                                    SetValueInternal(index, value);

                                    Debug.Assert(cont.IsTransformed);
                                    Debug.Assert(cont.IsDead);
                                    Debug.Assert(value.IsTransformed); // TODO: Can transform twice?
                                    Debug.Assert(cont.ParentInfo is null);
                                    Debug.Assert(((PdfDictionary)value).ParentInfo is not null);
                                }
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

                // Ensure not a type mismatch.
                if (valueType != null)
                {
                    if (!valueType.IsInstanceOfType(value))
                    {
                        if (throwOnTypeMismatch)
                            throw ExceptionOnTypeMismatch(index, valueType, value.GetType());
                        return null;
                    }
                }
                Debug.Assert(value != null); // TODO: Ensure.
                return value;
            }

            /// <summary>
            /// Implementation of SetValue.
            /// Handles setting same value, setting indirect object,
            /// and releasing old value.
            /// Keep in sync with PdfDictionary.
            /// </summary>
            void SetValueInternal(int index, PdfItem value)
            {
#if DEBUG_
                //if (ShouldBreak1)
                //    Debugger.Break();
                if (value is PdfReference { ObjectID.ObjectNumber: 5 })
                    _ = typeof(int);

#endif
#if true_  // TODO REMOVE or throw exception in DEBUG
#if DEBUG
                // Are we reading a PDF document?
                if ((OwningArray.Document?.IrefTable ?? null) is { IsUnderConstruction: true })
                {
                    //Debugger.Break();
                    _ = typeof(int);
                }
#endif
#endif
                EnsureIndex(index, true);

                // Already checked by caller.
                Debug.Assert(value is not null);

                if (value.IsDead)
                    throw new InvalidOperationException("TODO: Is Dead."); // /messages/ObjectIsDead.html

                // Special treatment for PdfRectangle.
                if (value is PdfRectangle rect)
                    value = rect.GetAsArrayOfValues();
                else
                    PdfReference.ToReference(ref value);

                PdfItem? oldItem = null;
                if (index < Count)
                {
                    oldItem = _elements[index];
                    if (ReferenceEquals(oldItem, value))
                    {
                        LogWarning();
                        return;
                    }
                }
                else
                {
                    if (index > Count)
                    {
                        throw new IndexOutOfRangeException(
                            "Index must not be greater than Count."); // /messages/ObjectIsDead.html
                    }
                    // else: value is appended.
                }

                if (value is PdfObject obj)
                {
                    if (obj.Reference != null)
                    {
                        Debug.Assert(false, "Should not come here anymore.");

                        // Case: Indirect object.

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
                        // Case: Direct primitive object.
                        // E.g. non-indirect PdfStringObject is used instead of PdfString.
                        FailForDirectPrimitiveObject(obj);
                    }
                    else
                    {
                        // Case: Direct container object.
                        Debug.Assert(obj is PdfContainer);

                        if (obj.ParentInfo != null)
                            throw new InvalidOperationException("A direct object can only be added once.");

                        obj.SetStructureParent(this, index);
                    }
                }
                else
                {
                    // Case: value is just a PdfItem - nothing special to do.
                    // Case: value is PDF reference or primitive - nothing special to do.
                    Debug.Assert(value is PdfReference or PdfPrimitive);
                }

                if (index < Count)
                    _elements[index] = value;
                else
                    _elements.Add(value);

                // oldItem can be null here if index was equal to Count
                // or value was inserted.
                if (oldItem != null)
                    ReleaseItem(oldItem);

                return;

                void LogWarning()
                {
                    //PdfSharpLogHost.Logger.LogWarning("Setting same value in dictionary.");
                }
            }

            /// <summary>
            /// The array this elements object belongs to.
            /// </summary>
            PdfArray OwningArray => (PdfArray)OwningContainer;

            #region IList Members

            /// <summary>
            /// Returns false.
            /// </summary>
            public bool IsReadOnly => false;

            /// <summary>
            /// Gets or sets an item at the specified index.
            /// </summary>
            public PdfItem this[int index]
            {
                // Always get the raw value.
                get => _elements[index];
                set
                {
                    if (value == null)
                        throw new ArgumentNullException(nameof(value));

                    //// Note that if index is Count value is appended.
                    //if (index < 0 || index > Count)
                    //    throw new ArgumentOutOfRangeException(nameof(index), index, SyMsgs.IndexOutOfRange3);
                    EnsureIndex(index, true);

                    SetValueInternal(index, value);
                }
            }

            /// <summary>
            /// Removes the item at the specified index.
            /// </summary>
            public void RemoveAt(int index)
            {
                var oldItem = _elements[index];
                _elements.RemoveAt(index);
                ReleaseItem(oldItem);
                FixIndexInParentInfo(index, -1);
            }

            /// <summary>
            /// Removes the first occurrence of a specific object from the array/>.
            /// </summary>
            public bool Remove(PdfItem item)
            {
                int index = IndexOf(item);
                if (index >= 0)
                {
                    RemoveAt(index);
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Inserts the item the specified index.
            /// </summary>
            public void Insert(int index, PdfItem value)
            {
                // TODO: Check reuse, check moved objects.
                //_elements.Insert(index, value);
                // TODO: Let the element know its index.

                // Make space by inserting a dummy value.
                _elements.Insert(index, PdfNull.Value);
                FixIndexInParentInfo(index + 1, 1);
                SetValueInternal(index, value);
            }

            /// <summary>
            /// Determines whether the specified value is in the array.
            /// </summary>
            public bool Contains(PdfItem value)
            {
                if (value is PdfObject { Reference: not null } obj)
                    value = obj.Reference;

                return _elements.Contains(value);
            }

            /// <summary>
            /// Removes all items from the array.
            /// </summary>
            public void Clear()
            {
                var oldItems = _elements.ToArray();
                _elements.Clear();
                foreach (var item in oldItems)
                    ReleaseItem(item);
            }

            /// <summary>
            /// Gets the index of the specified item.
            /// </summary>
            public int IndexOf(PdfItem value)
            {
                if (value is PdfReference iref)
                    return _elements.IndexOf(value);

                if (value is PdfObject { Reference: not null } obj)
                    return _elements.IndexOf(obj.RequiredReference);

                return _elements.IndexOf(value);
            }

            /// <summary>
            /// Appends the specified object to the array.
            /// </summary>
            public void Add(PdfItem value)
            {
                if (value == null!)
                    throw new ArgumentNullException(nameof(value));

                SetValueInternal(Count, value);
            }

            /// <summary>
            /// Returns false.
            /// </summary>
            public bool IsFixedSize => false;

            #endregion

            #region ICollection Members

            /// <summary>
            /// Returns false.
            /// </summary>
            public bool IsSynchronized => false;

            /// <summary>
            /// Gets the number of elements in the array.
            /// </summary>
            public int Count => _elements.Count;

            /// <summary>
            /// Copies the elements of the array to the specified array.
            /// </summary>
            public void CopyTo(PdfItem[] array, int index)
                => _elements.CopyTo(array, index);

            /// <summary>
            /// The current implementation return null.
            /// </summary>
            public object SyncRoot => null!;

            /// <summary>
            /// Returns an enumerator that iterates through the array.
            /// </summary>
            public IEnumerator<PdfItem> GetEnumerator()
                => _elements.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator()
                => _elements.GetEnumerator();

            #endregion

            /// <summary>
            /// If an item is added or removed from the array, the subsequent direct object’s
            /// index in their ParentInfo must be adjusted accordingly.
            /// </summary>
            /// <param name="index">Index of the first item to be adjusted.</param>
            /// <param name="offset">1 or -1, depending on insert or delete.</param>
            void FixIndexInParentInfo(int index, int offset)
            {
                for (int idx = index; idx < _elements.Count; idx++)
                {
                    var obj = _elements[idx] as PdfObject;
                    var parentInfo = obj?.ParentInfo;
                    if (parentInfo is { IsArray: true })
                        parentInfo.AdjustIndex(offset);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void EnsureIndex(int index, bool writeValue)
            {
                // TODO: IL code shows that the function is not inlined.
                // Even in release build. Analyse why.

                var count = _elements.Count;
                if (index < 0 || writeValue ? index > count : index >= count)
                    ThrowIndexOutOfRange(index, writeValue);
            }

            void ThrowIndexOutOfRange(int index, bool writeValue)
            {
                throw new IndexOutOfRangeException(
                    $"Index '{index}' is out of range for {(writeValue ? "writing into" : "reading from")} PdfArray.");
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void EnsureNoCreationFlags(VCF options)
            {
                if (options is VCF.Create or VCF.CreateIndirect)
                    ThrowCreationFlags(options);
            }

            void ThrowCreationFlags(VCF options)
            {
                throw new InvalidOperationException(
                    $"Flag 'ACF.{options.ToString()}' must not be set for array elements because an array entry cannot be undefined.");
            }

            Exception ExceptionOnNull(int index)
            {
                return new InvalidOperationException($"Value at index '{index}' has wrong type.");
            }

            Exception ExceptionOnTypeMismatch(int index, Type expected, Type found)
            {
                return new InvalidOperationException($"Value at index '{index}' is expected to be of type {expected.FullName}, but is of type {found.FullName}.");
            }

            /// <summary>
            /// The elements of the array.
            /// </summary>
            List<PdfItem> _elements = [];
        }

        /// <summary>
        /// Gets the DebuggerDisplayAttribute text.
        /// </summary>
        // ReSharper disable UnusedMember.Local
        string DebuggerDisplay
            => Invariant($"{GetType().Name}({ObjectID.DebuggerDisplay}, count={_elements?.Count ?? 0})");
        // ReSharper restore UnusedMember.Local
    }
}

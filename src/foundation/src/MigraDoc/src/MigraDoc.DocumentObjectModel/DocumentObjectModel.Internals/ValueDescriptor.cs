// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.ComponentModel;
using System.Reflection;

namespace MigraDoc.DocumentObjectModel.Internals
{
    /// <summary>
    /// Base class of all value descriptor classes.
    /// </summary>
    [DebuggerDisplay("({this.GetType().Name}, Name={this.ValueName})")]
    public abstract class ValueDescriptor
    {
        internal ValueDescriptor(string valueName, Type valueType, PropertyInfo propertyInfo, VDFlags flags)
        {
            ValueName = valueName;
            ValueType = valueType;
            PropertyInfo = propertyInfo;
            _flags = flags;
        }

        /// <summary>
        /// Creates the value this descriptor describes.
        /// </summary>
        public object CreateValue()
        {
            var value = Activator.CreateInstance(ValueType);
            return value!;
        }

        /// <summary>
        /// Gets the value from document object as nullable object.
        /// </summary>
        /// <param name="doc">The document object.</param>
        /// <param name="flags">The GetValue flags.</param>
        /// <returns></returns>
        public abstract object? GetValue(DocumentObject doc, GV flags);

        /// <summary>
        /// Sets the value in the document object.
        /// </summary>
        /// <param name="doc">The document object.</param>
        /// <param name="val">The value to set.</param>
        public abstract void SetValue(DocumentObject doc, object? val);

        /// <summary>
        /// Sets the value in the document object to null.
        /// </summary>
        /// <param name="doc">The document object.</param>
        public abstract void SetNull(DocumentObject doc);

        /// <summary>
        /// Determines whether the specified value from the document object is null.
        /// </summary>
        /// <param name="doc">The document object.</param>
        public abstract bool IsNull(DocumentObject doc);

        /// <summary>
        /// Creates the value descriptor based on a PropertyInfo.
        /// </summary>
        internal static ValueDescriptor CreateValueDescriptor(PropertyInfo propertyInfo, DVAttribute? attr)
        {
            var flags = VDFlags.None;
            if (attr?.RefOnly ?? false)
                flags |= VDFlags.RefOnly;
            if (attr?.ReadOnly ?? false)
                flags |= VDFlags.ReadOnly;

            var valueName = propertyInfo.Name;
            var valueType = propertyInfo.PropertyType;

#if true_  // This is now checked in unit tests and must not be rechecked on every startup.
            if (valueType.IsValueType)
            {
                var underlyingValueType = Nullable.GetUnderlyingType(valueType);
                if (underlyingValueType is null)
                {
                    // Case: The type of the property is non-nullable.
                    throw new InvalidOperationException(
                        $"The type '{valueType.Name}' of property '{valueName}' of document object type '{propertyInfo.DeclaringType!.Name}' must be nullable.");
                }
                else
                {
                    // Case: The type of the property is nullable.
                    //ValueStyleInternal |= ValueStyleInternal.Nullable;
                    //_ = typeof(int);
                }
            }
#endif
            // All nullable value types like Int32, Color, or Unit and all enums.
            if (valueType.IsValueType)
                return new ValueTypeDescriptor(valueName, Nullable.GetUnderlyingType(valueType)!, propertyInfo, flags);

            // Types derived from DocumentObjectCollection.
            if (typeof(DocumentObjectCollection).IsAssignableFrom(valueType))
                return new DocumentObjectCollectionDescriptor(valueName, valueType, propertyInfo, flags);

            // Types derived from DocumentObject.
            if (typeof(DocumentObject).IsAssignableFrom(valueType))
                return new DocumentObjectDescriptor(valueName, valueType, propertyInfo, flags);

            // All other reference type like String.
            return new ReferenceTypeDescriptor(valueName, valueType, propertyInfo, flags);
        }

        /// <summary>
        /// Gets a value indicating whether the value is 'reference only'.
        /// A 'reference only' value does not contribute to the state of the object that contains it,
        /// i.e. it does not e.g. taken into account on a IsNull check of the document object.
        /// </summary>
        public bool IsRefOnly => (_flags & VDFlags.RefOnly) == VDFlags.RefOnly;

        /// <summary>
        /// Gets a value indicating whether the value must not be set using SetValue.
        /// </summary>
        public bool IsReadOnly => (_flags & VDFlags.ReadOnly) == VDFlags.ReadOnly;

        /// <summary>
        /// The property information of the described value.
        /// </summary>
        public PropertyInfo PropertyInfo;

        /// <summary>
        /// The name of the value.
        /// </summary>
        public readonly string ValueName;

        /// <summary>
        /// The type of the described value, e.g. typeof(Int32) for an int?.
        /// </summary>
        public readonly Type ValueType;

        /// <summary>
        /// Ensures the get value flags are defined in the enum type.
        /// </summary>
        protected void EnsureGetValueFlags(GV flags)
        {
            if (!Enum.IsDefined(typeof(GV), flags))
                throw new InvalidEnumArgumentException(DomSR.InvalidEnumValue(flags));
        }

        /// <summary>
        /// Ensures the value can be written.
        /// </summary>
        protected void EnsureCanWrite()
        {
            if (IsReadOnly || PropertyInfo.CanWrite is false)
                throw new InvalidOperationException($"Value '{ValueName}' cannot be written.");
        }

        /// <summary>
        /// The flags of the described property, e.g. RefOnly.
        /// </summary>
        readonly VDFlags _flags;
    }
}

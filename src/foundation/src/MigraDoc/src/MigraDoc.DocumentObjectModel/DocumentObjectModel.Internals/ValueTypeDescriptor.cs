// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Reflection;

namespace MigraDoc.DocumentObjectModel.Internals
{
    /// <summary>
    /// Value type descriptor of all kinds of value types.
    /// </summary>
    class ValueTypeDescriptor : ValueDescriptor
    {
        internal ValueTypeDescriptor(string valueName, Type valueType, PropertyInfo propertyInfo, VDFlags flags)
            : base(valueName, valueType, propertyInfo, flags)
        { }

        public override object? GetValue(DocumentObject dom, GV flags)
        {
            EnsureGetValueFlags(flags);

            var value = PropertyInfo.GetValue(dom.BaseValues);
            if (flags == GV.ReadWrite)
            {
                if (value is null)
                {
                    value = CreateValue();
                    PropertyInfo.SetValue(dom.BaseValues, value);
                }
            }
            else
            {
                if (flags == GV.GetNull && value is INullableValue { IsNull: true })
                    return null;
            }
            return value;
        }

        public override void SetValue(DocumentObject dom, object? value)
        {
            EnsureCanWrite();
            PropertyInfo.SetValue(dom.BaseValues, value);
        }

        public override void SetNull(DocumentObject doc)
        {
            EnsureCanWrite();

            // All properties are nullable value types or nullable reference types.
            PropertyInfo.SetValue(doc.BaseValues, null);
        }

        /// <summary>
        /// Determines whether the given DocumentObject is null (not set).
        /// </summary>
        public override bool IsNull(DocumentObject dom)
        {
            var value = PropertyInfo.GetValue(dom.BaseValues);

            if (value is INullableValue nullableValue)
                return nullableValue.IsNull;

            return value is null;
        }
    }
}

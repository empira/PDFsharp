// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Reflection;

namespace MigraDoc.DocumentObjectModel.Internals
{
    /// <summary>
    /// Reference type descriptor of value types.
    /// </summary>
    class ReferenceTypeDescriptor : ValueDescriptor
    {
        internal ReferenceTypeDescriptor(string valueName, Type valueType, PropertyInfo propertyInfo, VDFlags flags)
            : base(valueName, valueType, propertyInfo, flags)
        {
            // Only String and Object should come here.
            // Checked in unit tests.
        }

        public override object? GetValue(DocumentObject dom, GV flags)
        {
            EnsureGetValueFlags(flags);

            var value = PropertyInfo.GetValue(dom.BaseValues);

            // Not yet used. We have only String and Object.
            //if (value is INullableValue { IsNull: true } && flags == GV.GetNull)
            //    return null;

            return value;
        }

        public override void SetValue(DocumentObject dom, object? value)
        {
            EnsureCanWrite();

            PropertyInfo.SetValue(dom.BaseValues, value);
        }

        public override void SetNull(DocumentObject dom)
        {
            EnsureCanWrite();

            // All properties are nullable value types.
            PropertyInfo.SetValue(dom.BaseValues, null);
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

// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Reflection;

namespace MigraDoc.DocumentObjectModel.Internals
{
    /// <summary>
    /// Value descriptor of DocumentObjectCollection.
    /// </summary>
    class DocumentObjectCollectionDescriptor : ValueDescriptor
    {
        internal DocumentObjectCollectionDescriptor(string valueName, Type valueType, PropertyInfo propertyInfo, VDFlags flags)
            : base(valueName, valueType, propertyInfo, flags)
        { }

        public override object? GetValue(DocumentObject dom, GV flags)
        {
            EnsureGetValueFlags(flags);

            var value = (DocumentObjectCollection?)PropertyInfo.GetValue(dom.BaseValues);
            if (flags == GV.ReadWrite)
            {
                if (value is null)
                {
                    value = (DocumentObjectCollection)CreateValue();
                    value.Parent = dom;
                    PropertyInfo.SetValue(dom.BaseValues, value);
                }
            }
            else
            {
                if (value is not null && flags == GV.GetNull && value.IsNull())
                    return null;
            }
            return value;
        }

        public override void SetValue(DocumentObject dom, object? val)
        {
            EnsureCanWrite();

            PropertyInfo.SetValue(dom.BaseValues, val);
        }

        public override void SetNull(DocumentObject dom)
        {
            EnsureCanWrite();

            var value = (DocumentObjectCollection?)PropertyInfo.GetValue(dom.BaseValues);
            value?.SetNull();
        }

        /// <summary>
        /// Determines whether the given DocumentObject is null (not set).
        /// </summary>
        public override bool IsNull(DocumentObject dom)
        {
            var value = (DocumentObjectCollection?)PropertyInfo.GetValue(dom.BaseValues);
            if (value == null)
                return true;
            return value.IsNull();
        }
    }
}

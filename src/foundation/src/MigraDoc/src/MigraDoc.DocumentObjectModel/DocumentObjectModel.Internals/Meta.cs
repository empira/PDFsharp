// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Reflection;

namespace MigraDoc.DocumentObjectModel.Internals
{
    /// <summary>
    /// Metaclass for document objects.
    /// </summary>
    public sealed class Meta
    {
        /// <summary>
        /// Initializes a new instance of the Meta class.
        /// </summary>
        public Meta(Type documentObjectType)
        {
            AddValueDescriptors(this, documentObjectType);
        }

        /// <summary>
        /// Gets the metaobject of the specified document object.
        /// </summary>
        /// <param name="documentObject">The document object the meta is returned for.</param>
        public static Meta GetMeta(DocumentObject documentObject) => documentObject.Meta;

        /// <summary>
        /// Gets the object specified by name from dom.
        /// </summary>
        public object? GetValue(DocumentObject dom, string name, GV flags)
        {
            int dot = name.IndexOf('.');
            if (dot == 0)
                throw new ArgumentException(MdDomMsgs.InvalidValueName(name).Message);
            string? trail = null;
            if (dot > 0)
            {
                trail = name[(dot + 1)..];
                name = name[..dot];
            }

            var vd = ValueDescriptors[name];
            if (vd == null)
                throw new ArgumentException(MdDomMsgs.InvalidValueName(name).Message);

            var value = vd.GetValue(dom, flags);
            if (value == null && flags == GV.GetNull)  //??? also for GV.ReadOnly?
                return null;

            if (trail != null)
            {
                if (value == null || trail == "")
                    throw new ArgumentException(MdDomMsgs.InvalidValueName(name).Message);
                if (value is not DocumentObject doc)
                    throw new ArgumentException(MdDomMsgs.InvalidValueName(name).Message);
                value = doc.GetValue(trail, flags);
            }
            return value;
        }

        /// <summary>
        /// Sets the member of dom specified by name to val.
        /// If a member with the specified name does not exist an ArgumentException will be thrown.
        /// </summary>
        public void SetValue(DocumentObject dom, string name, object? val)
        {
            int dot = name.IndexOf('.');
            if (dot == 0)
                throw new ArgumentException(MdDomMsgs.InvalidValueName(name).Message);
            string? trail = null;
            if (dot > 0)
            {
                trail = name[(dot + 1)..];
                name = name[..dot];
            }
            var vd = ValueDescriptors[name];
            if (vd == null)
                throw new ArgumentException(MdDomMsgs.InvalidValueName(name).Message);

            if (trail != null)
            {
                //REVIEW DaSt: dom.GetValue(name) and call SetValue recursively,
                //             or dom.GetValue(name.BisVorletzteElement) and then call SetValue?
                var doc = (DocumentObject?)dom.GetValue(name);
                if (doc == null)
                    throw new InvalidOperationException($"No value named '{name}' exists.");
                doc.SetValue(trail, val);
            }
            else
                vd.SetValue(dom, val);
        }

        /// <summary>
        /// Determines whether this meta contains a value with the specified name.
        /// </summary>
        public bool HasValue(string name)
        {
            // BUG_OLD: HasValue("a.b") not handled
            if (name.Contains('.'))
                throw new NotImplementedException($"'{name}' contains a dot.");

            return ValueDescriptors.HasName(name);
        }

        /// <summary>
        /// Sets the member of dom specified by name to null.
        /// If a member with the specified name does not exist an ArgumentException will be thrown.
        /// </summary>
        public void SetNull(DocumentObject dom, string name)
        {
            if (name.Contains('.'))
                throw new NotImplementedException($"'{name}' contains a dot.");

            var vd = ValueDescriptors[name];
            if (vd == null)
                throw new ArgumentException(MdDomMsgs.InvalidValueName(name).Message);

            vd.SetNull(dom);
        }

        /// <summary>
        /// Determines whether the member of dom specified by name is null.
        /// If a member with the specified name does not exist an ArgumentException will be thrown.
        /// </summary>
        public /* not virtual */ bool IsNull(DocumentObject dom, string name)
        {
            if (String.IsNullOrEmpty(name) /*|| name == "this"*/)
                return IsNull(dom);

            int dot = name.IndexOf('.');
            if (dot == 0)
                throw new ArgumentException(MdDomMsgs.InvalidValueName(name).Message);
            string? trail = null;
            if (dot > 0)
            {
                trail = name[(dot + 1)..];
                name = name[..dot];
            }

            var vd = ValueDescriptors[name];
            if (vd == null)
                throw new ArgumentException(MdDomMsgs.InvalidValueName(name).Message);

#if true
            if (trail == null)
                return vd.IsNull(dom);

            var value = vd.GetValue(dom, GV.GetNull);
            if (value == null)
                return true;

            if (/*value == null || */trail == "")
                throw new ArgumentException(MdDomMsgs.InvalidValueName(name).Message);
            if (value is not DocumentObject doc)
                throw new ArgumentException(MdDomMsgs.InvalidValueName(name).Message);

            return doc.IsNull(trail);
#else
            if (/*vd is NullableDescriptor ||*/ vd is ValueTypeDescriptor)
            {
                if (trail != null)
                    throw new ArgumentException(DomSR.InvalidValueName(name));
                return vd.IsNull(dom);
            }
            var docObj = (DocumentObject?)vd.GetValue(dom, GV.ReadOnly);
            if (docObj == null)
                return true;
            if (trail != null)
                return docObj.IsNull(trail);
            return docObj.IsNull();
#endif

            //      DomValueDescriptor vd = vds[name];
            //      if (vd == null)
            //        throw new ArgumentException(DomSR.InvalidValueName(name));
            //      
            //      return vd.IsNull(dom);
        }

        /// <summary>
        /// Sets all members of the specified dom to null.
        /// </summary>
        public /* must not be virtual */ void SetNull(DocumentObject dom)
        {
            int count = ValueDescriptors.Count;
            for (int index = 0; index < count; index++)
            {

                var vd = ValueDescriptors[index];
                if (vd.ValueName is "Owner" or "Tag" or "Document" or "Section")
                    continue;

                if (vd.IsRefOnly is false)
                {
                    //ValueDescriptors[index].SetNull(dom);
                    vd.SetNull(dom);
                }
            }
        }

        /// <summary>
        /// Determines whether all members of the specified dom are null. If dom contains no members IsNull
        /// returns true.
        /// </summary>
        public bool IsNull(DocumentObject dom)
        {
            int count = ValueDescriptors.Count;
            for (int index = 0; index < count; index++)
            {
                var vd = ValueDescriptors[index];
                if (vd.IsRefOnly)
                    continue;
                if (!vd.IsNull(dom))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Gets the DomValueDescriptor of the member specified by name from the DocumentObject.
        /// </summary>
        public ValueDescriptor this[string name] => ValueDescriptors[name];

        /// <summary>
        /// Determines whether this metaobject contains a value descriptor with the specified name.
        /// </summary>
        public bool HasName(string name)
            => ValueDescriptors.HasName(name);

        /// <summary>
        /// Gets the DomValueDescriptorCollection of the DocumentObject.
        /// </summary>
        public ValueDescriptorCollection ValueDescriptors { get; } = new();

        /// <summary>
        /// Adds a value descriptor for each field and property found in type to meta.
        /// </summary>
        static void AddValueDescriptors(Meta meta, Type type)
        {
            if (type.IsSubclassOf(typeof(DocumentObject)) is false)
                throw new ArgumentException($"'{type.Name}' is not derived from 'DocumentObject'.");

            var valuesType = type.GetNestedType(type.Name + "Values");
            if (valuesType == null)
                throw new ArgumentException($"'{type.Name}' does not contain a nested type '{type.Name}Values'.");

            var propInfos = valuesType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (var propInfo in propInfos)
            {
                DVAttribute? attr = null;
                var dvs = (DVAttribute[])propInfo.GetCustomAttributes(typeof(DVAttribute), false);
                if (dvs.Length == 1)
                    attr = dvs[0];

                var vd = ValueDescriptor.CreateValueDescriptor(propInfo, attr);
                meta.ValueDescriptors.Add(vd);
            }
        }
    }
}

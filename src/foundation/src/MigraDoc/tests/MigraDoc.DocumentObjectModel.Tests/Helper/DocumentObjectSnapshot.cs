using System.Collections.ObjectModel;
using FluentAssertions;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;

namespace MigraDoc.DocumentObjectModel.Tests.Helper
{
    class DocumentObjectSnapshot
    {
        // This is the list with elements and format objects, that may usually be used for auto creation (see DefaultObjectsForAutoCreation).
        static readonly ReadOnlyDictionary<Type, List<String>> UsualObjectsForAutoCreation = new(new Dictionary<Type, List<String>>
        {
            { typeof(Document), new List<String> { nameof(Document.Sections), nameof(Document.Styles) } },
            { typeof(Style), new List<String> { nameof(Style.ParagraphFormat) } },
            { typeof(ParagraphFormat), new List<String> { nameof(ParagraphFormat.Font) } },
            { typeof(Section), new List<String> { nameof(Section.Headers), nameof(Section.Footers) } },
            { typeof(HeaderFooter), new List<String> { nameof(HeaderFooter.Elements) } },
            { typeof(Paragraph), new List<String> { nameof(Paragraph.Elements), nameof(Paragraph.Format) } },
            { typeof(FormattedText), new List<String> { nameof(FormattedText.Elements), nameof(FormattedText.Font) } },
            { typeof(TextFrame), new List<String> { nameof(TextFrame.Elements) } },
            { typeof(Hyperlink), new List<String> { nameof(Hyperlink.Elements), nameof(Hyperlink.Font) } },
            { typeof(Table), new List<String> { nameof(Table.Rows), nameof(Table.Format) } },
            { typeof(Row), new List<String> { nameof(Row.Cells), nameof(Row.Format) } },
            { typeof(Cell), new List<String> { nameof(Cell.Elements), nameof(Cell.Format) } },
        });
        
        /// <summary>
        /// Elements objects like Document.Sections, Section.Headers and Table.Rows and format objects like Paragraph.Format are automatically created if required as soon as the corresponding property is called.
        /// New DocumentObjectSnapshot() usually evaluates the properties of the Values objects and this way doesn't create the elements objects, while the method to be tested may call the properties directly and this way create the objects.
        /// Snapshots with a missing elements object and an empty one on the other hand can't compare them, as the code cannot recognize if there is simply an empty elements object created or if there are other accidental changes.
        /// As for the document there should be no difference between e. g. a missing and an empty Sections object, we can enforce to create these objects by setting objectsForAutoCreation for the snapshot.
        /// This way the elements objects are called directly on the DocumentObject (and not via the Values object) and are therefore created while taking the snapshot.
        /// DefaultObjectsForAutoCreation contains the objects used for auto creation, if objectsForAutoCreation is not set explicit for the snapshot.
        /// </summary>
#if true
        public static readonly Dictionary<Type, List<String>>? DefaultObjectsForAutoCreation = new (UsualObjectsForAutoCreation);
#else
        public static readonly Dictionary<Type, List<String>>? DefaultObjectsForAutoCreation = null;
#endif

        // This properties are used for test debugging and have public getters to allow reading in further tests.
        // ReSharper disable MemberCanBePrivate.Local
        // ReSharper disable UnusedAutoPropertyAccessor.Local
        public object? Object { get; }

        public string? Name { get; }

        public int? Index { get; }

        public Type? Type { get; }

        public bool IsNull { get; }

        public bool IsDocumentObjectType { get; }

        public string PathToObject { get; }

        public DocumentObjectSnapshot? Parent { get; }

        public bool ExcludeFromComparison { get; }

        public bool DocumentObjectAlreadyProcessed { get; }

        public DocumentObjectSnapshot? FirstSnapshotForThisObject { get; }

        public Dictionary<String, DocumentObjectSnapshot> Values { get; } = new();

        public List<DocumentObjectSnapshot> Children { get; } = new();

        public Dictionary<DocumentObject, DocumentObjectSnapshot> ProcessedDocumentObjects { get; }

        public Dictionary<Type, List<String>>? ObjectsForAutoCreation { get; }

        public Dictionary<Type, List<String>> PropertiesExcludedFromComparison { get; }
        // ReSharper restore UnusedAutoPropertyAccessor.Local
        // ReSharper restore MemberCanBePrivate.Local

        /// <summary>
        /// Creates a recursive snapshot of obj with all its values and children.
        /// </summary>
        /// <param name="docObj">The DocumentObject to start the snapshot from.</param>
        /// <param name="useDefaultObjectsForAutoCreation">If true, DefaultObjectsForAutoCreation is used to determine which properties of which types shall be auto created while taking the snapshot (see DefaultObjectsForAutoCreation).
        /// If false, no uto creation is done.</param>
        /// <param name="propertiesExcludedFromComparison">A dictionary of Types and its properties, that shall not be compared. To improve comparability, use objectsForAutoCreation instead.</param>
        public DocumentObjectSnapshot(DocumentObject docObj, bool useDefaultObjectsForAutoCreation = true, Dictionary<Type, List<String>>? propertiesExcludedFromComparison = null)
            : this(docObj, null, null, null, null, new Dictionary<DocumentObject, DocumentObjectSnapshot>(), 
                useDefaultObjectsForAutoCreation ? DefaultObjectsForAutoCreation : null, 
                propertiesExcludedFromComparison ?? new Dictionary<Type, List<String>>())
        { }

        /// <summary>
        /// Creates a recursive snapshot of obj with all its values and children.
        /// </summary>
        /// <param name="docObj">The DocumentObject to start the snapshot from.</param>
        /// <param name="objectsForAutoCreation">A dictionary of types and its properties, that shall be auto created while taking the snapshot (see DefaultObjectsForAutoCreation).</param>
        /// <param name="propertiesExcludedFromComparison">A dictionary of types and its properties, that shall not be compared. To improve comparability, use objectsForAutoCreation instead.</param>
        public DocumentObjectSnapshot(DocumentObject docObj, Dictionary<Type, List<String>> objectsForAutoCreation, Dictionary<Type, List<String>>? propertiesExcludedFromComparison = null)
            : this(docObj, null, null, null, null, new Dictionary<DocumentObject, DocumentObjectSnapshot>(), 
                objectsForAutoCreation, 
                propertiesExcludedFromComparison ?? new Dictionary<Type, List<String>>())
        { }

        DocumentObjectSnapshot(object? obj, DocumentObjectSnapshot? parent, string? name, int? index, Type? type,
            Dictionary<DocumentObject, DocumentObjectSnapshot> processedDocumentObjects, Dictionary<Type, List<String>>? objectsForAutoCreation, Dictionary<Type, List<String>> propertiesExcludedFromComparison)
        {
            // Set properties used by all DocumentObjectSnapshots.
            Object = obj;
            Name = name;
            Index = index;
            Type = obj?.GetType() ?? type;
            PathToObject = AddToPathToObject(parent?.PathToObject, Type, name, index);
            Parent = parent;
            ProcessedDocumentObjects = processedDocumentObjects;
            ObjectsForAutoCreation = objectsForAutoCreation;
            PropertiesExcludedFromComparison = propertiesExcludedFromComparison;

            var parentType = Parent?.Type;
            ExcludeFromComparison = parentType is not null && Name is not null && PropertiesExcludedFromComparison.TryGetValue(parentType, out var propertiesToExclude) && propertiesToExclude.Contains(Name);

            IsNull = obj is null;
            IsDocumentObjectType = Type != null && Type.IsAssignableTo(typeof(DocumentObject));

            if (!IsDocumentObjectType || IsNull)
                return;

            var docObj = (DocumentObject)obj!;
            var alreadyProcessed = ProcessedDocumentObjects.ContainsKey(docObj);
            if (alreadyProcessed)
            {
                // Set properties for DocumentObjectSnapshot representing DocumentObjects that have been already processed. Values and children get references to the first Snapshot for this DocumentObject already existing.
                var firstMatch = ProcessedDocumentObjects[docObj];
                DocumentObjectAlreadyProcessed = true;
                FirstSnapshotForThisObject = firstMatch;
                Values = firstMatch.Values;
                Children = firstMatch.Children;
                return;
            }

            // For DocumentObjectSnapshots representing unknown DocumentObjects process is values and children.
            ProcessedDocumentObjects.Add(docObj, this);

            foreach (var pi in GetPropertyInformation(docObj))
                Values.Add(pi.PropertyName, new DocumentObjectSnapshot(pi.Value, this, pi.PropertyName, null, pi.PropertyType, ProcessedDocumentObjects, ObjectsForAutoCreation, PropertiesExcludedFromComparison));

            if (docObj is DocumentObjectCollection docObjCol)
            {
                for (var i = 0; i < docObjCol.Count; i++)
                {
                    var child = docObjCol[i];

                    Children.Add(new DocumentObjectSnapshot(child, this, null, i, null, ProcessedDocumentObjects, ObjectsForAutoCreation, PropertiesExcludedFromComparison));
                }
            }
        }

        IEnumerable<(String PropertyName, Type PropertyType, Object? Value)> GetPropertyInformation(DocumentObject docObj)
        {
            var type = docObj.GetType();

            var values = docObj.BaseValues;
            values.Should().NotBeNull("DocumentObject must contain a Values object by convention.");
            var valuesType = values.GetType();

            foreach (var valuesPropertyInfo in valuesType.GetProperties())
            {
                var propertyInfo = valuesPropertyInfo;
                var propertyName = propertyInfo.Name;
                object parentObject;

                // See AutoCreateElementsObjects.
                if (ObjectsForAutoCreation is not null && ObjectsForAutoCreation.ContainsKey(type) && ObjectsForAutoCreation[type].Contains(propertyName))
                {
                    parentObject = docObj;

                    var directPropertyInfo = type.GetProperty(propertyName);
                    directPropertyInfo.Should().NotBeNull("DocumentObject must contain a corresponding property with the same name as defined in its Values object by convention.");
                    propertyInfo = directPropertyInfo!;
                }
                else
                    parentObject = values;

                var value = propertyInfo.GetValue(parentObject);
                var propertyType = propertyInfo.PropertyType;
                propertyType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

                yield return (propertyName, propertyType, value);
            }
        }

        static string AddToPathToObject(string? pathToObject, Type? type, int index)
        {
            return AddToPathToObject(pathToObject, type, null, index);
        }

        static string AddToPathToObject(string? pathToObject, Type? type, string? propertyName = null, int? index = null)
        {
            var typeStr = type?.Name ?? "unknown";
            if (index != null)
                propertyName = Invariant($"{propertyName}[{index}]");

            var segment = propertyName == null
                ? $"({typeStr})"
                : propertyName == typeStr
                    ? propertyName
                    : $"{propertyName}({typeStr})";

            if (pathToObject == null)
                return segment;

            return $"{pathToObject} > {segment}";
        }

        public override String ToString()
        {
            var typeStr = Type?.Name ?? "unknown";

            string objString;
            if (IsNull)
                objString = "null";
            else if (IsDocumentObjectType)
                objString = ":DocumentObject";
            else
                objString = Object!.ToString() ?? "ToStringReturnsNull";

            return $"Snapshot({typeStr} {objString})";
        }

        /// <summary>
        /// Compares this snapshot to another. The reference is used as snapshot1, this one is used as snapshot2.
        /// </summary>
        public void CompareTo(DocumentObjectSnapshot reference)
        {
            CompareSnapshots(reference, this);
        }

        /// <summary>
        /// Compares to snapshots.
        /// </summary>
        public static void CompareSnapshots(DocumentObjectSnapshot snapshot1, DocumentObjectSnapshot snapshot2)
        {
            var pathToObject1 = snapshot1.PathToObject;
            var pathToObject2 = snapshot2.PathToObject;
            pathToObject1.Should().Be(pathToObject2, CreateSnapshotEqualityBecauseString(pathToObject1));

            var excludeFromComparison1 = snapshot1.ExcludeFromComparison;
            var excludeFromComparison2 = snapshot2.ExcludeFromComparison;
            excludeFromComparison1.Should().Be(excludeFromComparison2, CreateSnapshotEqualityBecauseString(pathToObject1));

            var isDocumentObjectType1 = snapshot1.IsDocumentObjectType;
            var isDocumentObjectType2 = snapshot2.IsDocumentObjectType;

            var isNull1 = snapshot1.IsNull;
            var isNull2 = snapshot2.IsNull;

            if (!excludeFromComparison1)
            {
                // For non-DocumentObjects check with Equals and return.
                isDocumentObjectType1.Should().Be(isDocumentObjectType2, CreateSnapshotEqualityBecauseString(pathToObject1));
                if (!isDocumentObjectType1)
                {
                    var obj1 = snapshot1.Object;
                    var obj2 = snapshot2.Object;
                    Equals(obj1, obj2).Should().BeTrue(CreateSnapshotEqualityBecauseString(pathToObject1));
                    return;
                }

                // For null return.
                isNull1.Should().Be(isNull2, CreateSnapshotEqualityBecauseString(pathToObject1));
                if (isNull1)
                    return;

                // For DocumentObjects don't check with Equals. Just do recursion for values and children further down... 
            }
            else
            {
                // If this snapshot is excluded from comparison, differing objects are accepted. Don't check for equality here and leave, if due to at least one object a comparison of values and children is not possible.
                if (!isDocumentObjectType1 || !isDocumentObjectType2)
                    return;

                if (isNull1 || isNull2)
                    return;
            }

            // AlreadyProcessed must be equal. If differing, we'll have a reference to an already processed object on the on hand and a copy on the other hand.
            var isAlreadyProcessed1 = snapshot1.DocumentObjectAlreadyProcessed;
            var isAlreadyProcessed2 = snapshot2.DocumentObjectAlreadyProcessed;
            var firstSnapshotForThisObjectPath1 = snapshot1.FirstSnapshotForThisObject?.PathToObject;
            var firstSnapshotForThisObjectPath2 = snapshot2.FirstSnapshotForThisObject?.PathToObject;

            isAlreadyProcessed1.Should().Be(isAlreadyProcessed2, CreateSnapshotEqualityBecauseString(pathToObject1));
            firstSnapshotForThisObjectPath1.Should().Be(firstSnapshotForThisObjectPath2, CreateSnapshotEqualityBecauseString(pathToObject1));
            if (isAlreadyProcessed1)
                return;

            // Recursion for values.
            var values1 = snapshot1.Values;
            var values2 = snapshot2.Values;
            values1.Count.Should().Be(values2.Count, CreateSnapshotEqualityBecauseString(pathToObject1)); // Should never fail as every Values class has its static set of properties.
            foreach (var key in values1.Keys)
            {
                var value1 = values1[key];
                values2.Should().ContainKey(key, CreateSnapshotEqualityBecauseString(pathToObject1)); // Should never fail as every Values class has its static set of properties.
                var value2 = values2[key];

                CompareSnapshots(value1, value2);
            }

            // Recursion for children.
            var children1 = snapshot1.Children;
            var children2 = snapshot2.Children;
            var childrenCount1 = children1.Count;
            var childrenCount2 = children2.Count;
            var maxCount = Math.Max(childrenCount1, childrenCount2);
            for (var i = 0; i < maxCount; i++)
            {
                var hasElement1 = i < childrenCount1;
                var hasElement2 = i < childrenCount2;

                var child1 = hasElement1 ? children1[i] : null;
                var child2 = hasElement2 ? children2[i] : null;

                var type = child1?.Type ?? child2?.Type;

                hasElement1.Should().BeTrue(CreateSnapshotEqualityBecauseString(DocumentObjectSnapshot.AddToPathToObject(pathToObject1, type, i)));
                hasElement2.Should().BeTrue(CreateSnapshotEqualityBecauseString(DocumentObjectSnapshot.AddToPathToObject(pathToObject2, type, i)));

                CompareSnapshots(child1!, child2!);
            }
        }

        static string CreateSnapshotEqualityBecauseString(string pathToObject)
        {
            return "mandatory snapshot equality\n" +
                   $"[ Check at {pathToObject} ]\n";
        }
    }
}

// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Reflection;

namespace MigraDoc.Rendering.UnitTest
{
    /// <summary>
    /// Summary description for ValueDumper.
    /// </summary>
    class ValueDumper
    {
        internal static string DumpValues(object obj)
        {
            string dumpString = "[" + obj.GetType() + "]\r\n";
            foreach (FieldInfo fieldInfo in obj.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (fieldInfo.FieldType.IsValueType)
                {
                    dumpString += "  " + fieldInfo.Name + " = " + fieldInfo.GetValue(obj) + "\r\n";
                }
            }
            return dumpString;
        }
    }
}

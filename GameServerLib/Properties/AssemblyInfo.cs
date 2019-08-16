using System.Reflection;
using System.Runtime.InteropServices;
using Log4Net.Async.MyNamespace;

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("cb004023-b938-4ec6-b036-29ee2f55cfa4")]
[assembly: ReferencedLibrary] // This forces the compiler to include Log4Net.Async library in the output as it is not directly used in code

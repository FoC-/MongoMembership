using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyProduct("MongoMembership")]

[assembly: AssemblyCompany("")]
[assembly: AssemblyCopyright("Copyright © Mykola Kush 2017")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: InternalsVisibleTo("MongoMembership.Tests")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: AssemblyVersion("0.1.1.0")]
//Change MAJOR version before release
//Change MINOR version when you add functionality in a backwards-compatible manner
//Change PATCH version when you make backwards-compatible bugfixes

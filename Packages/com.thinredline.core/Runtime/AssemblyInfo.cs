using System.Runtime.CompilerServices;

// Allow internal visibility for testing purposes.
// [assembly: InternalsVisibleTo("xxx.xxx.xxx")]

#if UNITY_EDITOR
[assembly: InternalsVisibleTo("ThinRL.Core.Editor")]
[assembly: InternalsVisibleTo("ThinRL.Core.Tests")]
[assembly: InternalsVisibleTo("ThinRL.Core.Editor.Tests")]
#endif

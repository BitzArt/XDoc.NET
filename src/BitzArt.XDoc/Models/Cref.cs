// using JetBrains.Annotations;
//
// namespace BitzArt.XDoc;
//
// /// <summary>
// /// Represents a parsed C# XML documentation code reference (cref) attribute.
// /// Extracts and stores the type and member information from a cref string.
// /// </summary>
// [PublicAPI]
// public record Cref
// {
//     /// <summary>
//     /// Initializes a new instance of the <see cref="Cref"/> record by parsing the provided cref string.
//     /// </summary>
//     /// <param name="cref">The cref string to parse (e.g. "T:Namespace.Type" or "M:Namespace.Type.Method").</param>
//     public Cref(string cref)
//     {
//         Prefix = cref[..2];
//
//         var lastIndexOf = cref.LastIndexOf('.');
//
//         switch (Prefix)
//         {
//             case "T:":
//                 Type = cref.Substring(2, cref.Length - 2);
//                 Member = null;
//                 break;
//             case "M:" or "P:" or "F:":
//                 Type = cref.Substring(2, lastIndexOf - 2);
//                 Member = cref.Substring(lastIndexOf + 1, cref.Length - lastIndexOf - 1);
//                 break;
//             default:
//                 throw new ArgumentException($"Invalid cref: {cref}");
//         }
//
//         var typeLastIndexOf = Type.LastIndexOf('.');
//
//         ShortType = Type.Substring(typeLastIndexOf + 1, Type.Length - typeLastIndexOf - 1);
//     }
//
//     /// <summary>
//     /// Is the cref a type reference (e.g. "T:")?
//     /// </summary>
//     public bool IsType => Prefix is "T:";
//
//     /// <summary>
//     /// Is the cref a member reference (e.g. "M:", "P:", "F:")?
//     /// </summary>
//     public bool IsMember => Prefix is "M:" or "P:" or "F:";
//
//     /// <summary>
//     /// The prefix of the cref (e.g. "T:", "M:", "P:", "F:").
//     /// </summary>
//     public string Prefix { get; init; }
//
//     /// <summary>
//     /// The type name
//     /// </summary>
//     public string Type { get; init; }
//
//     /// <summary>
//     /// The short type name (without namespace)
//     /// </summary>
//     public string ShortType { get; init; }
//
//     /// <summary>
//     /// Method, property, or field name if present in the cref.
//     /// Will be null for type references.
//     /// </summary>
//     public string? Member { get; init; }
//
//     /// <summary>
//     /// Returns a string that represents the current object.
//     /// </summary>
//     public override string ToString() => $"{Prefix}{Type}{(Member != null ? "." + Member : string.Empty)}";
//
//     /// <summary>
//     /// 
//     /// </summary>
//     /// <param name="value"></param>
//     /// <param name="cref"></param>
//     /// <returns></returns>
//     public static bool TryCreate(string? value, out Cref? cref)
//     {
//         try
//         {
//             cref = new Cref(value!);
//         }
//         catch
//         {
//             cref = null;
//         }
//
//         return cref != null;
//     }
// }
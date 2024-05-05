using System.Text.RegularExpressions;

namespace PolyLink.Server.Util;

public partial class RegexHelper
{
    [GeneratedRegex("^[a-z0-9_]{3,16}$")]
    public static partial Regex NameValidator();
}
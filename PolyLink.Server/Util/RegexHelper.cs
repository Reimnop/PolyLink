using System.Text.RegularExpressions;

namespace PolyLink.Server.Util;

public partial class RegexHelper
{
    [GeneratedRegex("Bearer ([a-z0-9]{32})")]
    public static partial Regex TokenParser();
    
    [GeneratedRegex("^[a-z0-9_]{3,16}$")]
    public static partial Regex NameValidator();
}
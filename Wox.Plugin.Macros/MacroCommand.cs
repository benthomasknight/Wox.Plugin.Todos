using System.ComponentModel;

namespace Wox.Plugin.Macros
{
    public enum MacroCommand
    {
        [Description("List")]
        L,
        [Description("Add")]
        A,
        [Description("Remove")]
        R,
        [Description("Help")]
        H,
        [Description("Reload")]
        Rl
    }
}

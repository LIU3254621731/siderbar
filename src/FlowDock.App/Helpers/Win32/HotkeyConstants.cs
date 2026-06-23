namespace FlowDock.App.Helpers.Win32;

public static class HotkeyConstants
{
    // Modifiers
    public const uint MOD_ALT = 0x0001;
    public const uint MOD_CONTROL = 0x0002;
    public const uint MOD_SHIFT = 0x0004;
    public const uint MOD_WIN = 0x0008;
    public const uint MOD_NOREPEAT = 0x4000;

    // Common virtual key codes
    public const uint VK_D = 0x44;
    public const uint VK_E = 0x45;
    public const uint VK_F = 0x46;
    public const uint VK_Q = 0x51;
    public const uint VK_S = 0x53;
    public const uint VK_W = 0x57;
    public const uint VK_SPACE = 0x20;
    public const uint VK_TAB = 0x09;
    public const uint VK_F1 = 0x70;
    public const uint VK_F2 = 0x71;

    // Default hotkey ID
    public const int DOCK_HOTKEY_ID = 9000;

    public static uint ParseModifiers(string modifiers)
    {
        uint code = 0;
        if (modifiers.Contains("Control", StringComparison.OrdinalIgnoreCase)) code |= MOD_CONTROL;
        if (modifiers.Contains("Alt", StringComparison.OrdinalIgnoreCase)) code |= MOD_ALT;
        if (modifiers.Contains("Shift", StringComparison.OrdinalIgnoreCase)) code |= MOD_SHIFT;
        if (modifiers.Contains("Win", StringComparison.OrdinalIgnoreCase)) code |= MOD_WIN;
        return code | MOD_NOREPEAT;
    }

    public static Dictionary<string, uint> KeyNameToVk { get; } = new()
    {
        ["A"] = 0x41, ["B"] = 0x42, ["C"] = 0x43, ["D"] = 0x44, ["E"] = 0x45,
        ["F"] = 0x46, ["G"] = 0x47, ["H"] = 0x48, ["I"] = 0x49, ["J"] = 0x4A,
        ["K"] = 0x4B, ["L"] = 0x4C, ["M"] = 0x4D, ["N"] = 0x4E, ["O"] = 0x4F,
        ["P"] = 0x50, ["Q"] = 0x51, ["R"] = 0x52, ["S"] = 0x53, ["T"] = 0x54,
        ["U"] = 0x55, ["V"] = 0x56, ["W"] = 0x57, ["X"] = 0x58, ["Y"] = 0x59,
        ["Z"] = 0x5A,
        ["0"] = 0x30, ["1"] = 0x31, ["2"] = 0x32, ["3"] = 0x33, ["4"] = 0x34,
        ["5"] = 0x35, ["6"] = 0x36, ["7"] = 0x37, ["8"] = 0x38, ["9"] = 0x39,
        ["F1"] = 0x70, ["F2"] = 0x71, ["F3"] = 0x72, ["F4"] = 0x73,
        ["F5"] = 0x74, ["F6"] = 0x75, ["F7"] = 0x76, ["F8"] = 0x77,
        ["F9"] = 0x78, ["F10"] = 0x79, ["F11"] = 0x7A, ["F12"] = 0x7B,
        ["Space"] = 0x20, ["Tab"] = 0x09,
    };
}

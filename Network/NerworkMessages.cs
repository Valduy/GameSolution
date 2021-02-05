using System;

namespace Network
{
    /// <summary>
    /// Типы сообщений.
    /// </summary>
    public enum NetworkMessages : uint
    {
        Hello   = 0x000000ff,
        Connect = 0x0000ff00,
        Info    = 0x00ff0000,
        Bye     = 0xff000000,
    }
}

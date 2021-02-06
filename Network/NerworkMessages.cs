using System;

namespace Network
{
    /// <summary>
    /// Типы сообщений.
    /// </summary>
    public enum NetworkMessages : uint
    {
        Hello   = 0x00000000,
        Connect = 0x0000000f,
        Host    = 0x000000f0,
        Client  = 0x00000f00,
        Info    = 0x0000f000,
        Bye     = 0x000f0000,
    }
}

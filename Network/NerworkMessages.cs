using System;

namespace Network
{
    /// <summary>
    /// Типы сообщений.
    /// </summary>
    public enum NetworkMessages : uint
    {
        Hello = 0x000000ff,
        Info  = 0x0000ff00,
        Bye   = 0x00ff0000,
    }
}

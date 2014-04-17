using System;

namespace TS3QueryLib.Core.Server.Entities
{
    [Flags]
    public enum ServerNotifyRegisterEvent
    {
        Server,
        Channel,
        TextServer,
        TextChannel,
        TextPrivate,
		TokenUsed
    }
}
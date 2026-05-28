using System;
using System.Collections.Generic;
using System.Text;

namespace Headquartz.Shared.Networking;

public class CommandPacket
{
    public string CommandType { get; set; } =
        "";

    public string JsonPayload { get; set; } =
        "";

    public string PlayerId { get; set; } =
        "";
}
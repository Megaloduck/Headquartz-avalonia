using System;
using System.Collections.Generic;
using System.Text;

namespace Headquartz.App.Models;

public class MessageModel
{
    public string SenderRole { get; set; } = "";
    public string SenderEmoji { get; set; } = "";
    public string Text { get; set; } = "";
    public string Timestamp { get; set; } = "";
    public bool IsSystem { get; set; }
}

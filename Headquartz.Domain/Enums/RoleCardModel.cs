using System;
using System.Collections.Generic;
using System.Text;

using Headquartz.Domain.Enums;

namespace Headquartz.App.Models;

public class RoleCardModel
{
    public PlayerRole Role { get; set; }

    public string Title { get; set; } = "";

    public string Department { get; set; } = "";

    public string Description { get; set; } = "";

    public string Emoji { get; set; } = "";

    public string AccentColor { get; set; } = "";
}
using System;
using System.Collections.Generic;
using System.Text;

using Headquartz.Domain.Enums;

namespace Headquartz.Domain.Entities;

public class Player
{
    public Guid Id { get; set; }

    public string Username { get; set; } = "";

    public PlayerRole Role { get; set; }

    public bool IsConnected { get; set; }

    public DateTime JoinedAt { get; set; }
}

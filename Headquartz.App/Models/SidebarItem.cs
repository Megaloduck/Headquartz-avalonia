using System;
using System.Collections.Generic;
using System.Text;

namespace Headquartz.App.Models;

public class SidebarItem
{
    public string Title { get; set; } = "";

    public string Icon { get; set; } = "";

    public string Route { get; set; } = "";

    public bool IsVisible { get; set; } = true;


}
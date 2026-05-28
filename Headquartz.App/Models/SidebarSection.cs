using System;
using System.Collections.Generic;
using System.Text;

namespace Headquartz.App.Models;

public class SidebarSection
{
    public string Title { get; set; } = "";

    public List<SidebarItem> Items { get; set; } = [];
}

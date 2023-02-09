﻿using ECommons.DalamudServices;
using System.Collections.Generic;
using static Dalamud.Game.Command.CommandInfo;

namespace ECommons;

public static class EzCmd
{
    internal static List<string> RegisteredCommands = new();

    public static void Add(string command, HandlerDelegate action, string helpMessage = null)
    {
        RegisteredCommands.Add(command);
        Svc.Commands.AddHandler(command, new(action)
        {
            HelpMessage = helpMessage ?? ""
        });
    }
}

﻿using ClickLib.Clicks;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ECommons.GenericHelpers;

namespace Saucy.CuffACur
{
    public class CufModule
    {
        public static Inputs Inputs { get; set; } = new Inputs();

        public static bool DisableInput = false;

        public static bool ModuleEnabled = false;

        public unsafe static void RunModule()
        {
            DisableInput = true;
            var prizeMenu = Svc.GameGui.GetAddonByName("GoldSaucerReward", 1);
            var addon = Svc.GameGui.GetAddonByName("PunchingMachine", 1);

            if (ECommons.GenericHelpers.TryGetAddonByName<AddonSelectString>("SelectString", out var startMenu) && startMenu->AtkUnitBase.IsVisible)
            {
                try
                {
                    ClickSelectString.Using((IntPtr)startMenu).SelectItem1();
                }
                catch
                {

                }
            }

            try
            {
                if (addon != IntPtr.Zero)
                {
                    var ui = (AtkUnitBase*)addon;

                    if (ui->IsVisible)
                    {
                        var slidingNode = ui->UldManager.NodeList[18];
                        var button = (IntPtr)ui->UldManager.NodeList[10];

                        if (slidingNode->Width >= 210 && slidingNode->Width <= 240)
                        {
                            Inputs.SimulatePress(Dalamud.Game.ClientState.Keys.VirtualKey.NUMPAD0);
                        }

                    }
                }

                GameObject* cuf = (GameObject*)Svc.Objects.Where(x => x.DataId == 2005029 || x.DataId == 197370).OrderBy(x => x.YalmDistanceX).FirstOrDefault()?.Address;
                if ((IntPtr)cuf == IntPtr.Zero)
                    return;

                TargetSystem* tg = TargetSystem.Instance();
                tg->InteractWithObject(cuf);
            }
            catch(Exception e) 
            { 
            
            }
        }

        private static bool TryGetAddonByName<T>(string v, out object startMenu)
        {
            throw new NotImplementedException();
        }
    }
}

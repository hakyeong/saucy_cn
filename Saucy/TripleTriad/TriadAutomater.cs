﻿using ClickLib.Enums;
using ClickLib.Structures;
using Dalamud.Hooking;
using Dalamud.Memory;
using Dalamud.Utility;
using ECommons.Automation;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using static ECommons.GenericHelpers;
using ValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;

namespace Saucy.TripleTriad
{
    internal static unsafe class TriadAutomater
    {

        public delegate int PlaceCardDelegate(IntPtr addon);
        public static Hook<PlaceCardDelegate> PlaceCardHook;
        public static bool ModuleEnabled = false;
        public static Dictionary<uint, int> TempCardsWonList = new Dictionary<uint, int>();

        public static bool PlayXTimes = false;
        public static bool PlayUntilCardDrops = false;
        public static int NumberOfTimes = 1;
        public static bool LogOutAfterCompletion = false;
        public static bool PlayUntilAllCardsDropOnce = false;

        public static int PlaceCardDetour(IntPtr a1)
        {
            return PlaceCardHook.Original(a1);
        }
        public static void PlaceCard(int which, int slot)
        {
            try
            {
                if (TryGetAddonByName<AtkUnitBase>("TripleTriad", out var addon))
                {
                    var values = stackalloc AtkValue[2];
                    values[0] = new()
                    {
                        Type = FFXIVClientStructs.FFXIV.Component.GUI.ValueType.Int,
                        Int = 14,
                    };
                    values[1] = new()
                    {
                        Type = FFXIVClientStructs.FFXIV.Component.GUI.ValueType.UInt,
                        UInt = (uint)slot + ((uint)which << 16),
                    };
                    addon->FireCallback(2, values);

                    PlaceCardHook ??= Hook<PlaceCardDelegate>.FromAddress(Svc.SigScanner.ScanText("40 56 48 83 EC 20 48 8B F1 E8 ?? ?? ?? ?? 83 BE"), PlaceCardDetour);
                    PlaceCardHook.Original((IntPtr)addon);
                }
            }
            catch
            {

            }
        }

        public static unsafe void ClickButton(AtkUnitBase* window, AtkComponentButton* target, uint which, EventType type = EventType.CHANGE)
    => ClickAddonComponent(window, target->AtkComponentBase.OwnerNode, which, type);

        public static unsafe void ClickAddonCheckBox(AtkUnitBase* window, AtkComponentCheckBox* target, uint which, EventType type = EventType.CHANGE)
             => ClickAddonComponent(window, target->AtkComponentButton.AtkComponentBase.OwnerNode, which, type);


        public static unsafe void ClickAddonComponent(AtkUnitBase* UnitBase, AtkComponentNode* target, uint which, EventType type, EventData? eventData = null, InputData? inputData = null)
        {
            eventData ??= EventData.ForNormalTarget(target, UnitBase);
            inputData ??= InputData.Empty();

            InvokeReceiveEvent(&UnitBase->AtkEventListener, type, which, eventData, inputData);
        }

        /// <summary>
        /// AtkUnitBase receive event delegate.
        /// </summary>
        /// <param name="eventListener">Type receiving the event.</param>
        /// <param name="evt">Event type.</param>
        /// <param name="which">Internal routing number.</param>
        /// <param name="eventData">Event data.</param>
        /// <param name="inputData">Keyboard and mouse data.</param>
        /// <returns>The addon address.</returns>
        internal unsafe delegate IntPtr ReceiveEventDelegate(AtkEventListener* eventListener, EventType evt, uint which, void* eventData, void* inputData);


        /// <summary>
        /// Invoke the receive event delegate.
        /// </summary>
        /// <param name="eventListener">Type receiving the event.</param>
        /// <param name="type">Event type.</param>
        /// <param name="which">Internal routing number.</param>
        /// <param name="eventData">Event data.</param>
        /// <param name="inputData">Keyboard and mouse data.</param>
        private static unsafe void InvokeReceiveEvent(AtkEventListener* eventListener, EventType type, uint which, EventData eventData, InputData inputData)
        {
            var receiveEvent = GetReceiveEvent(eventListener);
            receiveEvent(eventListener, type, which, eventData.Data, inputData.Data);
        }

        private static unsafe ReceiveEventDelegate GetReceiveEvent(AtkEventListener* listener)
        {
            var receiveEventAddress = new IntPtr(listener->vfunc[2]);
            return Marshal.GetDelegateForFunctionPointer<ReceiveEventDelegate>(receiveEventAddress)!;
        }

        private static unsafe ReceiveEventDelegate GetReceiveEvent(AtkComponentBase* listener)
            => GetReceiveEvent(&listener->AtkEventListener);

        private static unsafe ReceiveEventDelegate GetReceiveEvent(AtkUnitBase* listener)
            => GetReceiveEvent(&listener->AtkEventListener);

        public static void RunModule()
        {
            if (Saucy.TTSolver.preGameDecks.Count > 0)
            {
                var selectedDeck = Service.Configuration.SelectedDeckIndex;
                if (selectedDeck >= 0 && !Saucy.TTSolver.preGameDecks.ContainsKey(selectedDeck))
                {
                    Service.Configuration.SelectedDeckIndex = -1;
                }
            }

            if (Saucy.TTSolver.hasMove)
            {
                PlaceCard(Saucy.TTSolver.moveCardIdx, Saucy.TTSolver.moveBoardIdx);
            }

            //Challenge Screen
            {
                try
                {
                    if (TryGetAddonByName<AtkUnitBase>("TripleTriadRequest", out var addon))
                    {
                        var button = (AtkComponentButton*)addon->UldManager.NodeList[4];
                        ClickButton(addon, button, 1);
                    }
                
                }
                catch { }
            }

            //Deck Select
            {
                try
                {
                    if (TryGetAddonByName<AtkUnitBase>("TripleTriadSelDeck", out var addon) && addon->IsVisible && !TryGetAddonByName<AtkUnitBase>("TripleTriad", out var _))
                    {

                        if (Service.Configuration.UseRecommendedDeck || Service.Configuration.SelectedDeckIndex == -1)
                        {
                            var button = (AtkComponentButton*)addon->UldManager.NodeList[3];
                            ClickButton(addon, button, 2);
                        }
                        else
                        {
                            var values = stackalloc AtkValue[1];
                            //Deck Index
                            values[0] = new()
                            {
                                Type = FFXIVClientStructs.FFXIV.Component.GUI.ValueType.Int,
                                Int = Service.Configuration.SelectedDeckIndex,
                            };
                            addon->FireCallback(1, values);
                            addon->Hide(true);
                        }
                    }
                }
                catch
                {

                }
            }
        }

        public static bool Logout()
        {
            var isLoggedIn = Svc.Condition.Any();
            if (!isLoggedIn) return true;

            Chat.Instance.SendMessage("/logout");
            return true;
        }

        public static bool SelectYesLogout()
        {
            var addon = GetSpecificYesno(Svc.Data.GetExcelSheet<Addon>()?.GetRow(115)?.Text.ToDalamudString().ExtractText());
            if (addon == null) return false;
            ClickLib.Clicks.ClickSelectYesNo.Using((nint)addon).Yes();
            //GenerateCallback(addon, 0);
            //addon->Hide(true);
            return true;
        }

        internal static AtkUnitBase* GetSpecificYesno(params string[] s)
        {
            for (int i = 1; i < 100; i++)
            {
                try
                {
                    var addon = (AtkUnitBase*)Svc.GameGui.GetAddonByName("SelectYesno", i);
                    if (addon == null) return null;
                    if (IsAddonReady(addon))
                    {
                        var textNode = addon->UldManager.NodeList[15]->GetAsAtkTextNode();
                        var text = MemoryHelper.ReadSeString(&textNode->NodeText).ExtractText();
                        if (text.EqualsAny(s))
                        {
                            Dalamud.Logging.PluginLog.Verbose($"SelectYesno {s} addon {i}");
                            return addon;
                        }
                    }
                }
                catch (Exception e)
                {
                    e.Log();
                    return null;
                }
            }
            return null;
        }

        public static void GenerateCallback(AtkUnitBase* unitBase, params object[] values)
        {
            var atkValues = CreateAtkValueArray(values);
            if (atkValues == null) return;
            try
            {
                unitBase->FireCallback(values.Length, atkValues);
            }
            finally
            {
                for (var i = 0; i < values.Length; i++)
                {
                    if (atkValues[i].Type == FFXIVClientStructs.FFXIV.Component.GUI.ValueType.String)
                    {
                        Marshal.FreeHGlobal(new IntPtr(atkValues[i].String));
                    }
                }
                Marshal.FreeHGlobal(new IntPtr(atkValues));
            }
        }

        public static AtkValue* CreateAtkValueArray(params object[] values)
        {
            var atkValues = (AtkValue*)Marshal.AllocHGlobal(values.Length * sizeof(AtkValue));
            if (atkValues == null) return null;
            try
            {
                for (var i = 0; i < values.Length; i++)
                {
                    var v = values[i];
                    switch (v)
                    {
                        case uint uintValue:
                            atkValues[i].Type = ValueType.UInt;
                            atkValues[i].UInt = uintValue;
                            break;
                        case int intValue:
                            atkValues[i].Type = ValueType.Int;
                            atkValues[i].Int = intValue;
                            break;
                        case float floatValue:
                            atkValues[i].Type = ValueType.Float;
                            atkValues[i].Float = floatValue;
                            break;
                        case bool boolValue:
                            atkValues[i].Type = ValueType.Bool;
                            atkValues[i].Byte = (byte)(boolValue ? 1 : 0);
                            break;
                        case string stringValue:
                            {
                                atkValues[i].Type = ValueType.String;
                                var stringBytes = Encoding.UTF8.GetBytes(stringValue);
                                var stringAlloc = Marshal.AllocHGlobal(stringBytes.Length + 1);
                                Marshal.Copy(stringBytes, 0, stringAlloc, stringBytes.Length);
                                Marshal.WriteByte(stringAlloc, stringBytes.Length, 0);
                                atkValues[i].String = (byte*)stringAlloc;
                                break;
                            }
                        default:
                            throw new ArgumentException($"Unable to convert type {v.GetType()} to AtkValue");
                    }
                }
            }
            catch
            {
                return null;
            }

            return atkValues;
        }
    }
}

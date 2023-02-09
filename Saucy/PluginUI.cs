using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using ECommons;
using ECommons.ImGuiMethods;
using FFTriadBuddy;
using ImGuiNET;
using NAudio.Lame;
using PunishLib.ImGuiMethods;
using Saucy.CuffACur;
using Saucy.TripleTriad;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using TriadBuddyPlugin;

namespace Saucy
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    class PluginUI : IDisposable
    {
        private Configuration configuration;

        // this extra bool exists for ImGui, since you can't ref a property
        private bool visible = false;
        public bool Visible
        {
            get { return visible; }
            set { visible = value; }
        }

        private bool settingsVisible = false;
        private GameNpcInfo currentNPC;

        public bool SettingsVisible
        {
            get { return settingsVisible; }
            set { settingsVisible = value; }
        }

        public GameNpcInfo CurrentNPC
        {
            get => currentNPC;
            set
            {
                if (currentNPC != value)
                {
                    TriadAutomater.TempCardsWonList.Clear();
                    currentNPC = value;
                }
            }
        }

        public PluginUI(Configuration configuration)
        {
            this.configuration = Service.Configuration;
        }

        public void Dispose()
        {
        }

        public bool Enabled { get; set; } = false;

        public void Draw()
        {
            // This is our only draw handler attached to UIBuilder, so it needs to be
            // able to draw any windows we might have open.
            // Each method checks its own visibility/state to ensure it only draws when
            // it actually makes sense.
            // There are other ways to do this, but it is generally best to keep the number of
            // draw delegates as low as possible.

            DrawMainWindow();
        }

        public void DrawMainWindow()
        {
            if (!Visible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(520, 420), ImGuiCond.FirstUseEver);
            //ImGui.SetNextWindowSizeConstraints(new Vector2(520, 420), new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin("Saucy Config", ref visible))
            {
                if (ImGui.BeginTabBar("Games"))
                {
                    if (ImGui.BeginTabItem("重击伽美什"))
                    {
                        DrawCufTab();
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("九宫幻卡"))
                    {
                        DrawTriadTab();
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("Stats"))
                    {
                        DrawStatsTab();
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("About"))
                    {
                        AboutTab.Draw(Saucy.P);
                        ImGui.EndTabItem();
                    }

                    ImGui.EndTabBar();
                }
            }
            ImGui.End();
        }

        private void DrawStatsTab()
        {
            ImGui.Columns(3, "统计数据", false);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText(ImGuiColors.ParsedGold, "SAUCY统计", true);
            ImGui.Columns(1);
            ImGui.BeginChild("TT Stats", new Vector2(0, ImGui.GetContentRegionAvail().Y - 30f), true);
            ImGui.Columns(3, null, false);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText(ImGuiColors.DalamudRed, "九宫幻卡", true);
            ImGuiHelpers.ScaledDummy(10f);
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("游玩次数", true);
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{Service.Configuration.Stats.GamesPlayedWithSaucy}");
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGui.Spacing();
            ImGuiEx.CenterColumnText("胜利", true);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("失败", true);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("平局", true);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{Service.Configuration.Stats.GamesWonWithSaucy}");
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{Service.Configuration.Stats.GamesLostWithSaucy}");
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{Service.Configuration.Stats.GamesDrawnWithSaucy}");
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("胜率", true);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("赢得的卡", true);
            ImGui.NextColumn();
            if (Service.Configuration.Stats.NPCsPlayed.Count > 0)
            {
                ImGuiEx.CenterColumnText("最受欢迎的NPC", true);
                ImGui.NextColumn();
            }
            else
            {
                ImGui.NextColumn();
            }

            if (Service.Configuration.Stats.GamesPlayedWithSaucy > 0)
            {
                ImGuiEx.CenterColumnText($"{Math.Round(((double)Service.Configuration.Stats.GamesWonWithSaucy / (double)Service.Configuration.Stats.GamesPlayedWithSaucy) * 100, 2)}%");
            }
            else
            {
                ImGuiEx.CenterColumnText("");
            }
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{Service.Configuration.Stats.CardsDroppedWithSaucy}");
            ImGui.NextColumn();

            if (Service.Configuration.Stats.NPCsPlayed.Count > 0)
            {
                ImGuiEx.CenterColumnText($"{Service.Configuration.Stats.NPCsPlayed.OrderByDescending(x => x.Value).First().Key}");
                ImGuiEx.CenterColumnText($"{Service.Configuration.Stats.NPCsPlayed.OrderByDescending(x => x.Value).First().Value} times");
                ImGui.NextColumn();
                ImGui.NextColumn();
                ImGui.NextColumn();
            }

            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("金蝶币赢得", true);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("总卡丢弃值", true);
            ImGui.NextColumn();
            if (Service.Configuration.Stats.CardsWon.Count > 0)
            {
                ImGuiEx.CenterColumnText("嬴的最多的卡", true);
            }
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{Service.Configuration.Stats.MGPWon} 金蝶币");
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{GetDroppedCardValues()} 金蝶币");
            ImGui.NextColumn();
            if (Service.Configuration.Stats.CardsWon.Count > 0)
            {
                ImGuiEx.CenterColumnText($"{TriadCardDB.Get().FindById((int)Service.Configuration.Stats.CardsWon.OrderByDescending(x => x.Value).First().Key).Name.GetLocalized()}");
                ImGui.NextColumn();
                ImGui.NextColumn();
                ImGui.NextColumn();
                ImGuiEx.CenterColumnText($"{Service.Configuration.Stats.CardsWon.OrderByDescending(x => x.Value).First().Value} 次");
            }

            ImGui.Columns(1);
            ImGui.EndChild();
            ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
            if (ImGui.Button("重置状态（按住Ctrl键点击）", new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetContentRegionAvail().Y)) && ImGui.GetIO().KeyCtrl)
            {
                Service.Configuration.Stats = new();
                Service.Configuration.Save();
            }
        }

        private int GetDroppedCardValues()
        {
            int output = 0;
            foreach (var card in Service.Configuration.Stats.CardsWon)
            {
                output += GameCardDB.Get().FindById((int)card.Key).SaleValue * Service.Configuration.Stats.CardsWon[card.Key];
            }

            return output;
        }

        public void DrawTriadTab()
        {
            bool enabled = TriadAutomater.ModuleEnabled;

            //ImGui.TextWrapped(@"How to use: Challenge an NPC you wish to play cards with. Once you have initiated the challenge, click ""Enable Triad Module"".");
            ImGui.TextWrapped(@"使用方法：挑战你想与之打牌的NPC。发起挑战后，单击""启用九宫幻卡模块""。");
            ImGui.Separator();

            if (ImGui.Checkbox("启用九宫幻卡模块", ref enabled))
            {
                TriadAutomater.ModuleEnabled = enabled;
            }

            int selectedDeck = configuration.SelectedDeckIndex;

            if (Saucy.TTSolver.preGameDecks.Count > 0)
            {
                string preview = selectedDeck >= 0 ? Saucy.TTSolver.preGameDecks[selectedDeck].name : string.Empty;
                if (ImGui.BeginCombo("选择幻卡", preview))
                {
                    if (ImGui.Selectable(""))
                    {
                        configuration.SelectedDeckIndex = -1;
                    }

                    foreach (var deck in Saucy.TTSolver.preGameDecks.Values)
                    {
                        var index = Saucy.TTSolver.preGameDecks.Where(x => x.Value == deck).First().Key;
                        if (ImGui.Selectable(deck.name, index == selectedDeck))
                        {
                            configuration.SelectedDeckIndex = index;
                            configuration.Save();
                        }
                    }

                    ImGui.EndCombo();
                }
            }
            else
            {
                // ImGui.TextWrapped("Please initiate a challenge with an NPC to populate your deck list.");
                ImGui.TextWrapped("请向NPC发起挑战以填充您的幻卡列表。");
            }

            if (ImGui.Checkbox("进行幻卡对局X次", ref TriadAutomater.PlayXTimes) && (TriadAutomater.NumberOfTimes <= 0 || TriadAutomater.PlayUntilCardDrops || TriadAutomater.PlayUntilAllCardsDropOnce))
            {
                TriadAutomater.NumberOfTimes = 1;
                TriadAutomater.PlayUntilCardDrops = false;
                TriadAutomater.PlayUntilAllCardsDropOnce = false;
            }

            if (ImGui.Checkbox("玩到任何幻卡掉落", ref TriadAutomater.PlayUntilCardDrops) && (TriadAutomater.NumberOfTimes <= 0 || TriadAutomater.PlayXTimes || TriadAutomater.PlayUntilAllCardsDropOnce))
            {
                TriadAutomater.NumberOfTimes = 1;
                TriadAutomater.PlayXTimes = false;
                TriadAutomater.PlayUntilAllCardsDropOnce = false;
            }

            if (Saucy.TTSolver.preGameNpc is not null)
            {
                if (GameNpcDB.Get().mapNpcs.TryGetValue(Saucy.TTSolver.preGameNpc?.Id ?? -1, out var npcInfo))
                {
                    CurrentNPC = npcInfo;

                    if (ImGui.Checkbox($"至少玩X次，直到所有卡牌从NPC掉落", ref TriadAutomater.PlayUntilAllCardsDropOnce))
                    {
                        TriadAutomater.PlayUntilCardDrops = false;
                        TriadAutomater.PlayXTimes = false;
                        TriadAutomater.NumberOfTimes = 1;
                    }

                    if (TriadAutomater.PlayUntilAllCardsDropOnce)
                    {
                        foreach (var card in CurrentNPC.rewardCards)
                        {
                            TriadAutomater.TempCardsWonList.TryAdd((uint)card, 0);
                        }
                    }
                }
                else
                {
                    CurrentNPC = null;
                }
            }

            if (TriadAutomater.PlayXTimes || TriadAutomater.PlayUntilCardDrops || TriadAutomater.PlayUntilAllCardsDropOnce)
            {
                ImGui.PushItemWidth(150f);
                ImGui.Text("多少次:");
                ImGui.SameLine();
                if (ImGui.InputInt("", ref TriadAutomater.NumberOfTimes))
                {
                    if (TriadAutomater.NumberOfTimes <= 0)
                        TriadAutomater.NumberOfTimes = 1;
                }

                ImGui.Checkbox("完成后退出FF14", ref TriadAutomater.LogOutAfterCompletion);

                bool playSound = Service.Configuration.PlaySound;

                ImGui.Columns(2, null, false);
                if (ImGui.Checkbox("完成后播放声音", ref playSound))
                {
                    Service.Configuration.PlaySound = playSound;
                    Service.Configuration.Save();
                }

                if (playSound)
                {
                    ImGui.NextColumn();
                    ImGui.Text("选择声音");
                    if (ImGui.BeginCombo("###SelectSound", Service.Configuration.SelectedSound))
                    {
                        string path = Path.Combine(Service.Interface.AssemblyLocation.Directory.FullName, "Sounds");
                        foreach (var file in new DirectoryInfo(path).GetFiles())
                        {
                            if (ImGui.Selectable($"{Path.GetFileNameWithoutExtension(file.FullName)}", Service.Configuration.SelectedSound == Path.GetFileNameWithoutExtension(file.FullName)))
                            {
                                Service.Configuration.SelectedSound = Path.GetFileNameWithoutExtension(file.FullName);
                                Service.Configuration.Save();
                            }
                        }

                        ImGui.EndCombo();
                    }

                    if (ImGui.Button("打开声音文件夹"))
                    {
                        Process.Start("explorer.exe", @$"{Path.Combine(Service.Interface.AssemblyLocation.Directory.FullName, "Sounds")}");
                    }
                    ImGuiComponents.HelpMarker("将任何MP3文件放入声音文件夹以添加您自己的自定义声音。");
                }
                ImGui.Columns(1);
            }
        }


        public void DrawCufTab()
        {
            bool enabled = CufModule.ModuleEnabled;

            ImGui.TextWrapped(@"使用方法:单击""启用重击伽美什""模块，然后走到重击伽美什家具前。");
            ImGui.Separator();

            if (ImGui.Checkbox("启用重击伽美什", ref enabled))
            {
                CufModule.ModuleEnabled = enabled;
            }
        }
    }
}

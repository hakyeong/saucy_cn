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
                    if (ImGui.BeginTabItem("???????????????"))
                    {
                        DrawCufTab();
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("????????????"))
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
            ImGui.Columns(3, "????????????", false);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText(ImGuiColors.ParsedGold, "SAUCY??????", true);
            ImGui.Columns(1);
            ImGui.BeginChild("TT Stats", new Vector2(0, ImGui.GetContentRegionAvail().Y - 30f), true);
            ImGui.Columns(3, null, false);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText(ImGuiColors.DalamudRed, "????????????", true);
            ImGuiHelpers.ScaledDummy(10f);
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("????????????", true);
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{Service.Configuration.Stats.GamesPlayedWithSaucy}");
            ImGui.NextColumn();
            ImGui.NextColumn();
            ImGui.Spacing();
            ImGuiEx.CenterColumnText("??????", true);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("??????", true);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("??????", true);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{Service.Configuration.Stats.GamesWonWithSaucy}");
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{Service.Configuration.Stats.GamesLostWithSaucy}");
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{Service.Configuration.Stats.GamesDrawnWithSaucy}");
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("??????", true);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("????????????", true);
            ImGui.NextColumn();
            if (Service.Configuration.Stats.NPCsPlayed.Count > 0)
            {
                ImGuiEx.CenterColumnText("???????????????NPC", true);
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
            ImGuiEx.CenterColumnText("???????????????", true);
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText("???????????????", true);
            ImGui.NextColumn();
            if (Service.Configuration.Stats.CardsWon.Count > 0)
            {
                ImGuiEx.CenterColumnText("??????????????????", true);
            }
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{Service.Configuration.Stats.MGPWon} ?????????");
            ImGui.NextColumn();
            ImGuiEx.CenterColumnText($"{GetDroppedCardValues()} ?????????");
            ImGui.NextColumn();
            if (Service.Configuration.Stats.CardsWon.Count > 0)
            {
                ImGuiEx.CenterColumnText($"{TriadCardDB.Get().FindById((int)Service.Configuration.Stats.CardsWon.OrderByDescending(x => x.Value).First().Key).Name.GetLocalized()}");
                ImGui.NextColumn();
                ImGui.NextColumn();
                ImGui.NextColumn();
                ImGuiEx.CenterColumnText($"{Service.Configuration.Stats.CardsWon.OrderByDescending(x => x.Value).First().Value} ???");
            }

            ImGui.Columns(1);
            ImGui.EndChild();
            ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
            if (ImGui.Button("?????????????????????Ctrl????????????", new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetContentRegionAvail().Y)) && ImGui.GetIO().KeyCtrl)
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
            ImGui.TextWrapped(@"??????????????????????????????????????????NPC???????????????????????????""????????????????????????""???");
            ImGui.Separator();

            if (ImGui.Checkbox("????????????????????????", ref enabled))
            {
                TriadAutomater.ModuleEnabled = enabled;
            }

            int selectedDeck = configuration.SelectedDeckIndex;

            if (Saucy.TTSolver.preGameDecks.Count > 0)
            {
                string preview = selectedDeck >= 0 ? Saucy.TTSolver.preGameDecks[selectedDeck].name : string.Empty;
                if (ImGui.BeginCombo("????????????", preview))
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
                ImGui.TextWrapped("??????NPC??????????????????????????????????????????");
            }

            if (ImGui.Checkbox("??????????????????X???", ref TriadAutomater.PlayXTimes) && (TriadAutomater.NumberOfTimes <= 0 || TriadAutomater.PlayUntilCardDrops || TriadAutomater.PlayUntilAllCardsDropOnce))
            {
                TriadAutomater.NumberOfTimes = 1;
                TriadAutomater.PlayUntilCardDrops = false;
                TriadAutomater.PlayUntilAllCardsDropOnce = false;
            }

            if (ImGui.Checkbox("????????????????????????", ref TriadAutomater.PlayUntilCardDrops) && (TriadAutomater.NumberOfTimes <= 0 || TriadAutomater.PlayXTimes || TriadAutomater.PlayUntilAllCardsDropOnce))
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

                    if (ImGui.Checkbox($"?????????X???????????????????????????NPC??????", ref TriadAutomater.PlayUntilAllCardsDropOnce))
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
                ImGui.Text("?????????:");
                ImGui.SameLine();
                if (ImGui.InputInt("", ref TriadAutomater.NumberOfTimes))
                {
                    if (TriadAutomater.NumberOfTimes <= 0)
                        TriadAutomater.NumberOfTimes = 1;
                }

                ImGui.Checkbox("???????????????FF14", ref TriadAutomater.LogOutAfterCompletion);

                bool playSound = Service.Configuration.PlaySound;

                ImGui.Columns(2, null, false);
                if (ImGui.Checkbox("?????????????????????", ref playSound))
                {
                    Service.Configuration.PlaySound = playSound;
                    Service.Configuration.Save();
                }

                if (playSound)
                {
                    ImGui.NextColumn();
                    ImGui.Text("????????????");
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

                    if (ImGui.Button("?????????????????????"))
                    {
                        Process.Start("explorer.exe", @$"{Path.Combine(Service.Interface.AssemblyLocation.Directory.FullName, "Sounds")}");
                    }
                    ImGuiComponents.HelpMarker("?????????MP3??????????????????????????????????????????????????????????????????");
                }
                ImGui.Columns(1);
            }
        }


        public void DrawCufTab()
        {
            bool enabled = CufModule.ModuleEnabled;

            ImGui.TextWrapped(@"????????????:??????""?????????????????????""????????????????????????????????????????????????");
            ImGui.Separator();

            if (ImGui.Checkbox("?????????????????????", ref enabled))
            {
                CufModule.ModuleEnabled = enabled;
            }
        }
    }
}

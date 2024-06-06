using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using ImGuiNET;

namespace Brandy.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;
    private Plugin Plugin;

    private uint[] IconIds = new uint[]
    {
        60545,
        60561, 60562, 60563, 60918, 60919, 60920, 60921, 60922, 60923, 60924, 60925, 61060,
        61201, 61202, 61203, 61204, 61205, 61206, 61207, 61208,
        61241, 61242, 61243, 61244, 61245, 61246, 61247, 61248,
        61531,
        199006
    };

    private List<MarkInfo> MarksToRemove = new List<MarkInfo>();

    public ConfigWindow(Plugin plugin) : base(
        "Brandy Settings",
        ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollWithMouse)
    {
        Plugin        = plugin;
        Size          = new Vector2(400, 800);
        SizeCondition = ImGuiCond.Appearing;

        Configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        if (ImGui.BeginTabBar("Settings Tabs"))
        {
            if (ImGui.BeginTabItem("Mark Profiles"))
            {
                foreach (var info in Configuration.MarkInfos)
                {
                    DrawMarkProfile(info);
                }

                if (ImGui.Button("Add new profile"))
                {
                    var markInfo = new MarkInfo()
                    {
                        Name = $"New {Configuration.MarkInfos.Count}"
                    };

                    Configuration.MarkInfos.Add(markInfo);
                }
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Active Marks"))
            {
                foreach (var mark in Plugin.Marks)
                {
                    ImGui.Text($"{mark.Object?.Name ?? "null"} - {mark.MarkInfo.Name}");
                    ImGui.SameLine();
                    if (ImGui.Button("Remove"))
                    {
                        Plugin.RemoveMark(mark);
                    }
                }

                ImGui.EndTabItem();
            }
        }
        ImGui.EndTabBar();

        if (ImGui.Button("Save"))
        {
            PluginLog.Log("Config changed, saving.");
            Configuration.Save();
        }

        foreach (var markToRemove in MarksToRemove)
        {
            Configuration.MarkInfos.Remove(markToRemove);
        }
        MarksToRemove.Clear();
    }

    private void DrawMarkProfile(MarkInfo info)
    {
        ImGui.PushID($"{info.Name} - Settings");
        ImGui.Text($"{info.Name} - Settings");
        ImGui.InputText("Name", ref info.Name, 256);
        DrawIconPicker(info);
        ImGui.SliderFloat2("Icon Size", ref info.Size, 1, 1000);
        ImGui.SliderInt("Vert Offset", ref info.VertOffset, 0, 100);

        Vector4 tint = ImGui.ColorConvertU32ToFloat4(info.Tint);
        ImGui.ColorEdit4("Icon Tint", ref tint, ImGuiColorEditFlags.NoSidePreview);
        info.Tint = ImGui.ColorConvertFloat4ToU32(tint);

        if (ImGui.Button("Remove Profile", new Vector2(-1, 0)))
        {
            MarksToRemove.Add(info);
        }

        ImGui.PopID();

        ImGui.Separator();
    }

    private void DrawIconPicker(MarkInfo info)
    {
        if (info.IconId != 0)
        {
            var icon = Plugin.TextureProvider.GetIcon(info.IconId);
            ImGui.Image(icon.ImGuiHandle, new Vector2(32, 32), new Vector2(0, 0), new Vector2(1, 1), ImGui.ColorConvertU32ToFloat4(info.Tint));
            ImGui.SameLine();
        }

        if (ImGui.BeginCombo("Icon", info.IconId != 0 ? "Icon" : "Custom Icon", ImGuiComboFlags.NoPreview | ImGuiComboFlags.HeightRegular))
        {
            foreach (var iconId in IconIds)
            {
                if (ImGui.Selectable($"##{iconId}", iconId == info.IconId, ImGuiSelectableFlags.None, new Vector2(32, 32)))
                {
                    info.IconId = iconId;
                }
                ImGui.SameLine();
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() - 40);
                var icon = Plugin.TextureProvider.GetIcon(iconId);
                ImGui.Image(icon.ImGuiHandle, new Vector2(32, 32));
            }

            ImGui.EndCombo();
        }
    }
}

using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using System.Linq;
using Brandy.Windows;
using Dalamud.Plugin.Services;
using Dalamud.Game.Gui.ContextMenu;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using ImGuiNET;
using System.Numerics;

namespace Brandy
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Brandy";
        private const string CommandName = "/Brandy";

        private DalamudPluginInterface PluginInterface;
        private ICommandManager CommandManager;
        private IContextMenu ContextMenu;
        private IGameGui GameGui;
        public ITextureProvider TextureProvider;
        public Configuration Configuration;
        public WindowSystem WindowSystem = new("Brandy");

        private ConfigWindow ConfigWindow { get; init; }

        public List<MarkedObject> Marks              = new List<MarkedObject>();
        public List<MarkedObject> PendingRemoveMarks = new List<MarkedObject>();

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] ICommandManager commandManager,
            [RequiredVersion("1.0")] IContextMenu contextMenu,
            [RequiredVersion("1.0")] IGameGui gameGui,
            [RequiredVersion("1.0")] ITextureProvider textureProvider
        )
        {
            PluginInterface = pluginInterface;
            CommandManager  = commandManager;
            ContextMenu     = contextMenu;
            GameGui         = gameGui;
            TextureProvider = textureProvider;

            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize(PluginInterface);

            ConfigWindow = new ConfigWindow(this);
            
            WindowSystem.AddWindow(ConfigWindow);

            CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "A useful message to display in /xlhelp"
            });

            PluginInterface.UiBuilder.Draw += DrawUI;
            PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

            ContextMenu.OnMenuOpened += ContextMenuOnOpened;
        }

        private void ContextMenuOnOpened(MenuOpenedArgs args)
        {
            if (args.MenuType == ContextMenuType.Default)
            {
                var target = args.Target as Dalamud.Game.Gui.ContextMenu.MenuTargetDefault;

                var markTypes = new List<MenuItem>();
                var clearMarksItem = new MenuItem()
                {
                    Name = $"Clear Mark",
                    OnClicked = (MenuItemClickedArgs onClickedArgs) =>
                    {
                        RemoveMark(target.TargetObject);
                    }
                };

                markTypes.Add(clearMarksItem);

                foreach (var mark in Configuration.MarkInfos)
                {
                    var markMenuItem = new MenuItem()
                    {
                        Name = $"Mark - {mark.Name}",
                        OnClicked = (MenuItemClickedArgs onClickedArgs) =>
                        {
                            MarkObject(target.TargetObject, mark);
                        }
                    };

                    markTypes.Add(markMenuItem);
                }

                var rootItem = new MenuItem()
                {
                    Name      = "Brandy",
                    IsSubmenu = true,
                    OnClicked = (MenuItemClickedArgs onClickedArgs) =>
                    {
                        onClickedArgs.OpenSubmenu(markTypes);
                    }
                };

                args.AddMenuItem(rootItem);
            }
        }

        public void MarkObject(GameObject obj, MarkInfo markInfo)
        {
            if (obj)
            {
                if (Marks.Any(x => x.ObjectId == obj.ObjectId))
                {
                    RemoveMark(obj);
                }

                Marks.Add(new MarkedObject(obj, markInfo));
            }
            else
            {
                PluginLog.Warning("Mark obj was null");
            }
        }

        public void RemoveMark(MarkedObject mark)
        {
            if (mark != null)
            {
                PendingRemoveMarks.Add(mark);
                //PluginLog.Information($"Removed mark on {mark.Object.Name}");
            }
        }

        public void RemoveMark(GameObject obj)
        {
            var mark = Marks.FirstOrDefault(x => x.ObjectId == obj.ObjectId);
            RemoveMark(mark);
        }

        public void Dispose()
        {
            WindowSystem.RemoveAllWindows();
            
            ConfigWindow.Dispose();
            
            CommandManager.RemoveHandler(CommandName);
        }

        private void OnCommand(string command, string args)
        {
            // in response to the slash command, just display our main ui
            ConfigWindow.IsOpen = true;
        }

        private void DrawUI()
        {
            this.WindowSystem.Draw();

            DrawMarkers();

            // Check if any marked actors are invalid or changed and remove
            foreach (var mark in Marks)
            {
                if (!mark.Object.IsValid() || mark.ObjectId != mark.Object.ObjectId)
                {
                    RemoveMark(mark);
                }
            }

            foreach (var mark in PendingRemoveMarks)
            {
                Marks.Remove(mark);
            }
            PendingRemoveMarks.Clear();
        }

        private unsafe void DrawMarkers()
        {
            foreach (var mark in Marks)
            {
                if (mark.Object)
                {
                    var icon = TextureProvider.GetIcon(mark.MarkInfo.IconId);
                    var markObjStruct = (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)mark.Object.Address;
                    var markPos = mark.Object.Position + new Vector3(0, 2 * markObjStruct->Height, 0);

                    if (GameGui.WorldToScreen(markPos, out var pos))
                    {
                        pos.Y -= mark.MarkInfo.VertOffset;
                        var topLeft     = pos - (mark.MarkInfo.HalfSize);
                        var bottomRight = pos + (mark.MarkInfo.HalfSize);
                        ImGui.GetForegroundDrawList().AddImage(icon.ImGuiHandle, topLeft, bottomRight, new Vector2(0, 0), new Vector2(1, 1), mark.MarkInfo.Tint);
                    }
                }
            }
        }

        public void DrawConfigUI()
        {
            ConfigWindow.IsOpen = true;
        }
    }
}

using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game.Object;

namespace Brandy
{
    public class MarkedObject
    {
        public ulong ObjectId;
        public IGameObject Object;
        public MarkInfo MarkInfo;

        public MarkedObject(IGameObject @object, MarkInfo markInfo)
        {
            Object = @object;
            MarkInfo = markInfo;
            ObjectId = Object.GameObjectId;
        }
    }
}

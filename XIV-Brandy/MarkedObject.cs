using Dalamud.Game.ClientState.Objects.Types;

namespace Brandy
{
    public class MarkedObject
    {
        public uint ObjectId;
        public GameObject Object;
        public MarkInfo MarkInfo;

        public MarkedObject(GameObject @object, MarkInfo markInfo)
        {
            Object = @object;
            MarkInfo = markInfo;
            ObjectId = Object.ObjectId;
        }
    }
}

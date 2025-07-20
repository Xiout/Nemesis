using UnityEngine;
using Board.Rooms;

namespace Board.Corridors
{
    public abstract class Corridor : MonoBehaviour
    {
        internal Room Room1 { get; set; }
        internal bool IsDoubleCorridor { get; set; }
        internal abstract bool HasNoise{ get; set; }
        internal GameObject NoiseMarkerGO { get; set; }

        public abstract bool MakeNoise();
        public abstract void ClearNoise();
    }
}

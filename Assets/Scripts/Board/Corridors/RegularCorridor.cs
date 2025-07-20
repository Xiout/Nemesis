using UnityEngine;
using Board.Rooms;

namespace Board.Corridors
{
    public class RegularCorridor : Corridor
    {
        internal Room Room2;
        internal DoorEnum Door;

        internal override bool HasNoise { get; set; }

        public bool Init(Room room1, Room room2)
        {
            NoiseMarkerGO = null;
            if (room1 != null && room2 != null)
            {
                Room1 = room1;
                Room2 = room2;

                HasNoise = false;
                Door = DoorEnum.Open;

                return true;
            }

            return false;
        }

        public override bool MakeNoise()
        {
            if (HasNoise)
            {
                return true;
            }
            else
            {
                HasNoise = true;
                NoiseMarkerGO = GameObject.Instantiate(Ship.GetInstance().NoiseMarkerPrefab);
                NoiseMarkerGO.transform.position = new Vector3(transform.position.x, 0.125f, transform.position.z);
                NoiseMarkerGO.transform.SetParent(transform);

                return false;
            }
        }

        public override void ClearNoise()
        {
            HasNoise = false;
            GameObject.Destroy(NoiseMarkerGO);
            NoiseMarkerGO = null;
        }
    }
}

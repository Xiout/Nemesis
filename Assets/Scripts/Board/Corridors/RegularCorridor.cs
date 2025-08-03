using Board.Rooms;
using UnityEngine;
using static UnityEngine.UI.Image;

namespace Board.Corridors
{
    public class RegularCorridor : Corridor
    {
        internal Room Room2;
        internal DoorEnum Door;

        private GameObject _doorGO;

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

        public bool CloseDoor()
        {
            if (Door == DoorEnum.Broken) return false;

            Door = DoorEnum.Closed;

            if (_doorGO != null)
            {
                _doorGO.SetActive(true);
                return true;
            }

            Vector3 corridorVec = Room1.transform.position - Room2.transform.position;
            var doorGO = GameObject.Instantiate(Ship.GetInstance().ClosedDoorPrefab);
            doorGO.name = "Door";
            doorGO.transform.SetParent(transform);

            float offset = 0.25f;
            Vector3 offsetVector = corridorVec.normalized * offset;
            doorGO.transform.localPosition = new Vector3(offsetVector.x, doorGO.transform.localPosition.y, offsetVector.z);

            float angle = Vector3.SignedAngle(Vector3.forward, corridorVec.normalized, Vector3.up);
            doorGO.transform.rotation = Quaternion.Euler(0, angle, 0);
            _doorGO = doorGO;
            return true;
        }

        public void BreakDoor()
        {
            Door = DoorEnum.Broken;

            var doorGO = GameObject.Instantiate(Ship.GetInstance().BrokenDoorPrefab);
            doorGO.name = "Door";
            doorGO.transform.SetParent(transform);
            doorGO.transform.localPosition = _doorGO.transform.localPosition;
            doorGO.transform.rotation = _doorGO.transform.rotation;
            doorGO.transform.Rotate(Vector3.up, 90);
            GameObject.Destroy(_doorGO);
            _doorGO = doorGO;
        }

        public bool OpenDoor()
        {
            if(Door == DoorEnum.Broken) return false;

            Door = DoorEnum.Open;

            if (_doorGO != null)
            {
                _doorGO.SetActive(false);
            }

            return true;
        }
    }
}

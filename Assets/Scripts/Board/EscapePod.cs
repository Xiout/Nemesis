using Board.Rooms;
using System.Collections.Generic;
using UnityEngine;

namespace Board
{
    [DefaultExecutionOrder(5)]
    public class EscapePod : MonoBehaviour
    {
        public int PodNumber;
        public bool IsLocked;
        public Room EscapeSection { get; private set; }
        public List<Player> PlayersInPod;
        private bool hasLeft;

        void Awake()
        {
            PlayersInPod = new List<Player>();
            hasLeft = false;
            IsLocked = false; //FOR TESTING PURPOSE ONLY, NEED TO BE CHANGED TO true

            ResetMaterial();
        }

        public bool SetEscapeSession(Room room)
        {
            if (EscapeSection != null)
            {
                Debug.LogWarning($"This escape pod was already assigned to {EscapeSection.name} ({EscapeSection.GetRoomFunctionName()})");
                return false;
            }

            if(room == null)
            {
                Debug.LogWarning("Escape pod cannot be assigned to a null room");
                return false;
            }

            EscapeSection = room;
            return true;
        }

        internal void Vizualize()
        {
            gameObject.GetComponent<MeshRenderer>().SetMaterials(new List<Material>() { Ship.GetInstance().SelectableMaterial });
        }

        internal void ResetMaterial()
        {
            Material material = null;
            switch (IsLocked)
            {
                case true:
                    material = Ship.GetInstance().EscapePodLocked;
                    break;
                case false:
                    material = Ship.GetInstance().EscapePodUnlocked;
                    break;
            }
            gameObject.GetComponent<MeshRenderer>().SetMaterials(new List<Material>() { material });
        }
    }
}


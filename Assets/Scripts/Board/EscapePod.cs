using Board.Rooms;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

namespace Board
{
    [DefaultExecutionOrder(5)]
    public class EscapePod : MonoBehaviour
    {
        public int PodNumber;
        public bool IsLocked;
        public Room EvacuationSection { get; private set; }
        public List<Player> PlayersInPod;
        private bool hasLeft;

        private List<Tuple<Vector3, bool>> SpawnPositions;

        void Awake()
        {
            PlayersInPod = new List<Player>();
            hasLeft = false;
            IsLocked = false; //FOR TESTING PURPOSE ONLY, NEED TO BE CHANGED TO true

            ResetMaterial();

            SpawnPositions = new List<Tuple<Vector3, bool>>();
            for (int i = 0; i < transform.childCount; ++i)
            {
                var child = transform.GetChild(i);
                if (child.name.StartsWith("SpawnPos"))
                {
                    SpawnPositions.Add(Tuple.Create(child.localPosition, true));
                }
            }
        }

        public bool SetEscapeSession(Room room)
        {
            if (EvacuationSection != null)
            {
                Debug.LogWarning($"This escape pod was already assigned to {EvacuationSection.name} ({EvacuationSection.GetRoomFunctionName()})");
                return false;
            }

            if(room == null)
            {
                Debug.LogWarning("Escape pod cannot be assigned to a null room");
                return false;
            }

            EvacuationSection = room;
            return true;
        }

        internal bool PlacePlayerInPod(Player player)
        {
            if (PlayersInPod.Count >= 2)
            {
                Debug.LogWarning($"{player.name} could not enter in {this.name} : No space left");
                return false;
            }

            PlayersInPod.Add(player);

            var index = SpawnPositions.FindIndex(x => x.Item2);
            var spawnPos = SpawnPositions[index].Item1;

            Matrix4x4 localToParentMatrix = Matrix4x4.TRS(transform.localPosition, transform.localRotation, Vector3.one);
            var posToParent = localToParentMatrix.MultiplyPoint3x4(spawnPos);

            player.transform.position = new Vector3(posToParent.x, player.transform.position.y, posToParent.z);

            SpawnPositions.RemoveAt(index);
            SpawnPositions.Insert(index, Tuple.Create(spawnPos, false));
            player.PosInRoomIndex = index;
            player.EscapePod = this;
            return true;
        }

        internal bool RemovePlayerFromPod(Player player)
        {
            if (!PlayersInPod.Contains(player))
            {
                Debug.LogWarning($"{player.name} was not in {this.name}");
                return false;
            }

            PlayersInPod.Remove(player);
            player.EscapePod = null;

            if (player.PosInRoomIndex != null)
            {
                int index = (int)player.PosInRoomIndex;
                var spawnPos = SpawnPositions[index].Item1;
                SpawnPositions.RemoveAt(index);
                SpawnPositions.Insert(index, Tuple.Create(spawnPos, true));
            }

            return true;
        }

        public void Launch()
        {
            for (int i = 0; i < PlayersInPod.Count; i++)
            {
                PlayersInPod[i].HasEscape = true;
                PlayersInPod[i].gameObject.SetActive(false);
            }

            hasLeft = true;
            gameObject.SetActive(false);
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


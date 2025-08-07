using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Board.Corridors;
using System;
using Randomness;

namespace Board.Rooms
{
    [DefaultExecutionOrder(1)]
    public class Room : MonoBehaviour
    {
        public List<Room> AdjacentRooms;
        public RoomTypeEnum RoomType;
        public bool IsRequiredRoom;
        public bool HasTechnicalCorridor;

        public Dictionary<int, Corridor> Corridors;
        private List<Tuple<Vector3, bool>> SpawnPositions;
        public List<Player> Players;
        public List<Intruder> Intruders;
        private RoomFunction _roomFunction;
        public bool IsBroken { get; private set; }
        public bool IsOnFire { get; private set; }
        public int ObjectCount;

        private GameObject _fireTokenGO;
        private GameObject _brokenTokenGO;

        private void Awake()
        {
            Corridors = new Dictionary<int, Corridor>();
            Players = new List<Player>();
            Intruders = new List<Intruder>();
            IsBroken = false;
            IsOnFire = false;
            ObjectCount = 0;
            ResetMaterial();

            if (RoomType == RoomTypeEnum.Special)
            {
                _roomFunction = RoomFunction.AllRooms.ToList().Find(r => r.Name == name);
            }
            else
            {
                _roomFunction = null;
            }

            SpawnPositions = new List<Tuple<Vector3, bool>>();
            for (int i = 0; i < transform.childCount; ++i)
            {
                var child = transform.GetChild(i);
                if (child.name.StartsWith("SpawnPos"))
                {
                    SpawnPositions.Add(Tuple.Create(child.localPosition, true));
                }
            }

            _fireTokenGO = GameObject.Instantiate(Ship.GetInstance().FireTokenPrefab);
            _fireTokenGO.transform.SetParent(transform);
            _fireTokenGO.transform.localPosition = new Vector3(-0.15f, _fireTokenGO.transform.position.y, -0.5f);
            _fireTokenGO.SetActive(false);

            _brokenTokenGO = GameObject.Instantiate(Ship.GetInstance().BrokenTokenPrefab);
            _brokenTokenGO.transform.SetParent(transform);
            _brokenTokenGO.transform.localPosition = new Vector3(0.15f, _fireTokenGO.transform.position.y, -0.5f);
            _brokenTokenGO.SetActive(false);
        }

        void Start()
        {
            if (HasTechnicalCorridor)
            {
                var TechCorrGO = GameObject.Instantiate(Ship.GetInstance().TechnicalCorridorMarkerPrefab);
                TechCorrGO.name = $"TechnicalCorridor_{gameObject.name}";
                TechCorrGO.transform.SetParent(transform);
                TechCorrGO.transform.position = new Vector3(transform.position.x + (transform.localScale.x / 2.0f + 0.5f),
                    0.0f,
                    transform.position.z + (transform.localScale.z / 2.0f + 0.5f)
                );

                var technicalCorridor = TechCorrGO.AddComponent<TechnicalCorridor>();
                technicalCorridor.Init(this);
                Ship.GetInstance().Corridors.Add(technicalCorridor);
                AddCorridor(technicalCorridor, FreeCorridorNumber()[0]);

                if (CountCorridors() < 4)
                {
                    AddCorridor(technicalCorridor, FreeCorridorNumber()[0]);
                    technicalCorridor.IsDoubleCorridor = true;
                }
            }
        }

        internal void PlacePlayerInRoom(Player player)
        {
            player.CurrentRoom = this;
            Players.Add(player);

            if (Intruders.Count + Players.Count > SpawnPositions.Count)
            {
                Debug.LogWarning("No SpawnPosition Left in this room");
                player.transform.position = new Vector3(transform.position.x, player.transform.position.y, transform.position.z);
                player.PosInRoomIndex = null;
                return;
            }

            var index = SpawnPositions.FindIndex(x => x.Item2);
            var spawnPos = SpawnPositions[index].Item1;

            Matrix4x4 localToParentMatrix = Matrix4x4.TRS(transform.localPosition, transform.localRotation, Vector3.one);
            var posToParent = localToParentMatrix.MultiplyPoint3x4(spawnPos);

            player.transform.position = new Vector3(posToParent.x, player.transform.position.y, posToParent.z);

            SpawnPositions.RemoveAt(index);
            SpawnPositions.Insert(index, Tuple.Create(spawnPos, false));
            player.PosInRoomIndex = index;
        }

        internal void PlaceIntruderInRoom(Intruder intruder)
        {
            intruder.CurrentRoom = this;
            Intruders.Add(intruder);

            if (Intruders.Count + Players.Count > SpawnPositions.Count)
            {
                Debug.LogWarning("No SpawnPosition Left in this room");
                intruder.transform.position = new Vector3(transform.position.x, intruder.transform.position.y, transform.position.z);
                intruder.PosInRoomIndex = null;
                return;
            }

            var index = SpawnPositions.FindIndex(x => x.Item2);
            var spawnPos = SpawnPositions[index].Item1;

            Matrix4x4 localToParentMatrix = Matrix4x4.TRS(transform.localPosition, transform.localRotation, Vector3.one);
            var posToParent = localToParentMatrix.MultiplyPoint3x4(spawnPos);

            intruder.transform.position = new Vector3(posToParent.x, intruder.transform.position.y, posToParent.z);

            SpawnPositions.RemoveAt(index);
            SpawnPositions.Insert(index, Tuple.Create(spawnPos, false));

            intruder.PosInRoomIndex = index;
        }

        internal void RemovePlayerFromRoom(Player player)
        {
            if (!Players.Contains(player))
            {
                return;
            }

            Players.Remove(player);
            player.CurrentRoom = null;

            if (player.PosInRoomIndex != null)
            {
                int index = (int)player.PosInRoomIndex;
                var spawnPos = SpawnPositions[index].Item1;
                SpawnPositions.RemoveAt(index);
                SpawnPositions.Insert(index, Tuple.Create(spawnPos, true));
            }
        }

        internal void RemoveIntruderFromRoom(Intruder intruder)
        {
            if (!Intruders.Contains(intruder))
            {
                return;
            }

            Intruders.Remove(intruder);
            intruder.CurrentRoom = null;

            if (intruder.PosInRoomIndex != null)
            {
                int index = (int)intruder.PosInRoomIndex;
                var spawnPos = SpawnPositions[index].Item1;
                SpawnPositions.RemoveAt(index);
                SpawnPositions.Insert(index, Tuple.Create(spawnPos, true));
            }
        }


        internal int AddCorridor(Corridor corridor, int number)
        {
            if (CountCorridors() >= 4)
            {
                Debug.LogError($"Too many corridors for {gameObject.name} ({CountCorridors()})");
                return -1;
            }

            if(corridor.Room1 == this || (corridor as RegularCorridor)?.Room2 == this)
            {
                Corridors.Add(number, corridor);
                return number;
            }

            Debug.LogError($"{corridor.name} is not connected to {gameObject.name}");
            return -1;
        } 

        internal void VisualizeRoomAndAdjacents()
        {
            gameObject.GetComponent<MeshRenderer>().SetMaterials(new List<Material>() { Ship.GetInstance().SelectedMaterial });
            for (int i = 0; i < AdjacentRooms.Count; ++i)
            {
                var adjacent = AdjacentRooms[i];
                var corridor = Corridors.Values.Where(
                                    c => (c.Room1 == this && (c as RegularCorridor)?.Room2 == adjacent) ||
                                         (c.Room1 == adjacent && (c as RegularCorridor)?.Room2 == this)
                                ).First() as RegularCorridor;

                if(corridor?.Door == DoorEnum.Closed)
                {
                    continue;
                }

                adjacent.Vizualize();
            }
        }

        internal void Vizualize()
        {
            gameObject.GetComponent<MeshRenderer>().SetMaterials(new List<Material>() { Ship.GetInstance().SelectableMaterial });
        }

        internal void ResetMaterial()
        {
            Material material = null;
            switch (RoomType)
            {
                case RoomTypeEnum.Green:
                    material = Ship.GetInstance().GreenTileMaterial;
                    break;
                case RoomTypeEnum.Yellow:
                    material = Ship.GetInstance().YellowTileMaterial;
                    break;
                case RoomTypeEnum.Red:
                    material = Ship.GetInstance().RedTileMaterial;
                    break;
                case RoomTypeEnum.Special:
                    material = Ship.GetInstance().SpecialTileMaterial;
                    break;
                case RoomTypeEnum.Generalist:
                    material = Ship.GetInstance().GeneralistTileMaterial;
                    break;
                case RoomTypeEnum.Unknown:
                    material = Ship.GetInstance().UnexploredTileMaterial;
                    break;

            }
            gameObject.GetComponent<MeshRenderer>().SetMaterials(new List<Material>() { material });
        }

        internal int CountCorridors()
        {
            return Corridors.Values.Count;
        }

        internal List<int> FreeCorridorNumber()
        {
            List<int> list = new List<int>() { 1, 2, 3, 4 };

            var keys = Corridors.Keys.ToList();
            for (int i=0; i< keys.Count; ++i)
            {
                list.Remove(keys[i]);
            }
            return list;
        }

        public bool DiscoverRoomTile()
        {
            if(_roomFunction != null)
            {
                Debug.LogWarning($"{name}'s function was already set to {_roomFunction.Name}");
                return false;
            }

            _roomFunction = RoomExploration.DrawRoomTile(IsRequiredRoom);
            Debug.Log($"Discover {_roomFunction.Name}");
            RoomType = _roomFunction.Type;

            ResetMaterial();

            if (_roomFunction.Name.StartsWith("Evacuation Section"))
            {
                List<EscapePod> listPods;
                if (_roomFunction.Name.EndsWith("A"))
                {
                    listPods = Ship.GetInstance().EscapePods.Where(p => p.PodNumber % 2 == 1).ToList();
                }
                else if (_roomFunction.Name.EndsWith("B"))
                {
                    listPods = Ship.GetInstance().EscapePods.Where(p => p.PodNumber % 2 == 0).ToList();
                }
                else
                {
                    Debug.LogWarning($"Invalid Evacuation Section naming : {_roomFunction.Name}");
                    return false;
                }

                foreach (EscapePod pod in listPods)
                {
                    pod.SetEscapeSession(this);
                }
            }

            return true;
        }

        public void SetOnFire(bool onFire)
        {
            IsOnFire = onFire;
            _fireTokenGO.SetActive(IsOnFire);
        }

        public void SetBroken(bool broken)
        {
            IsBroken = broken;
            _brokenTokenGO.SetActive(IsBroken);
        }

        public bool IsRoomEmpty()
        {
            return Players.Count <= 0 && Intruders.Count <= 0;
        }

        public string GetRoomInfo()
        {
            return $"{_roomFunction.Name} ({ObjectCount})";
        }

        public string GetRoomFunctionName()
        {
            return _roomFunction.Name;
        }

        public int GetRoomActionCount()
        {
            if(_roomFunction.RoomActions == null)
            {
                return 0;
            }

            return _roomFunction.RoomActions.Count;
        }

        public string GetRoomActionName(int index)
        {
            if (GetRoomActionCount() <= index)
            {
                return null;
            }

            return _roomFunction.RoomActions[index].Name;
        }

        public bool ExecuteRoomFunction(int index)
        {
            return _roomFunction.ExecuteAction(index);
        }

        public bool IsRoomActionAuto(int index)
        {
            return _roomFunction.RoomActions[index].IsAutoAction;
        }
    }
}

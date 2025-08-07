using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using Board.Corridors;
using Board.Rooms;
using Randomness;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Board
{
    [DefaultExecutionOrder(0)]
    public class Ship : MonoBehaviour
    {
        private static Ship _instanceShip;

        public int PlayerCount;

        internal List<Room> Rooms;
        internal List<Corridor> Corridors;
        internal List<Player> Players;
        internal List<Intruder> Intruders;
        internal List<EscapePod> EscapePods;

        internal bool[] _engineStatus;
        private DirectionEnum[] DirectionsArray;
        private int _indexDirection;
        private int _roundTrack;
        private bool _isSelfDesctructOn;
        private int? _selfDestructTrack;

        public Room RoomToDepressurize;

        public float CorridorThickness;
        public GameObject TechnicalCorridorMarkerPrefab;
        public GameObject NoiseMarkerPrefab;
        public GameObject ClosedDoorPrefab;
        public GameObject BrokenDoorPrefab;
        public List<GameObject> IntruderPrefabs;
        public List<GameObject> PlayerPrefabs;
        public GameObject BrokenTokenPrefab;
        public GameObject FireTokenPrefab;
        public Material DefaultButtonMaterial;
        public Material DisabledButtonMaterial;
        public Material SelectedButtonMaterial;
        public Material SelectedMaterial;
        public Material SelectableMaterial;
        public Material SingleCorridorMaterial;
        public Material DoubleCorridorMaterial;
        public Material ErrorMaterial;
        public Material TechnicalCorridorWithNoise;
        public Material TechnicalCorridorNoiseClear;
        public Material UnexploredTileMaterial;
        public Material GreenTileMaterial;
        public Material RedTileMaterial;
        public Material YellowTileMaterial;
        public Material SpecialTileMaterial;
        public Material GeneralistTileMaterial;
        public Material EscapePodLocked;
        public Material EscapePodUnlocked;

        public GameObject EscapePodPopUpGO;

        private GameObject _moveButtonGO;
        private GameObject _shootButtonGO;
        private GameObject _meleeButtonGO;
        private GameObject _repairButtonGO;
        private List<GameObject> _actionRoomButtonGO;

        private bool _isMoveActionSelected;
        private bool _isShootActionSelected;
        private bool _isMeleeActionSelected;
        private bool _isRepairButtonSelected;
        public int? SelectedRoomActionIndex { get; private set; }

        public Player CurrentPlayer { get; private set; }
        public GameObject SelectedGameObject { get; private set; }

        private void Awake()
        {
            if(Ship._instanceShip == null)
            {
                Ship._instanceShip = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            Rooms = new List<Room>();
            Corridors = new List<Corridor>();
            Players = new List<Player>();
            Intruders = new List<Intruder>();
            EscapePods = new List<EscapePod>();
            DirectionsArray = new DirectionEnum[]{ DirectionEnum.Earth, DirectionEnum.Mars, DirectionEnum.Venus, DirectionEnum.DeepSpace };
            _engineStatus = new bool[3];
            RoomToDepressurize = null;

            _moveButtonGO = GameObject.Find("MoveButton");
            _shootButtonGO = GameObject.Find("ShootButton");
            _meleeButtonGO = GameObject.Find("MeleeButton");
            _repairButtonGO = GameObject.Find("RepairButton");
            _actionRoomButtonGO = new List<GameObject>();
            _actionRoomButtonGO.Add(GameObject.Find("RoomAction1Button"));
            _actionRoomButtonGO.Add(GameObject.Find("RoomAction2Button"));
            _actionRoomButtonGO.Add(GameObject.Find("RoomAction3Button"));

            EscapePodPopUpGO.SetActive(false);

            SelectedRoomActionIndex = null;
        }

        public static Ship GetInstance()
        {
            return _instanceShip;
        }

        void Start()
        {
            //Fetch Board Component
            for (int i = 0; i < transform.childCount; ++i)
            {
                var child = transform.GetChild(i);
                var room = child.gameObject.GetComponent<Room>();
                if (room != null)
                {
                    Rooms.Add(room);
                    if (room.gameObject.GetComponent<Collider>() == null)
                    {
                        var meshCollider = room.gameObject.AddComponent<MeshCollider>();
                        meshCollider.sharedMesh = room.gameObject.GetComponent<MeshFilter>().mesh;
                    }
                }
                else
                {
                    var escapePod = child.gameObject.GetComponent<EscapePod>();
                    if(escapePod != null)
                    {
                        int maxPod;
                        if (PlayerCount <= 2){maxPod = 2;}
                        else if (PlayerCount <= 4){ maxPod = 3; }
                        else { maxPod = 4;}

                        if (EscapePods.Count < maxPod)
                        {
                            EscapePods.Add(escapePod);
                            if (escapePod.gameObject.GetComponent<Collider>() == null)
                            {
                                var meshCollider = escapePod.gameObject.AddComponent<MeshCollider>();
                                meshCollider.sharedMesh = escapePod.gameObject.GetComponent<MeshFilter>().mesh;
                            }
                        }
                        else
                        {
                            GameObject.Destroy(escapePod.gameObject);
                        }
                    }
                }
            }

            //Generate Corridors between adjacent rooms
            GenerateCorridors();

            //Set-up Ship Direction and Engine Status
            DirectionsArray = RandomUtils.DrawWithoutReplacement(DirectionsArray.ToList(), DirectionsArray.Length).ToArray();
            _indexDirection = 2;
            for(int i=0; i< _engineStatus.Length; ++i)
            {
                _engineStatus[i] = RandomUtils.GetRandomBool();
            }

            //First Round Management
            _isSelfDesctructOn = false;
            _selfDestructTrack = null;
            _roundTrack = 16; //TODO double check if 16 or 15

            //Set-up player
            InitPlayers();

            SetRoomInfo();

            SelectedGameObject = null;
        }

        void Update()
        {
            if(CurrentPlayer == null)
            {
                return;
            }

            if (CurrentPlayer.ActionCountTurn >= 2)
            {
                NextPlayer();

                ResetAllActionsToOff();
                ResetAllBoardComponents();
                CurrentPlayer.Visualize();
            }

            if (!CurrentPlayer.IsInCombat())
            {
                SetShootActionOff();
                SetEnableButton(_shootButtonGO.GetComponent<Button>(), false);

                SetMeleeActionOff();
                SetEnableButton(_meleeButtonGO.GetComponent<Button>(), false);

                SetEnableButton(_repairButtonGO.GetComponent<Button>(), (CurrentPlayer.CurrentRoom != null && CurrentPlayer.CurrentRoom.IsBroken));

                for(int i=0; i<_actionRoomButtonGO.Count; ++i)
                {
                    if (_actionRoomButtonGO[i]?.activeSelf ?? false)
                    {
                        SetEnableButton(_actionRoomButtonGO[i].GetComponent<Button>(), (CurrentPlayer.CurrentRoom != null && !CurrentPlayer.CurrentRoom.IsBroken));
                    }
                }     
            }
            else
            {
                SetEnableButton(_shootButtonGO.GetComponent<Button>(), CurrentPlayer.Weapon.AmmoCount > 0);
                SetEnableButton(_meleeButtonGO.GetComponent<Button>(), true);

                SetRepairActionOff();
                SetEnableButton(_repairButtonGO.GetComponent<Button>(), false);

                SelectedRoomActionIndex = null; 
                for (int i = 0; i < _actionRoomButtonGO.Count; ++i)
                {
                    if (_actionRoomButtonGO[i]?.activeSelf ?? false)
                    {
                        SetEnableButton(_actionRoomButtonGO[i].GetComponent<Button>(), false);
                    }
                }
            }
            
            var moveButton_tmp = GameObject.Find("MoveButton")?.GetComponentInChildren<TextMeshProUGUI>();
            if (moveButton_tmp != null)
            {
                if (CurrentPlayer.IsInCombat())
                {
                    moveButton_tmp.text = "Retreat";
                }
                else
                {
                    moveButton_tmp.text = "Move";
                }
            } 

            if (!_isMoveActionSelected && !_isMeleeActionSelected && !_isShootActionSelected && SelectedRoomActionIndex == null)
            {
                ResetAllBoardComponents(true);
            }

            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit raycastHit;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out raycastHit, 100))
                {
                    SelectedGameObject = raycastHit.collider.gameObject;
                    Debug.Log($"Hit on "+ SelectedGameObject.name);

                    if(_isMoveActionSelected){
                        var hitRoom = SelectedGameObject.GetComponent<Room>();
                        if (hitRoom != null)
                        {
                            if (CurrentPlayer != null && CurrentPlayer.CurrentRoom.AdjacentRooms.Contains(hitRoom))
                            {
                                var corridor = CurrentPlayer.CurrentRoom.Corridors.Values.Where(
                                    c => (c.Room1 == CurrentPlayer.CurrentRoom && (c as RegularCorridor)?.Room2 == hitRoom) ||
                                         (c.Room1 == hitRoom && (c as RegularCorridor)?.Room2 == CurrentPlayer.CurrentRoom)
                                ).First() as RegularCorridor;

                                if (corridor?.Door == DoorEnum.Closed)
                                {
                                    SelectedGameObject.GetComponent<MeshRenderer>().SetMaterials(new List<Material>() { ErrorMaterial });
                                }
                                else
                                {
                                    CurrentPlayer.PerformMoveAction(hitRoom);
                                }
                            }
                            else
                            {
                                SelectedGameObject.GetComponent<MeshRenderer>().SetMaterials(new List<Material>() { ErrorMaterial });
                            }
                        }
                    }
                
                    if(_isMeleeActionSelected || _isShootActionSelected)
                    {
                        var hitIntruder = SelectedGameObject.GetComponent<Intruder>();
                        if (hitIntruder != null && hitIntruder.CurrentRoom == CurrentPlayer.CurrentRoom)
                        {
                            CurrentPlayer.PerformFightAction(hitIntruder, _isShootActionSelected);
                        }
                        else
                        {
                            SelectedGameObject.GetComponent<MeshRenderer>().SetMaterials(new List<Material>() { ErrorMaterial });
                        }
                    }

                    if (SelectedRoomActionIndex != null)
                    {
                        bool success = CurrentPlayer.PerformRoomAction((int)SelectedRoomActionIndex);

                        if(!success)
                        {
                            Debug.Log("Room action unsuccessful");
                            if(SelectedGameObject != null)
                            {
                                SelectedGameObject.GetComponent<MeshRenderer>().SetMaterials(new List<Material>() { ErrorMaterial });
                            }
                        }
                    }
                }
                else
                {
                    SelectedGameObject = null;
                }
            }
        }

        private void InitPlayers()
        {
            var playerGOs = RandomUtils.DrawWithoutReplacement(PlayerPrefabs, PlayerCount);

            List<int> playerOrdersSource = new List<int>();
            for(int i=1; i<=PlayerCount; i++)
            {
                playerOrdersSource.Add(i);
            }

            var playerOrders = RandomUtils.DrawWithoutReplacement(playerOrdersSource, PlayerCount);

            var hibernationRoom = Rooms.Find(r => r.transform.name == "Hibernation");
            for (int i=0; i<PlayerCount; ++i)
            {
                var playerGo = GameObject.Instantiate(playerGOs[i]);
                playerGo.transform.SetParent(transform);
                var player = playerGo.GetComponent<Player>();
            
                player.PlayerOrder = playerOrders[i];
                if (player.PlayerOrder == 1){
                    CurrentPlayer = player;
                }

                playerGo.name = $"Player {player.PlayerOrder}";

                hibernationRoom.PlacePlayerInRoom(player);

                Players.Add(player);
            }

            CurrentPlayer = Players.Find(p => p.PlayerOrder == 1);
        }

        internal void NextPlayer()
        {
            Player next;
            int startIndex = Players.IndexOf(CurrentPlayer);

            CurrentPlayer = null;
            for (int i = 1; i <= Players.Count; ++i)
            {
                int indexNextPlayer = (startIndex + i) % Players.Count;
                next = Players[indexNextPlayer];

                if (!next.IsDead && !next.IsHibernating && !next.HasEscape)
                {
                    CurrentPlayer = next;
                    break;
                }
            }

            if (CurrentPlayer == null)
            {
                Debug.LogWarning("No more active players in game");
                return;
            }

            CurrentPlayer.ResetTurnActionCount();
            Player.UpdateHealthStatCurrentPlayerDebug();
            SetRoomInfo();
        }

        private void VisualizeSelectableIntruders()
        {
            for(int i = 0; i < CurrentPlayer.CurrentRoom.Intruders.Count; ++i)
            {
                CurrentPlayer.CurrentRoom.Intruders[i].Visualize();
            }
        }

        private void VisulizeSelectableForRoomAction(string roomAction)
        {
            if (roomAction == "Depressurize")
            {
                VisualizeYellowRoomExcludingCurrent();
            }
            else if (roomAction == "Check Engine")
            {
                VisualizeEngines();
            }else if (roomAction == "Check Unexplored Room")
            {
                VizualizeUnexploredRooms();
            }else if (roomAction == "Anti-Fire Procedure")
            {
                VizualizeRoomsWithFireOrIntruder();
            }else if(roomAction == "Enter Escape Pod")
            {
                VisualizeAvailableEscapePod();
            }
            else
            {
                ResetAllBoardComponents(true);
            }
        }

        private void VisualizeYellowRoomExcludingCurrent()
        {
            var yellowRooms = Rooms.Where(r => r.RoomType == RoomTypeEnum.Yellow);

            foreach (var room in yellowRooms)
            {
                if(room != CurrentPlayer.CurrentRoom)
                    room.Vizualize();
            }
        }

        private void VisualizeEngines()
        {
            var engineRooms = Rooms.Where(r => r.GetRoomFunctionName().StartsWith("Engine"));

            foreach (var room in engineRooms)
            {
                room.Vizualize();
            }
        }

        private void VizualizeUnexploredRooms()
        {
            var unexploredRoom = Rooms.Where(r => r.RoomType == RoomTypeEnum.Unknown);

            foreach (var room in unexploredRoom)
            {
                room.Vizualize();
            }
        }

        private void VizualizeRoomsWithFireOrIntruder()
        {
            for (int i = 0; i < Rooms.Count; i++)
            {
                var room = Rooms[i];
                if(room.IsOnFire || room.Intruders.Count > 0)
                {
                    room.Vizualize();
                }
            }
        }

        private void VisualizeAvailableEscapePod()
        {
            string s = "";
            for (int i = 0; i < EscapePods.Count; i++)
            {
                var pod = EscapePods[i];
                if(!pod.IsLocked && pod.EvacuationSection == CurrentPlayer.CurrentRoom)
                {
                    pod.Vizualize();
                    s += $"{pod.name}, ";
                }
            }
            Debug.Log("Available Pod : " + s);
        }

        private void ResetAllBoardComponents(bool skipCurrentPlayer = false)
        {
            for (int i = 0; i < Rooms.Count; i++)
            {
                Rooms[i].ResetMaterial();
            }

            for (int i = 0; i < Intruders.Count; ++i)
            {
                Intruders[i].ResetMaterial();
            }

            for (int i = 0; i < Players.Count; ++i)
            {
                if (skipCurrentPlayer && CurrentPlayer == Players[i])
                    continue;

                Players[i].ResetMaterial();
            }

            for (int i = 0; i < EscapePods.Count; ++i)
            {
                EscapePods[i].ResetMaterial();
            }
        }

        private void GenerateCorridors()
        {
            int corridorCount = 0;

            string debugString = "Generation order : ";
            for (int iRoom = 0; iRoom < Rooms.Count; ++iRoom)
            {
                Room room = Rooms[iRoom];
                debugString += room.name+", ";

                Debug.Log("Set Corridors for " + room.gameObject.name);
                for (int iAdjacent = 0; iAdjacent < room.AdjacentRooms.Count; ++iAdjacent)
                {
                    Room adjacentRoom = room.AdjacentRooms[iAdjacent];
                    Debug.Log("Set Corridor between " + room.gameObject.name + " and " +adjacentRoom.gameObject.name);

                    //Skip already generated Corridors
                    if (Corridors.Exists(c => (c as RegularCorridor) != null && (c as RegularCorridor).Room2 == room && c.Room1 == adjacentRoom))
                    {
                        continue;
                    }

                    //Manage Double Corridors
                    var corridor = Corridors.Find(c => (c as RegularCorridor) != null && c.Room1 == room && (c as RegularCorridor).Room2 == adjacentRoom) as RegularCorridor;
                    GameObject corridorGO;
                    MeshRenderer meshRenderer;
                    int corridorNumber = 0;
                    if (corridor != null)
                    {
                        corridorNumber = room.FreeCorridorNumber().Find(n => adjacentRoom.FreeCorridorNumber().Contains(n));
                        corridorNumber = room.AddCorridor(corridor, corridorNumber);
                        Debug.Log("Second Corridor Number is " + corridorNumber);
                        if (corridorNumber == -1)
                        {
                            continue;
                        }
                        adjacentRoom.AddCorridor(corridor, corridorNumber);
                        corridor.IsDoubleCorridor = true;

                        corridorGO = corridor.gameObject;
                        meshRenderer = corridorGO.transform.GetComponent<MeshRenderer>();
                        meshRenderer.SetMaterials(new List<Material>() { DoubleCorridorMaterial });
                        continue;
                    }

                    //Generate Simple Corridors
                    corridorGO = new GameObject();
                    corridorGO.transform.parent = transform;
                    corridorGO.transform.name = $"Corridor ({++corridorCount})";

                    corridor = corridorGO.transform.AddComponent<RegularCorridor>();
                    corridor.Init(room, adjacentRoom);

                    corridorNumber = room.FreeCorridorNumber().Find(n => adjacentRoom.FreeCorridorNumber().Contains(n));
                    corridorNumber = room.AddCorridor(corridor, corridorNumber);
                    Debug.Log("First Corridor Number is " + corridorNumber);
                    if (corridorNumber == -1)
                    {
                        continue;
                    }
                    adjacentRoom.AddCorridor(corridor, corridorNumber);
                    Corridors.Add(corridor);

                    var collider = corridorGO.GetComponent<MeshCollider>();
                    GameObject.Destroy(collider);

                    var corridorMesh = corridorGO.AddComponent<MeshFilter>().mesh;
                    corridorMesh.Clear();

                    List<Vector3> vertices = new List<Vector3>();
                    List<int> triangles = new List<int>();

                    Vector3 LenghtCorridorVector = new Vector3(
                        adjacentRoom.transform.position.x - room.transform.position.x,
                        adjacentRoom.transform.position.y - room.transform.position.y,
                        adjacentRoom.transform.position.z - room.transform.position.z
                        );

                    corridorGO.transform.position = (adjacentRoom.transform.position + room.transform.position) / 2.0f;
                    Matrix4x4 worldToCorridor = corridor.transform.worldToLocalMatrix;

                    for (int i = 0; i < 4; ++i)
                    {
                        Vector3 posRoom = (i < 2) ? room.transform.position : adjacentRoom.transform.position;

                        var WidthCorridorVector = Vector3.Cross(LenghtCorridorVector.normalized, Vector3.up.normalized);
                        var vertex = new Vector3(
                            posRoom.x + (WidthCorridorVector.x * ((1-2*(i % 2))) * CorridorThickness/2), 
                            0.0f, 
                            posRoom.z + (WidthCorridorVector.z * ((1-2*(i % 2))) * CorridorThickness/2));
                        vertices.Add(worldToCorridor.MultiplyPoint3x4(vertex));
                    }

                    triangles.Add(0);
                    triangles.Add(2);
                    triangles.Add(1);

                    triangles.Add(1);
                    triangles.Add(2);
                    triangles.Add(3);

                    corridorMesh.SetVertices(vertices);
                    corridorMesh.SetTriangles(triangles, 0);
                    corridorMesh.RecalculateNormals();

                    meshRenderer = corridorGO.transform.AddComponent<MeshRenderer>();
                    meshRenderer.SetMaterials(new List<Material>() { SingleCorridorMaterial });
                }
            }

            Debug.Log(debugString);
        }
        
        public void FlipEngine(int index)
        {
            _engineStatus[index] = !_engineStatus[index];
        }

        public bool TurnSelfDestructOnOff(bool? isOn)
        {
            if(isOn == null)
            {
                _isSelfDesctructOn = !_isSelfDesctructOn;
            }
            else
            {
                _isSelfDesctructOn = (bool)isOn;
            }

            if(_isSelfDesctructOn)
            {
                _selfDestructTrack = 6;
            }

            return _isSelfDesctructOn;
        }

        public Intruder ResolveIntruderEncounter()
        {
            var intruderToken = EncounterManager.DrawEncounter();

            if(intruderToken == null)
            {
                Debug.LogWarning("There are no Intruder Tokens left in the bag");
                return null;
            }

            Debug.Log($"{intruderToken.Item1} ({intruderToken.Item2})");

            if(intruderToken.Item1 == IntruderTypeEnum.Blank)
            {
                CurrentPlayer.NoiseRollDanger();
                if (EncounterManager.IsBagEmpty())
                {
                    Debug.Log("Adult Token Added in the bag");
                    EncounterManager.AddToIntruderBag(IntruderTypeEnum.Adult);
                }

                EncounterManager.AddToIntruderBag(IntruderTypeEnum.Blank);
                return null;
            }

            var IntruderGO = GameObject.Instantiate(IntruderPrefabs.Find(x => x.GetComponent<Intruder>().IntruderType == intruderToken.Item1));
            var newIntruder = IntruderGO.GetComponent<Intruder>();
            newIntruder.SurpriseAttackCount = intruderToken.Item2;
            CurrentPlayer.CurrentRoom.PlaceIntruderInRoom(newIntruder);
            Intruders.Add(newIntruder);
            IntruderGO.name = newIntruder.IntruderType.ToString();

            if(CurrentPlayer.CurrentRoom.GetRoomFunctionName().StartsWith("Evacuation Section"))
            {
                foreach(var pod in EscapePods)
                {
                    if (pod.EvacuationSection == CurrentPlayer.CurrentRoom)
                    {
                        foreach (var player in pod.PlayersInPod)
                        {
                            pod.RemovePlayerFromPod(player);
                            pod.EvacuationSection.PlacePlayerInRoom(player);
                        }
                    }
                }
            }

            return newIntruder;
        }

        public void EscapePodLaunchOrWait(int button)
        {
            if (CurrentPlayer.EscapePod == null)
            {
                Debug.LogWarning($"{CurrentPlayer.name} is not in an escape pod");
            }
            else
            {
                switch (button)
                {
                    case 0: //Launch
                        CurrentPlayer.EscapePod.Launch();
                        NextPlayer();
                        break;
                    case 1: //Wait
                        //TODO : PASS
                        NextPlayer();
                        break;
                    case 2: //Exit
                        var EvacutationSection = CurrentPlayer.EscapePod.EvacuationSection;
                        CurrentPlayer.EscapePod.RemovePlayerFromPod(CurrentPlayer);
                        EvacutationSection.PlacePlayerInRoom(CurrentPlayer);
                        break;
                }
                
                ResetAllActionsToOff();
            }

            EscapePodPopUpGO.SetActive(false);
        }

        internal void DebugShip()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach(Room room in Rooms)
            {
                string s = $"{room.name} : {room.transform.position}, hasTechCorridor : {room.HasTechnicalCorridor}";
                stringBuilder.AppendLine(s);
                foreach(var corridor in room.Corridors)
                {
                    bool isTechnical = (corridor.Value as TechnicalCorridor) != null;
                    s = $"Corridor {corridor.Key} : {corridor.Value.name} isTechnical : {isTechnical}, hasNoise :{corridor.Value.HasNoise}, {(isTechnical?"":$"Between {corridor.Value.Room1.name} and {(corridor.Value as RegularCorridor).Room2.name}")}";
                    stringBuilder.AppendLine(s);
                }
                stringBuilder.AppendLine("");
            }

            Debug.Log(stringBuilder.ToString());
        }

        private void ResetAllActionsToOff()
        {
            SetMoveActionOff();
            SetShootActionOff();
            SetMeleeActionOff();
            SetRepairActionOff();

            for(int i= 0; i< _actionRoomButtonGO.Count; ++i)
            {
                if (_actionRoomButtonGO[i] != null)
                {
                    SetRoomActionOff(i);
                }
            }
        }

        public void SetMoveActionOnOff_UI()
        {
            bool previousValue = _isMoveActionSelected;
            ResetAllActionsToOff();
            
            if(!previousValue)
            {
                _isMoveActionSelected = true;
                _moveButtonGO.GetComponent<Image>().material = SelectedButtonMaterial;

                ResetAllBoardComponents(true);
                CurrentPlayer.CurrentRoom.VisualizeRoomAndAdjacents();
            }
            else
            {
                ResetAllBoardComponents(true);
            }
            
        }

        internal void SetMoveActionOff()
        {
            if (!_isMoveActionSelected) return;

            _isMoveActionSelected = false;
            _moveButtonGO.GetComponent<Image>().material = DefaultButtonMaterial;
        }

        public void SetShootActionOnOff_UI()
        {
            bool previousValue = _isShootActionSelected;
            ResetAllActionsToOff();

            if (!previousValue)
            {
                _isShootActionSelected = true;
                _shootButtonGO.GetComponent<Image>().material = SelectedButtonMaterial;

                ResetAllBoardComponents(true);
                VisualizeSelectableIntruders();
            }
            else
            {
                ResetAllBoardComponents(true);
            }
        }

        internal void SetShootActionOff()
        {
            if (!_isShootActionSelected) return;

            _isShootActionSelected = false;
            _shootButtonGO.GetComponent<Image>().material = DefaultButtonMaterial;
        }

        public void SetMeleeActionOnOff_UI()
        {
            bool previousValue = _isMeleeActionSelected;
            ResetAllActionsToOff();

            if (!previousValue)
            {
                _isMeleeActionSelected = true;
                _meleeButtonGO.GetComponent<Image>().material = SelectedButtonMaterial;

                ResetAllBoardComponents(true);
                VisualizeSelectableIntruders();
            }
            else
            {
                ResetAllBoardComponents(true);
            }
        }

        internal void SetMeleeActionOff()
        {
            if (!_isMeleeActionSelected) return;

            _isMeleeActionSelected = false;
            _meleeButtonGO.GetComponent<Image>().material = DefaultButtonMaterial;
        }

        public void SetRepairActionOnOff_UI()
        {
            ResetAllActionsToOff();
            ResetAllBoardComponents(true);

            CurrentPlayer.PerformRepairAction();
            _isRepairButtonSelected = false;
        }

        internal void SetRepairActionOff()
        {
            if (!_isRepairButtonSelected) return;

            _isRepairButtonSelected = false;
            _repairButtonGO.GetComponent<Image>().material = DefaultButtonMaterial;
        }


        public void SetRoomActionOnOff_UI(int index)
        {
            int? previousValue = SelectedRoomActionIndex;
            ResetAllActionsToOff();

            if (previousValue == null || previousValue != index)
            {
                SelectedRoomActionIndex = index;
                if (CurrentPlayer.CurrentRoom.IsRoomActionAuto(index))
                {
                    bool success = CurrentPlayer.PerformRoomAction(index);
                    if (success)
                    {
                        SelectedRoomActionIndex = null;
                    }
                }
                else
                {
                    ResetAllBoardComponents(true);
                    VisulizeSelectableForRoomAction(CurrentPlayer.CurrentRoom.GetRoomActionName(index));
                    _actionRoomButtonGO[index].GetComponent<Image>().material = SelectedButtonMaterial;
                } 
            }
            else
            {
                SelectedRoomActionIndex = null;
                ResetAllBoardComponents(true);
                _actionRoomButtonGO[index].GetComponent<Image>().material = DefaultButtonMaterial;
            }
        }

        internal void SetRoomActionOff(int index)
        {
            if (SelectedRoomActionIndex == index)
            {
                SelectedRoomActionIndex = null;
                _actionRoomButtonGO[index].GetComponent<Image>().material = DefaultButtonMaterial;
            }
        }

        private void SetEnableButton(Button button, bool enable) 
        {
            if (button.enabled == enable)
                return;

            button.enabled = enable;

            if (enable)
            {
                button.gameObject.GetComponent<Image>().material = DefaultButtonMaterial;
            }
            else
            {
                button.gameObject.GetComponent<Image>().material = DisabledButtonMaterial;
            }
        }

        public void SetRoomInfo()
        {
            GameObject labelGO = GameObject.Find("RoomInfoLabel");
           
            Room room = CurrentPlayer.CurrentRoom;
            if (room  == null && CurrentPlayer.EscapePod != null)
            {
                room = CurrentPlayer.EscapePod.EvacuationSection;
                EscapePodPopUpGO.SetActive(true);
                EscapePodPopUpGO.transform.Find("ExitButton").gameObject.SetActive(true);
            }

            labelGO.GetComponent<TextMeshProUGUI>().text = CurrentPlayer.CurrentRoom.GetRoomInfo();

            int actionCount = room.GetRoomActionCount();

            for (int i=_actionRoomButtonGO.Count; i>0; --i)
            {
                if(i > actionCount)
                {
                    _actionRoomButtonGO[i-1].SetActive(false);
                }
                else
                {
                    _actionRoomButtonGO[i-1].SetActive(true);
                    var roomActionButton_tmp = _actionRoomButtonGO[i - 1].GetComponentInChildren<TextMeshProUGUI>();
                    if (roomActionButton_tmp != null)
                    {
                        roomActionButton_tmp.text = room.GetRoomActionName(i-1);
                    }
                }
            }
        }
    }
}

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

        private GameObject _moveButtonGO;
        private GameObject _shootButtonGO;
        private GameObject _meleeButtonGO;
        private GameObject _actionRoom1ButtonGO;
        private GameObject _repairButtonGO;

        private bool _isMoveActionSelected;
        private bool _isShootActionSelected;
        private bool _isMeleeActionSelected;
        private bool _isRoomAction1Selected;
        private bool _isRepairButtonSelected;

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
            _actionRoom1ButtonGO = GameObject.Find("RoomAction1Button");
            _repairButtonGO = GameObject.Find("RepairButton");

            _isMoveActionSelected = false;
            _isShootActionSelected = false;
            _isMeleeActionSelected = false;
            _isRoomAction1Selected = false;
            _isRepairButtonSelected = false;
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

        internal void NextPlayer()
        {
            int indexNextPlayer = (Players.IndexOf(CurrentPlayer) + 1) % Players.Count;
            CurrentPlayer = Players[indexNextPlayer];
            CurrentPlayer.ResetTurnActionCount();
            Player.UpdateHealthStatCurrentPlayerDebug();
        }

        void Update()
        {
            if (CurrentPlayer.ActionCountTurn >= 2)
            {
                do
                {
                    NextPlayer();
                } while (CurrentPlayer.IsDead);
                
                ResetAllActionsToOff();
                ResetAllBoardComponents();
                CurrentPlayer.Visualize();
                SetRoomInfo();
            }

            if (!CurrentPlayer.IsInCombat())
            {
                SetShootActionOff();
                SetEnableButton(_shootButtonGO.GetComponent<Button>(), false);

                SetMeleeActionOff();
                SetEnableButton(_meleeButtonGO.GetComponent<Button>(), false);

                SetEnableButton(_repairButtonGO.GetComponent<Button>(), CurrentPlayer.CurrentRoom.IsBroken);

                if (_actionRoom1ButtonGO?.activeSelf ?? false)
                {
                    SetEnableButton(_actionRoom1ButtonGO.GetComponent<Button>(), !CurrentPlayer.CurrentRoom.IsBroken);
                }         
            }
            
            else
            {
                SetEnableButton(_shootButtonGO.GetComponent<Button>(), CurrentPlayer.Weapon.AmmoCount > 0);
                SetEnableButton(_meleeButtonGO.GetComponent<Button>(), true);

                SetRepairActionOff();
                SetEnableButton(_repairButtonGO.GetComponent<Button>(), false);

                SetRoomAction1Off();
                if (_actionRoom1ButtonGO?.activeSelf ?? false)
                {
                    SetEnableButton(_actionRoom1ButtonGO.GetComponent<Button>(), false);
                }
            }
            
            var moveButton_tmp = GameObject.Find("MoveButton")?.GetComponentInChildren<TextMeshProUGUI>();
            if (moveButton_tmp != null)
            {
                if (CurrentPlayer.IsInCombat())
                {
                    moveButton_tmp.text = "Escape";
                }
                else
                {
                    moveButton_tmp.text = "Move";
                }
            } 

            if (!_isMoveActionSelected && !_isMeleeActionSelected && !_isShootActionSelected && !_isRoomAction1Selected)
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

                    if (_isRoomAction1Selected)
                    {
                        bool success = CurrentPlayer.PerformRoomAction(0);

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
                if(!pod.IsLocked && pod.EscapeSection == CurrentPlayer.CurrentRoom)
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

            return newIntruder;
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
            SetRoomAction1Off();
            SetRepairActionOff();
        }

        public void SetMoveActionOnOff_UI()
        {
            _isMoveActionSelected = !_isMoveActionSelected;

            if(_isMoveActionSelected)
            {
                _moveButtonGO.GetComponent<Image>().material = SelectedButtonMaterial;

                SetShootActionOff();
                SetMeleeActionOff();
                SetRoomAction1Off();
                SetRepairActionOff();

                ResetAllBoardComponents(true);
                CurrentPlayer.CurrentRoom.VisualizeRoomAndAdjacents();
            }
            else
            {
                ResetAllBoardComponents(true);
                _moveButtonGO.GetComponent<Image>().material = DefaultButtonMaterial;
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
            _isShootActionSelected = !_isShootActionSelected;

            if (_isShootActionSelected)
            {
                _shootButtonGO.GetComponent<Image>().material = SelectedButtonMaterial;

                SetMoveActionOff();
                SetMeleeActionOff();
                SetRoomAction1Off();
                SetRepairActionOff();

                ResetAllBoardComponents(true);
                VisualizeSelectableIntruders();
            }
            else
            {
                ResetAllBoardComponents(true);
                _shootButtonGO.GetComponent<Image>().material = DefaultButtonMaterial;
            }

            Debug.Log("Button Click: " + _shootButtonGO.GetComponent<Image>().material.name);

        }

        internal void SetShootActionOff()
        {
            if (!_isShootActionSelected) return;

            _isShootActionSelected = false;
            _shootButtonGO.GetComponent<Image>().material = DefaultButtonMaterial;
        }

        public void SetMeleeActionOnOff_UI()
        {
            _isMeleeActionSelected = !_isMeleeActionSelected;

            if (_isMeleeActionSelected)
            {
                _meleeButtonGO.GetComponent<Image>().material = SelectedButtonMaterial;

                SetMoveActionOff();
                SetShootActionOff();
                SetRoomAction1Off();
                SetRepairActionOff();

                ResetAllBoardComponents(true);
                VisualizeSelectableIntruders();
            }
            else
            {
                ResetAllBoardComponents(true);
                _meleeButtonGO.GetComponent<Image>().material = DefaultButtonMaterial;
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
            _isRepairButtonSelected = !_isRepairButtonSelected;

            if (_isRepairButtonSelected)
            {
                _repairButtonGO.GetComponent<Image>().material = SelectedButtonMaterial;

                SetMoveActionOff();
                SetShootActionOff();
                SetMeleeActionOff();
                SetRoomAction1Off();

                ResetAllBoardComponents(true);

                CurrentPlayer.PerformRepairAction();

                _isRepairButtonSelected = false;
            }
        }

        internal void SetRepairActionOff()
        {
            if (!_isRepairButtonSelected) return;

            _isRepairButtonSelected = false;
            _repairButtonGO.GetComponent<Image>().material = DefaultButtonMaterial;
        }


        public void SetRoomAction1OnOff_UI()
        {
            _isRoomAction1Selected = !_isRoomAction1Selected;

            if (_isRoomAction1Selected)
            {
                SetMoveActionOff();
                SetShootActionOff();
                SetMeleeActionOff();
                SetRepairActionOff();

                if (CurrentPlayer.CurrentRoom.IsRoomActionAuto(0))
                {
                    bool success = CurrentPlayer.PerformRoomAction(0);
                    if (success)
                    {
                        SetRoomAction1Off();
                    }
                }
                else
                {
                    ResetAllBoardComponents(true);
                    VisulizeSelectableForRoomAction(CurrentPlayer.CurrentRoom.GetRoomActionName(0));
                    _actionRoom1ButtonGO.GetComponent<Image>().material = SelectedButtonMaterial;
                } 
            }
            else
            {
                ResetAllBoardComponents(true);
                _actionRoom1ButtonGO.GetComponent<Image>().material = DefaultButtonMaterial;
            }
        }

        internal void SetRoomAction1Off()
        {
            if (!_isRoomAction1Selected) return;

            _isRoomAction1Selected = false;
            _actionRoom1ButtonGO.GetComponent<Image>().material = DefaultButtonMaterial;
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
            labelGO.GetComponent<TextMeshProUGUI>().text = CurrentPlayer.CurrentRoom.GetRoomInfo();

            if(CurrentPlayer.CurrentRoom.GetRoomActionCount() == 0)
            {
                _actionRoom1ButtonGO.SetActive(false);
            }
            else
            {
                _actionRoom1ButtonGO.SetActive(true);
                var roomAction1Button_tmp = GameObject.Find("RoomAction1Button")?.GetComponentInChildren<TextMeshProUGUI>();
                if (roomAction1Button_tmp != null)
                {
                    roomAction1Button_tmp.text = CurrentPlayer.CurrentRoom.GetRoomActionName(0);
                }
            }
        }
    }
}

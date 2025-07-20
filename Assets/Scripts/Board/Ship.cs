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

        public float CorridorThickness;
        public GameObject TechnicalCorridorMarkerPrefab;
        public GameObject NoiseMarkerPrefab;
        public GameObject ClosedDoorPrefab;
        public GameObject BrokenDoorPrefab;
        public List<GameObject> IntruderPrefabs;
        public List<GameObject> PlayerPrefabs;
        public Material DefaultButtonMaterial;
        public Material DisabledButtonMaterial;
        public Material SelectedButtonMaterial;
        public Material SelectedMaterial;
        public Material AdjacentMaterial;
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

        private bool _isMoveActionSelected;
        private bool _isShootActionSelected;
        private bool _isMeleeActionSelected;

        public Player CurrentPlayer { get; private set; }

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

            _isMoveActionSelected = false;
            _isShootActionSelected = false;
            _isMeleeActionSelected = false;
        }

        public static Ship GetInstance()
        {
            return _instanceShip;
        }

        void Start()
        {
            //Fetch Board Component
            Rooms = new List<Room>();
            Players = new List<Player>();
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
            }

            //Generate Corridors between adjacent rooms
            GenerateCorridors();

            //Set-up player
            InitPlayers();

            SetRoomInfo();
        }

        void Update()
        {
            if (CurrentPlayer.ActionCountTurn >= 2)
            {
                int indexNextPlayer = (Players.IndexOf(CurrentPlayer) + 1) % Players.Count;
                CurrentPlayer = Players[indexNextPlayer];
                CurrentPlayer.ResetTurnActionCount();

                _isMeleeActionSelected = false;
                _isMoveActionSelected = false;
                _isShootActionSelected = false;
            }

            if (!_isShootActionSelected)
            {
                var shootButton = GameObject.Find("ShootButton")?.GetComponent<Button>();
                SetEnableButton(shootButton, CurrentPlayer.IsInCombat());
            }

            if (!_isMeleeActionSelected)
            {
                var meleeButton = GameObject.Find("MeleeButton")?.GetComponent<Button>();
                SetEnableButton(meleeButton, CurrentPlayer.IsInCombat());
            }

            if (!_isMoveActionSelected && !_isMeleeActionSelected && !_isShootActionSelected)
            {
                ResetAllBoardComponents();
            }

            CurrentPlayer.Visualize();

            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit raycastHit;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out raycastHit, 100))
                {
                    var hitGo = raycastHit.collider.gameObject;
                    Debug.Log($"Hit on "+ hitGo.name);

                    if(_isMoveActionSelected){
                        var hitRoom = hitGo.GetComponent<Room>();
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
                                    hitGo.GetComponent<MeshRenderer>().SetMaterials(new List<Material>() { ErrorMaterial });
                                    Debug.Log($"Cannot move to room {hitRoom.name} from {CurrentPlayer.CurrentRoom.name} because the door is closed in {corridor.name}");
                                }
                                else
                                {
                                    CurrentPlayer.PerformMoveAction(hitRoom);
                                    ResetAllBoardComponents();
                                }
                            }
                            else
                            {
                                hitGo.GetComponent<MeshRenderer>().SetMaterials(new List<Material>() { ErrorMaterial });
                            }
                        }
                    }
                
                    if(_isMeleeActionSelected || _isShootActionSelected)
                    {
                        var hitIntruder = hitGo.GetComponent<Intruder>();
                        if (hitIntruder != null)
                        {
                            if (hitIntruder.CurrentRoom == CurrentPlayer.CurrentRoom)
                            {
                                CurrentPlayer.PerformFightAction(hitIntruder, _isShootActionSelected);
                            }
                            else
                            {
                                hitGo.GetComponent<MeshRenderer>().SetMaterials(new List<Material>() { ErrorMaterial });
                            }
                        }
                    }
                }
                else
                {
                    ResetAllBoardComponents();
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

        }

        private void VisualizeIntruders()
        {
            for(int i = 0; i < CurrentPlayer.CurrentRoom.Intruders.Count; ++i)
            {
                CurrentPlayer.CurrentRoom.Intruders[i].Visualize();
            }
        }

        private void ResetAllBoardComponents()
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
                Players[i].ResetMaterial();
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

        public void SetMoveActionOnOff()
        {
            GameObject buttonGO = GameObject.Find("MoveButton");
            _isMoveActionSelected = !_isMoveActionSelected;

            if(_isMoveActionSelected)
            {
                buttonGO.GetComponent<Image>().material = SelectedButtonMaterial;
                if (_isShootActionSelected)
                {
                    if (_isMeleeActionSelected) { SetMeleeActionOnOff(); }
                    if (_isShootActionSelected) { SetShootActionOnOff(); }
                }

                CurrentPlayer.CurrentRoom.VisualizeRoomAndAdjacents();
            }
            else
            {
                buttonGO.GetComponent<Image>().material = DefaultButtonMaterial;
            }
            
        }

        public void SetShootActionOnOff()
        {
            GameObject buttonGO = GameObject.Find("ShootButton");
            _isShootActionSelected = !_isShootActionSelected;

            if (_isShootActionSelected)
            {
                buttonGO.GetComponent<Image>().material = SelectedButtonMaterial;
                if (_isShootActionSelected)
                {
                    if (_isMoveActionSelected)  { SetMoveActionOnOff();  }
                    if (_isMeleeActionSelected) { SetMeleeActionOnOff(); }
                }

                VisualizeIntruders();
            }
            else
            {
                buttonGO.GetComponent<Image>().material = DefaultButtonMaterial;
            }

        }

        public void SetMeleeActionOnOff()
        {
            GameObject buttonGO = GameObject.Find("MeleeButton");
            _isMeleeActionSelected = !_isMeleeActionSelected;

            if (_isMeleeActionSelected)
            {
                buttonGO.GetComponent<Image>().material = SelectedButtonMaterial;
                if (_isMeleeActionSelected)
                {
                    if (_isMoveActionSelected)  { SetMoveActionOnOff();  }
                    if (_isShootActionSelected) { SetShootActionOnOff(); }
                }

                VisualizeIntruders();
            }
            else
            {
                buttonGO.GetComponent<Image>().material = DefaultButtonMaterial;
            }

        }

        public void SetEnableButton(Button button, bool enable) 
        { 
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
        }
    }
}

using Randomness;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Board.Rooms;
using Board.Corridors;
using Unity.VisualScripting;
using System;

namespace Board
{
    [DefaultExecutionOrder(3)]
    public class Player : MonoBehaviour
    {
        public PlayerRoleEnum Role;
        internal int PlayerOrder;
        internal int? PosInRoomIndex;

        internal int ActionCountTurn { get; private set; }

        public Room CurrentRoom { get; set; }
        private bool _isSlimed;
        private bool _hasSentSignal;

        private Weapon _weapon;

        private Material _defaultMaterial;
        private void Awake()
        {
            _defaultMaterial = GetComponent<MeshRenderer>().material;
            _isSlimed = false;
            _hasSentSignal = false;
            _weapon = new Weapon("TestWeapon", 5, true);
        }

        public void PerformMoveAction(Room room)
        {
            ActionCountTurn++;
            bool isNoiseRollNeeded = room.IsRoomEmpty();

            //Move
            var origin = CurrentRoom;
            CurrentRoom.RemovePlayerFromRoom(this);
            room.PlacePlayerInRoom(this);

            //Explore room
            if(CurrentRoom.RoomType == RoomTypeEnum.Unknown)
            {
                CurrentRoom.DiscoverRoomTile();
                isNoiseRollNeeded = DiscoverExplorationTokenTile(origin);
            }

            //Slime Room
            if(CurrentRoom.GetRoomFunctionName() == "Slime")
            {
                SetSlime(true);
            }

            //Noise Roll
            if (isNoiseRollNeeded)
            {
                PerformNoiseRoll();
            }

            Ship.GetInstance().SetMoveActionOnOff();
            Ship.GetInstance().SetRoomInfo();
        }

        public void PerformFightAction(Intruder intruder, bool withWeapon) 
        {
            ActionCountTurn++;
            var rollResult = DiceManager.RollCombatDice();
            bool hasHit = false;
            Debug.Log("Roll " + rollResult);

            if(!withWeapon)
            {
                //TODO Take Contamination Card
            }

            switch (rollResult)
            {
                case CombatRollEnum.Blank:
                    break;
                case CombatRollEnum.Creeper:
                    if(intruder.IntruderType == IntruderTypeEnum.Creeper || intruder.IntruderType == IntruderTypeEnum.Larva) 
                    {
                        intruder.DealDamage(1);
                        hasHit = true;
                    }
                    break;
                case CombatRollEnum.Adult:
                    if (intruder.IntruderType == IntruderTypeEnum.Adult ||
                        intruder.IntruderType == IntruderTypeEnum.Creeper || 
                        intruder.IntruderType == IntruderTypeEnum.Larva)
                    {
                        intruder.DealDamage(1);
                        hasHit = true;
                    }
                    break;
                case CombatRollEnum.Shot:
                    intruder.DealDamage(1);
                    hasHit = true;
                    break;
                case CombatRollEnum.DoubleShot:
                    if(withWeapon)
                        intruder.DealDamage(2);
                    else
                        intruder.DealDamage(1);
                    hasHit = true;
                    break;
            }

            if (hasHit)
            {
                intruder.ResolveDeathOrRetreat();
            } 
            else
            {
                if (!withWeapon)
                {
                    //Implement Serious Wound upon combat failure
                }
            }

            //Reset Actions
            if (withWeapon)
            {
                Ship.GetInstance().SetShootActionOnOff();
            }
            else
            {
                Ship.GetInstance().SetMeleeActionOnOff();
            }  
        }

        public void PerformNoiseRoll()
        {
            var diceResult = DiceManager.RollNoiseDice();
            if(!_isSlimed && diceResult == NoiseResultEnum.Silent)
            {
                Debug.Log("Silent Roll");
                return;
            }

            if (diceResult == NoiseResultEnum.Danger || (_isSlimed && diceResult == NoiseResultEnum.Silent))
            {
                Debug.Log("Danger Roll");
                NoiseRollDanger();

                return;
            }

            Debug.Log("Roll " + diceResult);

            if (CurrentRoom.Corridors[(int)diceResult].MakeNoise())
            {
                Debug.Log("Intruder Encounter");
                Ship.GetInstance().ResolveIntruderEncounter();

                //TODO implement suprise attack from result of Intruder Encounter

                foreach (var corridor in CurrentRoom.Corridors.Values.Distinct())
                {
                    corridor.ClearNoise();
                }
            }
        }

        internal void NoiseRollDanger()
        {
            var adjacentIntruders = new List<Intruder>();
            for(int i = 0; i<CurrentRoom.AdjacentRooms.Distinct().Count(); i++)
            {
                adjacentIntruders.AddRange(CurrentRoom.AdjacentRooms.Distinct().ToList()[i].Intruders);
            }

            var intrudersToBeMoved = adjacentIntruders.Where(intr => !intr.IsInCombat()).ToList();
            bool intrudersHasMoved = intrudersToBeMoved.Count()>0;
            for(int i = 0; i<intrudersToBeMoved.Count(); i++)
            {
                intrudersToBeMoved[i].CurrentRoom.RemoveIntruderFromRoom(intrudersToBeMoved[i]);
                CurrentRoom.PlaceIntruderInRoom(intrudersToBeMoved[i]);
            }

            if(intrudersHasMoved)
            {
                return;
            }


            foreach (var corridor in CurrentRoom.Corridors.Values.Distinct())
            {
                if (!corridor.HasNoise)
                {
                    corridor.MakeNoise();
                }
            }
        }

        private bool DiscoverExplorationTokenTile(Room origin)
        {
            var explorationToken = RoomExploration.DrawExplorationToken();

            Debug.Log($"Exploration Token : {explorationToken.Item1} ({explorationToken.Item2} object(s))");

            if (CurrentRoom.RoomType != RoomTypeEnum.Special && CurrentRoom.RoomType != RoomTypeEnum.Unknown)
            {
                CurrentRoom.ObjectCount = explorationToken.Item2;
            }

            switch (explorationToken.Item1)
            {
                case ExplorationTokenEnum.Fire:
                    CurrentRoom.IsOnFire = true;
                    break;
                case ExplorationTokenEnum.Broken:
                    CurrentRoom.IsBroken = true;
                    break;
                case ExplorationTokenEnum.Slime:
                    Ship.GetInstance().CurrentPlayer.SetSlime(true);
                    break;
                case ExplorationTokenEnum.Door:
                    //Implement Door Mechanics
                    var corridor = CurrentRoom.Corridors.Values.ToList().Find(c => (c.Room1 == CurrentRoom && (c as RegularCorridor)?.Room2 == origin) || (c.Room1 == origin && (c as RegularCorridor)?.Room2 == CurrentRoom));
                    if (corridor)
                    {
                        (corridor as RegularCorridor).Door = DoorEnum.Closed;

                        Vector3 corridorVec = origin.transform.position - CurrentRoom.transform.position;
                        var doorGO = GameObject.Instantiate(Ship.GetInstance().ClosedDoorPrefab);
                        doorGO.transform.SetParent(corridor.transform);

                        float offset = 0.25f;
                        Vector3 offsetVector = corridorVec.normalized * offset;
                        doorGO.transform.localPosition = new Vector3(offsetVector.x, doorGO.transform.localPosition.y, offsetVector.z);

                        float angle = Vector3.SignedAngle(Vector3.forward, corridorVec.normalized, Vector3.up);
                        doorGO.transform.rotation = Quaternion.Euler(0, angle, 0);

                    }
                    else
                    {
                        Debug.LogWarning($"Could not find a Corridor between {CurrentRoom} and {origin}");
                    }
                    break;
                case ExplorationTokenEnum.Silent:
                    if (_isSlimed)
                    {
                        NoiseRollDanger();
                    }
                    return false;
                case ExplorationTokenEnum.Danger:
                    //Implement Danger;
                    NoiseRollDanger();
                    return false;
            }

            return true;
        }

        public void SetSlime(bool slime)
        {
            _isSlimed = slime;
        }

        public void SendSignal()
        {
            _hasSentSignal = true;
        }

        internal void VisualizePlayer()
        {
            gameObject.GetComponent<MeshRenderer>().SetMaterials(new List<Material>() { Ship.GetInstance().SelectedMaterial });
        }

        internal void ResetMaterial()
        {
            gameObject.GetComponent<MeshRenderer>().SetMaterials(new List<Material>() { _defaultMaterial });
        }

        internal void Visualize()
        {
            gameObject.GetComponent<MeshRenderer>().SetMaterials(new List<Material>() { Ship.GetInstance().AdjacentMaterial });
        }

        internal bool IsInCombat()
        {
            return CurrentRoom.Intruders.Count > 0;
        }

        public void ResetTurnActionCount()
        {
            ActionCountTurn = 0;
        }

        public void ReloadWeaponFull()
        {
            _weapon.AmmoCount = _weapon.AmmoCapacity;
        }

        public void ReloadWeapon(int ammo)
        {
            _weapon.AmmoCount += ammo;
            Math.Min(_weapon.AmmoCount, _weapon.AmmoCapacity);
        }
    }
}


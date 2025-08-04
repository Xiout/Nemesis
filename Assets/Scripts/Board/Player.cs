using Randomness;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Board.Rooms;
using Board.Corridors;
using TMPro;
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

        private bool _isHibernating;
        private bool _hasEscape;

        public Weapon Weapon;

        internal bool IsInfected;
        private int _lightWoundCount;
        private List<SeriousWoundEnum> _seriousWounds;
        private List<bool> _contaminationCards;
        public bool IsDead {  get; private set; }

        private static TextMeshProUGUI _healthStatLabel;

        private Material _defaultMaterial;
        void Awake()
        {
            _defaultMaterial = GetComponent<MeshRenderer>().material;
            _isSlimed = false;
            _hasSentSignal = false;
            Weapon = new Weapon("TestWeapon", 5, true);
            
             IsDead = false;
            IsInfected = false;
            _seriousWounds = new List<SeriousWoundEnum>();
            _contaminationCards = new List<bool>();
            _lightWoundCount = 0;

            _isHibernating = false;
            _hasEscape = false;
        }

        private void Start()
        {
            _healthStatLabel = GameObject.Find("DebugHealthStatLabel")?.GetComponent<TextMeshProUGUI>();
            UpdateHealthStatCurrentPlayerDebug();
        }

        public void PerformMoveAction(Room room)
        {
            ActionCountTurn++;

            if (IsInCombat())
            {
                //Perform Intruder's attacks
                List<Intruder> currentIntruder = new List<Intruder>(CurrentRoom.Intruders);
                for(int i= 0; i< currentIntruder.Count(); ++i)
                {
                    currentIntruder[i].PerformIntruderAttack(this);
                }
            }

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

            Ship.GetInstance().SetMoveActionOff();
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
                TakeContaminationCard();
            }
            else
            {
                --Weapon.AmmoCount;
            }

                switch (rollResult)
                {
                    case CombatRollEnum.Blank:
                        break;
                    case CombatRollEnum.Creeper:
                        if (intruder.IntruderType == IntruderTypeEnum.Creeper || intruder.IntruderType == IntruderTypeEnum.Larva)
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
                        if (withWeapon)
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
                    TakeSeriousDamage();
                }
            }

            //Reset Actions
            if (withWeapon)
            {
                Ship.GetInstance().SetShootActionOff();
            }
            else
            {
                Ship.GetInstance().SetMeleeActionOff();
            }  
        }

        public bool PerformRepairAction()
        {
            if (IsInCombat())
            {
                Debug.LogWarning($"{this.name} cannot repair room {CurrentRoom.name} : In Combat");
                return false;
            }

            if (!CurrentRoom.IsBroken)
            {
                Debug.LogWarning($"{this.name} cannot repair room {CurrentRoom.name} : Room not broken");
            }
            
            CurrentRoom.SetBroken(false);
            ActionCountTurn++;
            return true;
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
                var intruder = Ship.GetInstance().ResolveIntruderEncounter();

                //TODO implement suprise attack from result of Intruder Encounter

                if(intruder != null)
                {
                    foreach (var corridor in CurrentRoom.Corridors.Values.Distinct())
                    {
                        corridor.ClearNoise();
                    }
                }
            }
        }

        public bool PerformRoomAction(int index)
        {
            bool success = CurrentRoom.ExecuteRoomFunction(index);
            if (success)
            {
                ++ActionCountTurn;
                Ship.GetInstance().SetRoomAction1Off();
            }

            return success;
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
                    CurrentRoom.SetOnFire(true);
                    break;
                case ExplorationTokenEnum.Broken:
                    CurrentRoom.SetBroken(true);
                    break;
                case ExplorationTokenEnum.Slime:
                    Ship.GetInstance().CurrentPlayer.SetSlime(true);
                    break;
                case ExplorationTokenEnum.Door:
                    var corridor = CurrentRoom.Corridors.Values.ToList().Find(c => (c.Room1 == CurrentRoom && (c as RegularCorridor)?.Room2 == origin) || (c.Room1 == origin && (c as RegularCorridor)?.Room2 == CurrentRoom));
                    if (corridor != null)
                    {
                        (corridor as RegularCorridor).CloseDoor();
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

        public bool TakeLightDamage(int damage)
        {
            Debug.Log($"Take {damage} light damages");
            if (_seriousWounds.Count>=3)
            {
                Debug.Log($"Player {PlayerOrder} has exceed the maximum damage");
                Death();
                return true;
            }

            _lightWoundCount += damage;
            if(_lightWoundCount >= 3)
            {
                TakeSeriousDamage();
                _lightWoundCount = _lightWoundCount % 3;
            }

            UpdateHealthStatCurrentPlayerDebug();
            return false;
        }

        public void TakeContaminationCard()
        {
            bool card = EventAndIntruderAttackManager.DrawContaminationCard();
            Debug.Log($"Take Contamination Card : {card}");
            _contaminationCards.Add(card);

            UpdateHealthStatCurrentPlayerDebug();
        }

        public int SeriousWoundCount()
        {
            return _seriousWounds.Count;
        }

        public bool TakeSeriousDamage()
        {
            if (_seriousWounds.Count >= 3)
            {
                Debug.Log($"Player {PlayerOrder} has exceed the maximum damage");
                Death();
                return true;
            }
            var card = EventAndIntruderAttackManager.DrawSeriousWoundCard();
            Debug.Log($"Serious Wound : {card}");
           _seriousWounds.Add(card);
            
            UpdateHealthStatCurrentPlayerDebug();
            return false;
        }

        public void Death()
        {
            Debug.Log($"Player {PlayerOrder} ({Role}) died)");
            IsDead = true;
            gameObject.SetActive(false);
            //Todo : Spawn character corpse
            Ship.GetInstance().NextPlayer();
        }

        public void SetSlime(bool slime)
        {
            _isSlimed = slime;

            UpdateHealthStatCurrentPlayerDebug();
        }

        public void SendSignal()
        {
            _hasSentSignal = true;
        }

        internal void Visualize()
        {
            gameObject.GetComponent<MeshRenderer>().SetMaterials(new List<Material>() { Ship.GetInstance().SelectedMaterial });
        }

        internal void ResetMaterial()
        {
            gameObject.GetComponent<MeshRenderer>().SetMaterials(new List<Material>() { _defaultMaterial });
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
            Weapon.AmmoCount = Weapon.AmmoCapacity;
        }

        public void ReloadWeapon(int ammo)
        {
            Weapon.AmmoCount += ammo;
            Math.Min(Weapon.AmmoCount, Weapon.AmmoCapacity);
        }

        public bool Hibernate()
        {
            if (_isHibernating || _hasEscape)
            {
                return false;
            }

            _isHibernating = true;
            CurrentRoom.RemovePlayerFromRoom(this);
            gameObject.SetActive(false);
            return true;
        }
    }

        private string GetHealthStat()
        {

            string seriousWoundsString = "";
            for (int i = 0; i < _seriousWounds.Count; ++i)
                seriousWoundsString += _seriousWounds[i] + ",";

            if (string.IsNullOrEmpty(seriousWoundsString))
            {
                seriousWoundsString = "None";
            }

            return $"Player {PlayerOrder} ({Role})\n" +
                   $"Slimed : {_isSlimed}\n" +
                   $"Infected : {IsInfected}\n" +
                   $"Contamination : {_contaminationCards.Count}\n" +
                   $"Light Wounds : {_lightWoundCount}\n" +
                   $"Serious Wound : "+ seriousWoundsString;
        }

        public static void UpdateHealthStatCurrentPlayerDebug()
        {
            var player = Ship.GetInstance().CurrentPlayer;
            if (_healthStatLabel == null)
            {
                return;
            }

            _healthStatLabel.text = player.GetHealthStat();
        }
    }
}


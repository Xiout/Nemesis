using Board.Corridors;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Board.Rooms
{
    public class RoomFunction
    {
        public string Name { get; set; }
        public RoomTypeEnum Type { get; set; }
        public bool IsRequired { get; set; }
        public bool IsExplorable { get; set; }

        public List<RoomAction> RoomActions { get; set; }

        public RoomFunction(string name, RoomTypeEnum type, bool required, bool isExplorable, List<RoomAction> actions = null)
        {
            Name= name;
            Type = type;
            IsRequired = required;
            IsExplorable = isExplorable;
            RoomActions = actions;
        }

        public bool ExecuteAction(int index)
        {
            if (RoomActions == null || index >= RoomActions.Count)
                return false;

            return RoomActions[index]?.Action?.Invoke() ?? false;
        }

        private static bool Hibernate()
        {
            Debug.Log("Perform Room Action Hibernate");

            var ship = Ship.GetInstance();
            var player = ship.CurrentPlayer;

            //TODO check round number for availability

            player.PerformNoiseRoll();
            if(player.CurrentRoom.Intruders.Count <= 0)
            {
                Debug.Log("Hibernation Successfull");

                player.Hibernate();
            }

            return true;
        }

        private static bool Escape(string section)
        {
            var ship = Ship.GetInstance();
            var player = ship.CurrentPlayer;

            EscapePod pod = ship.SelectedGameObject.GetComponent<EscapePod>();

            if (pod == null)
            {
                Debug.Log($"Could not perform Escape Action on GameObject {ship.SelectedGameObject.name}");
                return false;
            }

            if (pod.IsLocked)
            {
                Debug.Log($"{pod.name} is locked ; Could not perform Escape Action on this escape Pod.");
                return false;
            }

            if (!pod.EscapeSection.GetRoomFunctionName().EndsWith(section))
            {
                Debug.Log($"{pod.name} belongs to the wrong evacuation section ; Could not perform Escape Action on this escape Pod.");
                return false;
            }

            Debug.Log("Perform Room Action Escape");

            player.PerformNoiseRoll();
            if (player.CurrentRoom.Intruders.Count <= 0)
            {
                Debug.Log("Escape Successfull (NOT IMPLEMENTED)");
                //TODO Implement Escape
            }

            return true;
        }

        private static bool Shower() {
            Debug.Log("Perform Room Action Shower");
            Ship.GetInstance().CurrentPlayer.SetSlime(false);
            return true;
        }

        private static bool BreakFixEngine(int index)
        {
            Debug.Log($"Perform Room Action Break/Fix Engine {index+1}");
            Ship.GetInstance().FlipEngine(index);
            return true;
        }

        private static bool SendSignal()
        {
            Debug.Log("Perform Room Action Send Signal");
            Ship.GetInstance().CurrentPlayer.SendSignal();
            return true;
        }

        private static bool ReloadWeapon()
        {
            Debug.Log("Perform Room Action Reload Weapon");
            Ship.GetInstance().CurrentPlayer.ReloadWeapon(2);
            return true;
        }

        private static bool TurnSelfDestructOnOff()
        {
            Debug.Log("Perform Room Action Self-Destruct On/Off");
            Ship.GetInstance().TurnSelfDestructOnOff(null);
            return true;
        }

        private static bool StealEgg()
        {
            Debug.Log("Perform Room Action Steal an Egg");
            var ship = Ship.GetInstance();
            var player = ship.CurrentPlayer;

            player.PerformNoiseRoll();
            if (player.CurrentRoom.Intruders.Count <= 0)
            {
                Debug.Log("Egg Stealth Successful (NOT IMPLEMENTED)");
                //Implement Steal Egg
            }

            return true;
        }

        private static bool AntiFireProcedureOnSelectedRoom()
        {
            Debug.Log("Perform Room Action Anti-Fire Procedure");
            var room = Ship.GetInstance().SelectedGameObject.GetComponent<Room>();
            if (room == null)
            {
                return false;
            }

            if (!room.IsOnFire && room.Intruders.Count <= 0)
            {
                return false;
            }

            for(int i =0; i< room.Intruders.Count; ++i)
            {
                bool hasMoved = room.Intruders[0].MoveIntruderWithCard();
                if (hasMoved)
                {
                    i--;
                }
            }

            room.SetOnFire(false);
            return true;
        }

        private static bool SetDepressurizationOnSelectedRoom()
        {
            Debug.Log("Perform Room Action Depressurization");
            var room = Ship.GetInstance().SelectedGameObject.GetComponent<Room>();
            if (room == null)
            {
                return false;
            }

            if (room == Ship.GetInstance().CurrentPlayer.CurrentRoom || room.RoomType != RoomTypeEnum.Yellow)
            {
                return false;
            }

            foreach (var c in room.Corridors.Values)
            {
                var corridor = (c as RegularCorridor);

                if (corridor == null)
                {
                    continue;
                }

                if(corridor.Door == DoorEnum.Broken)
                {
                    return false;
                }

                corridor.CloseDoor();
            }

            Ship.GetInstance().RoomToDepressurize = room;
            return true;
        }



        public readonly static RoomFunction[] AllRooms =
        {
            new RoomFunction("Hibernation", RoomTypeEnum.Special, true, false, new List<RoomAction> { new RoomAction("Hibernate", true, Hibernate) }),
            new RoomFunction("Cockpit", RoomTypeEnum.Special, true, false), //TODO LATER : check direction, set direction
            new RoomFunction("Engine 1", RoomTypeEnum.Special, true, false, new List<RoomAction> { new RoomAction("Break/Fix Engine", true, () => BreakFixEngine(0)) }), //TODO LATER : check engine
            new RoomFunction("Engine 2", RoomTypeEnum.Special, true, false, new List<RoomAction> { new RoomAction("Break/Fix Engine", true, () => BreakFixEngine(1)) }), //TODO LATER : check engine
            new RoomFunction("Engine 3", RoomTypeEnum.Special, true, false, new List<RoomAction> { new RoomAction("Break/Fix Engine", true, () => BreakFixEngine(2)) }), //TODO LATER : check engine
            new RoomFunction("Armory", RoomTypeEnum.Red, true, true, new List<RoomAction> { new RoomAction("Reload Weapon", true, ReloadWeapon) }),
            new RoomFunction("Communication", RoomTypeEnum.Yellow, true, true,  new List<RoomAction> { new RoomAction("Send Signal", true, SendSignal) }),
            new RoomFunction("Emergency", RoomTypeEnum.Green, true, true),
            new RoomFunction("Evacuation Section A", RoomTypeEnum.Generalist, true, true, new List<RoomAction> { new RoomAction("Enter Escape Pod", false, () => Escape("A")) }),
            new RoomFunction("Evacuation Section B", RoomTypeEnum.Generalist, true, true, new List<RoomAction> { new RoomAction("Enter Escape Pod", false, () => Escape("B")) }),
            new RoomFunction("Fire Control System", RoomTypeEnum.Yellow, true, true, new List<RoomAction> { new RoomAction("Anti-Fire Procedure", false, AntiFireProcedureOnSelectedRoom)}),
            new RoomFunction("Generator", RoomTypeEnum.Yellow, true, true, new List<RoomAction> { new RoomAction("Self Destruct On/Off", true, TurnSelfDestructOnOff) }),
            new RoomFunction("Laboratory", RoomTypeEnum.Green, true, true),
            new RoomFunction("Nest", RoomTypeEnum.Special, true, true, new List<RoomAction> { new RoomAction("Steal an Egg", true, StealEgg) }),
            new RoomFunction("Storage", RoomTypeEnum.Red, true, true), //TODO Implement when inventory and objects are implemented
            new RoomFunction("Surgery", RoomTypeEnum.Green, true, true),
            new RoomFunction("Airlock", RoomTypeEnum.Yellow, false, true,  new List<RoomAction> { new RoomAction("Depressurize", false, SetDepressurizationOnSelectedRoom) }),
            new RoomFunction("Cabins", RoomTypeEnum.Generalist, false, true), //TODO Implement effect during Round check
            new RoomFunction("Canteen", RoomTypeEnum.Green, false, true),
            new RoomFunction("Command Center", RoomTypeEnum.Red, false, true),
            new RoomFunction("Engine Control", RoomTypeEnum.Yellow, false, true), //TODO LATER : check engine
            new RoomFunction("Hatch", RoomTypeEnum.Generalist, false, true),
            new RoomFunction("Monitoring Room", RoomTypeEnum.Red, false, true),
            new RoomFunction("Shower", RoomTypeEnum.Generalist, false, true, new List<RoomAction> { new RoomAction("Shower", true, Shower) }),
            new RoomFunction("Slime", RoomTypeEnum.Special, false, true, null), //Implemented in Player Movement
        };
    }

    public class RoomAction
    {
        public String Name { get; private set; }
        public bool IsAutoAction { get; private set; }
        public Func<bool> Action { get; private set; }

        public RoomAction(string name, bool isAutoAction, Func<bool> action)
        {
            Name = name;
            IsAutoAction = isAutoAction;
            Action = action;
        }
    }
}

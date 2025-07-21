using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Board.Rooms
{
    public class RoomFunction
    {
        public string Name { get; set; }
        public RoomTypeEnum Type { get; set; }
        public bool IsRequired { get; set; }
        public bool IsExplorable { get; set; }

        public List<Func<bool>> RoomActions { get; set; }

        public RoomFunction(string name, RoomTypeEnum type, bool required, bool isExplorable, List<Func<bool>> actions = null)
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

            return RoomActions[index]?.Invoke() ?? false;
        }

        private static bool Hibernate()
        {
            var ship = Ship.GetInstance();
            var player = ship.CurrentPlayer;

            //check round number

            player.PerformNoiseRoll();
            if(player.CurrentRoom.Intruders.Count > 0)
            {
                return false;
            }

            //Implement Hibernation
            return true;
        }

        private static bool Escape()
        {
            var ship = Ship.GetInstance();
            var player = ship.CurrentPlayer;

            //check if any escape pod of these section is unlocked

            player.PerformNoiseRoll();
            if (player.CurrentRoom.Intruders.Count > 0)
            {
                return false;
            }

            //Implement Escape
            return true;
        }

        private static bool Shower() {
            Ship.GetInstance().CurrentPlayer.SetSlime(false);
            return true;
        }

        private static bool BreakFixEngine(int index)
        {
            Ship.GetInstance().FlipEngine(index);
            return true;
        }

        private static bool SendSignal()
        {
            Ship.GetInstance().CurrentPlayer.SendSignal();
            return true;
        }

        private static bool ReloadWeapon()
        {
            Ship.GetInstance().CurrentPlayer.ReloadWeapon(2);
            return true;
        }

        private static bool TurnSelfDestructOnOff()
        {
            return Ship.GetInstance().TurnSelfDestructOnOff(null);
        }

        private static bool StealEgg()
        {
            var ship = Ship.GetInstance();
            var player = ship.CurrentPlayer;

            player.PerformNoiseRoll();
            if (player.CurrentRoom.Intruders.Count > 0)
            {
                return false;
            }

            //Implement Steal Egg
            return true;
        }

        public readonly static RoomFunction[] AllRooms =
        {
            new RoomFunction("Hibernation", RoomTypeEnum.Special, true, false, new List<Func<bool>> { Hibernate }),
            new RoomFunction("Cockpit", RoomTypeEnum.Special, true, false), //TODO LATER : check direction, set direction
            new RoomFunction("Engine 1", RoomTypeEnum.Special, true, false, new List<Func<bool>> { () => BreakFixEngine(0) }), //TODO LATER : check engine
            new RoomFunction("Engine 2", RoomTypeEnum.Special, true, false, new List<Func<bool>> { () => BreakFixEngine(1) }), //TODO LATER : check engine
            new RoomFunction("Engine 3", RoomTypeEnum.Special, true, false, new List<Func<bool>> { () => BreakFixEngine(2) }), //TODO LATER : check engine
            new RoomFunction("Armory", RoomTypeEnum.Red, true, true, new List<Func<bool>> { ReloadWeapon }),
            new RoomFunction("Communication", RoomTypeEnum.Yellow, true, true,  new List<Func<bool>> { SendSignal }),
            new RoomFunction("Emergency", RoomTypeEnum.Green, true, true),
            new RoomFunction("Evacuation Section A", RoomTypeEnum.Generalist, true, true, new List<Func<bool>> { Escape }),
            new RoomFunction("Evacuation Section B", RoomTypeEnum.Generalist, true, true, new List<Func<bool>> { Escape }),
            new RoomFunction("Fire Control System", RoomTypeEnum.Yellow, true, true),
            new RoomFunction("Generator", RoomTypeEnum.Yellow, true, true),
            new RoomFunction("Laboratory", RoomTypeEnum.Green, true, true, new List<Func<bool>> { TurnSelfDestructOnOff }),
            new RoomFunction("Nest", RoomTypeEnum.Special, true, true, new List<Func<bool>> { StealEgg }),
            new RoomFunction("Storage", RoomTypeEnum.Red, true, true), //TODO Implement when inventory and objects are implemented
            new RoomFunction("Surgery", RoomTypeEnum.Green, true, true),
            new RoomFunction("Airlock", RoomTypeEnum.Yellow, false, true),
            new RoomFunction("Cabins", RoomTypeEnum.Generalist, false, true), //TODO Implement effect during Round check
            new RoomFunction("Canteen", RoomTypeEnum.Green, false, true),
            new RoomFunction("Command Center", RoomTypeEnum.Red, false, true),
            new RoomFunction("Engine Control", RoomTypeEnum.Yellow, false, true), //TODO LATER : check engine
            new RoomFunction("Hatch", RoomTypeEnum.Generalist, false, true),
            new RoomFunction("Monitoring Room", RoomTypeEnum.Red, false, true),
            new RoomFunction("Shower", RoomTypeEnum.Generalist, false, true, new List<Func<bool>> { Shower }),
            new RoomFunction("Slime", RoomTypeEnum.Special, false, true, null), //Implemented in Player Movement
        };
    }
}

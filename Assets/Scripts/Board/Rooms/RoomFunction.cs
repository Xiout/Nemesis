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
        //public Func<bool> RoomAction { get; set; }

        public RoomFunction(string name, RoomTypeEnum type, bool required, bool isExplorable /*, Func<bool> action*/)
        {
            Name= name;
            Type = type;
            IsRequired = required;
            IsExplorable = isExplorable;
            //RoomAction = action;
        }

        //public bool ExecuteAction()
        //{
        //    return RoomAction?.Invoke() ?? false;
        //}

        public readonly static RoomFunction[] AllRooms =
        {
            new RoomFunction("Hibernation", RoomTypeEnum.Special, true, false),
            new RoomFunction("Cockpit", RoomTypeEnum.Special, true, false),
            new RoomFunction("Engine 1", RoomTypeEnum.Green, true, false),
            new RoomFunction("Engine 2", RoomTypeEnum.Green, true, false),
            new RoomFunction("Engine 3", RoomTypeEnum.Green, true, false),
            new RoomFunction("Armory", RoomTypeEnum.Red, true, true),
            new RoomFunction("Communication", RoomTypeEnum.Yellow, true, true),
            new RoomFunction("Emergency", RoomTypeEnum.Green, true, true),
            new RoomFunction("Evacuation Section A", RoomTypeEnum.Generalist, true, true),
            new RoomFunction("Evacuation Section B", RoomTypeEnum.Generalist, true, true),
            new RoomFunction("Fire Control System", RoomTypeEnum.Yellow, true, true),
            new RoomFunction("Generator", RoomTypeEnum.Yellow, true, true),
            new RoomFunction("Laboratory", RoomTypeEnum.Green, true, true),
            new RoomFunction("Nest", RoomTypeEnum.Special, true, true),
            new RoomFunction("Storage", RoomTypeEnum.Red, true, true),
            new RoomFunction("Surgery", RoomTypeEnum.Green, true, true),
            new RoomFunction("Airlock", RoomTypeEnum.Yellow, false, true),
            new RoomFunction("Cabins", RoomTypeEnum.Generalist, false, true),
            new RoomFunction("Canteen", RoomTypeEnum.Green, false, true),
            new RoomFunction("Command Center", RoomTypeEnum.Red, false, true),
            new RoomFunction("Engine Control", RoomTypeEnum.Yellow, false, true),
            new RoomFunction("Hatch", RoomTypeEnum.Generalist, false, true),
            new RoomFunction("Monitoring Room", RoomTypeEnum.Red, false, true),
            new RoomFunction("Shower", RoomTypeEnum.Generalist, false, true),
            new RoomFunction("Slime", RoomTypeEnum.Special, false, true),
        };
    }
}

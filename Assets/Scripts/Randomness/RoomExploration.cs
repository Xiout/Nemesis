using Board.Rooms;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Randomness
{
    public class RoomExploration
    {
        private static List<RoomFunction> UnexploredRoomDeck = null;
        private static List<Tuple<ExplorationTokenEnum, int>> ExplorationTokenDeck = null;

        private static Tuple<ExplorationTokenEnum, int>[] AllExplorationToken ={
            Tuple.Create(ExplorationTokenEnum.Danger, 2),
            Tuple.Create(ExplorationTokenEnum.Danger, 3),
            Tuple.Create(ExplorationTokenEnum.Door, 1),
            Tuple.Create(ExplorationTokenEnum.Door, 2),
            Tuple.Create(ExplorationTokenEnum.Door, 3),
            Tuple.Create(ExplorationTokenEnum.Door, 4),
            Tuple.Create(ExplorationTokenEnum.Fire, 1),
            Tuple.Create(ExplorationTokenEnum.Fire, 2),
            Tuple.Create(ExplorationTokenEnum.Broken, 1),
            Tuple.Create(ExplorationTokenEnum.Broken, 1),
            Tuple.Create(ExplorationTokenEnum.Broken, 2),
            Tuple.Create(ExplorationTokenEnum.Broken, 2),
            Tuple.Create(ExplorationTokenEnum.Broken, 2),
            Tuple.Create(ExplorationTokenEnum.Broken, 2),
            Tuple.Create(ExplorationTokenEnum.Broken, 3),
            Tuple.Create(ExplorationTokenEnum.Broken, 4),
            Tuple.Create(ExplorationTokenEnum.Silent, 1),
            Tuple.Create(ExplorationTokenEnum.Silent, 1),
            Tuple.Create(ExplorationTokenEnum.Slime, 3),
            Tuple.Create(ExplorationTokenEnum.Slime, 4),
        };

        public static bool SetUpUnexploredRoomDeck()
        {
            if(UnexploredRoomDeck != null)
            {
                return false;
            }

            UnexploredRoomDeck = new List<RoomFunction>();
            UnexploredRoomDeck.AddRange(RoomFunction.AllRooms.Where(r => r.IsRequired && r.IsExplorable));
            UnexploredRoomDeck.AddRange(RandomUtils.DrawWithoutReplacement(RoomFunction.AllRooms.Where(r => !r.IsRequired && r.IsExplorable).ToList(), 5));
            return true;
        }

        public static bool SetUpExplorationTokenDeck()
        {
            if(ExplorationTokenDeck != null)
            {
                return false;
            }

            ExplorationTokenDeck = new List<Tuple<ExplorationTokenEnum, int>>();
            ExplorationTokenDeck.AddRange(RandomUtils.DrawWithoutReplacement(AllExplorationToken.ToList(), 16));
            return true;
        }

        public static RoomFunction DrawRoomTile(bool isRequiredRoom)
        {
            if(UnexploredRoomDeck == null)
            {
                SetUpUnexploredRoomDeck();
            }

            Debug.Log($"Draw Tile Room {isRequiredRoom}");
            var drawResult = RandomUtils.DrawOnce(UnexploredRoomDeck.Where(r => r.IsRequired == isRequiredRoom).ToList());
            UnexploredRoomDeck.Remove(drawResult);

            return drawResult;
        }

        public static Tuple<ExplorationTokenEnum, int> DrawExplorationToken()
        {
            if (ExplorationTokenDeck == null)
            {
                SetUpExplorationTokenDeck();
            }

            Debug.Log($"Draw Exploration Token");
            var drawResult = RandomUtils.DrawOnce(ExplorationTokenDeck);
            ExplorationTokenDeck.Remove(drawResult);

            return drawResult;
        }
    }
}

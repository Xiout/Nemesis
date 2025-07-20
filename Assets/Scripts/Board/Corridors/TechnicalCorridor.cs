using System.Collections.Generic;
using UnityEngine;
using Board.Rooms;

namespace Board.Corridors
{
    public class TechnicalCorridor : Corridor
    {
        private static bool _hasNoiseInTechnicalCorridors = false;
        internal override bool HasNoise
        {
            get { return _hasNoiseInTechnicalCorridors; }
            set { }
        }


        public bool Init(Room room1)
        {
            IsDoubleCorridor = false;
            NoiseMarkerGO = null;

            if (room1 != null)
            {
                Room1 = room1;

                return true;
            }

            return false;
        }

        public override bool MakeNoise()
        {
            if(HasNoise)
            {
                return true;
            }
            else
            {
                foreach (Corridor corridor in Ship.GetInstance().Corridors)
                {
                    var technicalCorridor = corridor as TechnicalCorridor;
                    if(technicalCorridor == null) 
                    {
                        continue;
                    }

                    technicalCorridor.gameObject.GetComponent<MeshRenderer>().SetMaterials(new List<Material>() { Ship.GetInstance().TechnicalCorridorWithNoise });
                }

                _hasNoiseInTechnicalCorridors = true;
                return false;
            }
        }

        public override void ClearNoise()
        {
            HasNoise = false;
            _hasNoiseInTechnicalCorridors = false;

            foreach (Corridor corridor in Ship.GetInstance().Corridors)
            {
                var technicalCorridor = corridor as TechnicalCorridor;
                if (technicalCorridor == null)
                {
                    continue;
                }

                technicalCorridor.gameObject.GetComponent<MeshRenderer>().SetMaterials(new List<Material>() { Ship.GetInstance().TechnicalCorridorNoiseClear });
            }
        }
    }
}

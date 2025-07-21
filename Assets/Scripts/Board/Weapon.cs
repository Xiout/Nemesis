using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Board
{
    public class Weapon
    {
        public string Name;
        public readonly int AmmoCapacity;
        public int AmmoCount; 

        public Weapon(string name, int maxAmmo, bool fullyLoaded = false) { 
            Name = name;
            AmmoCapacity = maxAmmo;
            if (fullyLoaded)
            {
                AmmoCount = AmmoCapacity;
            }
            else
            {
                AmmoCount = 1;
            }
        }
    }
}

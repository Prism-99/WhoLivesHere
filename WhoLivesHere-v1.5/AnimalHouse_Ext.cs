using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;

namespace WhoLivesHere
{
    internal static class AnimalHouse_Ext
    {
        public static Farm GetParentLocation(this AnimalHouse house)
        {
            var parent = Game1.locations.Where(p => p is BuildableGameLocation).Where(p => ((BuildableGameLocation)p).buildings.Where(p => p.nameOfIndoors == house.NameOrUniqueName).Any());
            if (parent.Any())
                return (Farm)parent.FirstOrDefault();


            return null;
        }
    }
}

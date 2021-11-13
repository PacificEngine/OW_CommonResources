using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PacificEngine.OW_CommonResources.Game.Resource
{
    public static class SectorHelper
    {
        public static List<Sector> getSector(Sector.Name name)
        {
            var sectors = new List<Sector>();
            foreach (Sector sector in SectorManager.GetRegisteredSectors())
            {
                if (name.Equals(sector.GetName()))
                {
                    sectors.Add(sector);
                }
            }
            return sectors;
        }
    }
}

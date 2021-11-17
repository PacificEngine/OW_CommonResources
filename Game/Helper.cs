using OWML.ModHelper;
using System.Collections.Generic;

namespace PacificEngine.OW_CommonResources.Game
{
    public static class Helper
    {
        public static ModHelper helper;

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

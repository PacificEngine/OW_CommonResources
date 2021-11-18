using OWML.ModHelper;
using System;
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

        public static IEnumerable<IEnumerable<T>> Subsets<T>(IEnumerable<T> source)
        {
            List<T> list = new List<T>(source);
            int length = list.Count;
            int max = (int)Math.Pow(2, list.Count);

            for (int count = 0; count < max; count++)
            {
                List<T> subset = new List<T>();
                uint rs = 0;
                while (rs < length)
                {
                    if ((count & (1u << (int)rs)) > 0)
                    {
                        subset.Add(list[(int)rs]);
                    }
                    rs++;
                }
                yield return subset;
            }
        }
    }
}

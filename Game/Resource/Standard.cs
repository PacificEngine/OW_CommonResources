using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PacificEngine.OW_CommonResources.Game.Resource
{
    public static class HeavenlyBodies
    {
        private const string prefix = "OuterWild_Standard_";

        public static HeavenlyBody None = HeavenlyBody.None;
        public static HeavenlyBody Player = new HeavenlyBody($"{prefix}Player");
        public static HeavenlyBody Ship = new HeavenlyBody($"{prefix}Ship");
        public static HeavenlyBody Probe = new HeavenlyBody($"{prefix}Probe");
        public static HeavenlyBody ModelShip = new HeavenlyBody($"{prefix}Model_Ship");
        public static HeavenlyBody Sun = new HeavenlyBody($"{prefix}Sun");
        public static HeavenlyBody SunStation = new HeavenlyBody($"{prefix}Sun_Station");
        public static HeavenlyBody HourglassTwins = new HeavenlyBody($"{prefix}Hourglass_Twins");
        public static HeavenlyBody AshTwin = new HeavenlyBody($"{prefix}Ash_Twin");
        public static HeavenlyBody EmberTwin = new HeavenlyBody($"{prefix}Ember_Twin");
        public static HeavenlyBody TimberHearth = new HeavenlyBody($"{prefix}Timber_Hearth");
        public static HeavenlyBody TimberHearthProbe = new HeavenlyBody($"{prefix}Timber_Hearth_Probe");
        public static HeavenlyBody Attlerock = new HeavenlyBody($"{prefix}Attlerock");
        public static HeavenlyBody BrittleHollow = new HeavenlyBody($"{prefix}Brittle_Hollow");
        public static HeavenlyBody HollowLantern = new HeavenlyBody($"{prefix}Hollow_Lantern");
        public static HeavenlyBody GiantsDeep = new HeavenlyBody($"{prefix}Giants_Deep");
        public static HeavenlyBody ProbeCannon = new HeavenlyBody($"{prefix}Probe_Cannon");
        public static HeavenlyBody NomaiProbe = new HeavenlyBody($"{prefix}Nomai_Probe");
        public static HeavenlyBody NomaiEmberTwinShuttle = new HeavenlyBody($"{prefix}Nomai_Ember_Twin_Shuttle");
        public static HeavenlyBody NomaiBrittleHollowShuttle = new HeavenlyBody($"{prefix}Nomai_Brittle_Hollow_Shuttle");
        public static HeavenlyBody DarkBramble = new HeavenlyBody($"{prefix}Dark_Bramble");
        public static HeavenlyBody InnerDarkBramble_Hub = new HeavenlyBody($"{prefix}Inner_Dark_Bramble_Hub");
        public static HeavenlyBody InnerDarkBramble_EscapePod = new HeavenlyBody($"{prefix}Inner_Dark_Bramble_Escape_Pod");
        public static HeavenlyBody InnerDarkBramble_Nest = new HeavenlyBody($"{prefix}Inner_Dark_Bramble_Nest");
        public static HeavenlyBody InnerDarkBramble_Feldspar = new HeavenlyBody($"{prefix}Inner_Dark_Bramble_Feldspar");
        public static HeavenlyBody InnerDarkBramble_Gutter = new HeavenlyBody($"{prefix}Inner_Dark_Bramble_Gutter");
        public static HeavenlyBody InnerDarkBramble_Vessel = new HeavenlyBody($"{prefix}Inner_Dark_Bramble_Vessel");
        public static HeavenlyBody InnerDarkBramble_Maze = new HeavenlyBody($"{prefix}Inner_Dark_Bramble_Maze");
        public static HeavenlyBody InnerDarkBramble_SmallNest = new HeavenlyBody($"{prefix}Inner_Dark_Bramble_Small_Nest");
        public static HeavenlyBody InnerDarkBramble_Secret = new HeavenlyBody($"{prefix}Inner_Dark_Bramble_Secret");
        public static HeavenlyBody Interloper = new HeavenlyBody($"{prefix}Interloper");
        public static HeavenlyBody WhiteHole = new HeavenlyBody($"{prefix}White_Hole");
        public static HeavenlyBody WhiteHoleStation = new HeavenlyBody($"{prefix}White_Hole_Station");
        public static HeavenlyBody Stranger = new HeavenlyBody($"{prefix}Stranger");
        public static HeavenlyBody DreamWorld = new HeavenlyBody($"{prefix}Dream_World");
        public static HeavenlyBody QuantumMoon = new HeavenlyBody($"{prefix}Quantum_Moon");
        public static HeavenlyBody SatiliteBacker = new HeavenlyBody($"{prefix}Satilite_Backer");
        public static HeavenlyBody SatiliteMapping = new HeavenlyBody($"{prefix}Satilite_Mapping");
        public static HeavenlyBody EyeOfTheUniverse = new HeavenlyBody($"{prefix}Eye_Of_The_Universe");
        public static HeavenlyBody EyeOfTheUniverse_Vessel = new HeavenlyBody($"{prefix}Eye_Of_The_Universe_Vessel");
    }
}

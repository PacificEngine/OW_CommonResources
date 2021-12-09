using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OWML.Utils;
using PacificEngine.OW_CommonResources.Game.Display;
using PacificEngine.OW_CommonResources.Game.Player;
using PacificEngine.OW_CommonResources.Game.Resource;
using PacificEngine.OW_CommonResources.Geometry;
using UnityEngine;

namespace PacificEngine.OW_CommonResources.Game.State
{

    /*
     * 
     * 
Sun: None -> (0,0,0) (0,0,0)	PacificEngine's Common Resources	
Sun_Body (OWRigidbody): (-1503.337,45.48741,8645.851) (0.0156923,-0.0001838,-0.0886022)	PacificEngine's Common Resources	
SunStation: Sun -> (856.6848,4997.78,-4209.924) (138.718,-199.4064,-166.2115) (0.1473, 7679.16504, 118.2905, 208.2669, 286.8161, 432.40189)	PacificEngine's Common Resources	
SunStation_Body (OWRigidbody): (-646.652,5043.267,4435.926) (138.7337,-199.4066,-166.3001)	PacificEngine's Common Resources	
HourglassTwins: Sun -> (-4337.524,-1610.932,5085.286) (163.9537,5.974478,138.0381) (0.1036, 6923.39893, 62.9805, 324.022, 166.2869, 769.2298)	PacificEngine's Common Resources	
FocalBody (OWRigidbody): (-5840.861,-1565.444,13731.14) (163.9694,5.974294,137.9495)	PacificEngine's Common Resources	
AshTwin: HourglassTwins -> (220.2691,2.44E-05,118.2435) (13.37773,2.3E-06,-24.9206) (0, 250, 90, 233.8092, 180, 41.86327)	PacificEngine's Common Resources	
TowerTwin_Body (OWRigidbody): (-5620.592,-1565.444,13849.38) (177.3471,5.974297,113.0289)	PacificEngine's Common Resources	
EmberTwin: HourglassTwins -> (-220.088,-2.44E-05,-118.8026) (-13.42618,-2.3E-06,24.89629) (0.0004, 250.119278, 90, 53.8093, 180, 41.86327)	PacificEngine's Common Resources	
CaveTwin_Body (OWRigidbody): (-6060.949,-1565.444,13612.33) (150.5432,5.974292,162.8458)	PacificEngine's Common Resources	
TimberHearth: Sun -> (-2256.721,-2131.402,-12906.99) (123.4782,86.57681,-4.817823) (0.3208, 10566.9414, 87.8664, 49.6819, 34.4526, 1217.672)	PacificEngine's Common Resources	
TimberHearth_Body (OWRigidbody): (-3760.058,-2085.915,-4261.137) (123.4939,86.57662,-4.906425)	PacificEngine's Common Resources	
TimberHearthProbe: TimberHearth -> (-500.8128,265.239,-44.29037) (-15.78783,18.62094,-43.83947) (0.2692, 518.742859, 68.2095, 60.033, 330.3031, 1686.04077)	PacificEngine's Common Resources	
Satellite_Body (OWRigidbody): (-4260.871,-1820.676,-4305.427) (107.7061,105.1976,-48.74589)	PacificEngine's Common Resources	
Attlerock: TimberHearth -> (-187.157,-880.573,128.2526) (42.04585,-19.06054,-2.262977) (0.2677, 745.115234, 8.3165, 256.1905, 155.0574, 1757.34399)	PacificEngine's Common Resources	
Moon_Body (OWRigidbody): (-3947.215,-2966.488,-4132.884) (165.5398,67.51608,-7.169401)	PacificEngine's Common Resources	
BrittleHollow: Sun -> (11545.04,8.07E-05,-1846.206) (29.19203,8E-06,182.6637) (0.0002, 11693.7646, 90, 20.21, 0, 363.92078)	PacificEngine's Common Resources	
BrittleHollow_Body (OWRigidbody): (10041.7,45.48749,6799.645) (29.20772,-0.0001758,182.5751)	PacificEngine's Common Resources	
HollowLantern: BrittleHollow -> (-382.8877,-828.1821,-10.25498) (37.78378,-10.07452,1.635448) (0.2173, 765.924377, 2.2997, 186.5926, 261.4404, 1794.71606)	PacificEngine's Common Resources	
VolcanicMoon_Body (OWRigidbody): (9658.816,-782.6946,6789.39) (66.99151,-10.0747,184.2105)	PacificEngine's Common Resources	
GiantsDeep: Sun -> (-3947.366,4353.013,3258.374) (132.2766,285.1644,11.70869) (0.3559, 8496.22363, 150.2702, 29.1215, 236.0424, 787.42603)	PacificEngine's Common Resources	
GiantsDeep_Body (OWRigidbody): (-5450.703,4398.5,11904.22) (132.2923,285.1642,11.62008)	PacificEngine's Common Resources	
ProbeCannon: GiantsDeep -> (-1718.681,-299.5815,406.1335) (-33.62236,-39.53369,-36.74788) (0.0125, 1772.5625, 160.1317, 350.0277, 329.782, 1237.49377)	PacificEngine's Common Resources	
OrbitalProbeCannon_Body (OWRigidbody): (-7169.385,4098.919,12310.36) (98.66991,245.6306,-25.1278)	PacificEngine's Common Resources	
DarkBramble: Sun -> (504.3677,-6018.628,-415.0482) (-50.59129,64.92033,-240.1016) (0.1778, 5815.37158, 100.6031, 297.1072, 95.5274, 651.62604)	PacificEngine's Common Resources	
DarkBramble_Body (OWRigidbody): (-998.9691,-5973.141,8230.803) (-50.5756,64.92014,-240.1902)	PacificEngine's Common Resources
WhiteHole: Sun -> (-10044.79,-20479.03,7978.999) (0,0,0)	PacificEngine's Common Resources	
WhiteHole_Body (OWRigidbody): (-11548.13,-20433.54,16624.85) (0.0156923,-0.0001838,-0.0886022)	PacificEngine's Common Resources	
WhiteHoleStation: Sun -> (-9967.049,-20347.99,7972.927) (0,0,0)	PacificEngine's Common Resources	
WhiteholeStation_Body (OWRigidbody): (-11470.39,-20302.5,16618.78) (0.0156923,-0.0001838,-0.0886022)	PacificEngine's Common Resources	
Interloper: Sun -> (13061.07,16001.08,-1864.2) (-59.58035,-54.93875,-122.0336) (0.0622, 21765.4863, 59.3671, 142.9227, 227.7128, 1128.33167)	PacificEngine's Common Resources	
Comet_Body (OWRigidbody): (11557.73,16046.56,6781.65) (-59.56466,-54.93893,-122.1222)	PacificEngine's Common Resources	
Stranger: Sun -> (8168.2,8400,2049.5) (0,0,0)	PacificEngine's Common Resources	
RingWorld_Body (OWRigidbody): (6664.863,8445.487,10695.35) (0.0156923,-0.0001838,-0.0886022)	PacificEngine's Common Resources	
BackerSatilite: Sun -> (-1275.282,5751.18,-1649.276) (-210.2238,-191.5218,-170.7644) (0.4778, 11250.9707, 41.4134, 232.6966, 263.9959, 721.0752)	PacificEngine's Common Resources	
BackerSatellite_Body (OWRigidbody): (-2778.619,5796.667,6996.574) (-210.2081,-191.522,-170.853)	PacificEngine's Common Resources	
MapSatilite: Sun -> (24763.83,-6609.085,4366.554) (31.03267,119.8681,5.499965) (0.0003, 25992.3047, 10.0033, 241.6748, 270.071, 706.70221)	PacificEngine's Common Resources	
HearthianMapSatellite_Body (OWRigidbody): (23260.49,-6563.598,13012.4) (31.04836,119.868,5.411363)	PacificEngine's Common Resources	
Player_Body (PlayerBody): (-3806.408,-2272.08,-4258.146) (218.4783,340.0797,-218.1527)	PacificEngine's Common Resources	
Ship_Body (ShipBody): (-3830.5,-2299.8,-4220.28) (219.8818,335.2814,-220.8138)	PacificEngine's Common Resources	
Probe_Body (OWRigidbody): (-3806.413,-2272.481,-4258.104) (219.4412,340.0635,-223.5757)	PacificEngine's Common Resources	
ModelRocket_Body (OWRigidbody): (-3781.622,-2285.134,-4238.474) (220.0872,339.3176,-224.8723)	PacificEngine's Common Resources	
NomaiProbe_Body (OWRigidbody): (-7215.745,4063.639,12319.75) (-320.2019,1887.952,-533.5971)	PacificEngine's Common Resources	
Shuttle_Body (ShuttleBody): (11558.2,15975.02,6746.439) (1490.643,-3880.947,-1866.554)	PacificEngine's Common Resources	
Shuttle_Body (ShuttleBody): (3826.228,20054.49,11950.62) (0,0,0)	PacificEngine's Common Resources	
GabbroIsland_Body (OWRigidbody): (-5927.59,4339.869,12060.37) (135.6335,285.1032,13.2421)	PacificEngine's Common Resources	
Debris_Body (OWRigidbody): (-6548.549,4364.48,12391.54) (132.5993,285.1642,10.17549)	PacificEngine's Common Resources	
WhiteholeStationSuperstructure_Body (OWRigidbody): (-11470.39,-20302.5,16618.78) (0.0156923,-0.0001838,-0.0886022)	PacificEngine's Common Resources	
StatueIsland_Body (OWRigidbody): (-4977.803,4319.271,11783.19) (131.6555,284.7484,8.641257)	PacificEngine's Common Resources	
GabbroShip_Body (OWRigidbody): (-4964.201,4473.2,11984.96) (130.5751,285.2585,15.39804)	PacificEngine's Common Resources	
ConstructionYardIsland_Body (OWRigidbody): (-5588.97,4454.776,11427.45) (135.0825,285.3597,10.98209)	PacificEngine's Common Resources	
Debris_Body (OWRigidbody): (-6559.109,4357.46,12389.55) (132.5993,285.1642,10.17549)	PacificEngine's Common Resources	
SS_Debris_Body (OWRigidbody): (-648.1945,5038.318,4429.997) (187.7988,32.19559,444.9814)	PacificEngine's Common Resources	
Debris_Body (2) (OWRigidbody): (-6526.418,4403.521,12323.5) (132.5993,285.1642,10.17549)	PacificEngine's Common Resources	
FakeCannonMuzzle_Body (1) (OWRigidbody): (-6521.109,4622.093,12495.28) (145.3729,282.0349,0.5227757)	PacificEngine's Common Resources	
Debris_Body (1) (OWRigidbody): (-6530.521,4406.938,12324.24) (132.5993,285.1642,10.17549)	PacificEngine's Common Resources	
QuantumIsland_Body (OWRigidbody): (-5513.137,4896.526,11913.01) (132.2923,285.1641,11.61998)	PacificEngine's Common Resources	
SS_Debris_Body (OWRigidbody): (-644.3224,5050.724,4444.785) (186.3585,30.53114,446.5789)	PacificEngine's Common Resources	
BrambleIsland_Body (OWRigidbody): (-5370.667,4489.811,12388.8) (129.4764,285.289,11.82934)
     * 
     */

    public static class Planet
    {
        private const string classId = "PacificEngine.OW_CommonResources.Game.State.Planet";

        public class Plantoid
        {
            public float size { get; }
            public float influence { get; }
            public float falloffExponent { get; }
            public float mass { get; }
            public Quaternion orientation { get; }
            public float rotationalSpeed { get; }
            public Position.HeavenlyBodies parent { get; }
            public float time { get; }
            public Vector3? startPosition { get; }
            public Vector3? startVelocity { get; }
            public Orbit.KeplerCoordinates orbit { get; }

            public Plantoid(float size, float influence, float falloffExponent, float mass, Quaternion orientation, float rotationalSpeed, Position.HeavenlyBodies parent, float time, Vector3? position, Vector3? velocity, Orbit.KeplerCoordinates orbit)
            {
                this.size = size;
                this.influence = influence;
                this.falloffExponent = falloffExponent;
                this.mass = mass;
                this.orientation = orientation;
                this.rotationalSpeed = rotationalSpeed;
                this.parent = parent;
                this.time = time;
                this.startPosition = position;
                this.startVelocity = velocity;
                this.orbit = orbit;
            }

            public override string ToString()
            {
                var position = startPosition == null ? "null" : DisplayConsole.logVector(startPosition.Value);
                var velocity = startVelocity == null ? "null" : DisplayConsole.logVector(startVelocity.Value);
                var orbit = (this.orbit?.ToString() ?? "");
                return $"({Math.Round(size, 4).ToString("G4")}, {Math.Round(influence, 4).ToString("G4")}, {Math.Round(falloffExponent,1).ToString("G1")}, {Math.Round(mass, 4).ToString("G4")}, {DisplayConsole.logQuaternion(orientation)}, {Math.Round(rotationalSpeed, 4).ToString("G4")}, {parent}, {position}, {velocity}, {orbit})";
            }

            public override bool Equals(System.Object other)
            {
                if (other != null && other is Plantoid)
                {
                    var obj = other as Plantoid;
                    return falloffExponent == obj.falloffExponent
                        && mass == obj.mass
                        && orientation == obj.orientation
                        && rotationalSpeed == obj.rotationalSpeed
                        && parent == obj.parent
                        && startPosition == obj.startPosition
                        && startVelocity == obj.startVelocity
                        && orbit == obj.orbit;
                }
                return false;
            }

            public override int GetHashCode()
            {
                return (falloffExponent.GetHashCode() * 4)
                    + (mass.GetHashCode() * 16)
                    + (orientation.GetHashCode() * 64)
                    + (rotationalSpeed.GetHashCode() * 256)
                    + (parent.GetHashCode() * 1024)
                    + ((startPosition?.GetHashCode() ?? 0) * 4096)
                    + ((startVelocity?.GetHashCode() ?? 0) * 16384)
                    + (orbit.GetHashCode() * 64884);
            }
        }

        private static float lastUpdate = 0f;
        private static List<string> debugIds = new List<string>();
        public static bool debugPlanetPosition { get; set; } = false;

        private static Dictionary<Position.HeavenlyBodies, Tuple<InitialMotion, Vector3, Vector3, Quaternion, Vector3, GravityVolume>> dict = new Dictionary<Position.HeavenlyBodies, Tuple<InitialMotion, Vector3, Vector3, Quaternion, Vector3, GravityVolume>>();
        private static Dictionary<Position.HeavenlyBodies, Plantoid> _mapping = defaultMapping;
        private static bool update = false;


        public static Dictionary<Position.HeavenlyBodies, Plantoid> mapping
        {
            get
            {
                var original = defaultMapping;
                var mapping = new Dictionary<Position.HeavenlyBodies, Plantoid>();
                foreach (Position.HeavenlyBodies body in _mapping.Keys)
                {
                    var owBody = Position.getBody(body);
                    if (owBody != null)
                    {
                        var parent = _mapping[body].parent;

                        var exponent = owBody?.GetAttachedGravityVolume()?.GetValue<float>("_falloffExponent") ?? 1;
                        var mass = owBody?.GetAttachedGravityVolume()?.GetValue<float>("_gravitationalMass") ?? ((owBody?.GetMass() ?? 0f) * 1000f);
                        var size = owBody?.GetAttachedGravityVolume()?.GetValue<float>("_upperSurfaceRadius") ?? _mapping[body].size;
                        var position = Position.getRelativePosition(parent, owBody);
                        var velocity = Position.getRelativeVelocity(parent, owBody);

                        var kepler = Position.getKepler(parent, owBody);
                        kepler = (kepler == null || !kepler.isOrbit()) ? null : kepler;
                        var time = Time.timeSinceLevelLoad;

                        mapping.Add(body, new Plantoid(size, _mapping[body].influence, exponent, mass, owBody?.GetRotation() ?? Quaternion.identity, owBody?.GetAngularVelocity().magnitude ?? 0f, parent, time, position, velocity, kepler));
                    }
                    else
                    {
                        mapping.Add(body, new Plantoid(_mapping[body].size, _mapping[body].influence, _mapping[body].falloffExponent, _mapping[body].mass, _mapping[body].orientation, _mapping[body].rotationalSpeed, _mapping[body].parent, _mapping[body].time, _mapping[body].startPosition, _mapping[body].startVelocity, _mapping[body].orbit));
                    }
                }

                return mapping;
            }
            set
            {
                Dictionary<Position.HeavenlyBodies, Plantoid> mapping = defaultMapping;
                foreach(var map in value)
                {
                    mapping[map.Key] = map.Value;
                }
                _mapping = mapping;
                update = false;
                updateList();
            }
        }

        public static Dictionary<Position.HeavenlyBodies, Plantoid> defaultMapping
        {
            get
            {
                var mapping = new Dictionary<Position.HeavenlyBodies, Plantoid>();
                mapping.Add(Position.HeavenlyBodies.Sun, new Plantoid(2400, 28000, 2, 4E+11f, new Quaternion(0, 0, 0, 1), 0f, Position.HeavenlyBodies.None, 0f, Vector3.zero, Vector3.zero, null));
                mapping.Add(Position.HeavenlyBodies.SunStation, new Plantoid(100, 100, 1, 1000, new Quaternion(0.5f, 0.5f, -0.5f, -0.5f), 0.1817f, Position.HeavenlyBodies.Sun, 0f, new Vector3(0, 0, -2296), new Vector3(417.4f, 0, 0), new Orbit.KeplerCoordinates(0.0002f, 2295.80151f, 90, 90, 0, 17.27803f)));
                mapping.Add(Position.HeavenlyBodies.HourglassTwins, new Plantoid(1000, 2000, 1, 1000, new Quaternion(0, -0.9f, 0, 0.5f), 0f, Position.HeavenlyBodies.Sun, 0f, new Vector3(-2867.9f, 0, 4095.8f), new Vector3(-231.7f, 0, -162.2f), new Orbit.KeplerCoordinates(0.0004f, 4999.06934f, 90, 334.142f, 0, 46.52228f)));
                mapping.Add(Position.HeavenlyBodies.AshTwin, new Plantoid(180, 600, 1, 1600000, new Quaternion(0, 1.0f, 0, 0.3f), 0.07f, Position.HeavenlyBodies.HourglassTwins, 0f, new Vector3(204.8f, 0, 143.4f), new Vector3(16.2f, 0, -23.2f), new Orbit.KeplerCoordinates(0, 250, 90, 233.8092f, 180, 41.86327f)));
                mapping.Add(Position.HeavenlyBodies.EmberTwin, new Plantoid(165, 600, 1, 1600000, new Quaternion(0, 0.9f, 0, 0.5f), 0.05f, Position.HeavenlyBodies.HourglassTwins, 0f, new Vector3(-204.8f, 0, -143.4f), new Vector3(-16.2f, 0, 23.2f), new Orbit.KeplerCoordinates(0.0004f, 250.119278f, 90, 53.8093f, 180, 41.86327f)));
                mapping.Add(Position.HeavenlyBodies.TimberHearth, new Plantoid(280, 1000, 1, 3000000, new Quaternion(0, 1, 0, 0.1f), 0.01f, Position.HeavenlyBodies.Sun, 0f, new Vector3(1492.2f, 0, -8462.5f), new Vector3(212.5f, 0, 37.5f), new Orbit.KeplerCoordinates(0.0004f, 8594.43066f, 90, 320.244f, 0, 222.33414f)));
                mapping.Add(Position.HeavenlyBodies.TimberHearthProbe, new Plantoid(0, 0, 1, 10, new Quaternion(0.7f, -0.7f, 0.1f, -0.1f), 0f, Position.HeavenlyBodies.TimberHearth, 0f, new Vector3(-344.8f, 0, -60.8f), new Vector3(9.5f, 0, -53.9f), new Orbit.KeplerCoordinates(0.0008f, 349.854706f, 90, 72.9561f, 0, 13.03971f)));
                mapping.Add(Position.HeavenlyBodies.Attlerock, new Plantoid(90, 250, 2, 50000000, new Quaternion(0, -0.6f, 0, -0.8f), 0.0609f, Position.HeavenlyBodies.TimberHearth, 0f, new Vector3(886.3f, 0, 156.3f), new Vector3(9.5f, 0, -53.9f), new Orbit.KeplerCoordinates(0.0008f, 899.295593f, 90, 265.2355f, 0, 29.99792f)));
                mapping.Add(Position.HeavenlyBodies.BrittleHollow, new Plantoid(250, 1000, 1, 3000000, new Quaternion(0, 0.6f, 0, -0.8f), 0.02f, Position.HeavenlyBodies.Sun, 0f, new Vector3(11513.3f, 0, -2030.1f), new Vector3(32.1f, 0, 182.2f), new Orbit.KeplerCoordinates(0.0002f, 11693.7646f, 90, 20.21f, 0, 363.92078f)));
                mapping.Add(Position.HeavenlyBodies.HollowLantern, new Plantoid(150, 250, 1, 910000, new Quaternion(-0.5f, 0.5f, -0.5f, -0.5f), 0.2f, Position.HeavenlyBodies.BrittleHollow, 0f, new Vector3(984.8f, 0, -173.6f), new Vector3(9.5f, 0, 53.9f), new Orbit.KeplerCoordinates(0.0008f, 999.227661f, 90, 122.3145f, 0, 72.51768f)));
                mapping.Add(Position.HeavenlyBodies.GiantsDeep, new Plantoid(1200, 2500, 1, 21780000, new Quaternion(0, 0.1f, 0, -1.0f), 0f, Position.HeavenlyBodies.Sun, 0f, new Vector3(3421.7f, 0, -16098.0f), new Vector3(152.5f, 0, 32.4f), new Orbit.KeplerCoordinates(0.0003f, 16456.3711f, 90, 151.9485f, 0, 239.52866f)));
                mapping.Add(Position.HeavenlyBodies.ProbeCannon, new Plantoid(100, 100, 1, 1000, new Quaternion(-0.3f, 0.5f, 0.4f, 0.7f), 0f, Position.HeavenlyBodies.GiantsDeep, 0f, new Vector3(-1006.4f, 0, 653.6f), new Vector3(80.4f, 0, 123.8f), new Orbit.KeplerCoordinates(0.0002f, 1200.30615f, 90, 351.1031f, 180, 5.94489f)));
                mapping.Add(Position.HeavenlyBodies.DarkBramble, new Plantoid(1000, 1500, 1, 3250000, new Quaternion(0, 1, 0, 0.1f), 0f, Position.HeavenlyBodies.Sun, 0f, new Vector3(-3473.0f, 0, 19696.2f), new Vector3(-139.3f, 0, -24.6f), new Orbit.KeplerCoordinates(0.0005f, 20007.2539f, 90, 135.9328f, 0, 800.35431f)));
                mapping.Add(Position.HeavenlyBodies.WhiteHole, new Plantoid(100, 200, 1, -1000, new Quaternion(0, 0.7071068f, 0, 0.7071068f), 0f, Position.HeavenlyBodies.Sun, 0f, new Vector3(-25557.49f, 45.44126f, 8395.418f), Vector3.zero, null));
                mapping.Add(Position.HeavenlyBodies.WhiteHoleStation, new Plantoid(50, 50, 1, 1000, new Quaternion(0, 0.04225808f, 0, -0.9991068f), 0f, Position.HeavenlyBodies.Sun, 0f, new Vector3(-25095.68f, 45.44126f, 8395.418f), Vector3.zero, null));
                mapping.Add(Position.HeavenlyBodies.Interloper, new Plantoid(120, 250, 1, 550000, new Quaternion(0, 1, 0, 0.1f), 0.0034f, Position.HeavenlyBodies.Sun, 0f, new Vector3(-24100, 0, 0), new Vector3(0, 0, 54.8f), new Orbit.KeplerCoordinates(0.8191f, 13248.3867f, 90, 180, 180, 239.51747f)));
                mapping.Add(Position.HeavenlyBodies.Stranger, new Plantoid(600, 600, 1, 300000000, new Quaternion(-0.4f, -0.9f, 0, -0.2f), 0.05f, Position.HeavenlyBodies.Sun, 0f, new Vector3(8168.2f, 8400f, 2049.5f), Vector3.zero, null));
                mapping.Add(Position.HeavenlyBodies.BackerSatilite, new Plantoid(5, 100, 1, 100, new Quaternion(0, 0, 0, 1), 0f, Position.HeavenlyBodies.Sun, 0f, new Vector3(41999.8f, 5001.7f, -22499.9f), new Vector3(-46.9f, 28.1f, 24.7f), new Orbit.KeplerCoordinates(0.8884588f, 30535.38f, 28.1183f, 81.81603f, 91.35296f, 1253.788f)));
                mapping.Add(Position.HeavenlyBodies.MapSatilite, new Plantoid(5, 100, 1, 500, new Quaternion(-0.1f, -0.8f, -0.1f, 0.6f), 0.0048f, Position.HeavenlyBodies.Sun, 0f, new Vector3(24732.5f, -6729.5f, 4361), new Vector3(31.6f, 119.8f, 5.6f), new Orbit.KeplerCoordinates(0.0003f, 25992.3047f, 10.0033f, 241.6748f, 270.071f, 706.70221f)));
                
                return mapping;
            }
        }

        public static void Start()
        {
            Helper.helper.HarmonyHelper.AddPrefix<OrbitLine>("Update", typeof(Planet), "onOrbitLineUpdate");
            Helper.helper.HarmonyHelper.AddPrefix<EllipticOrbitLine>("Update", typeof(Planet), "onOrbitLineUpdate");
            Helper.helper.HarmonyHelper.AddPrefix<MapSatelliteOrbitLine>("Update", typeof(Planet), "onOrbitLineUpdate");
            Helper.helper.HarmonyHelper.AddPostfix<OWRigidbody>("Awake", typeof(Planet), "onOWRigidbodyAwake");
            Helper.helper.HarmonyHelper.AddPrefix<InitialMotion>("Start", typeof(Planet), "onInitialMotionStart");
            Helper.helper.HarmonyHelper.AddPrefix<InitialVelocity>("Start", typeof(Planet), "onInitialVelocityStart");
            Helper.helper.HarmonyHelper.AddPrefix<MatchInitialMotion>("Start", typeof(Planet), "onMatchInitialMotionStart");
            Helper.helper.HarmonyHelper.AddPrefix<KinematicRigidbody>("Move", typeof(Planet), "onKinematicRigidbodyMove");
        }

        public static void Awake()
        {
        }

        public static void Destroy()
        {
        }

        public static void Update()
        {
            var console = DisplayConsole.getConsole(ConsoleLocation.BottomRight);
            if (Time.time - lastUpdate > 0.2f)
            {
                lastUpdate = Time.time;
                foreach (var id in debugIds)
                {
                    console.setElement(id, "", 0f);
                }
                debugIds.Clear();

                if (debugPlanetPosition)
                {
                    var index = 16.0001f;

                    debugIds.Add(classId + ".Planet.Header");
                    console.setElement(classId + ".Planet.Header", "Planet Coordinates", index);
                    foreach (var map in mapping)
                    {
                        var id = classId + ".Planet." + map.Key;
                        debugIds.Add(id + ".1");
                        debugIds.Add(id + ".2");
                        console.setElement(id + ".1", $" {map.Key.ToString()}: {Math.Round(map.Value.falloffExponent, 1).ToString("G1")}, {Math.Round(map.Value.mass, 4).ToString("G4")}, {DisplayConsole.logQuaternion(map.Value.orientation)}, {Math.Round(map.Value.rotationalSpeed, 4).ToString("G4")}, {map.Value.parent.ToString()}", index + 0.0001f);
                        if (map.Value.orbit != null && map.Value.orbit.isOrbit())
                        {
                            console.setElement(id + ".2", $"{map.Value.time.ToString()}, {map.Value.orbit.ToString()}", index + 0.0002f);
                        }
                        else
                        {
                            console.setElement(id + ".2", $"{map.Value.time.ToString()}, {DisplayConsole.logVector(map.Value.startPosition)}, {DisplayConsole.logVector(map.Value.startVelocity)}", index + 0.0002f);
                        }                        

                        index += 0.0005f;
                    }
                }
            }

            updateList();
        }

        private static void updateList()
        {
            if (update && Time.timeSinceLevelLoad > 1f)
            {
                update = false;

                var movingItems = trackMovingItems();
                foreach (var body in _mapping.Keys)
                {
                    updatePlanet(body);
                }
                relocateMovingItems(movingItems);
            }
        }



        private static List<Tuple<OWRigidbody, Position.HeavenlyBodies, Vector3, Vector3, Quaternion, Vector3?, Orbit.KeplerCoordinates>> trackMovingItems()
        {
            var sunStation = Position.getBody(Position.HeavenlyBodies.SunStation);
            var giantDeep = Position.getBody(Position.HeavenlyBodies.GiantsDeep);
            var probeCannon = Position.getBody(Position.HeavenlyBodies.ProbeCannon);
            var whiteHole = Position.getBody(Position.HeavenlyBodies.WhiteHole);

            List<Tuple<OWRigidbody, Position.HeavenlyBodies, Vector3, Vector3, Quaternion, Vector3?, Orbit.KeplerCoordinates>> bodies = new List<Tuple<OWRigidbody, Position.HeavenlyBodies, Vector3, Vector3, Quaternion, Vector3?, Orbit.KeplerCoordinates>>();
            bodies.Add(captureState(Position.getBody(Position.HeavenlyBodies.Player)));
            bodies.Add(captureState(Position.getBody(Position.HeavenlyBodies.Ship)));
            bodies.Add(captureState(Position.getBody(Position.HeavenlyBodies.Probe)));
            bodies.Add(captureState(Position.getBody(Position.HeavenlyBodies.ModelShip)));
            bodies.Add(captureState(Position.getBody(Position.HeavenlyBodies.NomaiProbe)));
            bodies.Add(captureState(Position.getBody(Position.HeavenlyBodies.NomaiEmberTwinShuttle)));
            bodies.Add(captureState(Position.getBody(Position.HeavenlyBodies.NomaiBrittleHollowShuttle)));

            if (probeCannon != null)
            {
                bodies.Add(captureState(probeCannon, Position.HeavenlyBodies.GiantsDeep));
            }

            foreach (var child in GameObject.FindObjectsOfType<OWRigidbody>())
            {
                var name = child?.gameObject?.name;
                if (name == null)
                {
                    continue;
                }
                if (sunStation != null)
                {
                    if (child.GetOrigParentBody() == sunStation
                            && (name.StartsWith("SS_Debris_Body")))
                    {
                        bodies.Add(captureState(child, Position.HeavenlyBodies.SunStation));
                    }
                }
                if (probeCannon != null)
                {
                    if (child.GetOrigParentBody() == probeCannon
                        && (name.StartsWith("Debris_Body")
                            || name.StartsWith("FakeCannonMuzzle_Body")))
                    {
                        bodies.Add(captureState(child, Position.HeavenlyBodies.GiantsDeep));
                    }
                }
                if (giantDeep != null)
                {
                    if (child.GetOrigParentBody() == giantDeep
                        && (name.StartsWith("GabbroShip_Body")
                            || name.StartsWith("StatueIsland_Body")
                            || name.StartsWith("ConstructionYardIsland_Body")
                            || name.StartsWith("GabbroIsland_Body")
                            || name.StartsWith("QuantumIsland_Body")
                            || name.StartsWith("BrambleIsland_Body")))
                    {
                        bodies.Add(captureState(child, Position.HeavenlyBodies.GiantsDeep));
                    }
                }
                if (whiteHole != null)
                {
                    if (child.GetOrigParentBody() == whiteHole
                        && (name.StartsWith("WhiteholeStationSuperstructure_Body")))
                    {
                        bodies.Add(captureState(child, Position.HeavenlyBodies.WhiteHoleStation));
                    }
                }
            }

            return bodies;
        }

        private static void relocateMovingItems(List<Tuple<OWRigidbody, Position.HeavenlyBodies, Vector3, Vector3, Quaternion, Vector3?, Orbit.KeplerCoordinates>> movingItems)
        {
            var sunStation = Position.getBody(Position.HeavenlyBodies.SunStation);
            var giantDeep = Position.getBody(Position.HeavenlyBodies.GiantsDeep);
            var probeCannon = Position.getBody(Position.HeavenlyBodies.ProbeCannon);
            Tuple<OWRigidbody, Position.HeavenlyBodies, Vector3, Vector3, Quaternion, Vector3?, Orbit.KeplerCoordinates> originalProbeCannon = null;

            foreach(var movingItem in movingItems)
            {
                if (movingItem == null)
                {
                    continue;
                }

                Position.HeavenlyBodies parent = Position.HeavenlyBodies.None;
                Vector3 position;
                Vector3 velocity;
                bool isSurfaceVelocity = false;

                var name = movingItem?.Item1?.gameObject?.name;
                if (name != null && name.Equals(probeCannon?.gameObject?.name))
                {
                    originalProbeCannon = movingItem;
                    continue;
                } else if (name != null && 
                        (name.StartsWith("Debris_Body")
                            || name.StartsWith("FakeCannonMuzzle_Body")))
                {

                    parent = movingItem.Item2;
                    var mass = Position.getMass(parent);
                    if (originalProbeCannon != null && movingItem.Item7 != null)
                    {
                        var kepler = movingItem.Item7;
                        // TODO: Transform kepler coordinates to make them similar to a probe cannon (Need to account for circular orbits)
                        var cartesian = Orbit.toCartesian(GravityVolume.GRAVITATIONAL_CONSTANT, mass.Item2, mass.Item1, 0, kepler);
                        position = movingItem.Item3.normalized * cartesian.Item1.magnitude;
                        velocity = cartesian.Item2;
                    }
                    else
                    {
                        position = movingItem.Item3;
                        velocity = movingItem.Item4;
                    }
                }
                else
                {
                    parent = movingItem.Item2;
                    var mass = Position.getMass(parent);
                    if (movingItem.Item6.HasValue)
                    {
                        isSurfaceVelocity = true;
                        position = movingItem.Item3;
                        velocity = movingItem.Item6.Value;
                    }
                    else if (movingItem.Item7 != null)
                    {
                        var cartesian = Orbit.toCartesian(GravityVolume.GRAVITATIONAL_CONSTANT, mass.Item2, mass.Item1, 0, movingItem.Item7);
                        position = movingItem.Item3.normalized * cartesian.Item1.magnitude;
                        velocity = cartesian.Item2;
                    }
                    else
                    {
                        position = movingItem.Item3;
                        velocity = movingItem.Item4;
                    }
                }

                var parentBody = Position.getBody(parent);
                if (parentBody != null)
                {

                    Helper.helper.Console.WriteLine($"{movingItem.Item1}: {parent} -> {DisplayConsole.logVector(position)} {DisplayConsole.logVector(velocity)}");

                    position = parentBody.transform.TransformPoint(position);
                    if (isSurfaceVelocity)
                    {
                        velocity = velocity + parentBody.GetPointTangentialVelocity(position);
                    }
                    velocity = velocity + parentBody.GetVelocity();

                    var oreitnation = (movingItem.Item5 * parentBody.GetRotation()).normalized;
                    Teleportation.teleportObjectTo(movingItem.Item1, position, velocity, movingItem.Item1.GetAngularVelocity(), movingItem.Item1.GetAcceleration(), oreitnation);
                }
            }
        }

        private static Tuple<OWRigidbody, Position.HeavenlyBodies, Vector3, Vector3, Quaternion, Vector3?, Orbit.KeplerCoordinates> captureState(OWRigidbody item, Position.HeavenlyBodies nearest)
        {
            if (item == null
                    || nearest == Position.HeavenlyBodies.Sun
                    || nearest == Position.HeavenlyBodies.EyeOfTheUniverse
                    || nearest == Position.HeavenlyBodies.EyeOfTheUniverse_Vessel)
            {
                return null;
            }

            var position = Position.getRelativePosition(nearest, item);
            var velocity = Position.getRelativeVelocity(nearest, item);
            var orientation = Position.getRelativeOrientation(nearest, item);
            Vector3? surfaceVelocity = null;
            Orbit.KeplerCoordinates kepler = null;
            if (_mapping.ContainsKey(nearest))
            {
                var size = _mapping[nearest].size;
                if (position.magnitude < (size * size))
                {
                    surfaceVelocity = Position.getSurfaceVelocity(nearest, item);
                }
            }

            if (!surfaceVelocity.HasValue)
            {
                kepler = Position.getKepler(nearest, item);
                if (kepler == null || !kepler.isOrbit())
                {
                    kepler = null;
                }
            }

            return Tuple.Create(item, nearest, position, velocity, orientation, surfaceVelocity, kepler);
        }

        private static Tuple<OWRigidbody, Position.HeavenlyBodies, Vector3, Vector3, Quaternion, Vector3?, Orbit.KeplerCoordinates> captureState(OWRigidbody item)
        {
            if (item == null)
            {
                return null;
            }

            Position.HeavenlyBodies? nearest = null;
            foreach(var type in Position.getClosest(item.GetPosition(), false, 
                Position.HeavenlyBodies.Player,
                Position.HeavenlyBodies.Probe,
                Position.HeavenlyBodies.Ship,
                Position.HeavenlyBodies.ModelShip,
                Position.HeavenlyBodies.NomaiProbe,
                Position.HeavenlyBodies.NomaiBrittleHollowShuttle,
                Position.HeavenlyBodies.NomaiEmberTwinShuttle,
                Position.HeavenlyBodies.TimberHearthProbe))
            {
                if (type.Item1 == Position.HeavenlyBodies.Sun
                    || type.Item1 == Position.HeavenlyBodies.EyeOfTheUniverse
                    || type.Item1 == Position.HeavenlyBodies.EyeOfTheUniverse_Vessel)
                {
                    nearest = type.Item1;
                    break;
                }
                else if (_mapping.ContainsKey(type.Item1))
                {
                    var influence = _mapping[type.Item1].influence;
                    if (type.Item2 < (influence * influence))
                    {
                        nearest = type.Item1;
                        break;
                    }
                }
                else
                {
                    nearest = type.Item1;
                    break;
                }
            }

            if (!nearest.HasValue)
            {
                return null;
            }

            return captureState(item, nearest.Value);
        }

        private static void updatePlanet(Position.HeavenlyBodies body)
        {
            if (!_mapping.ContainsKey(body))
            {
                return;
            }

            var planet = _mapping[body];
            var parent = planet.parent;

            var position = planet.startPosition ?? Vector3.zero;
            var velocity = planet.startVelocity ?? Vector3.zero;

            if (_mapping.ContainsKey(parent))
            {
                var parentMap = _mapping[parent];
                if (planet.orbit != null && planet.orbit.isOrbit())
                { 
                    float exponent;
                    float mass;
                    if (parent == Position.HeavenlyBodies.HourglassTwins)
                    {
                        var emberTwin = _mapping[Position.HeavenlyBodies.EmberTwin];
                        var ashTwin = _mapping[Position.HeavenlyBodies.AshTwin];
                        exponent = (emberTwin.falloffExponent + ashTwin.falloffExponent) / 2f;
                        mass = (emberTwin.mass + ashTwin.mass) / 4f;
                    }
                    else
                    {
                        exponent = parentMap.falloffExponent;
                        mass = parentMap.mass;
                    }

                    var result = Orbit.toCartesian(GravityVolume.GRAVITATIONAL_CONSTANT, mass, exponent, Time.timeSinceLevelLoad, planet.orbit);
                    position = result.Item1;
                    velocity = result.Item2;
                }                
            }


            Helper.helper.Console.WriteLine($"{body}: {planet.parent} -> {DisplayConsole.logVector(position)} {DisplayConsole.logVector(velocity)} {planet.orbit}");

            updatePlanet(body, planet.parent, planet.mass, planet.falloffExponent, position, velocity, planet.orientation, planet.rotationalSpeed);
        }

        private static void updatePlanet(Position.HeavenlyBodies body, Position.HeavenlyBodies parent, float mass, float falloffExponent, Vector3 position, Vector3 velocity, Quaternion orientation, float rotationalSpeed)
        {
            var owBody = Position.getBody(body);
            if (owBody == null)
            {
                return;
            }

            // Adjust Mass
            var gravity = owBody?.GetAttachedGravityVolume();
            if (gravity != null)
            {
                var _upperSurfaceRadius = gravity.GetValue<float>("_upperSurfaceRadius");
                var _surfaceAcceleration = (GravityVolume.GRAVITATIONAL_CONSTANT * mass) / Mathf.Pow(_upperSurfaceRadius, falloffExponent);

                gravity.SetValue("_falloffExponent", falloffExponent);
                gravity.SetValue("_gravitationalMass", mass);
                gravity.SetValue("_surfaceAcceleration", _surfaceAcceleration);
            }
            owBody.SetMass(mass);

            var owParent = Position.getBody(parent);
            if (owParent != null)
            {
                if (owParent.transform != null)
                {
                    if (owBody?.transform != null)
                    {
                        owBody.transform.parent = owParent.transform;
                    }
                    owBody.SetValue("_origParent", owParent.transform);
                    owBody.SetValue("_origParentBody", owParent);
                    position = owParent.transform.TransformPoint(position);
                }
                else
                {
                    position += owParent.GetPosition();
                }
                velocity += owParent.GetVelocity();
            }
            else if (parent == Position.HeavenlyBodies.None)
            {
                if (owBody?.transform != null)
                {
                    owBody.transform.parent = null;
                }
                owBody.SetValue("_origParent", null);
                owBody.SetValue("_origParentBody", null);
                position += Locator.GetCenterOfTheUniverse()?.GetOffsetPosition() ?? Vector3.zero;
                velocity += Locator.GetCenterOfTheUniverse()?.GetOffsetVelocity() ?? Vector3.zero;
            }
            else
            {
                position += Locator.GetCenterOfTheUniverse()?.GetOffsetPosition() ?? Vector3.zero;
                velocity += Locator.GetCenterOfTheUniverse()?.GetOffsetVelocity() ?? Vector3.zero;
            }

            var angularVelocity = orientation * (Vector3.up * rotationalSpeed);
            Teleportation.teleportObjectTo(owBody, position, velocity, angularVelocity, owBody.GetAcceleration(), orientation);
        }

        private static bool onOrbitLineUpdate(ref OrbitLine __instance)
        {
            var _astroObject = __instance.GetValue<AstroObject>("_astroObject");
            AstroObject parentAstro = _astroObject != null ? _astroObject.GetPrimaryBody() : (AstroObject)null;
            if (parentAstro == null)
            {
                return true;
            }

            var body = Position.find(_astroObject);
            Plantoid planet;
            if (!_mapping.TryGetValue(body, out planet))
            {
                return true;
            }

            var parent = planet.parent;
            var owBody = Position.getBody(body);
            var kepler = Position.getKepler(parent, owBody);
            if (kepler == null && !kepler.isOrbit())
            {
                return true;
            }

            var _numVerts = __instance.GetValue<int>("_numVerts");
            var _lineRenderer = __instance.GetValue<LineRenderer>("_lineRenderer");
            var _verts = new Vector3[_numVerts];

            var parentMass = Position.getMass(parent);
            var semiAxis = new Vector2(kepler.semiMajorRadius, kepler.semiMinorRadius);
            var angle = Orbit.getEsscentricAnomalyAngle(GravityVolume.GRAVITATIONAL_CONSTANT, parentMass.Item2, parentMass.Item1, Time.timeSinceLevelLoad, kepler);
            var increment = Circle.getPercentageAngle(1f / (float)(_numVerts - 1));
            for (int index = 0; index < _numVerts; ++index)
            {
                var vert = Ellipse.fromPolar((angle + 180) - (index * increment), semiAxis);
                _verts[index] = new Vector3(vert.y, 0, vert.x);
            }
            _lineRenderer.SetPositions(_verts);

            var periapsis = Orbit.getPeriapsis(GravityVolume.GRAVITATIONAL_CONSTANT, parentMass.Item2, parentMass.Item1, kepler);
            var semiMinorDecending = Orbit.getSemiMinorDecending(GravityVolume.GRAVITATIONAL_CONSTANT, parentMass.Item2, parentMass.Item1, kepler);
            var _semiMajorAxis = periapsis.Item1.normalized * kepler.semiMajorRadius;
            var _semiMinorAxis = semiMinorDecending.Item1.normalized * kepler.semiMinorRadius;
            var _upAxisDir = Vector3.Cross(periapsis.Item1, semiMinorDecending.Item1);

            Vector3 foci = parentAstro.transform.position - periapsis.Item1.normalized * kepler.foci;

            __instance.transform.position = foci;
            __instance.transform.rotation = Quaternion.LookRotation(foci - parentAstro.transform.position, _upAxisDir);

            var _lineWidth = __instance.GetValue<float>("_lineWidth");
            var _maxLineWidth = __instance.GetValue<float>("_maxLineWidth");
            var _fade = __instance.GetValue<bool>("_fade");
            var _fadeStartDist = __instance.GetValue<float>("_fadeStartDist");
            var _fadeEndDist = __instance.GetValue<float>("_fadeEndDist");
            var _color = __instance.GetValue<Color>("_color");

            float ellipticalOrbitLine = DistanceToEllipticalOrbitLine(foci, _semiMajorAxis, _semiMinorAxis, _upAxisDir, Locator.GetActiveCamera().transform.position);
            float num1 = Mathf.Min(ellipticalOrbitLine * (_lineWidth / 1000f), _maxLineWidth);
            float num2 = _fade ? 1f - Mathf.Clamp01((ellipticalOrbitLine - _fadeStartDist) / (_fadeEndDist - _fadeStartDist)) : 1f;
            _lineRenderer.widthMultiplier = num1;
            _lineRenderer.startColor = new Color(_color.r, _color.g, _color.b, num2 * num2);

            return false;
        }

        private static float CalcProjectedAngleToCenter(Vector3 foci, Vector3 semiMajorAxis, Vector3 semiMinorAxis, Vector3 point)
        {
            Vector3 lhs = point - foci;
            Vector3 vector3 = new Vector3(Vector3.Dot(lhs, semiMajorAxis.normalized), 0.0f, Vector3.Dot(lhs, semiMinorAxis.normalized));
            vector3.x *= semiMinorAxis.magnitude / semiMajorAxis.magnitude;
            return (float)Math.Atan2(vector3.z, vector3.x);
        }

        private static float DistanceToEllipticalOrbitLine(Vector3 foci, Vector3 semiMajorAxis, Vector3 semiMinorAxis, Vector3 upAxis, Vector3 point)
        {
            float center = CalcProjectedAngleToCenter(foci, semiMajorAxis, semiMinorAxis, point);
            Vector3 b = foci + semiMajorAxis * Mathf.Cos(center) + semiMinorAxis * Mathf.Sin(center);
            return Vector3.Distance(point, b);
        }

        private static void onOWRigidbodyAwake(ref OWRigidbody __instance)
        {
            var body = Position.find(__instance);
            if (_mapping.ContainsKey(body))
            {
                update = true;
            }
        }

        private static bool onInitialMotionStart(ref InitialMotion __instance)
        {
            var owBody = __instance.GetValue<OWRigidbody>("_satelliteBody");
            var body = Position.find(owBody);
            if (_mapping.ContainsKey(body))
            {
                update = true;
            }

            return false;
        }

        private static bool onInitialVelocityStart(ref InitialMotion __instance)
        {
            var owBody = __instance.GetValue<OWRigidbody>("_owRigidbody");
            var body = Position.find(owBody);
            if (_mapping.ContainsKey(body))
            {
                update = true;
            }

            return false;
        }

        private static bool onMatchInitialMotionStart(ref MatchInitialMotion __instance)
        {
            var owBody = __instance.GetValue<OWRigidbody>("_owRigidbody");
            var body = Position.find(owBody);
            if (_mapping.ContainsKey(body))
            {
                update = true;
            }

            return false;
        }

        private static bool onKinematicRigidbodyMove(ref KinematicRigidbody __instance, ref Vector3 position, ref Quaternion rotation)
        {
            var owBody = __instance.GetValue<Rigidbody>("_rigidbody");

            owBody.MovePosition(position);
            owBody.MoveRotation(rotation.normalized);

            return false;
        }
    }
}

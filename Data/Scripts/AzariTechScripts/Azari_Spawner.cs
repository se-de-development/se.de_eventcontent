namespace AzariSpawnerScript
{

    using Sandbox.Definitions;
    using Sandbox.Game;
    using Sandbox.ModAPI;
    using System.Collections.Generic;
    using VRage;
    using VRage.Game;
    using VRage.Game.Components;
    using VRage.Game.ModAPI;
    using VRage.ModAPI;
    using VRage.ObjectBuilders;
    using VRageMath;

    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Projector), true, new string[] { "Azari_Shipspawner"})]
    public class AzariShipSpawner : MyGameLogicComponent
    {
        private const long EventOwnerId = 76561197997964507; // LeXx LeCryce
        private long eventOwner_ID;
        private IMyPlayer targetplayer;
        private string shipName;
        private Vector3 Vector3pos;
        private IMyPlayer eventOwner;
        private IMyGps gps;
        private readonly IMyCharacter character;
        private Vector3D signalLocation;
        private Sandbox.ModAPI.Ingame.IMyProjector projector;
        private MyBlockOrientation Orientation;

        public override void Init(Sandbox.Common.ObjectBuilders.MyObjectBuilder_EntityBase objectBuilder)
        {
            projector = (Entity as Sandbox.ModAPI.Ingame.IMyProjector);
            Entity.NeedsUpdate |= VRage.ModAPI.MyEntityUpdateEnum.EACH_100TH_FRAME;
        }

        public override void UpdateBeforeSimulation100()
        {
            if (projector.Enabled)
            {
                var players = new List<IMyPlayer>();
                Sandbox.ModAPI.MyAPIGateway.Players.GetPlayers(players);

                eventOwner = null;
                foreach (var player in players)
                {
                    if (player.SteamUserId == EventOwnerId)
                    {
                        eventOwner_ID = player.IdentityId;
                        break;
                    }
                }
                double x = projector.GetPosition().GetDim(0);
                double y = projector.GetPosition().GetDim(1);
                double z = projector.GetPosition().GetDim(2);

                double up_x = projector.WorldMatrix.GetOrientation().Up.GetDim(0);
                double up_y = projector.WorldMatrix.GetOrientation().Up.GetDim(1);
                double up_z = projector.WorldMatrix.GetOrientation().Up.GetDim(2);

                Vector3D up = projector.WorldMatrix.GetOrientation().Up;
                Vector3D forward = projector.WorldMatrix.GetOrientation().Forward;


                double x_1 = 0;
                double y_1 = 100;
                double z_1 = 0;

                switch (projector.CustomData)
                {
                    case "Spawner: MakeStatic":
                        IMyCubeGrid ship_grid = projector.CubeGrid as IMyCubeGrid;
                        if (ship_grid.IsStatic == false)
                        {
                            ship_grid.IsStatic = true;

                        }

                        break;
                    case "Spawner: Kumari":
                        SpawnPrefab("AZR Kumari", new Vector3(x + 800, y + 800, z + 800), eventOwner_ID, forward, up);
                        break;
                    case "Spawner: Scout":
                        SpawnPrefab("Azari - AE Hunter", new Vector3(x + x_1, y + y_1, z + z_1), eventOwner_ID, forward, up);
                        break;
                    case "Spawner: Sector Beacon Scout":
                        SpawnPrefab("Azari - AE Hunter", new Vector3(x + 200, y, z), eventOwner_ID, forward, up);
                        SpawnPrefab("Azari - AE Hunter", new Vector3(x, y, z - 200), eventOwner_ID, forward, up);
                        break;
                    case "Spawner: Sector Beacon":
                        SpawnPrefab("Azari - Sector Beacon", new Vector3(x + x_1, y + y_1, z + z_1), eventOwner_ID, forward, up);
                        break;
                    case "Spawner: Flares":
                        SpawnPrefab("Azari - Flare", new Vector3(x + 130, y, z), eventOwner_ID, forward, up);
                        SpawnPrefab("Azari - Flare2", new Vector3(x, y + 100, z), eventOwner_ID, forward, up);
                        SpawnPrefab("Azari - Flare2", new Vector3(x, y, z + 120), eventOwner_ID, forward, up);
                        SpawnPrefab("Azari - Flare", new Vector3(x - 110, y, z), eventOwner_ID, forward, up);
                        SpawnPrefab("Azari - Flare", new Vector3(x, y - 100, z), eventOwner_ID, forward, up);
                        SpawnPrefab("Azari - Flare2", new Vector3(x, y, z - 150), eventOwner_ID, forward, up);
                        break;
                    case "Spawner: gps":
                        signalLocation = projector.GetPosition();
                        foreach (var player in players)
                        {
                            if (player.IsBot || player.Character == null)
                                continue;
                            MyVisualScriptLogicProvider.ShowNotification(">>> WARNING AZARIAN SIGNAL DETECTED <<<",
                            4000, "Red", player.IdentityId);
                            gps = MyAPIGateway.Session.GPS.Create(">>> AZARIAN SIGNAL <<<", "WARNING: The SRS located a Azarian signal", signalLocation, true, true);
                            MyAPIGateway.Session.GPS.AddGps(player.IdentityId, gps);
                        }
                        break;
                }
            (projector as Sandbox.ModAPI.Ingame.IMyProjector).RequestEnable(false);
            }
        }

        public void SpawnPrefab(string shipName, Vector3 Vector3pos, long eventOwner_ID, Vector3D forward, Vector3D up)
        {
            var prefab = MyDefinitionManager.Static.GetPrefabDefinition(shipName);
            if (prefab.CubeGrids == null)
            {
                MyDefinitionManager.Static.ReloadPrefabsFromFile(prefab.PrefabPath);
                prefab = MyDefinitionManager.Static.GetPrefabDefinition(prefab.Id.SubtypeName);
            }
            var tempList = new List<MyObjectBuilder_EntityBase>();
            foreach (var grid in prefab.CubeGrids)
            {
                var gridBuilder = (MyObjectBuilder_CubeGrid)grid.Clone();
                gridBuilder.PositionAndOrientation = new MyPositionAndOrientation(Vector3pos, forward, up);


                foreach (var cube in gridBuilder.CubeBlocks)
                {
                    cube.Owner = eventOwner_ID;
                    cube.ShareMode = MyOwnershipShareModeEnum.None;
                }
                tempList.Add(gridBuilder);
            }
            var entities = new List<IMyEntity>();
            MyAPIGateway.Entities.RemapObjectBuilderCollection(tempList);
            foreach (var item in tempList)
                entities.Add(MyAPIGateway.Entities.CreateFromObjectBuilderAndAdd(item));
        }

    }
}
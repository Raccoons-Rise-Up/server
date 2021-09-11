namespace GameServer.Server
{
    public class StructureInfoHut : StructureInfo
    {
        public StructureInfoHut()
        {
            Description = "Housing for cats";
            Cost = new()
            {
                { ResourceType.Wood, 100 },
                { ResourceType.Wheat, 23 }
            };
        }
    }
}

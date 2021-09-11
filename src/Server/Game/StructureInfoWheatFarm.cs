namespace GameServer.Server
{
    public class StructureInfoWheatFarm : StructureInfo
    {
        public StructureInfoWheatFarm()
        {
            Description = "A source of food for the cats.";
            Cost = new()
            {
                { ResourceType.Wood, 100 }
            };

            Production = new() 
            {
                { ResourceType.Wheat, 1 }
            };
        }
    }
}

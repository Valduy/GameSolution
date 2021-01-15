namespace MatchmakerModels
{
    public class QueuedPlayer
    {
        public string Id { get; }
        public long Timer{ get; set; }

        public QueuedPlayer(string id)
        {
            Id = id;
        }
    }
}

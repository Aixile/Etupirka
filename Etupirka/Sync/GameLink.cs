using System.Collections.Generic;

namespace Etupirka.Sync
{
    public class GameLink
    {
        public string LinkId { get; set; }
        public string Title { get; set; }
        public Dictionary<string, string> Machines { get; set; }

        public GameLink()
        {
            Machines = new Dictionary<string, string>();
        }

        public GameLink(string linkId, string title)
        {
            LinkId = linkId;
            Title = title;
            Machines = new Dictionary<string, string>();
        }
    }

    public class GameLinksData
    {
        public Dictionary<string, GameLink> Links { get; set; }

        public GameLinksData()
        {
            Links = new Dictionary<string, GameLink>();
        }
    }
}

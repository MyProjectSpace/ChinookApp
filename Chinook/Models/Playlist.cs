using System;
using System.Collections.Generic;

namespace Chinook.Models
{
    public partial class Playlist
    {
        public Playlist()
        {
            Tracks = new HashSet<Track>();
        }

        public long PlaylistId { get; set; }
        public string? Name { get; set; }
        public string? UserId { get; set; }

        public virtual ICollection<Track> Tracks { get; set; }
        public ChinookUser User { get; set; }

    }
}

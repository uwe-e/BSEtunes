using BSEtunes.Domain.Entities;
using BSEtunes.Infrastructure.Models;

namespace BSEtunes.Infrastructure.Mapping
{
    public static class PlaylistMapper
    {
        public static Playlist? ToInfrastructure(PlaylistEntity entity, string ownerEmail)
        {
            if (entity == null) return null;
            
            return new Playlist
            {
                Name = entity.Name,
                Owner = ownerEmail,
                Guid = entity.Guid != Guid.Empty ? entity.Guid.ToString() : System.Guid.NewGuid().ToString(),
            };
        }

        public static PlaylistEntity? ToDomain(Playlist playlist)
        {
            if (playlist == null) return null;
            
            return new PlaylistEntity
            {
                Id = playlist.Id,
                Name = playlist.Name,
                Owner = playlist.Owner,
                Guid = Guid.TryParse(playlist.Guid, out var guid) ? guid : Guid.Empty
            };
        }
    }
}
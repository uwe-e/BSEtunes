using BSEtunes.Domain.Entities;
using BSEtunes.Infrastructure.Models;

namespace BSEtunes.Infrastructure.Mapping
{
    public static class HistoryMapper
    {
        public static HistoryEntity ToDomain(History dbHistory)
        {
            return new HistoryEntity
            {
                Id = dbHistory.Id,
                AppId = dbHistory.AppID,
                TitleId = dbHistory.TitleId,
                TrackId = dbHistory.TrackId,
                PlayedAt = dbHistory.PlayedAt,
                Artist = dbHistory.Artist,
                Title = dbHistory.Title,
                TrackName = dbHistory.TrackName,
                Owner = dbHistory.Owner
            };
        }

        public static History ToDatabase(HistoryEntity entity)
        {
            return new History
            {
                Id = entity.Id,
                AppID = entity.AppId,
                TitleId = entity.TitleId,
                TrackId = entity.TrackId,
                PlayedAt = entity.PlayedAt,
                Artist = entity.Artist,
                Title = entity.Title,
                TrackName = entity.TrackName,
                Owner = entity.Owner
            };
        }
    }
}
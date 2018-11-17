using Blazortastics.Core.Tools;
using Blazortastics.DB;
using Blazortastics.DB.Tables;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazortastics.App.Services.Puzzle
{
    public class LeaderboardService
    {
        private readonly BlazorDbContext _db;

        public LeaderboardService(BlazorDbContext db)
        {
            _db = db;
        }

        public async Task InsertRankingAsync(Ranking entry)
        {
            await _db.AddAsync(entry);
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<Ranking>> GetRankingsAsync(DateTime? month)
        {
            var query = _db.Rankings.AsQueryable();

            if (month != default)
            {
                var from = month?.GetFirstDayOfMonth().TruncateTime();
                var to = month?.GetLastDayOfMonth().MaximizeTime();

                query = query.Where(a => a.Timestamp >= from && a.Timestamp <= to);
            }

            var rankings = await query.ToListAsync();

            return rankings;
        }
    }
}

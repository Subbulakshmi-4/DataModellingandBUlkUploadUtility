using System.Collections.Generic;
using System.Linq;
using DMU_Git.Data;
using Microsoft.EntityFrameworkCore;
using DMU_Git.Models;

namespace DMU_Git.Services
{
    public class ExportExcelService
    {
        private readonly ApplicationDbContext _dbContext;

        public ExportExcelService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<LogChild>> GetLogChildsByParentIDAsync(int parentID)
        {
            return await _dbContext.logChilds
                .Where(c => c.ParentID == parentID)
                .ToListAsync();
        }
    }
}

using Microsoft.EntityFrameworkCore;
 


namespace DTS.DAL.DataModel
{
    public class DocumentsContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbSet<DocumentClass> Documents { get; set; }
        
        public DbSet<AttributeClass> Attributes { get; set; }

        public DbSet<FileClass> Files { get; set; }

        public DbSet<DocumentPatternClass> DocumentPatterns { get; set; }

        public DbSet<AttributePatternClass> AttributePatterns { get; set; }

        public DbSet<FilePatternClass> FilePatterns { get; set; }

        public DbSet<MatchResult> MatchResults { get; set; }

        public DocumentsContext(DbContextOptions<DocumentsContext> options) : base(options)
        {

        }
    }
}
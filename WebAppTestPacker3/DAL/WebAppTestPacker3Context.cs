using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;
using WebAppTestPacker3.Models;

namespace WebAppTestPacker3.DAL
{
    public class WebAppTestPacker3Context : DbContext
    {
        public WebAppTestPacker3Context()
            : base("WebAppTestPacker3Context")
        { }

        public DbSet<PerWebUserCache> PerUserCacheList { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }

    }
}
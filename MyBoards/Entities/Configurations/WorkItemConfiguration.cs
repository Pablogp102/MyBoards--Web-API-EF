using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBoards.Entities.Configurations
{
    public class WorkItemConfiguration 
        : IEntityTypeConfiguration<WorkItem>, 
        IEntityTypeConfiguration<Task>, 
        IEntityTypeConfiguration<Issue>,
        IEntityTypeConfiguration<Epic>
    {
        public void Configure(EntityTypeBuilder<WorkItem> eb)
        {
            eb.HasOne(w => w.State)
                .WithMany()
                .HasForeignKey(w => w.StateId);

            eb.Property(x => x.Area).HasColumnType("varchar(200)");
            eb.Property(wi => wi.IterationPath).HasColumnName("Iteration_Path");

            eb.Property(wi => wi.Priority).HasDefaultValue(1);
            eb.HasMany(w => w.Comments)
            .WithOne(c => c.WorkItem)
            .HasForeignKey(c => c.WorkItemId);

            eb.HasOne(w => w.Author)
            .WithMany(u => u.WorkItems)
            .HasForeignKey(w => w.AuthorId);

            eb.HasMany(w => w.Tags)
            .WithMany(t => t.WorkItems)
            .UsingEntity<WorkItemTag>(
                w => w.HasOne(wit => wit.Tag)
                .WithMany()
                .HasForeignKey(wit => wit.TagId),

                w => w.HasOne(wit => wit.WorkItem)
                .WithMany()
                .HasForeignKey(wit => wit.WorkItemId),

                wit =>
                {
                    wit.HasKey(x => new { x.TagId, x.WorkItemId });
                    wit.Property(x => x.PublicationDate).HasDefaultValueSql("getutcdate()");
                });
        }
        public void Configure(EntityTypeBuilder<Task> eb)
        {
            eb.Property(wi => wi.RemaningWork)
              .HasPrecision(14, 2);
            eb.Property(wi => wi.Activity)
                .HasMaxLength(200);
        }
        public void Configure(EntityTypeBuilder<Epic> eb)
        {
            eb.Property(wi => wi.EndDate)
              .HasPrecision(3);
        }

        public void Configure(EntityTypeBuilder<Issue> eb)
        {
            eb.Property(wi => wi.Efford)
                .HasColumnType("decimal(5,2)");
        }
    }
}
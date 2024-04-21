using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using MyBoards.Entities.VIewModels;

namespace MyBoards.Entities.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>, IEntityTypeConfiguration<TopAuthor>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasOne(u => u.Address)
                .WithOne(a => a.User)
                .HasForeignKey<Address>(a => a.UserId);

            builder.HasIndex(u => new { u.Email, u.FullName });


        }

        public void Configure(EntityTypeBuilder<TopAuthor> builder)
        {
            builder.ToView("View_TopAuthors");
            builder.HasNoKey();
        }
    }
}

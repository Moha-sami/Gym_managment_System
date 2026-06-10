using GymManagment.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagment.DAL.ModelsConfigrations
{
    internal class BookingConfig : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.Ignore(b=>b.Id);// Ignore the Id property since we are using a composite key
            builder.HasKey(b => new { b.MemberId, b.SessionId });// Composite key
            builder.Property(b => b.CreatedAt).HasColumnName("BookingDate").HasDefaultValueSql("GETDATE()");
        }
    }
}

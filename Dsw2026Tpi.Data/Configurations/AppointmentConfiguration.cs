using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dsw2026Tpi.Data.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");
        builder.Property(a => a.RowVersion).IsRowVersion();
        builder.HasOne(a => a.AvailabilitySlot)
            .WithMany() 
            .HasForeignKey(a => a.AvailabilitySlotId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
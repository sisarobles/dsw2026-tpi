using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dsw2026Tpi.Data.Configurations;

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.ToTable("Doctors");

        // 2. Validaciones para el Nombre
        builder.Property(d => d.Name)
               .IsRequired()             
               .HasMaxLength(100);      

        // 3. Validaciones para la Matrícula (LicenseNumber)
        builder.Property(d => d.LicenseNumber)
               .IsRequired()
               .HasMaxLength(50);        

        // 4. Configura la Relación (Foreign Key)
        builder.HasOne(d => d.Speciality)
               .WithMany()
               .HasForeignKey(d => d.SpecialityId)
               .OnDelete(DeleteBehavior.Restrict); 
    }
}

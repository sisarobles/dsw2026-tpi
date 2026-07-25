using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Data.Configurations
{
    public class AvailabilityRuleConfiguration : IEntityTypeConfiguration<AvailabilityRule>
    {
        public void Configure(EntityTypeBuilder<AvailabilityRule> builder)
        {
            builder.ToTable("AvailabilityRules");
        }
    }
}

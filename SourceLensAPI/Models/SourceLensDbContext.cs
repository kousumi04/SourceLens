using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SourceLensAPI.Models;

public partial class SourceLensDbContext : DbContext
{
    public SourceLensDbContext(DbContextOptions<SourceLensDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Citation> Citations { get; set; }

    public virtual DbSet<Claim> Claims { get; set; }

    public virtual DbSet<ClaimAssessment> ClaimAssessments { get; set; }

    public virtual DbSet<Evidence> Evidences { get; set; }

    public virtual DbSet<ResearchPaper> ResearchPapers { get; set; }

    public virtual DbSet<Source> Sources { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Citation>(entity =>
        {
            entity.Property(e => e.CitationText).HasMaxLength(500);

            entity.HasOne(d => d.Claim).WithMany(p => p.Citations)
                .HasForeignKey(d => d.ClaimId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Citations_Claims");

            entity.HasOne(d => d.Source).WithMany(p => p.Citations)
                .HasForeignKey(d => d.SourceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Citations_Sources");
        });

        modelBuilder.Entity<Claim>(entity =>
        {
            entity.HasOne(d => d.Paper).WithMany(p => p.Claims)
                .HasForeignKey(d => d.PaperId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Claims_ResearchPapers");
        });

        modelBuilder.Entity<ClaimAssessment>(entity =>
        {
            entity.HasKey(e => e.AssessmentId);

            entity.Property(e => e.ConfidenceScore).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Verdict).HasMaxLength(50);

            entity.HasOne(d => d.Claim).WithMany(p => p.ClaimAssessments)
                .HasForeignKey(d => d.ClaimId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClaimAssessments_Claims");

            entity.HasOne(d => d.Evidence).WithMany(p => p.ClaimAssessments)
                .HasForeignKey(d => d.EvidenceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClaimAssessments_Evidence");
        });

        modelBuilder.Entity<Evidence>(entity =>
        {
            entity.ToTable("Evidence");

            entity.HasOne(d => d.Source).WithMany(p => p.Evidences)
                .HasForeignKey(d => d.SourceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Evidence_Sources");
        });

        modelBuilder.Entity<ResearchPaper>(entity =>
        {
            entity.HasKey(e => e.PaperId);

            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(300);
            entity.Property(e => e.UploadDate).HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.ResearchPapers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ResearchPapers_User");
        });

        modelBuilder.Entity<Source>(entity =>
        {
            entity.Property(e => e.Authors).HasMaxLength(500);
            entity.Property(e => e.Doi)
                .HasMaxLength(200)
                .HasColumnName("DOI");
            entity.Property(e => e.SourceType).HasMaxLength(100);
            entity.Property(e => e.Title).HasMaxLength(500);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");

            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.Role).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

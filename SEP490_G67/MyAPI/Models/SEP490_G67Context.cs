using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace MyAPI.Models
{
    public partial class SEP490_G67Context : DbContext
    {
        public SEP490_G67Context()
        {
        }

        public SEP490_G67Context(DbContextOptions<SEP490_G67Context> options)
            : base(options)
        {
        }

        public virtual DbSet<Driver> Drivers { get; set; } = null!;
        public virtual DbSet<HistoryRentDriver> HistoryRentDrivers { get; set; } = null!;
        public virtual DbSet<HistoryRentVehicle> HistoryRentVehicles { get; set; } = null!;
        public virtual DbSet<LossCost> LossCosts { get; set; } = null!;
        public virtual DbSet<LossCostType> LossCostTypes { get; set; } = null!;
        public virtual DbSet<Payment> Payments { get; set; } = null!;
        public virtual DbSet<PaymentRentDriver> PaymentRentDrivers { get; set; } = null!;
        public virtual DbSet<PaymentRentVehicle> PaymentRentVehicles { get; set; } = null!;
        public virtual DbSet<PointUser> PointUsers { get; set; } = null!;
        public virtual DbSet<Promotion> Promotions { get; set; } = null!;
        public virtual DbSet<PromotionUser> PromotionUsers { get; set; } = null!;
        public virtual DbSet<Review> Reviews { get; set; } = null!;
        public virtual DbSet<Role> Roles { get; set; } = null!;
        public virtual DbSet<StatusPayment> StatusPayments { get; set; } = null!;
        public virtual DbSet<StatusTrip> StatusTrips { get; set; } = null!;
        public virtual DbSet<StopPoinTrip> StopPoinTrips { get; set; } = null!;
        public virtual DbSet<StopPoint> StopPoints { get; set; } = null!;
        public virtual DbSet<Ticket> Tickets { get; set; } = null!;
        public virtual DbSet<Trip> Trips { get; set; } = null!;
        public virtual DbSet<TypeOfDriver> TypeOfDrivers { get; set; } = null!;
        public virtual DbSet<TypeOfPayment> TypeOfPayments { get; set; } = null!;
        public virtual DbSet<TypeOfTicket> TypeOfTickets { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<UserCancleTicket> UserCancleTickets { get; set; } = null!;
        public virtual DbSet<UserRole> UserRoles { get; set; } = null!;
        public virtual DbSet<Vehicle> Vehicles { get; set; } = null!;
        public virtual DbSet<VehicleType> VehicleTypes { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("server =(local); database = SEP490_G67;uid=sa;pwd=123;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Driver>(entity =>
            {
                entity.ToTable("Driver");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Avatar)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("avatar");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.Dob)
                    .HasColumnType("datetime")
                    .HasColumnName("dob");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");

                entity.Property(e => e.NumberPhone)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("number_phone");

                entity.Property(e => e.Password)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("password");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.StatusWork)
                    .HasMaxLength(255)
                    .HasColumnName("status_work");

                entity.Property(e => e.TypeOfDriver).HasColumnName("type_of_driver");

                entity.Property(e => e.UpdateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("update_at");

                entity.Property(e => e.UpdateBy).HasColumnName("update_by");

                entity.Property(e => e.UserName)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("user_name");

                entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");

                entity.HasOne(d => d.TypeOfDriverNavigation)
                    .WithMany(p => p.Drivers)
                    .HasForeignKey(d => d.TypeOfDriver)
                    .HasConstraintName("FK__Driver__type_of___619B8048");
            });

            modelBuilder.Entity<HistoryRentDriver>(entity =>
            {
                entity.HasKey(e => e.HistoryId)
                    .HasName("PK__HistoryR__096AA2E994822938");

                entity.ToTable("HistoryRentDriver");

                entity.Property(e => e.HistoryId).HasColumnName("history_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.DriverId).HasColumnName("driver_id");

                entity.Property(e => e.EndStart).HasColumnType("datetime");

                entity.Property(e => e.TimeStart).HasColumnType("datetime");

                entity.Property(e => e.UpdateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("update_at");

                entity.Property(e => e.UpdateBy).HasColumnName("update_by");

                entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");

                entity.HasOne(d => d.Driver)
                    .WithMany(p => p.HistoryRentDrivers)
                    .HasForeignKey(d => d.DriverId)
                    .HasConstraintName("FK__HistoryRe__drive__628FA481");

                entity.HasOne(d => d.Vehicle)
                    .WithMany(p => p.HistoryRentDrivers)
                    .HasForeignKey(d => d.VehicleId)
                    .HasConstraintName("FK__HistoryRe__vehic__6383C8BA");
            });

            modelBuilder.Entity<HistoryRentVehicle>(entity =>
            {
                entity.HasKey(e => e.HistoryId)
                    .HasName("PK__HistoryR__096AA2E9ADEA6D06");

                entity.ToTable("HistoryRentVehicle");

                entity.Property(e => e.HistoryId).HasColumnName("history_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.DriverId).HasColumnName("driver_id");

                entity.Property(e => e.EndStart).HasColumnType("datetime");

                entity.Property(e => e.OwnerId).HasColumnName("owner_id");

                entity.Property(e => e.TimeStart).HasColumnType("datetime");

                entity.Property(e => e.UpdateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("update_at");

                entity.Property(e => e.UpdateBy).HasColumnName("update_by");

                entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");

                entity.HasOne(d => d.Driver)
                    .WithMany(p => p.HistoryRentVehicles)
                    .HasForeignKey(d => d.DriverId)
                    .HasConstraintName("FK__HistoryRe__drive__6477ECF3");

                entity.HasOne(d => d.Vehicle)
                    .WithMany(p => p.HistoryRentVehicles)
                    .HasForeignKey(d => d.VehicleId)
                    .HasConstraintName("FK__HistoryRe__vehic__656C112C");
            });

            modelBuilder.Entity<LossCost>(entity =>
            {
                entity.ToTable("LossCost");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.DateIncurred)
                    .HasColumnType("datetime")
                    .HasColumnName("date_incurred");

                entity.Property(e => e.Description)
                    .HasMaxLength(255)
                    .HasColumnName("description");

                entity.Property(e => e.LossCostTypeId).HasColumnName("loss_cost_type_id");

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("price");

                entity.Property(e => e.UpdateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("update_at");

                entity.Property(e => e.UpdateBy).HasColumnName("update_by");

                entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");

                entity.HasOne(d => d.LossCostType)
                    .WithMany(p => p.LossCosts)
                    .HasForeignKey(d => d.LossCostTypeId)
                    .HasConstraintName("FK__LossCost__loss_c__693CA210");

                entity.HasOne(d => d.Vehicle)
                    .WithMany(p => p.LossCosts)
                    .HasForeignKey(d => d.VehicleId)
                    .HasConstraintName("FK__LossCost__vehicl__68487DD7");
            });

            modelBuilder.Entity<LossCostType>(entity =>
            {
                entity.ToTable("LossCostType");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Description)
                    .HasMaxLength(255)
                    .HasColumnName("description");
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.ToTable("Payment");

                entity.Property(e => e.PaymentId).HasColumnName("payment_id");

                entity.Property(e => e.Code)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("code");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.Description)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("description");

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("price");

                entity.Property(e => e.StatusPayment).HasColumnName("status_payment");

                entity.Property(e => e.StatusTicket).HasColumnName("status_ticket");

                entity.Property(e => e.TicketId).HasColumnName("ticket_id");

                entity.Property(e => e.Time)
                    .HasColumnType("datetime")
                    .HasColumnName("time");

                entity.Property(e => e.TypeOfPayment).HasColumnName("type_of_payment");

                entity.Property(e => e.UpdateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("update_at");

                entity.Property(e => e.UpdateBy).HasColumnName("update_by");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.StatusPaymentNavigation)
                    .WithMany(p => p.Payments)
                    .HasForeignKey(d => d.StatusPayment)
                    .HasConstraintName("FK__Payment__status___75A278F5");

                entity.HasOne(d => d.Ticket)
                    .WithMany(p => p.Payments)
                    .HasForeignKey(d => d.TicketId)
                    .HasConstraintName("FK__Payment__ticket___72C60C4A");

                entity.HasOne(d => d.TypeOfPaymentNavigation)
                    .WithMany(p => p.Payments)
                    .HasForeignKey(d => d.TypeOfPayment)
                    .HasConstraintName("FK__Payment__type_of__74AE54BC");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Payments)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__Payment__user_id__73BA3083");
            });

            modelBuilder.Entity<PaymentRentDriver>(entity =>
            {
                entity.ToTable("PaymentRentDriver");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.Description)
                    .HasColumnType("text")
                    .HasColumnName("description");

                entity.Property(e => e.DriverId).HasColumnName("driver_id");

                entity.Property(e => e.HistoryRentDriverId).HasColumnName("history_rent_driver_id");

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("price");

                entity.Property(e => e.UpdateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("update_at");

                entity.Property(e => e.UpdateBy).HasColumnName("update_by");

                entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");

                entity.HasOne(d => d.HistoryRentDriver)
                    .WithMany(p => p.PaymentRentDrivers)
                    .HasForeignKey(d => d.HistoryRentDriverId)
                    .HasConstraintName("FK__PaymentRe__histo__66603565");
            });

            modelBuilder.Entity<PaymentRentVehicle>(entity =>
            {
                entity.ToTable("PaymentRentVehicle");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CarOwnerId).HasColumnName("car_owner_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.DriverId).HasColumnName("driver_id");

                entity.Property(e => e.HistoryRentVehicleId).HasColumnName("history_rent_vehicle_id");

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("price");

                entity.Property(e => e.UpdateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("update_at");

                entity.Property(e => e.UpdateBy).HasColumnName("update_by");

                entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");

                entity.HasOne(d => d.HistoryRentVehicle)
                    .WithMany(p => p.PaymentRentVehicles)
                    .HasForeignKey(d => d.HistoryRentVehicleId)
                    .HasConstraintName("FK__PaymentRe__histo__6754599E");
            });

            modelBuilder.Entity<PointUser>(entity =>
            {
                entity.ToTable("PointUser");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.Date)
                    .HasColumnType("date")
                    .HasColumnName("date");

                entity.Property(e => e.PaymentId).HasColumnName("payment_id");

                entity.Property(e => e.Points).HasColumnName("points");

                entity.Property(e => e.UpdateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("update_at");

                entity.Property(e => e.UpdateBy).HasColumnName("update_by");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Payment)
                    .WithMany(p => p.PointUsers)
                    .HasForeignKey(d => d.PaymentId)
                    .HasConstraintName("FK__PointUser__payme__5EBF139D");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.PointUsers)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__PointUser__user___5DCAEF64");
            });

            modelBuilder.Entity<Promotion>(entity =>
            {
                entity.ToTable("Promotion");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.Description)
                    .HasMaxLength(255)
                    .HasColumnName("description");

                entity.Property(e => e.Discount).HasColumnName("discount");

                entity.Property(e => e.EndDate)
                    .HasColumnType("date")
                    .HasColumnName("end_date");

                entity.Property(e => e.StartDate)
                    .HasColumnType("date")
                    .HasColumnName("start_date");

                entity.Property(e => e.UpdateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("update_at");

                entity.Property(e => e.UpdateBy).HasColumnName("update_by");
            });

            modelBuilder.Entity<PromotionUser>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.PromotionId })
                    .HasName("PK__Promotio__1B75A259692AF584");

                entity.ToTable("PromotionUser");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.PromotionId).HasColumnName("promotion_id");

                entity.Property(e => e.DateReceived)
                    .HasColumnType("date")
                    .HasColumnName("date_received");

                entity.HasOne(d => d.Promotion)
                    .WithMany(p => p.PromotionUsers)
                    .HasForeignKey(d => d.PromotionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Promotion__promo__5CD6CB2B");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.PromotionUsers)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Promotion__user___5BE2A6F2");
            });

            modelBuilder.Entity<Review>(entity =>
            {
                entity.ToTable("Review");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.Description)
                    .HasMaxLength(255)
                    .HasColumnName("description");

                entity.Property(e => e.TripId).HasColumnName("trip_id");

                entity.Property(e => e.UpdateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("update_at");

                entity.Property(e => e.UpdateBy).HasColumnName("update_by");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Trip)
                    .WithMany(p => p.Reviews)
                    .HasForeignKey(d => d.TripId)
                    .HasConstraintName("FK__Review__trip_id__5AEE82B9");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Reviews)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__Review__user_id__59FA5E80");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Role");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.RoleName)
                    .HasMaxLength(255)
                    .HasColumnName("role_name");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.UpdateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("update_at");

                entity.Property(e => e.UpdateBy).HasColumnName("update_by");
            });

            modelBuilder.Entity<StatusPayment>(entity =>
            {
                entity.ToTable("StatusPayment");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.TypeOfPayment)
                    .HasMaxLength(255)
                    .HasColumnName("type_of_payment");

                entity.Property(e => e.UpdateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("update_at");

                entity.Property(e => e.UpdateBy).HasColumnName("update_by");
            });

            modelBuilder.Entity<StatusTrip>(entity =>
            {
                entity.ToTable("StatusTrip");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.NewStartupdate)
                    .HasColumnType("date")
                    .HasColumnName("new_startupdate");

                entity.Property(e => e.Reason)
                    .HasColumnType("text")
                    .HasColumnName("reason");

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("status");

                entity.Property(e => e.TickedId).HasColumnName("ticked_id");

                entity.Property(e => e.UpdateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("update_at");

                entity.Property(e => e.UpdateBy).HasColumnName("update_by");

                entity.HasOne(d => d.Ticked)
                    .WithMany(p => p.StatusTrips)
                    .HasForeignKey(d => d.TickedId)
                    .HasConstraintName("FK__StatusTri__ticke__6FE99F9F");
            });

            modelBuilder.Entity<StopPoinTrip>(entity =>
            {
                entity.HasKey(e => new { e.TripId, e.StopPointId })
                    .HasName("PK__StopPoin__31BD6D9A0CBFD61E");

                entity.ToTable("StopPoinTrip");

                entity.Property(e => e.TripId).HasColumnName("trip_id");

                entity.Property(e => e.StopPointId).HasColumnName("stop_point_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.UpdateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("update_at");

                entity.Property(e => e.UpdateBy).HasColumnName("update_by");

                entity.HasOne(d => d.StopPoint)
                    .WithMany(p => p.StopPoinTrips)
                    .HasForeignKey(d => d.StopPointId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__StopPoinT__stop___71D1E811");

                entity.HasOne(d => d.Trip)
                    .WithMany(p => p.StopPoinTrips)
                    .HasForeignKey(d => d.TripId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__StopPoinT__trip___70DDC3D8");
            });

            modelBuilder.Entity<StopPoint>(entity =>
            {
                entity.HasKey(e => e.TripId)
                    .HasName("PK__StopPoin__302A5D9E267FA0E9");

                entity.ToTable("StopPoint");

                entity.Property(e => e.TripId).HasColumnName("trip_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.Location)
                    .HasMaxLength(255)
                    .HasColumnName("location");

                entity.Property(e => e.UpdateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("update_at");

                entity.Property(e => e.UpdateBy).HasColumnName("update_by");
            });

            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.ToTable("Ticket");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CodePromotion)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("code_promotion");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.PointEnd)
                    .HasMaxLength(255)
                    .HasColumnName("point_end");

                entity.Property(e => e.PointStart)
                    .HasMaxLength(255)
                    .HasColumnName("point_start");

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("price");

                entity.Property(e => e.PricePromotion)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("price_promotion");

                entity.Property(e => e.Reason)
                    .HasMaxLength(50)
                    .HasColumnName("reason");

                entity.Property(e => e.SeatCode).HasMaxLength(50);

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("status");

                entity.Property(e => e.TimeFrom)
                    .HasColumnType("datetime")
                    .HasColumnName("timeFrom");

                entity.Property(e => e.TimeTo)
                    .HasColumnType("datetime")
                    .HasColumnName("timeTo");

                entity.Property(e => e.TripId).HasColumnName("trip_id");

                entity.Property(e => e.TypeOfPayment).HasColumnName("type_of_payment");

                entity.Property(e => e.TypeOfTicket).HasColumnName("type_of_ticket");

                entity.Property(e => e.UpdateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("update_at");

                entity.Property(e => e.UpdateBy).HasColumnName("update_by");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");

                entity.HasOne(d => d.Trip)
                    .WithMany(p => p.Tickets)
                    .HasForeignKey(d => d.TripId)
                    .HasConstraintName("FK__Ticket__trip_id__6E01572D");

                entity.HasOne(d => d.TypeOfPaymentNavigation)
                    .WithMany(p => p.Tickets)
                    .HasForeignKey(d => d.TypeOfPayment)
                    .HasConstraintName("FK__Ticket__type_of___6EF57B66");

                entity.HasOne(d => d.TypeOfTicketNavigation)
                    .WithMany(p => p.Tickets)
                    .HasForeignKey(d => d.TypeOfTicket)
                    .HasConstraintName("FK__Ticket__type_of___6B24EA82");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Tickets)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__Ticket__user_id__6C190EBB");

                entity.HasOne(d => d.Vehicle)
                    .WithMany(p => p.Tickets)
                    .HasForeignKey(d => d.VehicleId)
                    .HasConstraintName("FK__Ticket__vehicle___6D0D32F4");
            });

            modelBuilder.Entity<Trip>(entity =>
            {
                entity.ToTable("Trip");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.Description)
                    .HasColumnType("text")
                    .HasColumnName("description");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("name");

                entity.Property(e => e.PointEnd)
                    .HasMaxLength(255)
                    .HasColumnName("point_end");

                entity.Property(e => e.PointStart)
                    .HasMaxLength(255)
                    .HasColumnName("point_start");

                entity.Property(e => e.StartDate)
                    .HasColumnType("date")
                    .HasColumnName("start_date");

                entity.Property(e => e.UpdateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("update_at");

                entity.Property(e => e.UpdateBy).HasColumnName("update_by");

                entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");

                entity.HasOne(d => d.Vehicle)
                    .WithMany(p => p.Trips)
                    .HasForeignKey(d => d.VehicleId)
                    .HasConstraintName("FK__Trip__vehicle_id__6A30C649");
            });

            modelBuilder.Entity<TypeOfDriver>(entity =>
            {
                entity.ToTable("TypeOfDriver");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.Description)
                    .HasColumnType("text")
                    .HasColumnName("description");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.UpdateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("update_at");

                entity.Property(e => e.UpdateBy).HasColumnName("update_by");
            });

            modelBuilder.Entity<TypeOfPayment>(entity =>
            {
                entity.ToTable("TypeOfPayment");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.TypeOfPayment1)
                    .HasMaxLength(255)
                    .HasColumnName("type_of_payment");

                entity.Property(e => e.UpdateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("update_at");

                entity.Property(e => e.UpdateBy).HasColumnName("update_by");
            });

            modelBuilder.Entity<TypeOfTicket>(entity =>
            {
                entity.ToTable("TypeOfTicket");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.Description)
                    .HasMaxLength(255)
                    .HasColumnName("description");

                entity.Property(e => e.UpdateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("update_at");

                entity.Property(e => e.UpdateBy).HasColumnName("update_by");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.HasIndex(e => e.Email, "UQ__User__AB6E616420E2D4BC")
                    .IsUnique();

                entity.HasIndex(e => e.Username, "UQ__User__F3DBC572EB072896")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Address)
                    .HasMaxLength(255)
                    .HasColumnName("address");

                entity.Property(e => e.Avatar)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("avatar");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.Dob)
                    .HasColumnType("datetime")
                    .HasColumnName("dob");

                entity.Property(e => e.Email)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("email");

                entity.Property(e => e.FullName)
                    .HasMaxLength(255)
                    .HasColumnName("fullName");

                entity.Property(e => e.NumberPhone)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("number_phone");

                entity.Property(e => e.Password)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("password");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.UpdateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("update_at");

                entity.Property(e => e.UpdateBy).HasColumnName("update_by");

                entity.Property(e => e.Username)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("username");
            });

            modelBuilder.Entity<UserCancleTicket>(entity =>
            {
                entity.ToTable("UserCancleTicket");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.PaymentId).HasColumnName("payment_id");

                entity.Property(e => e.ReasonCancle)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("reasonCancle");

                entity.Property(e => e.TicketId).HasColumnName("ticket_id");

                entity.Property(e => e.UpdateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("update_at");

                entity.Property(e => e.UpdateBy).HasColumnName("update_by");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Payment)
                    .WithMany(p => p.UserCancleTickets)
                    .HasForeignKey(d => d.PaymentId)
                    .HasConstraintName("FK__UserCancl__payme__76969D2E");

                entity.HasOne(d => d.Ticket)
                    .WithMany(p => p.UserCancleTickets)
                    .HasForeignKey(d => d.TicketId)
                    .HasConstraintName("FK__UserCancl__ticke__787EE5A0");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserCancleTickets)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__UserCancl__user___778AC167");
            });

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId })
                    .HasName("PK__UserRole__6EDEA153E8BC5BC5");

                entity.ToTable("UserRole");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.RoleId).HasColumnName("role_id");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.UserRoles)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__UserRole__role_i__59063A47");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserRoles)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__UserRole__user_i__5812160E");
            });

            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.ToTable("Vehicle");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CarTypeId).HasColumnName("car_type_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.Description)
                    .HasMaxLength(255)
                    .HasColumnName("description");

                entity.Property(e => e.DriverId).HasColumnName("driver_id");

                entity.Property(e => e.LicensePlate)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("license_plate");

                entity.Property(e => e.Seat).HasColumnName("seat");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.UpdateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("update_at");

                entity.Property(e => e.UpdateBy).HasColumnName("update_by");

                entity.HasOne(d => d.CarType)
                    .WithMany(p => p.Vehicles)
                    .HasForeignKey(d => d.CarTypeId)
                    .HasConstraintName("FK__Vehicle__car_typ__60A75C0F");

                entity.HasOne(d => d.Driver)
                    .WithMany(p => p.Vehicles)
                    .HasForeignKey(d => d.DriverId)
                    .HasConstraintName("FK__Vehicle__driver___5FB337D6");
            });

            modelBuilder.Entity<VehicleType>(entity =>
            {
                entity.ToTable("VehicleType");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.Description)
                    .HasMaxLength(255)
                    .HasColumnName("description");

                entity.Property(e => e.UpdateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("update_at");

                entity.Property(e => e.UpdateBy).HasColumnName("update_by");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}

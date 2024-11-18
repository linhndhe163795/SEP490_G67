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

        public virtual DbSet<ChangeTimeTrip> ChangeTimeTrips { get; set; } = null!;
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
        public virtual DbSet<Request> Requests { get; set; } = null!;
        public virtual DbSet<RequestDetail> RequestDetails { get; set; } = null!;
        public virtual DbSet<Review> Reviews { get; set; } = null!;
        public virtual DbSet<Role> Roles { get; set; } = null!;
        public virtual DbSet<Ticket> Tickets { get; set; } = null!;
        public virtual DbSet<Trip> Trips { get; set; } = null!;
        public virtual DbSet<TripDetail> TripDetails { get; set; } = null!;
        public virtual DbSet<TypeOfDriver> TypeOfDrivers { get; set; } = null!;
        public virtual DbSet<TypeOfPayment> TypeOfPayments { get; set; } = null!;
        public virtual DbSet<TypeOfRequest> TypeOfRequests { get; set; } = null!;
        public virtual DbSet<TypeOfTicket> TypeOfTickets { get; set; } = null!;
        public virtual DbSet<TypeOfTrip> TypeOfTrips { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<UserCancleTicket> UserCancleTickets { get; set; } = null!;
        public virtual DbSet<UserRole> UserRoles { get; set; } = null!;
        public virtual DbSet<Vehicle> Vehicles { get; set; } = null!;
        public virtual DbSet<VehicleSeatStatus> VehicleSeatStatuses { get; set; } = null!;
        public virtual DbSet<VehicleTrip> VehicleTrips { get; set; } = null!;
        public virtual DbSet<VehicleType> VehicleTypes { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("server =DELL-G5; database = SEP490_G67;uid=sa;pwd=123;TrustServerCertificate=true");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChangeTimeTrip>(entity =>
            {
                entity.ToTable("ChangeTimeTrip");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.NewStartupdate)
                    .HasColumnType("date")
                    .HasColumnName("new_startupdate");

                entity.Property(e => e.Reason)
                    .HasColumnType("ntext")
                    .HasColumnName("reason");

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .HasColumnName("status");

                entity.Property(e => e.TickedId).HasColumnName("ticked_id");

                entity.Property(e => e.UpdateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("update_at");

                entity.Property(e => e.UpdateBy).HasColumnName("update_by");

                entity.HasOne(d => d.Ticked)
                    .WithMany(p => p.ChangeTimeTrips)
                    .HasForeignKey(d => d.TickedId)
                    .HasConstraintName("FK__ChangeTim__ticke__797309D9");
            });

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
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.Dob)
                    .HasColumnType("datetime")
                    .HasColumnName("dob");

                entity.Property(e => e.Email)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.License)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("license");

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

                entity.HasOne(d => d.TypeOfDriverNavigation)
                    .WithMany(p => p.Drivers)
                    .HasForeignKey(d => d.TypeOfDriver)
                    .HasConstraintName("FK__Driver__type_of___7A672E12");
            });

            modelBuilder.Entity<HistoryRentDriver>(entity =>
            {
                entity.HasKey(e => e.HistoryId)
                    .HasName("PK__HistoryR__096AA2E9EF06E51C");

                entity.ToTable("HistoryRentDriver");

                entity.Property(e => e.HistoryId).HasColumnName("history_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("(getdate())");

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
                    .HasConstraintName("FK__HistoryRe__drive__7B5B524B");

                entity.HasOne(d => d.Vehicle)
                    .WithMany(p => p.HistoryRentDrivers)
                    .HasForeignKey(d => d.VehicleId)
                    .HasConstraintName("FK__HistoryRe__vehic__7C4F7684");
            });

            modelBuilder.Entity<HistoryRentVehicle>(entity =>
            {
                entity.HasKey(e => e.HistoryId)
                    .HasName("PK__HistoryR__096AA2E93060C9A0");

                entity.ToTable("HistoryRentVehicle");

                entity.Property(e => e.HistoryId).HasColumnName("history_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("(getdate())");

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
                    .HasConstraintName("FK__HistoryRe__drive__7D439ABD");

                entity.HasOne(d => d.Vehicle)
                    .WithMany(p => p.HistoryRentVehicles)
                    .HasForeignKey(d => d.VehicleId)
                    .HasConstraintName("FK__HistoryRe__vehic__7E37BEF6");
            });

            modelBuilder.Entity<LossCost>(entity =>
            {
                entity.ToTable("LossCost");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("(getdate())");

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
                    .HasConstraintName("FK__LossCost__loss_c__7F2BE32F");

                entity.HasOne(d => d.Vehicle)
                    .WithMany(p => p.LossCosts)
                    .HasForeignKey(d => d.VehicleId)
                    .HasConstraintName("FK__LossCost__vehicl__00200768");
            });

            modelBuilder.Entity<LossCostType>(entity =>
            {
                entity.ToTable("LossCostType");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.Description)
                    .HasMaxLength(255)
                    .HasColumnName("description");

                entity.Property(e => e.UpdateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("update_at");

                entity.Property(e => e.UpdateBy).HasColumnName("update_by");
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
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.Description)
                    .HasMaxLength(255)
                    .HasColumnName("description");

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("price");

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

                entity.HasOne(d => d.Ticket)
                    .WithMany(p => p.Payments)
                    .HasForeignKey(d => d.TicketId)
                    .HasConstraintName("FK__Payment__ticket___01142BA1");

                entity.HasOne(d => d.TypeOfPaymentNavigation)
                    .WithMany(p => p.Payments)
                    .HasForeignKey(d => d.TypeOfPayment)
                    .HasConstraintName("FK__Payment__type_of__02084FDA");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Payments)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__Payment__user_id__02FC7413");
            });

            modelBuilder.Entity<PaymentRentDriver>(entity =>
            {
                entity.ToTable("PaymentRentDriver");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.Description)
                    .HasColumnType("ntext")
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
                    .HasConstraintName("FK__PaymentRe__histo__03F0984C");
            });

            modelBuilder.Entity<PaymentRentVehicle>(entity =>
            {
                entity.ToTable("PaymentRentVehicle");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CarOwnerId).HasColumnName("car_owner_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("(getdate())");

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
                    .HasConstraintName("FK__PaymentRe__histo__04E4BC85");
            });

            modelBuilder.Entity<PointUser>(entity =>
            {
                entity.ToTable("PointUser");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.Date)
                    .HasColumnType("date")
                    .HasColumnName("date");

                entity.Property(e => e.PaymentId).HasColumnName("payment_id");

                entity.Property(e => e.Points).HasColumnName("points");

                entity.Property(e => e.PointsMinus).HasColumnName("points_minus");

                entity.Property(e => e.UpdateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("update_at");

                entity.Property(e => e.UpdateBy).HasColumnName("update_by");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Payment)
                    .WithMany(p => p.PointUsers)
                    .HasForeignKey(d => d.PaymentId)
                    .HasConstraintName("FK__PointUser__payme__05D8E0BE");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.PointUsers)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__PointUser__user___06CD04F7");
            });

            modelBuilder.Entity<Promotion>(entity =>
            {
                entity.ToTable("Promotion");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CodePromotion)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("code_promotion");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.Description)
                    .HasMaxLength(255)
                    .HasColumnName("description");

                entity.Property(e => e.Discount).HasColumnName("discount");

                entity.Property(e => e.EndDate)
                    .HasColumnType("date")
                    .HasColumnName("end_date");

                entity.Property(e => e.ExchangePoint).HasColumnName("exchange_point");

                entity.Property(e => e.ImagePromotion)
                    .HasMaxLength(255)
                    .HasColumnName("image_promotion");

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
                    .HasName("PK__Promotio__1B75A2590E0987D4");

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
                    .HasConstraintName("FK__Promotion__promo__07C12930");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.PromotionUsers)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Promotion__user___08B54D69");
            });

            modelBuilder.Entity<Request>(entity =>
            {
                entity.ToTable("Request");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.Description)
                    .HasColumnType("ntext")
                    .HasColumnName("description");

                entity.Property(e => e.Note)
                    .HasColumnType("ntext")
                    .HasColumnName("note");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.TypeId).HasColumnName("type_id");

                entity.Property(e => e.UpdateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("update_at");

                entity.Property(e => e.UpdateBy).HasColumnName("update_by");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Type)
                    .WithMany(p => p.Requests)
                    .HasForeignKey(d => d.TypeId)
                    .HasConstraintName("FK__Request__type_id__09A971A2");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Requests)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Request__user_id__0A9D95DB");
            });

            modelBuilder.Entity<RequestDetail>(entity =>
            {
                entity.HasKey(e => e.DetailId)
                    .HasName("PK__Request___38E9A224F690E724");

                entity.ToTable("Request_Details");

                entity.Property(e => e.DetailId).HasColumnName("detail_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.DriverId).HasColumnName("driver_id");

                entity.Property(e => e.EndLocation)
                    .HasMaxLength(255)
                    .HasColumnName("end_location");

                entity.Property(e => e.EndTime)
                    .HasColumnType("datetime")
                    .HasColumnName("end_time");

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(15)
                    .IsUnicode(false)
                    .HasColumnName("phone_number");

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(18, 0)")
                    .HasColumnName("price");

                entity.Property(e => e.RequestId).HasColumnName("request_id");

                entity.Property(e => e.Seats).HasColumnName("seats");

                entity.Property(e => e.StartLocation)
                    .HasMaxLength(255)
                    .HasColumnName("start_location");

                entity.Property(e => e.StartTime)
                    .HasColumnType("datetime")
                    .HasColumnName("start_time");

                entity.Property(e => e.TicketId).HasColumnName("ticket_id");

                entity.Property(e => e.UpdateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("update_at");

                entity.Property(e => e.UpdateBy).HasColumnName("update_by");

                entity.Property(e => e.UserName)
                    .HasMaxLength(100)
                    .HasColumnName("user_name");

                entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");

                entity.HasOne(d => d.Request)
                    .WithMany(p => p.RequestDetails)
                    .HasForeignKey(d => d.RequestId)
                    .HasConstraintName("FK__Request_D__reque__0B91BA14");

                entity.HasOne(d => d.Vehicle)
                    .WithMany(p => p.RequestDetails)
                    .HasForeignKey(d => d.VehicleId)
                    .HasConstraintName("FK__Request_D__vehic__0C85DE4D");
            });

            modelBuilder.Entity<Review>(entity =>
            {
                entity.ToTable("Review");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("(getdate())");

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
                    .HasConstraintName("FK__Review__trip_id__0D7A0286");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Reviews)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__Review__user_id__0E6E26BF");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Role");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("(getdate())");

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
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.Description)
                    .HasMaxLength(255)
                    .HasColumnName("description");

                entity.Property(e => e.Note)
                    .HasMaxLength(255)
                    .HasColumnName("note");

                entity.Property(e => e.NumberTicket).HasColumnName("numberTicket");

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

                entity.Property(e => e.Status)
                    .HasMaxLength(255)
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
                    .HasConstraintName("FK__Ticket__trip_id__0F624AF8");

                entity.HasOne(d => d.TypeOfPaymentNavigation)
                    .WithMany(p => p.Tickets)
                    .HasForeignKey(d => d.TypeOfPayment)
                    .HasConstraintName("FK__Ticket__type_of___114A936A");

                entity.HasOne(d => d.TypeOfTicketNavigation)
                    .WithMany(p => p.Tickets)
                    .HasForeignKey(d => d.TypeOfTicket)
                    .HasConstraintName("FK__Ticket__type_of___10566F31");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Tickets)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__Ticket__user_id__1332DBDC");

                entity.HasOne(d => d.Vehicle)
                    .WithMany(p => p.Tickets)
                    .HasForeignKey(d => d.VehicleId)
                    .HasConstraintName("FK__Ticket__vehicle___123EB7A3");
            });

            modelBuilder.Entity<Trip>(entity =>
            {
                entity.ToTable("Trip");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.Description)
                    .HasMaxLength(255)
                    .HasColumnName("description");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");

                entity.Property(e => e.PointEnd)
                    .HasMaxLength(255)
                    .HasColumnName("point_end");

                entity.Property(e => e.PointStart)
                    .HasMaxLength(255)
                    .HasColumnName("point_start");

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(18, 0)")
                    .HasColumnName("price");

                entity.Property(e => e.StartTime).HasColumnName("start_time");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.UpdateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("update_at");

                entity.Property(e => e.UpdateBy).HasColumnName("update_by");

                entity.HasOne(d => d.TypeOfTripNavigation)
                    .WithMany(p => p.Trips)
                    .HasForeignKey(d => d.TypeOfTrip)
                    .HasConstraintName("FK_Trip_TypeOfTrip");
            });

            modelBuilder.Entity<TripDetail>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.PointEndDetails)
                    .HasMaxLength(255)
                    .HasColumnName("point_end_details");

                entity.Property(e => e.PointStartDetails)
                    .HasMaxLength(255)
                    .HasColumnName("point_start_details");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.TimeEndDetails).HasColumnName("time_end_details");

                entity.Property(e => e.TimeStartDetils).HasColumnName("time_start_detils");

                entity.Property(e => e.TripId).HasColumnName("trip_id");

                entity.Property(e => e.UpdateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("update_at");

                entity.Property(e => e.UpdateBy).HasColumnName("update_by");

                entity.HasOne(d => d.Trip)
                    .WithMany(p => p.TripDetails)
                    .HasForeignKey(d => d.TripId)
                    .HasConstraintName("FK__TripDetai__trip___151B244E");
            });

            modelBuilder.Entity<TypeOfDriver>(entity =>
            {
                entity.ToTable("TypeOfDriver");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.Description)
                    .HasMaxLength(255)
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
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.TypeOfPayment1)
                    .HasMaxLength(255)
                    .HasColumnName("type_of_payment");

                entity.Property(e => e.UpdateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("update_at");

                entity.Property(e => e.UpdateBy).HasColumnName("update_by");
            });

            modelBuilder.Entity<TypeOfRequest>(entity =>
            {
                entity.ToTable("Type_of_Request");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Description)
                    .HasColumnType("ntext")
                    .HasColumnName("description");

                entity.Property(e => e.TypeName)
                    .HasMaxLength(50)
                    .HasColumnName("type_name");
            });

            modelBuilder.Entity<TypeOfTicket>(entity =>
            {
                entity.ToTable("TypeOfTicket");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.Description)
                    .HasMaxLength(255)
                    .HasColumnName("description");

                entity.Property(e => e.UpdateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("update_at");

                entity.Property(e => e.UpdateBy).HasColumnName("update_by");
            });

            modelBuilder.Entity<TypeOfTrip>(entity =>
            {
                entity.ToTable("TypeOfTrip");

                entity.Property(e => e.Description).HasColumnType("text");

                entity.Property(e => e.TypeName)
                    .HasMaxLength(50)
                    .HasColumnName("Type_name");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.HasIndex(e => e.Email, "UQ__User__AB6E616408968483")
                    .IsUnique();

                entity.HasIndex(e => e.Username, "UQ__User__F3DBC57276BCA9CD")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.ActiveCode)
                    .HasMaxLength(255)
                    .HasColumnName("activeCode");

                entity.Property(e => e.Address)
                    .HasMaxLength(255)
                    .HasColumnName("address");

                entity.Property(e => e.Avatar)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("avatar");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("(getdate())");

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
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.PaymentId).HasColumnName("payment_id");

                entity.Property(e => e.ReasonCancle)
                    .HasMaxLength(255)
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
                    .HasConstraintName("FK__UserCancl__payme__160F4887");

                entity.HasOne(d => d.Ticket)
                    .WithMany(p => p.UserCancleTickets)
                    .HasForeignKey(d => d.TicketId)
                    .HasConstraintName("FK__UserCancl__ticke__17036CC0");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserCancleTickets)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__UserCancl__user___17F790F9");
            });

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId })
                    .HasName("PK__UserRole__6EDEA15392D58EB5");

                entity.ToTable("UserRole");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.RoleId).HasColumnName("role_id");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.UserRoles)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__UserRole__role_i__18EBB532");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserRoles)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__UserRole__user_i__19DFD96B");
            });

            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.ToTable("Vehicle");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.Description)
                    .HasMaxLength(255)
                    .HasColumnName("description");

                entity.Property(e => e.DriverId).HasColumnName("driver_id");

                entity.Property(e => e.Image)
                    .HasMaxLength(255)
                    .HasColumnName("image");

                entity.Property(e => e.LicensePlate)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("license_plate");

                entity.Property(e => e.NumberSeat).HasColumnName("number_seat");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.UpdateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("update_at");

                entity.Property(e => e.UpdateBy).HasColumnName("update_by");

                entity.Property(e => e.VehicleOwner).HasColumnName("vehicle_owner");

                entity.Property(e => e.VehicleTypeId).HasColumnName("vehicle_type_id");

                entity.HasOne(d => d.Driver)
                    .WithMany(p => p.Vehicles)
                    .HasForeignKey(d => d.DriverId)
                    .HasConstraintName("FK__Vehicle__driver___1AD3FDA4");

                entity.HasOne(d => d.VehicleOwnerNavigation)
                    .WithMany(p => p.Vehicles)
                    .HasForeignKey(d => d.VehicleOwner)
                    .HasConstraintName("FK__Vehicle__vehicle__1CBC4616");

                entity.HasOne(d => d.VehicleType)
                    .WithMany(p => p.Vehicles)
                    .HasForeignKey(d => d.VehicleTypeId)
                    .HasConstraintName("FK__Vehicle__vehicle__1BC821DD");
            });

            modelBuilder.Entity<VehicleSeatStatus>(entity =>
            {
                entity.ToTable("VehicleSeatStatus");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.DepartureDate)
                    .HasColumnType("datetime")
                    .HasColumnName("departure_date");

                entity.Property(e => e.SeatNumber).HasColumnName("seat_number");

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .HasColumnName("status");

                entity.Property(e => e.UpdateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("update_at");

                entity.Property(e => e.UpdateBy).HasColumnName("update_by");

                entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");

                entity.HasOne(d => d.Vehicle)
                    .WithMany(p => p.VehicleSeatStatuses)
                    .HasForeignKey(d => d.VehicleId)
                    .HasConstraintName("FK__VehicleSe__vehic__1DB06A4F");
            });

            modelBuilder.Entity<VehicleTrip>(entity =>
            {
                entity.HasKey(e => new { e.TripId, e.VehicleId })
                    .HasName("PK__VehicleT__3F031A22E8565E38");

                entity.ToTable("VehicleTrip");

                entity.Property(e => e.TripId).HasColumnName("trip_id");

                entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.UpdateAt)
                    .HasColumnType("datetime")
                    .HasColumnName("update_at");

                entity.Property(e => e.UpdateBy).HasColumnName("update_by");

                entity.HasOne(d => d.Trip)
                    .WithMany(p => p.VehicleTrips)
                    .HasForeignKey(d => d.TripId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__VehicleTr__trip___1EA48E88");

                entity.HasOne(d => d.Vehicle)
                    .WithMany(p => p.VehicleTrips)
                    .HasForeignKey(d => d.VehicleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__VehicleTr__vehic__1F98B2C1");
            });

            modelBuilder.Entity<VehicleType>(entity =>
            {
                entity.ToTable("VehicleType");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("(getdate())");

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

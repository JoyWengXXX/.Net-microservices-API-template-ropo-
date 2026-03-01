using CommonLibrary.Helpers;
using SystemMain.Entities;
using Microsoft.EntityFrameworkCore;

namespace SystemMain
{
    public class SystemMainDbContext : DbContext
    {
        #region Entities
        public DbSet<UserInfo> UserInfo { get; set; }
        public DbSet<LogInRecord> LogInRecord { get; set; }
        public DbSet<UserRole> UserRole { get; set; }
        public DbSet<UserRoleBinding> UserRoleBinding { get; set; }
        public DbSet<Controller> Controller { get; set; }
        #endregion

        public SystemMainDbContext(DbContextOptions<SystemMainDbContext> options) : base(options)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // 對所有 DateTime 屬性應用相同的配置
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                    {
                        property.SetColumnType("timestamp without time zone");
                    }
                }
            }

            modelBuilder.Entity<UserInfo>(entity =>
            {
                //設定索引
                entity.HasKey(e => new { e.UserId });
                entity.HasIndex(e => e.IsVerify).IsClustered(false).HasDatabaseName("IX_UserInfo_IsVerify");
                entity.HasIndex(e => e.IsAdmin).IsClustered(false).HasDatabaseName("IX_UserInfo_IsAdmin");
                entity.HasIndex(e => e.UserStatus).IsClustered(false).HasDatabaseName("IX_UserInfo_UserStatus");
                entity.HasIndex(e => e.SSOType).IsClustered(false).HasDatabaseName("IX_UserInfo_SSOType");

                //新增欄位備註
                entity.Property(e => e.UserId).HasComment("使用者帳號");
                entity.Property(e => e.Password).HasComment("密碼");
                entity.Property(e => e.Name).HasComment("名稱");
                entity.Property(e => e.Email).HasComment("電子信箱");
                entity.Property(e => e.Phone).HasComment("電話");
                entity.Property(e => e.Photo).HasComment("頭貼");
                entity.Property(e => e.IsVerify).HasComment("是否驗證");
                entity.Property(e => e.IsAdmin).HasComment("是否為管理者");
                entity.Property(e => e.SSOType).HasComment("第三方登入方式");
                entity.Property(e => e.VerificationCode).HasComment("驗證碼");
                entity.Property(e => e.VerificationDate).HasComment("驗證日期");
                entity.Property(e => e.UserStatus).HasComment("帳號狀態");
                entity.Property(e => e.StatusMemo).HasComment("狀態備註");
                entity.Property(e => e.CreateDate).HasComment("建立時間");
                entity.Property(e => e.UpdateDate).HasComment("更新日期");
                entity.Property(e => e.Memo).HasComment("備註");
                entity.Property(e => e.TimeZoneID).HasComment("時區ID");

                //預設資料
                entity.HasData(new UserInfo
                {
                    UserId = "TEST@test.com",
                    Password = Convert.ToBase64String(EncryptDecryptHelper.Instance.CreateHash("Paokai@7878855", EncryptDecryptHelper.HashType.SHA256)).Replace("-", ""),
                    Name = "TESTUser",
                    IsVerify = true,
                    IsAdmin = true,
                    TimeZoneID = "Taipei Standard Time",
                    UserStatus = 1,
                    CreateDate = new DateTime(2024, 3, 20)
                });

                entity.Property(e => e.UserStatus).HasDefaultValue(1);
                entity.Property(e => e.IsAdmin).HasDefaultValue(false);
                entity.Property(e => e.StatusMemo).HasDefaultValue(2);
            });

            modelBuilder.Entity<LogInRecord>(entity =>
            {
                //設定索引
                entity.HasKey(e => new { e.UserId, e.Token }).IsClustered(false);
                entity.HasIndex(e => e.IsEnable).IsClustered(false).HasDatabaseName("IX_LogInRecord_IsEnable");

                //新增欄位備註
                entity.Property(e => e.UserId).HasComment("使用者編號");
                entity.Property(e => e.Token).HasComment("Token");
                entity.Property(e => e.IsEnable).HasComment("是否啟用");
                entity.Property(e => e.CreateDate).HasComment("建立時間");

                //預設資料
            });

            modelBuilder.Entity<UserRole>(entity =>
            {
                //設定索引
                entity.HasKey(e => e.RoleId);
                entity.HasIndex(e => e.IsEnable).IsClustered(false).HasDatabaseName("IX_UserRole_IsEnable");
                entity.HasIndex(e => e.RoleOrder).IsClustered(false).HasDatabaseName("IX_UserRole_RoleOrder");

                //新增欄位備註
                entity.Property(e => e.RoleId).HasComment("角色編號");
                entity.Property(e => e.RoleName).HasComment("角色名稱");
                entity.Property(e => e.RoleOrder).HasComment("角色順位");
                entity.Property(e => e.IsEnable).HasComment("是否啟用");
                entity.Property(e => e.CreateDate).HasComment("建立時間");
                entity.Property(e => e.UpdateDate).HasComment("更新日期");

                //預設資料
                entity.HasData(new UserRole
                {
                    RoleId = ConverterHelper.StringToGuid("StoreOwner"),
                    RoleName = "第一級",
                    RoleOrder = 1,
                    IsEnable = true,
                    CreateDate = new DateTime(2024, 8, 5)
                });
                entity.HasData(new UserRole
                {
                    RoleId = ConverterHelper.StringToGuid("Shareholder"),
                    RoleName = "第二級",
                    RoleOrder = 2,
                    IsEnable = true,
                    CreateDate = new DateTime(2024, 8, 5)
                });
                entity.HasData(new UserRole
                {
                    RoleId = ConverterHelper.StringToGuid("StoreManager"),
                    RoleName = "第三級",
                    RoleOrder = 3,
                    IsEnable = true,
                    CreateDate = new DateTime(2024, 8, 5)
                });
            });

            modelBuilder.Entity<UserRoleBinding>(entity =>
            {
                //設定索引
                entity.HasKey(e => new { e.UserId, e.RoleId, e.ControllerId }).IsClustered(false);

                //新增欄位備註
                entity.Property(e => e.UserId).HasComment("使用者編號");
                entity.Property(e => e.RoleId).HasComment("角色編號");
                entity.Property(e => e.ControllerId).HasComment("控制器編號");
                entity.Property(e => e.CreateAllowed).HasComment("是否允許新增");
                entity.Property(e => e.QueryAllowed).HasComment("是否允許查詢");
                entity.Property(e => e.UpdateAllowed).HasComment("是否允許更新");
                entity.Property(e => e.DeleteAllowed).HasComment("是否允許刪除");
                entity.Property(e => e.CreateDate).HasComment("建立時間");

                //預設資料
            });


            modelBuilder.Entity<Controller>(entity =>
            {
                //設定索引
                entity.HasKey(e => e.ControllerId);
                entity.HasIndex(e => e.IsEnable).IsClustered(false).HasDatabaseName("IX_APIController_IsEnable");

                //新增欄位備註
                entity.Property(e => e.ControllerId).HasComment("控制器編號");
                entity.Property(e => e.ControllerName).HasComment("控制器名稱");
                entity.Property(e => e.IsEnable).HasComment("是否啟用");
                entity.Property(e => e.CreateDate).HasComment("建立時間");
                entity.Property(e => e.UpdateDate).HasComment("更新日期");

                //預設資料
                #region
                entity.HasData(new Controller
                {
                    ControllerId = "Controller",
                    ControllerName = "功能設定",
                    IsEnable = true,
                    CreateDate = new DateTime(2024, 8, 5),
                });
                entity.HasData(new Controller
                {
                    ControllerId = "LogIn",
                    ControllerName = "登入",
                    IsEnable = true,
                    CreateDate = new DateTime(2024, 8, 5),
                });
                entity.HasData(new Controller
                {
                    ControllerId = "SignIn",
                    ControllerName = "註冊",
                    IsEnable = true,
                    CreateDate = new DateTime(2024, 8, 5),
                });
                entity.HasData(new Controller
                {
                    ControllerId = "UserRolePermission",
                    ControllerName = "角色權限",
                    IsEnable = true,
                    CreateDate = new DateTime(2024, 8, 5),
                });
                entity.HasData(new Controller
                {
                    ControllerId = "UserRole",
                    ControllerName = "使用者角色",
                    IsEnable = true,
                    CreateDate = new DateTime(2024, 8, 5),
                });
                #endregion
            });

        }
    }
}



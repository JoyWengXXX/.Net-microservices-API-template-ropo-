using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SystemMain.Migrations
{
    /// <inheritdoc />
    public partial class SystemMainInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Controller",
                columns: table => new
                {
                    ControllerId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "控制器編號"),
                    ControllerName = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "控制器名稱"),
                    IsEnable = table.Column<bool>(type: "boolean", nullable: false, comment: "是否啟用"),
                    CreateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "建立時間"),
                    UpdateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "更新日期")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Controller", x => x.ControllerId);
                });

            migrationBuilder.CreateTable(
                name: "LogInRecord",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false, comment: "使用者編號"),
                    Token = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false, comment: "Token"),
                    IsEnable = table.Column<bool>(type: "boolean", nullable: false, comment: "是否啟用"),
                    CreateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "建立時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogInRecord", x => new { x.UserId, x.Token });
                });

            migrationBuilder.CreateTable(
                name: "UserInfo",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false, comment: "使用者帳號"),
                    Password = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, comment: "密碼"),
                    Name = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "名稱"),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true, comment: "電子信箱"),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, comment: "電話"),
                    Photo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true, comment: "頭貼"),
                    IsVerify = table.Column<bool>(type: "boolean", nullable: false, comment: "是否驗證"),
                    IsAdmin = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false, comment: "是否為管理者"),
                    SSOType = table.Column<int>(type: "integer", nullable: false, comment: "第三方登入方式"),
                    VerificationCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true, comment: "驗證碼"),
                    VerificationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "驗證日期"),
                    UserStatus = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)1, comment: "帳號狀態"),
                    StatusMemo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, defaultValue: "2", comment: "狀態備註"),
                    CreateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "建立時間"),
                    UpdateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "更新日期"),
                    Memo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true, comment: "備註"),
                    TimeZoneID = table.Column<string>(type: "text", nullable: false, comment: "時區ID")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInfo", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false, comment: "角色編號"),
                    RoleName = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "角色名稱"),
                    RoleOrder = table.Column<short>(type: "smallint", nullable: false, comment: "角色順位"),
                    IsEnable = table.Column<bool>(type: "boolean", nullable: false, comment: "是否啟用"),
                    CreateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "建立時間"),
                    UpdateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, comment: "更新日期")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "UserRoleBinding",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false, comment: "使用者編號"),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false, comment: "角色編號"),
                    ControllerId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "控制器編號"),
                    CreateAllowed = table.Column<bool>(type: "boolean", nullable: false, comment: "是否允許新增"),
                    QueryAllowed = table.Column<bool>(type: "boolean", nullable: false, comment: "是否允許查詢"),
                    UpdateAllowed = table.Column<bool>(type: "boolean", nullable: false, comment: "是否允許更新"),
                    DeleteAllowed = table.Column<bool>(type: "boolean", nullable: false, comment: "是否允許刪除"),
                    CreateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, comment: "建立時間")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoleBinding", x => new { x.UserId, x.RoleId, x.ControllerId });
                });

            migrationBuilder.InsertData(
                table: "Controller",
                columns: new[] { "ControllerId", "ControllerName", "CreateDate", "IsEnable", "UpdateDate" },
                values: new object[,]
                {
                    { "Controller", "功能設定", new DateTime(2024, 8, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), true, null },
                    { "LogIn", "登入", new DateTime(2024, 8, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), true, null },
                    { "SignIn", "註冊", new DateTime(2024, 8, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), true, null },
                    { "UserRole", "使用者角色", new DateTime(2024, 8, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), true, null },
                    { "UserRolePermission", "角色權限", new DateTime(2024, 8, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), true, null }
                });

            migrationBuilder.InsertData(
                table: "UserInfo",
                columns: new[] { "UserId", "CreateDate", "Email", "IsAdmin", "IsVerify", "Memo", "Name", "Password", "Phone", "Photo", "SSOType", "TimeZoneID", "UpdateDate", "UserStatus", "VerificationCode", "VerificationDate" },
                values: new object[] { "TEST@test.com", new DateTime(2024, 3, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, true, null, "TESTUser", "zBid3apzguMvkuuvaV3ew7JWYtxkukVN2As7P5aDKLg=", null, null, 0, "Taipei Standard Time", null, (short)1, null, null });

            migrationBuilder.InsertData(
                table: "UserRole",
                columns: new[] { "RoleId", "CreateDate", "IsEnable", "RoleName", "RoleOrder", "UpdateDate" },
                values: new object[,]
                {
                    { new Guid("544cc745-6a1c-9937-9638-a53a1bd524fd"), new DateTime(2024, 8, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "第一級", (short)1, null },
                    { new Guid("c447d771-ac6c-1d30-af0f-fbb734171512"), new DateTime(2024, 8, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "第三級", (short)3, null },
                    { new Guid("dfa8f949-153a-7739-b659-0fbd3cbe502c"), new DateTime(2024, 8, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "第二級", (short)2, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_APIController_IsEnable",
                table: "Controller",
                column: "IsEnable");

            migrationBuilder.CreateIndex(
                name: "IX_LogInRecord_IsEnable",
                table: "LogInRecord",
                column: "IsEnable");

            migrationBuilder.CreateIndex(
                name: "IX_UserInfo_IsAdmin",
                table: "UserInfo",
                column: "IsAdmin");

            migrationBuilder.CreateIndex(
                name: "IX_UserInfo_IsVerify",
                table: "UserInfo",
                column: "IsVerify");

            migrationBuilder.CreateIndex(
                name: "IX_UserInfo_SSOType",
                table: "UserInfo",
                column: "SSOType");

            migrationBuilder.CreateIndex(
                name: "IX_UserInfo_UserStatus",
                table: "UserInfo",
                column: "UserStatus");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_IsEnable",
                table: "UserRole",
                column: "IsEnable");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_RoleOrder",
                table: "UserRole",
                column: "RoleOrder");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Controller");

            migrationBuilder.DropTable(
                name: "LogInRecord");

            migrationBuilder.DropTable(
                name: "UserInfo");

            migrationBuilder.DropTable(
                name: "UserRole");

            migrationBuilder.DropTable(
                name: "UserRoleBinding");
        }
    }
}

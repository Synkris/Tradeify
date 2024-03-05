using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.Migrations
{
    public partial class addUserRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("INSERT INTO CommonDropDowns VALUES (22 ,1,'Female',1,0,GETDATE());INSERT INTO CommonDropDowns VALUES (23 ,1,'Male',1,0,GETDATE());");

            migrationBuilder.Sql("INSERT INTO AspNetUsers (Id,Discriminator,FirstName,LastName,Password,GenderId,RegFeePaymentId,RegFeePaid,RefferrerId,ParentId,DateRegistered,CurrentLastLoginTime,LastLogoutTime,Deactivated,LastGenPaid,LastPendingGen,VTUActivationFeePaid,CordinatorId,UserName,NormalizedUserName,Email,NormalizedEmail,EmailConfirmed,PasswordHash,SecurityStamp,ConcurrencyStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnd,LockoutEnabled,AccessFailedCount) VALUES ('0b4c6369-e851-42ac-8945-deb415f590e5','ApplicationUser','System','User(Dont Delete)','Love@5678',23,NULL,1,NULL,NULL,GETDATE(),GETDATE(),GETDATE(),0,0,0,0, NULL,'systemuser','SYSTEMUSER','developers@bivisoft.com','developers@bivisoft.com',0,'AQAAAAEAACcQAAAAEHCHVhxfYgplns/e/dLbn3qjeQ/V87+dAIjYwMLFQ68Z41wafdH5AStgVbnKaqzoVg==','KLR3LCDHK47KIAMF6D46VNW5M3E626EV','6c01785a-5629-4761-b564-95efc7da8ce0','08107898164',0,0,NULL,1,0)");

            migrationBuilder.Sql("INSERT INTO AspNetRoles VALUES (NewId(),'SuperAdmin','SuperAdmin',NEWID());INSERT INTO AspNetRoles VALUES ('F41483A1-D881-4E28-BE7A-21282467C816','Admin','Admin',NEWID());INSERT INTO AspNetRoles VALUES (NewId(),'User','User',NEWID());");

            migrationBuilder.Sql("INSERT INTO AspNetUserRoles VALUES ('0b4c6369-e851-42ac-8945-deb415f590e5','F41483A1-D881-4E28-BE7A-21282467C816');");
        }
    }
}

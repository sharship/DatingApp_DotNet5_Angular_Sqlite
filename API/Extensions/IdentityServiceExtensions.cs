using System.Text;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace API.Extensions
{
    public static class IdentityServiceExtensions
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
        {

            #region config Identity
            services
                .AddIdentityCore<AppUser>(options =>
                {
                    options.Password.RequireNonAlphanumeric = false;
                }) // add and config Identity of AppUser
                .AddRoles<AppRole>() // add role related service
                .AddRoleManager<RoleManager<AppRole>>()  // manage role
                .AddSignInManager<SignInManager<AppUser>>()  // manage user login
                .AddRoleValidator<RoleValidator<AppRole>>()  // validate role
                .AddEntityFrameworkStores<DataContext>();  // add DB context implementation, with Identity tables we have configured
            
            #endregion


            #region  config JWT Authentication
            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)  // use JWT as authentication method
                .AddJwtBearer(options =>
                {
                    // config JWT options: how to authenticate via JWT
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        // compare token signature part (3rd part), via symetric key stored in server, to validate this token and included claims
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"])),

                        ValidateIssuer = false,  // back-end API server
                        ValidateAudience = false,  // front-end Angular
                    };
                });
            #endregion


            #region congif Authorization: add policies
            services.AddAuthorization(options => 
            {
                options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
                options.AddPolicy("ModeratePhotoRole", policy => policy.RequireRole("Admin", "Moderator"));
            });

            #endregion

            return services;
        }
    }
}
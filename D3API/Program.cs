using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using D3API.Data.Context;
using Microsoft.EntityFrameworkCore;
using D3API.Data.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

#region Default
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
#endregion

#region Database
var connectionString = builder.Configuration.GetConnectionString("Company_Connection_string");
builder.Services.AddDbContext<CompanyContext>(options => options.UseSqlServer(connectionString));

#endregion

#region Identity 
//the configuration of identity must be before the Authentication
//Mainly specify the context and the type of the user that the UserManger will use


builder.Services.AddIdentity<AppUser, IdentityRole>(options =>                      //send<user,role>
{
    options.Password.RequiredUniqueChars = 3;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 3;


    options.User.RequireUniqueEmail = true;
})                     
       .AddEntityFrameworkStores<CompanyContext>();                      //the user will stored in any datbase(which context)?



#endregion

#region Authentication
builder.Services.AddAuthentication(options =>          //authentication type that will be used in application(override the default)
{
    options.DefaultAuthenticateScheme = "Ahmed";           //scheme that will be used to authenticate
    options.DefaultChallengeScheme = "Ahmed";              //to handle challenge(if the user go to secured endpoint without authentication)

}).AddJwtBearer("Ahmed", options =>                     //configure some authentication schema (name of authentication scheme,key used when validating request)
{
    string? keyString = builder.Configuration.GetValue<string>("SecretKey");
    byte[] keyInBytes = Encoding.ASCII.GetBytes(keyString!);
    SymmetricSecurityKey key = new SymmetricSecurityKey(keyInBytes);

    options.TokenValidationParameters = new TokenValidationParameters
    {
        IssuerSigningKey=key,                //the issuer is the server(who create the token)
        ValidateAudience = false,           //the audience is the secured endpoint and the subject is the user itself
        ValidateIssuer = false
    };
});

#endregion

#region Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ForEmployeesOnly", policy =>              //create policy called ForEmployeesOnly
    {
        policy.RequireClaim(ClaimTypes.Role, "Employee")        //claim name , allowed values//if  you didnt write the allowd values,
              .RequireClaim(ClaimTypes.NameIdentifier);         // it will apply this policy on any one have this role
    });
    options.AddPolicy("ForManagersOnly", policy =>              //create policy called ForEmployeesOnly
    {
        policy.RequireClaim(ClaimTypes.Role, "Manager")        
              .RequireClaim(ClaimTypes.NameIdentifier);         
    });

});
#endregion

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();    //must be before Authorization

app.UseAuthorization();

app.MapControllers();

app.Run();

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using  D3API.Dtos;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using D3API.Data.Models;

namespace D3API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<AppUser> _userManager;

        public UsersController(IConfiguration configuration,UserManager<AppUser>userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
        }





        #region Static login -send token to the user
        [HttpPost]
        [Route("static-login")]
        public ActionResult<TokenDto> Login(LoginDto credentials)
        {
            if (credentials.UserName != "admin" || credentials.Password !="pass") { return Unauthorized("Wrong Credentials !!"); }
            List<Claim> claimsList = new()
            { 
                new Claim(ClaimTypes.NameIdentifier,Guid.NewGuid().ToString()),
                new Claim("Name","CoolAdmin")
            };
            //get value of the key that called SecretKey and convert it to string
            string? keyString = _configuration.GetValue<string>("SecretKey");  
            
            //covert from string to byte array and ! to stop the null
            byte[] keyInBytes=Encoding.ASCII.GetBytes(keyString!);

            //make Symmetric Security Key object
            SymmetricSecurityKey key =new SymmetricSecurityKey(keyInBytes);

            //make hashing creteria to get the hashing algorithm and secret key in one object
            var signingCredentials =new SigningCredentials(key,SecurityAlgorithms.HmacSha256Signature);

            //putting all parts together to generate the token
            DateTime expire = DateTime.Now.AddMinutes(15);
            var token = new JwtSecurityToken(
                claims:claimsList,
                signingCredentials:signingCredentials,
                expires:expire
                );
              
            //create object from token handler to generate the hashed token string
            var tokenHandler=new JwtSecurityTokenHandler();
            var tokenString=tokenHandler.WriteToken(token);

            return new TokenDto
            {
                Token = tokenString
            };
        }

        #endregion


        #region Register
        [HttpPost]
        [Route("register")]
         public ActionResult Register(RegisterDto registerDto)
        {
            var user = new AppUser
            {
                UserName = registerDto.UserName,
                Department = registerDto.Department,        //you can map any property from Dto except password ,its manager responsibility
                Email = registerDto.Email,

            };
            var creationResult= _userManager.CreateAsync(user,registerDto.Password).Result;
            if(!creationResult.Succeeded) { return BadRequest(creationResult.Errors); }
            var claims = new List<Claim>
            {
                new (ClaimTypes.NameIdentifier,user.Id),
                new (ClaimTypes.Role,"Employee"),
            };
            var addingClaimsResult=_userManager.AddClaimsAsync(user, claims).Result;
            if (!addingClaimsResult.Succeeded) { return BadRequest(addingClaimsResult.Errors); }

               return NoContent();
        }
        #endregion


        #region Clean Login
        [HttpPost]
        [Route("CleanLogin")]
        public ActionResult<TokenDto> Clean_login(LoginDto credentials)
        {
            AppUser? user = _userManager.FindByNameAsync(credentials.UserName).Result;
            if (user == null) { return BadRequest("USER NOT FOUND !!"); }

            bool isPasswordCorrect = _userManager.CheckPasswordAsync(user, credentials.Password).Result;
            if (!isPasswordCorrect) { return BadRequest("WRONG CREDENTIALS !!"); }

            List<Claim> claimsList = _userManager.GetClaimsAsync(user).Result.ToList();


            string? keyString = _configuration.GetValue<string>("SecretKey");

            byte[] keyInBytes = Encoding.ASCII.GetBytes(keyString!);

            SymmetricSecurityKey key = new SymmetricSecurityKey(keyInBytes);

            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            DateTime expire = DateTime.Now.AddMinutes(15);
            var token = new JwtSecurityToken(
                claims: claimsList,
                signingCredentials: signingCredentials,
                expires: expire
                );

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenString = tokenHandler.WriteToken(token);

            return new TokenDto
            {
                Token = tokenString
            };
        }

        #endregion 


    }
}












/*
 [static login]
1.write your secret key in secrets.
2.make DTO class that have username,password.
3.make POST Action that responsible for send token to user .
4.check if credential of the user is not valid ,return bad request.
5.if valid ,make the claim list that consists of keys and values.
6.we need the secret key from configuration,so you need to inject IConfiguration in the constructor and generate the private field.
7.get your secret key.
8.convert the secret key string to byte array.
9.make new object of type key (SymmetricSecurityKey).
10.get the hashing algorithm and secret key onject in one object together.
11.install-package system.identifyModel.Tokens.JWT     >>to generate the token automatically.
12.put all parts together in addition to expire date in new object  to generate the token .
13.generate the token by making new object of tokenHandler.
14.return the token string.

 ************************************************************
 
 [Get access of secured endpoint using token]
-make any endpoint secure,by adding  [Authorize] attribute on the action,like in data controller.
if we tried to access it, there will respond with 401 unauthorized,because there is no authentication schema has been created.
So to Configure it:
1.install-package Microsoft.AspNetCore.Authentication.JwtBearer      >>to add authentication as a middleware for .net
it is make the .net accept the authentication with jwt (build the identity from jwt claims)
2.configure the authentication in program.cs then use this authentication through writing :  app.UseAuthentication();    
*/

/*
 [Useing Asp.net identity ,using database]
1.install the 3  Ef core packages and the 2 packages that manage the identity functionalities and database models.
2.make context class that inherit from IdentityDbContext
3.make the constructore that take paramaerter options of type DbContextOptions and make chain with base and seding options to it.
4.make the connection string in secrets ang configure it in program.cs services.
5.make AppUser class (that will be our identity user) that inherit everything from IdentityUser and add things that you want to add like Department.
if you add migration and update database,you will find there is tables created automatically,but the appuser table dont contain
department,because it didn't determine yet who is the user,he used the default.
6.in the context i need to determine who is the user <AppUser>
7.now if you now add migration and update database you will find a new coulmn in user table called Department.
8.now we want to use this identity fuctionalities, through using builder.Services.AddIdentity in program.cs
9.TO USE this identity you must inject UserManager of type <AppUser>in the constructor and generate its private field.
10.make register Action that return string and take user of type RegisterDto (after making a RegisterDto class)
11.now we need to implement the login action,make your checks then generate the token.
 
 
 */


/*[Authorization]
 1.wite our policy in program.cs [authorization region]
2.to apply a policy on certain endpoint ,write attribute Authorize with policy name instead of authorize attribute only ,like in 
data controller.
3.make different endpoints with different policies to test.
*/

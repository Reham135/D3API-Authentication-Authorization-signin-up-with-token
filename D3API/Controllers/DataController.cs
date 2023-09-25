using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace D3API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataController : ControllerBase
    {
        [HttpGet]
        [Authorize]
        public ActionResult GetGeneralSecuredData()
        {
            return Ok(new
            {
                Name = "Reham",
                Age = 25,
            });
        }



        [HttpGet]
        [Authorize(Policy ="ForEmployeesOnly")]
        [Route("Employees")]
        public ActionResult GeSecuredDataEmployeesOnly() 
        {
            return Ok(new
            {
                Name="Reham Employee",
                Age=25,
            });
        }



        [HttpGet]
        [Authorize(Policy = "ForManagersOnly")]
        [Route("Managers")]

        public ActionResult GetSecuredDataForManagersOnly()
        {
            return Ok(new
            {
                Name = "Reham Manager",
                Age = 25
            });
        }
    }
}

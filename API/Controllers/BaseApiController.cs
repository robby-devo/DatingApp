using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]  //takes url like /mebers or /users
    [ApiController]  //JSON Formatting
    public class BaseApiController : ControllerBase
    {
    }
}

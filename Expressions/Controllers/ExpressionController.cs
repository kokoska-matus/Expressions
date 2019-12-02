using Expressions.Domain;
using Expressions.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Expressions.Controllers
{
    [Route("compute")]
    [ApiController]
    public class ExpressionController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<string> Get(string expr)
        {
            try
            {
                return new Expression(expr).GetResult().ToString();
            }
            catch (ExpressionException e)
            {
                return e.Message;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MicroRabbit.Banking.Application.Interfaces;
using MicroRabbit.Banking.Application.Models;
using MicroRabbit.Banking.Domain.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MicroRabbit.Banking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BankingController : ControllerBase
    {
        private readonly IAccountService _AccountService;
        public BankingController(IAccountService AccountService)
        {
            _AccountService = AccountService;
        }
        // GET: api/<BankingController>
        [HttpGet]
        public ActionResult<IEnumerable<Account>> Get()
        {
            return Ok(_AccountService.GetAccounts());
        }

        // GET api/<BankingController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }
        [HttpPost]
        public IActionResult Post([FromBody] AccountTransfer accountTransfer)
        {
            _AccountService.Transfer(accountTransfer);
            return Ok(accountTransfer);
        }

       
    }
}

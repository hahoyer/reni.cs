using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc;
using ReniUI.RestFul;

namespace ReniCompilerService.Controllers
{
    [Route("Channel")]
    public sealed class ChannelController : Controller
    {
        [HttpGet]
        public string Get() => Repo.AddAndGetNewId();

        [HttpGet("{id}")]
        public Channel Get(string id) => Repo.Find(id);

        [HttpGet("{id}/Result")]
        public string GetResult(string id) => Repo.Find(id).GetResult();

        [HttpPut("{id}")]
        public void Put(string id, [FromBody] Channel value) => Repo.Update(id,value);

        [HttpDelete("{id}")]
        public void Delete(string id) => Repo.Remove(id);

        [FromServices]
        public IChannelRepo Repo { get; set; }
    }
}
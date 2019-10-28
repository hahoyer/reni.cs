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
        [HttpGet("{id}")]
        public Channel Get(string id) => Repo.Find(id);

        [HttpGet("{id}/Output")]
        public string GetOutput(string id) => Repo.Find(id).GetOutput();

        [HttpPost("{id}/Output")]
        public string PostOutput(string id)
        {
            var channel = Repo.Find(id);
            channel.ResetResult();
            return channel.GetOutput();
        }

        [HttpGet("{id}/UnexpectedErrors")]
        public string GetUnexpectedErrors(string id) => Repo.Find(id).GetUnexpectedErrors();

        [HttpPost("{id}/UnexpectedErrors")]
        public string PostUnexpectedErrors(string id)
        {
            var channel = Repo.Find(id);
            channel.ResetResult();
            return channel.GetUnexpectedErrors();
        }

        [HttpGet("{id}/Issues")]
        public Issue[] GetIssues(string id) => Repo.Find(id).GetIssues();

        [HttpPost("{id}/Issues")]
        public Issue[] PostIssues(string id)
        {
            var channel = Repo.Find(id);
            channel.ResetResult();
            return channel.GetIssues();
        }

        [HttpPut("{id}")]
        public void Put(string id, [FromBody] Channel value) => Repo.Update(id, value);

        [HttpPost]
        public string Post([FromBody] Channel value) => Repo.Create(value);

        [HttpDelete("{id}")]
        public void Delete(string id) => Repo.Remove(id);

        [FromServices]
        public IChannelRepo Repo { get; set; }
    }
}
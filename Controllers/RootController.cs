//using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using restapi.Models;

namespace restapi.Controllers
{
    public class RootController : Controller
    {
        // GET api/values
        [Route("~/")]
        [HttpGet]
        [Produces(ContentTypes.Root)]
        [ProducesResponseType(typeof(IDictionary<ApplicationRelationship, DocumentLink>), 200)]
        public IDictionary<ApplicationRelationship, DocumentLink> Get()
        {
            return new Dictionary<ApplicationRelationship, DocumentLink>()
            {  
                { 
                    ApplicationRelationship.Timesheets, new DocumentLink() 
                    { 
                        Method = Method.Get,
                        Type = ContentTypes.Timesheets,
                        Relationship = DocumentRelationship.Timesheets,
                        Reference = "/timesheets"
                    }   
                }
            };
        }

        [Route("~/")]
        [HttpPost]
        [Produces(ContentTypes.Timesheet)]
        [ProducesResponseType(typeof(Timecard), 200)]
        public Timecard Create([FromBody] DocumentResource resource)
        {
            var timecard = new Timecard(resource.Resource);

            var entered = new Entered() { Resource = resource.Resource };

            timecard.Transitions.Add(new Transition(entered));

            Database.Add(timecard);

            return timecard;
        }
    }
}

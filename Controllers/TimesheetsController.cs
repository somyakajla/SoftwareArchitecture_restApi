using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using restapi.Models;

namespace restapi.Controllers
{
    [Route("[controller]")]
    public class TimesheetsController : Controller
    {
        [HttpGet]
        [Produces(ContentTypes.Timesheets)]
        [ProducesResponseType(typeof(IEnumerable<Timecard>), 200)]
        public IEnumerable<Timecard> GetAll()
        {
            return Database
                .All
                .OrderBy(t => t.Opened);
        }

        [HttpGet("{id}")]
        [Produces(ContentTypes.Timesheet)]
        [ProducesResponseType(typeof(Timecard), 200)]
        [ProducesResponseType(404)]
        public IActionResult GetOne(string id)
        {
            Timecard timecard = Database.Find(id);

            if (timecard != null)
            {
                return Ok(timecard);
            }
            else
            {
                return NotFound();
            }
        }

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

        [HttpDelete("{id}/{resourceId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(403)]
        [ProducesResponseType(401)]
        public IActionResult DeleteLine(string id, int resourceId)
        {
            if (UserDB.Find(resourceId) != null)
            {
                Timecard timecard = Database.Find(id);

                if (timecard == null)
                {
                    return NotFound();
                }

                if (timecard.Resource != resourceId)
                {
                    return StatusCode(403, new NotAuthorized() { });
                }

                if (timecard.Status != TimecardStatus.Cancelled && timecard.Status != TimecardStatus.Draft)
                {
                    return StatusCode(409, new InvalidStateError() { });
                }

                Database.Delete(id);
                return Ok();
            }
            return StatusCode(401, new InvalidUser() { });
        }

        [HttpGet("{id}/lines")]
        [Produces(ContentTypes.TimesheetLines)]
        [ProducesResponseType(typeof(IEnumerable<AnnotatedTimecardLine>), 200)]
        [ProducesResponseType(404)]
        public IActionResult GetLines(string id)
        {
            Timecard timecard = Database.Find(id);

            if (timecard != null)
            {
                var lines = timecard.Lines
                    .OrderBy(l => l.WorkDate)
                    .ThenBy(l => l.Recorded);

                return Ok(lines);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost("{id}/lines")]
        [Produces(ContentTypes.TimesheetLine)]
        [ProducesResponseType(typeof(AnnotatedTimecardLine), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(InvalidStateError), 409)]
        public IActionResult AddLine(string id, [FromBody] TimecardLine timecardLine)
        {
            Timecard timecard = Database.Find(id);

            if (timecard != null)
            {
                if (timecard.Status != TimecardStatus.Draft)
                {
                    return StatusCode(409, new InvalidStateError() { });
                }

                var annotatedLine = timecard.AddLine(timecardLine);

                return Ok(annotatedLine);
            }
            else
            {
                return NotFound();
            }
        }


        /*
        *Replaced a timecardLine object 
        *
        * */
        [HttpPost("{timecardId}/lines/{lineId}")]
        [ProducesResponseType(typeof(AnnotatedTimecardLine), 200)]
        [ProducesResponseType(404)]
        public IActionResult UpdateLine(string timecardId, string lineId, [FromBody] TimecardLine timecardLine)
        {
            Timecard timecard = Database.Find(timecardId);

            if (timecard == null)
            {
                return NotFound();
            }
            if (timecard.Lines != null)
            {

                return Ok(timecard.ReplaceLine(lineId, timecardLine));
            }

            return NotFound();
        }

        /*
        * Update a timecardLine item/some items
        *
        * */
        [HttpPatch("{timecardId}/{lineId}")]
        [ProducesResponseType(typeof(AnnotatedTimecardLine), 200)]
        [ProducesResponseType(404)]
        public IActionResult UpdateLineItems(string timecardId, string lineId, [FromBody] TimecardLineRequest timecardLine)
        {
            Timecard timecard = Database.Find(timecardId);
            if (timecard == null)
            {
                return NotFound();
            }

            if (timecard.Lines != null)
            {
                return Ok(timecard.UpdateLine(lineId, timecardLine));
            }

            return NotFound();
        }


        [HttpGet("{id}/transitions")]
        [Produces(ContentTypes.Transitions)]
        [ProducesResponseType(typeof(IEnumerable<Transition>), 200)]
        [ProducesResponseType(404)]
        public IActionResult GetTransitions(string id)
        {
            Timecard timecard = Database.Find(id);

            if (timecard != null)
            {
                return Ok(timecard.Transitions);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost("{id}/submittal")]
        [Produces(ContentTypes.Transition)]
        [ProducesResponseType(typeof(Transition), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(403)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(InvalidStateError), 409)]
        [ProducesResponseType(typeof(EmptyTimecardError), 409)]
        public IActionResult Submit(string id, [FromBody] Submittal submittal)
        {
            if (UserDB.Find(submittal.Resource) != null)
            {
                Timecard timecard = Database.Find(id);

                if (timecard != null)
                {
                    if (timecard.Status != TimecardStatus.Draft)
                    {
                        return StatusCode(409, new InvalidStateError() { });
                    }

                    if (timecard.Lines.Count < 1)
                    {
                        return StatusCode(409, new EmptyTimecardError() { });
                    }
                    /*
                     * Submittal should be same as the creater of the timecard.        
                     */
                    if (timecard.Resource == submittal.Resource)
                    {
                        var transition = new Transition(submittal, TimecardStatus.Submitted);
                        timecard.Transitions.Add(transition);
                        return Ok(transition);
                    }

                    /*
                     * user is not authroized to perform this task.
                     */
                    return StatusCode(403, new NotAuthorized { });
                }
                else
                {
                    return NotFound();
                }
            }
            return StatusCode(401, new InvalidUser() { });
        }

        [HttpGet("{id}/submittal")]
        [Produces(ContentTypes.Transition)]
        [ProducesResponseType(typeof(Transition), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(MissingTransitionError), 409)]
        public IActionResult GetSubmittal(string id)
        {
            Timecard timecard = Database.Find(id);

            if (timecard != null)
            {
                if (timecard.Status == TimecardStatus.Submitted)
                {
                    var transition = timecard.Transitions
                                        .Where(t => t.TransitionedTo == TimecardStatus.Submitted)
                                        .OrderByDescending(t => t.OccurredAt)
                                        .FirstOrDefault();

                    return Ok(transition);
                }
                else
                {
                    return StatusCode(409, new MissingTransitionError() { });
                }
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost("{id}/cancellation")]
        [Produces(ContentTypes.Transition)]
        [ProducesResponseType(typeof(Transition), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(403)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(InvalidStateError), 409)]
        public IActionResult Cancel(string id, [FromBody] Cancellation cancellation)
        {
            // Cancle can be done by anyone Employee/Superviser
            if (UserDB.Find(cancellation.Resource) != null)
            {
                Timecard timecard = Database.Find(id);

                if (timecard != null)
                {
                    if (timecard.Status != TimecardStatus.Draft && timecard.Status != TimecardStatus.Submitted)
                    {
                        return StatusCode(409, new InvalidStateError() { });
                    }

                    var transition = new Transition(cancellation, TimecardStatus.Cancelled);
                    timecard.Transitions.Add(transition);
                    return Ok(transition);
                }
                else
                {
                    return NotFound();
                }
            }
            return StatusCode(401, new InvalidUser() { });
        }

        [HttpGet("{id}/cancellation")]
        [Produces(ContentTypes.Transition)]
        [ProducesResponseType(typeof(Transition), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(MissingTransitionError), 409)]
        public IActionResult GetCancellation(string id)
        {
            Timecard timecard = Database.Find(id);

            if (timecard != null)
            {
                if (timecard.Status == TimecardStatus.Cancelled)
                {
                    var transition = timecard.Transitions
                                        .Where(t => t.TransitionedTo == TimecardStatus.Cancelled)
                                        .OrderByDescending(t => t.OccurredAt)
                                        .FirstOrDefault();

                    return Ok(transition);
                }
                else
                {
                    return StatusCode(409, new MissingTransitionError() { });
                }
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost("{id}/rejection")]
        [Produces(ContentTypes.Transition)]
        [ProducesResponseType(typeof(Transition), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(403)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(InvalidStateError), 409)]
        [ProducesResponseType(typeof(EmptyTimecardError), 409)]
        public IActionResult Rejection(string id, [FromBody] Rejection rejection)
        {
            if (UserDB.Find(rejection.Resource) != null)
            {
                Timecard timecard = Database.Find(id);

                if (timecard != null)
                {
                    if (timecard.Status != TimecardStatus.Submitted)
                    {
                        return StatusCode(409, new InvalidStateError() { });
                    }

                    if (timecard.Lines.Count < 1)
                    {
                        return StatusCode(409, new EmptyTimecardError() { });
                    }

                    /*
                     * Rejection cannot be performed by same user who has submitted the time card.
                     * and rejection should be done by superviser.                     
                     */
                    if (timecard.Resource != rejection.Resource && UserDB.Find(rejection.Resource).Role == Roles.Superviser)
                    {
                        var transition = new Transition(rejection, TimecardStatus.Rejected);
                        timecard.Transitions.Add(transition);
                        return Ok(transition);
                    }

                    /*
                     * user is not authroized to perform this task.
                     */
                    return StatusCode(403, new NotAuthorized { });
                }
                else
                {
                    return NotFound();
                }
            }
            return StatusCode(401, new InvalidUser() { });
        }

        [HttpGet("{id}/rejection")]
        [Produces(ContentTypes.Transition)]
        [ProducesResponseType(typeof(Transition), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(MissingTransitionError), 409)]
        public IActionResult GetRejection(string id)
        {
            Timecard timecard = Database.Find(id);

            if (timecard != null)
            {
                if (timecard.Status == TimecardStatus.Rejected)
                {
                    var transition = timecard.Transitions
                                        .Where(t => t.TransitionedTo == TimecardStatus.Rejected)
                                        .OrderByDescending(t => t.OccurredAt)
                                        .FirstOrDefault();

                    return Ok(transition);
                }
                else
                {
                    return StatusCode(409, new MissingTransitionError() { });
                }
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost("{id}/approval")]
        [Produces(ContentTypes.Transition)]
        [ProducesResponseType(typeof(Transition), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(403)]
        [ProducesResponseType(401)]
        [ProducesResponseType(typeof(InvalidStateError), 409)]
        [ProducesResponseType(typeof(EmptyTimecardError), 409)]
        public IActionResult Approve(string id, [FromBody] Approval approval)
        {
            if (UserDB.Find(approval.Resource) != null)
            {
                Timecard timecard = Database.Find(id);

                if (timecard != null)
                {
                    if (timecard.Status != TimecardStatus.Submitted)
                    {
                        return StatusCode(409, new InvalidStateError() { });
                    }

                    if (timecard.Lines.Count < 1)
                    {
                        return StatusCode(409, new EmptyTimecardError() { });
                    }

                    /*
                     * Approval cannot be performed by same user who has submitted the time card.
                     * and approver should be superviser.                     
                     */
                    if (timecard.Resource != approval.Resource && UserDB.Find(approval.Resource).Role == Roles.Superviser)
                    {
                        var transition = new Transition(approval, TimecardStatus.Approved);
                        timecard.Transitions.Add(transition);
                        return Ok(transition);
                    }

                    /*
                     * user is not authroized to perform this task.
                     */                    
                    return StatusCode(403, new NotAuthorized { });
                }
                else
                {
                    return NotFound();
                }
            }
            return StatusCode(401, new InvalidUser() { });
        }

        [HttpGet("{id}/approval")]
        [Produces(ContentTypes.Transition)]
        [ProducesResponseType(typeof(Transition), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(MissingTransitionError), 409)]
        public IActionResult GetApproval(string id)
        {
            Timecard timecard = Database.Find(id);

            if (timecard != null)
            {
                if (timecard.Status == TimecardStatus.Approved)
                {
                    var transition = timecard.Transitions
                                        .Where(t => t.TransitionedTo == TimecardStatus.Approved)
                                        .OrderByDescending(t => t.OccurredAt)
                                        .FirstOrDefault();

                    return Ok(transition);
                }
                else
                {
                    return StatusCode(409, new MissingTransitionError() { });
                }
            }
            else
            {
                return NotFound();
            }
        }
    }
}

using EventManagementService.Models;
using EventManagementService.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventManagementService.Controllers;

[ApiController]
[Route("events")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet]
    public ActionResult<List<Event>> GetAll()
    {
        var events = _eventService.GetAll();

        return Ok(events);
    }

    [HttpGet("{id}")]
    public ActionResult<Event> GetById(int id)
    {
        var eventItem = _eventService.GetById(id);

        if (eventItem == null)
        {
            return NotFound();
        }

        return Ok(eventItem);
    }

    [HttpPost]
    public ActionResult<Event> Create(Event eventItem)
    {
        if (eventItem.EndAt <= eventItem.StartAt)
        {
            return BadRequest("EndAt должна быть позже чем StartAt.");
        }

        var createdEvent = _eventService.Create(eventItem);

        return CreatedAtAction(
            nameof(GetById),
            new { id = createdEvent.Id },
            createdEvent);
    }
}
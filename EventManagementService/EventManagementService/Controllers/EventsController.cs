using EventManagementService.DTOs;
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
    public ActionResult<PaginatedResult> GetAll(string? title, DateTime? from, DateTime? to, int page = 1, int pageSize = 10)
    {
        var result = _eventService.GetAll(title, from, to, page, pageSize);

        return Ok(result);
    }

    [HttpGet("{id}")]
    public ActionResult<Event> GetById(Guid id)
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
            return BadRequest("EndAt должна быть позже чем StartAt");
        }

        var createdEvent = _eventService.Create(eventItem);

        return CreatedAtAction(
            nameof(GetById),
            new { id = createdEvent.Id },
            createdEvent);
    }
    [HttpPut("{id}")]
    public IActionResult Update(Guid id, Event updatedEvent)
    {
        if (updatedEvent.EndAt <= updatedEvent.StartAt)
        {
            return BadRequest("EndAt должна быть позже чем StartAt");
        }

        var updated = _eventService.Update(id, updatedEvent);

        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }
    [HttpDelete("{id}")]
    public IActionResult Delete(Guid id)
    {
        var deleted = _eventService.Delete(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
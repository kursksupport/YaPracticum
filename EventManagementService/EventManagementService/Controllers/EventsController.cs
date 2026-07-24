using EventManagementService.DTOs;
using EventManagementService.Services;
using Microsoft.AspNetCore.Mvc;
using EventManagementService.Domain.Entities;
using EventManagementService.Domain.Enums;

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
    public async Task<ActionResult<PaginatedResult>> GetAll(
        string? title,
        DateTime? from,
        DateTime? to,
        int page = 1,
        int pageSize = 10)
    {
        var result = await _eventService.GetAllAsync(
            title,
            from,
            to,
            page,
            pageSize);

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EventInfoDto>> GetById(Guid id)
    {
        var eventItem = await _eventService.GetByIdAsync(id);

        if (eventItem == null)
        {
            return NotFound();
        }

        return Ok(new EventInfoDto
        {
            Id = eventItem.Id,
            Title = eventItem.Title,
            Description = eventItem.Description,
            StartAt = eventItem.StartAt,
            EndAt = eventItem.EndAt,
            TotalSeats = eventItem.TotalSeats,
            AvailableSeats = eventItem.AvailableSeats
        });
    }

    [HttpPost]
    public async Task<ActionResult<EventInfoDto>> Create(
        CreateEventDto createEventDto)
    {
        if (createEventDto.EndAt <= createEventDto.StartAt)
        {
            return BadRequest("EndAt должна быть позже чем StartAt");
        }

        var createdEvent = await _eventService.CreateAsync(createEventDto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = createdEvent.Id },
            createdEvent);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        Guid id,
        Event updatedEvent)
    {
        if (updatedEvent.EndAt <= updatedEvent.StartAt)
        {
            return BadRequest("EndAt должна быть позже чем StartAt");
        }

        var updated = await _eventService.UpdateAsync(id, updatedEvent);

        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _eventService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
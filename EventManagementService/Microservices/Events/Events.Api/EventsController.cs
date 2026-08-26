using Events.Application; 
using Events.Domain; 
using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc;

namespace Events.Api;
[ApiController, Route("events")] public sealed class EventsController(IEventService service) : ControllerBase
{
    [HttpGet] public Task<List<Event>> GetAll() => service.GetAllAsync();
    [HttpGet("{id:guid}")] public async Task<IActionResult> Get(Guid id) => await service.GetAsync(id) is { } item ? Ok(item) : NotFound();
    [HttpPost, Authorize(Roles = "Admin")] public async Task<IActionResult> Create(EventRequest request) 
    { 
        var item = await service.CreateAsync(request); 
        return CreatedAtAction(nameof(Get), new { id = item.Id }, item); 
    }

    [HttpPut("{id:guid}"), Authorize(Roles = "Admin")] 
    public async Task<IActionResult> Update(Guid id, EventRequest request) => await service.UpdateAsync(id, request) ? NoContent() : NotFound();
    [HttpDelete("{id:guid}"), Authorize(Roles = "Admin")] 
    public async Task<IActionResult> Delete(Guid id) => await service.DeleteAsync(id) ? NoContent() : NotFound();
}

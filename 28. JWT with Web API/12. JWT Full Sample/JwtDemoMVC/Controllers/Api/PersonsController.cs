using JwtDemoMVC.Models;
using JwtDemoMVC.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtDemoMVC.Controllers.Api;

/// <summary>
/// API اشخاص — محافظت‌شده با JWT (گام 13).
/// برای تست در Swagger: ابتدا از api/auth/login یا api/auth/register توکن بگیرید،
/// دکمه‌ی Authorize را بزنید و accessToken را وارد کنید. توکن به‌صورت خودکار در هدر
/// «Authorization: Bearer ...» ارسال می‌شود (حالت اولِ گام 11).
/// بدون توکن معتبر یا با توکن منقضی‌شده، پاسخ 401 می‌گیرید.
/// </summary>
[ApiController]
[Route("api/persons")]
[Authorize] // گام 13 — همان ویژگی Microsoft.AspNetCore.Authorization، این بار روی API
[Produces("application/json")]
public class PersonsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public PersonsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>لیست همه‌ی اشخاص</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<Person>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll()
        => Ok(await _unitOfWork.Persons.GetAllAsync());

    /// <summary>دریافت یک شخص با شناسه</summary>
    /// <param name="id">شناسه‌ی شخص</param>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Person), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(int id)
    {
        var person = await _unitOfWork.Persons.GetByIdAsync(id);
        return person is null
            ? NotFound(new { message = "شخصی با این شناسه یافت نشد." })
            : Ok(person);
    }

    /// <summary>افزودن شخص جدید</summary>
    /// <remarks>بدنه: { "firstName": "نام", "lastName": "نام خانوادگی", "age": 30 }</remarks>
    [HttpPost]
    [ProducesResponseType(typeof(Person), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] Person person)
    {
        await _unitOfWork.Persons.AddAsync(person);
        await _unitOfWork.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = person.Id }, person);
    }

    /// <summary>ویرایش شخص</summary>
    /// <param name="id">شناسه‌ی شخص</param>
    /// <param name="person">اطلاعات جدید شخص</param>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(Person), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(int id, [FromBody] Person person)
    {
        var existing = await _unitOfWork.Persons.GetByIdAsync(id);
        if (existing is null)
            return NotFound(new { message = "شخصی با این شناسه یافت نشد." });

        existing.FirstName = person.FirstName;
        existing.LastName = person.LastName;
        existing.Age = person.Age;
        _unitOfWork.Persons.Update(existing);
        await _unitOfWork.SaveChangesAsync();
        return Ok(existing);
    }

    /// <summary>حذف شخص</summary>
    /// <param name="id">شناسه‌ی شخص</param>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(int id)
    {
        var person = await _unitOfWork.Persons.GetByIdAsync(id);
        if (person is null)
            return NotFound(new { message = "شخصی با این شناسه یافت نشد." });

        _unitOfWork.Persons.Remove(person);
        await _unitOfWork.SaveChangesAsync();
        return NoContent();
    }
}

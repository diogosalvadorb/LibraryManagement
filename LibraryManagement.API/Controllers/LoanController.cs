using LibraryManagement.Application.DTOs;
using LibraryManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LoansController : ControllerBase
    {
        private readonly ILoanService _loanService;

        public LoansController(ILoanService loanService)
        {
            _loanService = loanService;
        }
 
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<LoanDto>>> GetAll()
        {
            var loans = await _loanService.GetAllLoansAsync();
            return Ok(loans);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<LoanDto>>> GetByUser(int userId)
        {
            var loans = await _loanService.GetLoansByUserIdAsync(userId);
            return Ok(loans);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LoanDto>> GetById(int id)
        {
            var loan = await _loanService.GetLoanByIdAsync(id);

            if (loan is null)
                return NotFound();

            return Ok(loan);
        }

        [HttpPost]
        public async Task<ActionResult<LoanDto>> Create([FromBody] CreateLoanDto request)
        {
            try
            {
                var created = await _loanService.CreateLoanAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating loan: {ex.Message}");
            }
        }

        [HttpPatch("{id}/return")]
        public async Task<ActionResult<LoanDto>> ReturnBook(int id)
        {
            try
            {
                var loan = await _loanService.ReturnBookAsync(id);
                return Ok(loan);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error returning book: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _loanService.DeleteLoanAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest($"Error deleting loan: {ex.Message}");
            }
        }
    }
}
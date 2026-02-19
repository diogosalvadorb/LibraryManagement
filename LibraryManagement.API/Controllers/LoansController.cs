using LibraryManagement.Application.DTOs;
using LibraryManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]   // todos os endpoints exigem autenticação
    public class LoansController : ControllerBase
    {
        private readonly ILoanService _loanService;

        public LoansController(ILoanService loanService)
        {
            _loanService = loanService;
        }

        /// <summary>
        /// Lista todos os empréstimos (ativos e inativos).
        /// Exclusivo para Admin.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<LoanDto>>> GetAll()
        {
            var loans = await _loanService.GetAllLoansAsync();
            return Ok(loans);
        }

        /// <summary>
        /// Lista apenas empréstimos ativos.
        /// Exclusivo para Admin.
        /// </summary>
        [HttpGet("active")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<LoanDto>>> GetAllActive()
        {
            var loans = await _loanService.GetAllActiveLoansAsync();
            return Ok(loans);
        }

        /// <summary>
        /// Lista os empréstimos ativos de um usuário.
        /// Qualquer usuário autenticado.
        /// </summary>
        [HttpGet("user/{userId:int}")]
        public async Task<ActionResult<IEnumerable<LoanDto>>> GetByUser(int userId)
        {
            var loans = await _loanService.GetLoansByUserIdAsync(userId);
            return Ok(loans);
        }

        /// <summary>
        /// Retorna um empréstimo pelo ID.
        /// Qualquer usuário autenticado.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<LoanDto>> GetById(int id)
        {
            var loan = await _loanService.GetLoanByIdAsync(id);

            if (loan is null)
                return NotFound();

            return Ok(loan);
        }

        /// <summary>
        /// Cria um novo empréstimo.
        /// Aplica as regras de prazo e limite por perfil automaticamente.
        /// Qualquer usuário autenticado.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<LoanDto>> Create([FromBody] CreateLoanDto request)
        {
            try
            {
                var created = await _loanService.CreateLoanAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating loan: {ex.Message}");
            }
        }

        /// <summary>
        /// Registra a devolução de um livro.
        /// Marca o empréstimo como Returned e o livro como disponível.
        /// Qualquer usuário autenticado.
        /// </summary>
        [HttpPatch("{id:int}/return")]
        public async Task<ActionResult<LoanDto>> ReturnBook(int id)
        {
            try
            {
                var loan = await _loanService.ReturnBookAsync(id);
                return Ok(loan);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error returning book: {ex.Message}");
            }
        }

        /// <summary>
        /// Soft delete do empréstimo.
        /// Desativa o registro e, se o livro ainda estava emprestado,
        /// o marca como disponível novamente.
        /// Exclusivo para Admin.
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _loanService.DeleteLoanAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error deleting loan: {ex.Message}");
            }
        }
    }
}
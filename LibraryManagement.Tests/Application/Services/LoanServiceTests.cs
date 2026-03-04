using LibraryManagement.Application.DTOs;
using LibraryManagement.Application.Services;
using LibraryManagement.Domain.Entities;
using LibraryManagement.Domain.Enums;
using LibraryManagement.Domain.Interfaces;
using Moq;
using NSubstitute;

namespace LibraryManagement.Tests.Application.Services
{
    public class LoanServiceTests
    {
        private static User MakeUser(UserRole role = UserRole.Common)
            => new("John Doe", "john@test.com", "hashedpassword", role);

        private static Book MakeBook(bool isAvailable = true, bool active = true)
        {
            var book = new Book("Clean Code", "Robert C. Martin", "9780132350884", "A handbook.", 2008);
            if (!isAvailable) book.MarkAsUnavailable();
            if (!active) book.Deactivate();
            return book;
        }

        private static Loan MakeLoan(int userId = 1, int bookId = 1, UserRole role = UserRole.Common)
            => Loan.Create(userId, bookId, role);

        private static LoanService MakeService(
            ILoanRepository loanRepo,
            IUserRepository userRepo,
            IBookRepository bookRepo)
            => new LoanService(loanRepo, userRepo, bookRepo);

        // ──────────────────────────────────────────
        // GetAllLoansAsync
        // ──────────────────────────────────────────

        /// <summary>
        /// Verifica se todos os empréstimos retornados pelo repositório são corretamente
        /// convertidos para DTOs e se a quantidade retornada corresponde ao total cadastrado.
        /// </summary>
        [Fact]
        public async Task LoansExist_GetAll_ReturnsAllLoansAsDtos()
        {
            var loans = new List<Loan>
            {
                MakeLoan(1, 1, UserRole.Common),
                MakeLoan(2, 2, UserRole.Premium)
            };

            var loanRepo = Substitute.For<ILoanRepository>();
            var userRepo = Substitute.For<IUserRepository>();
            var bookRepo = Substitute.For<IBookRepository>();

            loanRepo.GetAllAsync().Returns(loans);

            var service = MakeService(loanRepo, userRepo, bookRepo);

            var result = await service.GetAllLoansAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            await loanRepo.Received(1).GetAllAsync();
        }

        /// <summary>
        /// Verifica se o serviço retorna uma coleção vazia quando não há
        /// nenhum empréstimo cadastrado no repositório.
        /// </summary>
        [Fact]
        public async Task NoLoansExist_GetAll_ReturnsEmpty()
        {
            var loanRepo = Substitute.For<ILoanRepository>();
            var userRepo = Substitute.For<IUserRepository>();
            var bookRepo = Substitute.For<IBookRepository>();

            loanRepo.GetAllAsync().Returns(new List<Loan>());

            var service = MakeService(loanRepo, userRepo, bookRepo);

            var result = await service.GetAllLoansAsync();

            Assert.NotNull(result);
            Assert.Empty(result);
            await loanRepo.Received(1).GetAllAsync();
        }

        // ──────────────────────────────────────────
        // GetLoansByUserIdAsync
        // ──────────────────────────────────────────

        /// <summary>
        /// Verifica se o serviço retorna somente os empréstimos ativos
        /// do usuário informado.
        /// </summary>
        [Fact]
        public async Task LoansExist_GetByUserId_ReturnsUserLoans()
        {
            var loans = new List<Loan> { MakeLoan(1, 1, UserRole.Common) };

            var loanRepo = Substitute.For<ILoanRepository>();
            var userRepo = Substitute.For<IUserRepository>();
            var bookRepo = Substitute.For<IBookRepository>();

            loanRepo.GetActiveLoansByUserIdAsync(1).Returns(loans);

            var service = MakeService(loanRepo, userRepo, bookRepo);

            var result = await service.GetLoansByUserIdAsync(1);

            Assert.NotNull(result);
            Assert.Single(result);
            await loanRepo.Received(1).GetActiveLoansByUserIdAsync(1);
        }

        // ──────────────────────────────────────────
        // GetLoanByIdAsync
        // ──────────────────────────────────────────

        /// <summary>
        /// Verifica se o serviço retorna o LoanDto correto quando um empréstimo
        /// com o ID informado existe no repositório.
        /// </summary>
        [Fact]
        public async Task LoanExists_GetById_ReturnsLoanDto()
        {
            var loan = MakeLoan(1, 1, UserRole.Common);

            var loanRepo = Substitute.For<ILoanRepository>();
            var userRepo = Substitute.For<IUserRepository>();
            var bookRepo = Substitute.For<IBookRepository>();

            loanRepo.GetByIdAsync(1).Returns(loan);

            var service = MakeService(loanRepo, userRepo, bookRepo);

            var result = await service.GetLoanByIdAsync(1);

            Assert.NotNull(result);
            await loanRepo.Received(1).GetByIdAsync(1);
        }

        /// <summary>
        /// Verifica se o serviço retorna null quando nenhum empréstimo
        /// corresponde ao ID informado.
        /// </summary>
        [Fact]
        public async Task LoanDoesNotExist_GetById_ReturnsNull()
        {
            var loanRepo = Substitute.For<ILoanRepository>();
            var userRepo = Substitute.For<IUserRepository>();
            var bookRepo = Substitute.For<IBookRepository>();

            loanRepo.GetByIdAsync(Arg.Any<int>()).Returns((Loan?)null);

            var service = MakeService(loanRepo, userRepo, bookRepo);

            var result = await service.GetLoanByIdAsync(999);

            Assert.Null(result);
            await loanRepo.Received(1).GetByIdAsync(999);
        }

        // ──────────────────────────────────────────
        // CreateLoanAsync
        // ──────────────────────────────────────────

        /// <summary>
        /// Verifica se o serviço cria o empréstimo corretamente quando o usuário e
        /// o livro existem, o livro está disponível e o usuário não atingiu o limite de empréstimos.
        /// </summary>
        [Fact]
        public async Task ValidData_CreateLoan_ReturnsLoanDto()
        {
            var user = MakeUser(UserRole.Common);
            var book = MakeBook();
            var createdLoan = MakeLoan(1, 1, UserRole.Common);

            var loanRepo = Substitute.For<ILoanRepository>();
            var userRepo = Substitute.For<IUserRepository>();
            var bookRepo = Substitute.For<IBookRepository>();

            userRepo.GetUserByIdAsync(1).Returns(user);
            bookRepo.GetByIdAsync(1).Returns(book);
            loanRepo.GetActiveLoansByUserIdAsync(1).Returns(new List<Loan>());
            loanRepo.CreateAsync(Arg.Any<Loan>()).Returns(Task.CompletedTask);
            bookRepo.UpdateAsync(Arg.Any<Book>()).Returns(Task.CompletedTask);
            loanRepo.GetByIdAsync(Arg.Any<int>()).Returns(createdLoan);

            var service = MakeService(loanRepo, userRepo, bookRepo);

            var result = await service.CreateLoanAsync(new CreateLoanDto(1, 1));

            Assert.NotNull(result);
            Assert.Equal(LoanStatus.Borrowed.ToString(), result.LoanStatus);
            await loanRepo.Received(1).CreateAsync(Arg.Any<Loan>());
            await bookRepo.Received(1).UpdateAsync(Arg.Any<Book>());
        }

        /// <summary>
        /// Verifica se o serviço lança KeyNotFoundException quando o usuário
        /// informado não existe no repositório.
        /// </summary>
        [Fact]
        public async Task UserDoesNotExist_CreateLoan_ThrowsKeyNotFoundException()
        {
            var loanRepo = Substitute.For<ILoanRepository>();
            var userRepo = Substitute.For<IUserRepository>();
            var bookRepo = Substitute.For<IBookRepository>();

            userRepo.GetUserByIdAsync(Arg.Any<int>()).Returns((User?)null);

            var service = MakeService(loanRepo, userRepo, bookRepo);

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => service.CreateLoanAsync(new CreateLoanDto(999, 1))
            );

            Assert.Contains("999", exception.Message);
            await loanRepo.DidNotReceive().CreateAsync(Arg.Any<Loan>());
        }

        /// <summary>
        /// Verifica se o serviço lança KeyNotFoundException quando o livro
        /// informado não existe no repositório.
        /// </summary>
        [Fact]
        public async Task BookDoesNotExist_CreateLoan_ThrowsKeyNotFoundException()
        {
            var user = MakeUser();

            var loanRepo = Substitute.For<ILoanRepository>();
            var userRepo = Substitute.For<IUserRepository>();
            var bookRepo = Substitute.For<IBookRepository>();

            userRepo.GetUserByIdAsync(1).Returns(user);
            bookRepo.GetByIdAsync(Arg.Any<int>()).Returns((Book?)null);

            var service = MakeService(loanRepo, userRepo, bookRepo);

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => service.CreateLoanAsync(new CreateLoanDto(1, 999))
            );

            Assert.Contains("999", exception.Message);
            await loanRepo.DidNotReceive().CreateAsync(Arg.Any<Loan>());
        }

        /// <summary>
        /// Verifica se o serviço lança InvalidOperationException quando o livro
        /// solicitado está inativo.
        /// </summary>
        [Fact]
        public async Task BookIsInactive_CreateLoan_ThrowsInvalidOperationException()
        {
            var user = MakeUser();
            var book = MakeBook(active: false);

            var loanRepo = Substitute.For<ILoanRepository>();
            var userRepo = Substitute.For<IUserRepository>();
            var bookRepo = Substitute.For<IBookRepository>();

            userRepo.GetUserByIdAsync(1).Returns(user);
            bookRepo.GetByIdAsync(1).Returns(book);

            var service = MakeService(loanRepo, userRepo, bookRepo);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateLoanAsync(new CreateLoanDto(1, 1))
            );

            Assert.Contains("inactive", exception.Message);
            await loanRepo.DidNotReceive().CreateAsync(Arg.Any<Loan>());
        }

        /// <summary>
        /// Verifica se o serviço lança InvalidOperationException quando o livro
        /// solicitado está indisponível (já emprestado).
        /// </summary>
        [Fact]
        public async Task BookIsUnavailable_CreateLoan_ThrowsInvalidOperationException()
        {
            var user = MakeUser();
            var book = MakeBook(isAvailable: false);

            var loanRepo = Substitute.For<ILoanRepository>();
            var userRepo = Substitute.For<IUserRepository>();
            var bookRepo = Substitute.For<IBookRepository>();

            userRepo.GetUserByIdAsync(1).Returns(user);
            bookRepo.GetByIdAsync(1).Returns(book);

            var service = MakeService(loanRepo, userRepo, bookRepo);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateLoanAsync(new CreateLoanDto(1, 1))
            );

            Assert.Contains("unavailable", exception.Message);
            await loanRepo.DidNotReceive().CreateAsync(Arg.Any<Loan>());
        }

        /// <summary>
        /// Verifica se o serviço lança InvalidOperationException quando o usuário
        /// (role Common, limite 1) já atingiu o número máximo de empréstimos ativos.
        /// </summary>
        [Fact]
        public async Task UserReachedLoanLimit_CreateLoan_ThrowsInvalidOperationException()
        {
            var user = MakeUser(UserRole.Common);
            var book = MakeBook();
            var existingLoan = MakeLoan(1, 2, UserRole.Common);

            var loanRepo = Substitute.For<ILoanRepository>();
            var userRepo = Substitute.For<IUserRepository>();
            var bookRepo = Substitute.For<IBookRepository>();

            userRepo.GetUserByIdAsync(1).Returns(user);
            bookRepo.GetByIdAsync(1).Returns(book);
            loanRepo.GetActiveLoansByUserIdAsync(1).Returns(new List<Loan> { existingLoan });

            var service = MakeService(loanRepo, userRepo, bookRepo);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateLoanAsync(new CreateLoanDto(1, 1))
            );

            Assert.Contains("1", exception.Message);
            await loanRepo.DidNotReceive().CreateAsync(Arg.Any<Loan>());
        }

        /// <summary>
        /// Verifica se o serviço lança InvalidOperationException quando o usuário
        /// (role Common, limite 1) já atingiu o número máximo de empréstimos ativos (MOQ).
        /// </summary>
        [Fact]
        public async Task UserReachedLoanLimit_CreateLoan_ThrowsInvalidOperationException_MOQ()
        {
            var user = MakeUser(UserRole.Common);
            var book = MakeBook();
            var existingLoan = MakeLoan(1, 2, UserRole.Common);

            var mockLoanRepo = new Mock<ILoanRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockBookRepo = new Mock<IBookRepository>();

            mockUserRepo.Setup(r => r.GetUserByIdAsync(1)).ReturnsAsync(user);
            mockBookRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(book);
            mockLoanRepo.Setup(r => r.GetActiveLoansByUserIdAsync(1))
                        .ReturnsAsync(new List<Loan> { existingLoan });

            var service = MakeService(mockLoanRepo.Object, mockUserRepo.Object, mockBookRepo.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateLoanAsync(new CreateLoanDto(1, 1))
            );

            mockLoanRepo.Verify(r => r.CreateAsync(It.IsAny<Loan>()), Times.Never);
        }

        // ──────────────────────────────────────────
        // ReturnBookAsync
        // ──────────────────────────────────────────

        /// <summary>
        /// Verifica se o serviço processa corretamente a devolução de um empréstimo ativo,
        /// marcando o status como Returned e o livro como disponível.
        /// </summary>
        [Fact]
        public async Task LoanExists_ReturnBook_ReturnsUpdatedLoanDto()
        {
            var loan = MakeLoan(1, 1, UserRole.Common);
            var book = MakeBook(isAvailable: false);
            var returnedLoan = MakeLoan(1, 1, UserRole.Common);
            returnedLoan.Return();

            var loanRepo = Substitute.For<ILoanRepository>();
            var userRepo = Substitute.For<IUserRepository>();
            var bookRepo = Substitute.For<IBookRepository>();

            loanRepo.GetByIdAsync(1).Returns(loan, returnedLoan);
            bookRepo.GetByIdAsync(loan.BookId).Returns(book);
            loanRepo.UpdateAsync(Arg.Any<Loan>()).Returns(Task.CompletedTask);
            bookRepo.UpdateAsync(Arg.Any<Book>()).Returns(Task.CompletedTask);

            var service = MakeService(loanRepo, userRepo, bookRepo);

            var result = await service.ReturnBookAsync(1);

            Assert.NotNull(result);
            Assert.Equal(LoanStatus.Returned.ToString(), result.LoanStatus);
            await loanRepo.Received(1).UpdateAsync(Arg.Any<Loan>());
            await bookRepo.Received(1).UpdateAsync(Arg.Any<Book>());
        }

        /// <summary>
        /// Verifica se o serviço lança KeyNotFoundException quando tenta
        /// devolver um empréstimo que não existe.
        /// </summary>
        [Fact]
        public async Task LoanDoesNotExist_ReturnBook_ThrowsKeyNotFoundException()
        {
            var loanRepo = Substitute.For<ILoanRepository>();
            var userRepo = Substitute.For<IUserRepository>();
            var bookRepo = Substitute.For<IBookRepository>();

            loanRepo.GetByIdAsync(Arg.Any<int>()).Returns((Loan?)null);

            var service = MakeService(loanRepo, userRepo, bookRepo);

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => service.ReturnBookAsync(999)
            );

            Assert.Contains("999", exception.Message);
            await loanRepo.DidNotReceive().UpdateAsync(Arg.Any<Loan>());
        }

        /// <summary>
        /// Verifica se o serviço lança InvalidOperationException ao tentar
        /// devolver um empréstimo já inativo.
        /// </summary>
        [Fact]
        public async Task LoanIsInactive_ReturnBook_ThrowsInvalidOperationException()
        {
            var loan = MakeLoan(1, 1, UserRole.Common);
            loan.Deactivate();

            var loanRepo = Substitute.For<ILoanRepository>();
            var userRepo = Substitute.For<IUserRepository>();
            var bookRepo = Substitute.For<IBookRepository>();

            loanRepo.GetByIdAsync(1).Returns(loan);

            var service = MakeService(loanRepo, userRepo, bookRepo);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.ReturnBookAsync(1)
            );

            Assert.Equal("This loan is already inactive.", exception.Message);
            await loanRepo.DidNotReceive().UpdateAsync(Arg.Any<Loan>());
        }

        /// <summary>
        /// Verifica se o serviço lança InvalidOperationException ao tentar
        /// devolver um empréstimo cujo livro já foi devolvido anteriormente.
        /// </summary>
        [Fact]
        public async Task LoanAlreadyReturned_ReturnBook_ThrowsInvalidOperationException()
        {
            var loan = MakeLoan(1, 1, UserRole.Common);
            loan.Return();

            var loanRepo = Substitute.For<ILoanRepository>();
            var userRepo = Substitute.For<IUserRepository>();
            var bookRepo = Substitute.For<IBookRepository>();

            loanRepo.GetByIdAsync(1).Returns(loan);

            var service = MakeService(loanRepo, userRepo, bookRepo);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.ReturnBookAsync(1)
            );

            Assert.Equal("This book has already been returned.", exception.Message);
            await loanRepo.DidNotReceive().UpdateAsync(Arg.Any<Loan>());
        }

        // ──────────────────────────────────────────
        // DeleteLoanAsync
        // ──────────────────────────────────────────

        /// <summary>
        /// Verifica se o serviço desativa o empréstimo com sucesso quando
        /// o empréstimo existe e o livro já foi devolvido (status Returned).
        /// </summary>
        [Fact]
        public async Task LoanExists_Delete_Success()
        {
            var loan = MakeLoan(1, 1, UserRole.Common);
            loan.Return();

            var loanRepo = Substitute.For<ILoanRepository>();
            var userRepo = Substitute.For<IUserRepository>();
            var bookRepo = Substitute.For<IBookRepository>();

            loanRepo.GetByIdAsync(1).Returns(loan);
            loanRepo.UpdateAsync(Arg.Any<Loan>()).Returns(Task.CompletedTask);

            var service = MakeService(loanRepo, userRepo, bookRepo);

            await service.DeleteLoanAsync(1);

            Assert.False(loan.Active);
            await loanRepo.Received(1).UpdateAsync(loan);
        }

        /// <summary>
        /// Verifica se o serviço desativa o empréstimo e libera o livro
        /// quando o empréstimo ainda está com status Borrowed no momento da exclusão.
        /// </summary>
        [Fact]
        public async Task LoanBorrowed_Delete_MarksBookAsAvailable()
        {
            var loan = MakeLoan(1, 1, UserRole.Common);
            var book = MakeBook(isAvailable: false);

            var loanRepo = Substitute.For<ILoanRepository>();
            var userRepo = Substitute.For<IUserRepository>();
            var bookRepo = Substitute.For<IBookRepository>();

            loanRepo.GetByIdAsync(1).Returns(loan);
            bookRepo.GetByIdAsync(loan.BookId).Returns(book);
            loanRepo.UpdateAsync(Arg.Any<Loan>()).Returns(Task.CompletedTask);
            bookRepo.UpdateAsync(Arg.Any<Book>()).Returns(Task.CompletedTask);

            var service = MakeService(loanRepo, userRepo, bookRepo);

            await service.DeleteLoanAsync(1);

            Assert.False(loan.Active);
            Assert.True(book.IsAvailable);
            await bookRepo.Received(1).UpdateAsync(book);
            await loanRepo.Received(1).UpdateAsync(loan);
        }

        /// <summary>
        /// Verifica se o serviço lança KeyNotFoundException contendo o ID informado
        /// quando tenta excluir um empréstimo que não existe.
        /// </summary>
        [Fact]
        public async Task LoanDoesNotExist_Delete_ThrowsKeyNotFoundException()
        {
            var loanRepo = Substitute.For<ILoanRepository>();
            var userRepo = Substitute.For<IUserRepository>();
            var bookRepo = Substitute.For<IBookRepository>();

            loanRepo.GetByIdAsync(Arg.Any<int>()).Returns((Loan?)null);

            var service = MakeService(loanRepo, userRepo, bookRepo);

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => service.DeleteLoanAsync(999)
            );

            Assert.Contains("999", exception.Message);
            await loanRepo.DidNotReceive().UpdateAsync(Arg.Any<Loan>());
        }

        /// <summary>
        /// Verifica se o serviço lança InvalidOperationException
        /// quando tenta excluir um empréstimo que já está inativo.
        /// </summary>
        [Fact]
        public async Task LoanAlreadyInactive_Delete_ThrowsInvalidOperationException()
        {
            var loan = MakeLoan(1, 1, UserRole.Common);
            loan.Deactivate();

            var loanRepo = Substitute.For<ILoanRepository>();
            var userRepo = Substitute.For<IUserRepository>();
            var bookRepo = Substitute.For<IBookRepository>();

            loanRepo.GetByIdAsync(1).Returns(loan);

            var service = MakeService(loanRepo, userRepo, bookRepo);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.DeleteLoanAsync(1)
            );

            Assert.Equal("Loan is already inactive.", exception.Message);
            await loanRepo.DidNotReceive().UpdateAsync(Arg.Any<Loan>());
        }

        /// <summary>
        /// Verifica se o serviço lança KeyNotFoundException contendo o ID informado
        /// quando tenta excluir um empréstimo que não existe (MOQ).
        /// </summary>
        [Fact]
        public async Task LoanDoesNotExist_Delete_ThrowsKeyNotFoundException_MOQ()
        {
            var mockLoanRepo = new Mock<ILoanRepository>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockBookRepo = new Mock<IBookRepository>();

            mockLoanRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Loan?)null);

            var service = MakeService(mockLoanRepo.Object, mockUserRepo.Object, mockBookRepo.Object);

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => service.DeleteLoanAsync(999)
            );

            Assert.Contains("999", exception.Message);
            mockLoanRepo.Verify(r => r.UpdateAsync(It.IsAny<Loan>()), Times.Never);
        }
    }
}
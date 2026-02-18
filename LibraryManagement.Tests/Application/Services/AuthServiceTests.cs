using LibraryManagement.Application.Services;
using Microsoft.Extensions.Configuration;

namespace LibraryManagement.Tests.Application.Services
{
    public class AuthServiceTests
    {
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                { "Jwt:Key",      "SuperSecretTestKey_AtLeast32Chars!!" },
                { "Jwt:Issuer",   "TestIssuer" },
                { "Jwt:Audience", "TestAudience" }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _authService = new AuthService(configuration);
        }

        /// <summary>
        /// Verifica se o hash gerado possui exatamente 64 caracteres e 
        /// contém apenas caracteres hexadecimais válidos (letras de 'a' a 'f' e números de 0 a 9).
        /// </summary>
        [Fact]
        public void ComputeSha256Hash_Should_Return_64_Char_Hex_String()
        {
            var hash = _authService.ComputeSha256Hash("StrongPass1!");

            Assert.Equal(64, hash.Length);
            Assert.Matches("^[0-9a-f]+$", hash);
        }

        /// <summary>
        /// Garante o princípio básico de hashing: a mesma senha de entrada 
        /// deve sempre resultar no mesmo hash de saída.
        /// </summary>
        [Fact]
        public void ComputeSha256Hash_Should_Return_Same_Hash_For_Same_Input()
        {
            var hash1 = _authService.ComputeSha256Hash("StrongPass1!");
            var hash2 = _authService.ComputeSha256Hash("StrongPass1!");

            Assert.Equal(hash1, hash2);
        }

        /// <summary>
        /// Garante que senhas diferentes resultem em hashes diferentes, 
        /// evitando colisões no banco de dados.
        /// </summary>
        [Fact]
        public void ComputeSha256Hash_Should_Return_Different_Hash_For_Different_Input()
        {
            var hash1 = _authService.ComputeSha256Hash("StrongPass1!");
            var hash2 = _authService.ComputeSha256Hash("OtherPass2@");

            Assert.NotEqual(hash1, hash2);
        }

        /// <summary>
        /// Verifica se o algoritmo diferencia letras maiúsculas de minúsculas (Case Sensitive).
        /// As senhas "password" e "PASSWORD" devem gerar hashes completamente distintos.
        /// </summary>
        [Fact]
        public void ComputeSha256Hash_Should_Be_Case_Sensitive()
        {
            var lower = _authService.ComputeSha256Hash("password");
            var upper = _authService.ComputeSha256Hash("PASSWORD");

            Assert.NotEqual(lower, upper);
        }

        /// <summary>
        /// Verifica se a geração do token JWT realmente retorna uma string com conteúdo
        /// (não pode ser nula, vazia ou apenas espaços em branco).
        /// </summary>
        [Fact]
        public void GenerateTokenJwt_Should_Return_Non_Empty_String()
        {
            var token = _authService.GenerateTokenJwt("user@test.com", "Admin");

            Assert.False(string.IsNullOrWhiteSpace(token));
        }

        /// <summary>
        /// Confere se a string retornada obedece à estrutura padrão de um token JWT, 
        /// que é composta obrigatoriamente por 3 partes separadas por um ponto (.) 
        /// (Header . Payload . Signature).
        /// </summary>
        [Fact]
        public void GenerateTokenJwt_Should_Return_Valid_Jwt_Format()
        {
            var token = _authService.GenerateTokenJwt("user@test.com", "Admin");

            var parts = token.Split('.');
            Assert.Equal(3, parts.Length);
        }

        /// <summary>
        /// Descriptografa/Lê o token gerado e verifica se a informação (claim) 
        /// do e-mail do usuário foi injetada corretamente dentro dele.
        /// </summary>
        [Fact]
        public void GenerateTokenJwt_Should_Contain_Email_Claim()
        {
            var email = "user@test.com";
            var token = _authService.GenerateTokenJwt(email, "Common");

            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "email");
            Assert.NotNull(emailClaim);
            Assert.Equal(email, emailClaim.Value);
        }

        /// <summary>
        /// Descriptografa/Lê o token gerado e verifica se a informação (claim) 
        /// da permissão/cargo (Role) do usuário foi injetada corretamente.
        /// Verifica tanto a nomenclatura padrão do .NET (ClaimTypes.Role) quanto "role".
        /// </summary>
        [Fact]
        public void GenerateTokenJwt_Should_Contain_Role_Claim()
        {
            var role = "Admin";
            var token = _authService.GenerateTokenJwt("user@test.com", role);

            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var roleClaim = jwtToken.Claims.FirstOrDefault(c =>
                c.Type == System.Security.Claims.ClaimTypes.Role ||
                c.Type == "role");

            Assert.NotNull(roleClaim);
            Assert.Equal(role, roleClaim.Value);
        }

        /// <summary>
        /// Testa o comportamento de erro (Caminho Triste). 
        /// Verifica se o serviço lança uma exceção do tipo InvalidOperationException 
        /// caso a chave secreta ("Jwt:Key") não esteja configurada no arquivo appsettings.json (ou configuração).
        /// </summary>
        [Fact]
        public void GenerateTokenJwt_Should_Throw_When_Key_Is_Missing()
        {
            var configWithoutKey = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Jwt:Issuer",   "TestIssuer" },
                    { "Jwt:Audience", "TestAudience" }
                    // A chave "Jwt:Key" foi intencionalmente omitida aqui para simular o erro
                })
                .Build();

            var serviceWithoutKey = new AuthService(configWithoutKey);

            Assert.Throws<InvalidOperationException>(
                () => serviceWithoutKey.GenerateTokenJwt("user@test.com", "Admin"));
        }
    }
}
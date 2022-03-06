using AutoMapper;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace WireguardAdmin.Models
{
    public class ClientRepository : IClientRepository
    {
        private AdminDBContext context;
        private readonly IMapper mapper;
        public ClientRepository(AdminDBContext ctx, IMapper mapper)
        {
            context = ctx;
            this.mapper = mapper;
        }
        public async Task<NewUserModel> AddAsync(string username, string password)
        {
            // generate a 128-bit salt using a cryptographically strong random sequence of nonzero values
            byte[] salt = new byte[128 / 8];

            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                rngCsp.GetNonZeroBytes(salt);
            }

            //Console.WriteLine($"Salt: {Convert.ToBase64String(salt)}");

            // derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)
            string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            //Console.WriteLine($"Hashed: {hashedPassword}");

            var UserSessionId = Guid.NewGuid().ToString();
            var UserSessionExpiration = TimeSpan.FromMinutes(2);

            NewUserModelDbo user = new()
            {
                ID = Guid.NewGuid().ToString(),
                UserName = username,
                PasswordHash = hashedPassword,
                PasswordSalt = salt,
                DateAdded = DateTime.Now,
                SessionId = UserSessionId,
                SessionExpiration = UserSessionExpiration
            };

            return await addNewDbUserAsync(user);
        }

        private async Task<NewUserModel> addNewDbUserAsync(NewUserModelDbo dbPerson)
        {
            await context.NewUsers.AddAsync(dbPerson);
            await context.SaveChangesAsync();
            return mapper.Map<NewUserModel>(dbPerson);
        }

    }
}

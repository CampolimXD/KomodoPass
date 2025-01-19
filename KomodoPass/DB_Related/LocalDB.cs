
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using NSec.Cryptography;

namespace KomodoPass
{
    public class LocalDB
    {
        private const string KomodoPassDB = "KomodoPassLocalDB.db3";
        private readonly SQLiteAsyncConnection _conn;

        public LocalDB()
        {
            _conn = new SQLiteAsyncConnection(Path.Combine(FileSystem.AppDataDirectory, KomodoPassDB));
            _conn.CreateTableAsync<KomodoPassword>();
            _conn.CreateTableAsync<MasterPassword>();
        }

        // Relacionado ao CRUD das senhas 
        public async Task<List <KomodoPassword>> GetKomodoPasswords()
        {
            return await _conn.Table<KomodoPassword>().ToListAsync();
        }
        public async Task<KomodoPassword> GetById(int id)
        {
            return await _conn.Table<KomodoPassword>().Where(x => x.Id == id).FirstOrDefaultAsync();
        }
        public async Task Create(KomodoPassword password)
        {
            await _conn.InsertAsync(password);
        }
        public async Task Update(KomodoPassword password)
        {
            await _conn.UpdateAsync(password);
        }
        public async Task Delete(KomodoPassword password)
        {
            await _conn.DeleteAsync(password);
        }
        public async Task<List<KomodoPassword>> SearchPasswords(string query)
        {
            string sql = @"
        SELECT * FROM KomodoPasswords 
        WHERE Title LIKE '%' || @q || '%' 
           OR Username LIKE '%' || @q || '%' 
           OR Email LIKE '%' || @q || '%' 
           OR Website LIKE '%' || @q || '%';
            ";

            return await _conn.QueryAsync<KomodoPassword>(sql, query, query, query, query);
        }
        //

        //Relacionado a senha mestra e user
       
            public static byte[] GenerateSalt(int size = 32) 
            {
                var salt = new byte[size];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(salt);
                }
                return salt;
            }
            string pepper = "Peeper K0m0d0P455";

        public static byte[] HashPassword(byte[] PrePasswordHash, byte[] salt, string pepper)
        {

            // Adiciona o pepper à senha antes de gerar o hash
            string passwordWithPepper = PrePasswordHash + pepper;

            // Configurar parâmetros do Argon2id
            var parameters = new Argon2Parameters
            {
                DegreeOfParallelism = 1,      // p = 1
                MemorySize = 12288,           // m = 12288 (12 MiB)
                NumberOfPasses = 3            // t = 3
            };

            // Criar o Argon2id com os parâmetros definidos
            var argon2id = Argon2id.Argon2id(parameters);

            // Derivar os bytes a partir da senha + pepper e salt
            byte[] hashedPassword = argon2id.DeriveBytes(passwordWithPepper, salt, 32); // Deriva 32 bytes de hash

            return hashedPassword;
        }
        public async Task Update(MasterPassword masterPassword)
        {
            // Defina o valor do pepper (não armazene isso no banco de dados)
            string pepper = "Peeper K0m0d0P455";

            // Gerar um salt novo
            byte[] salt = GenerateSalt(32); // Salt de 32 bytes

            // A senha mestra que será usada é a que vem do campo "Username"
            // A senha é a "username" do objeto, ou seja, o campo que você usa para representar a senha

            byte[] hashedPassword = HashPassword(masterPassword, salt, pepper);

            // Agora, você pode salvar o hash da senha e o salt no banco de dados
            masterPassword.PasswordHash = hashedPassword;  // Armazenando o hash da senha
            masterPassword.Salt = salt;  // Armazenando o salt

            // Atualiza a senha mestra no banco de dados
            await _conn.UpdateAsync(masterPassword);
        }






    }
}


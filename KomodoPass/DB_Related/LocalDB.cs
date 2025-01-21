
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Konscious.Security.Cryptography;


namespace KomodoPass
{
    public class LocalDB
    {
        private const string KomodoPassDB = "KomodoPassLocalDB.db3";
        private readonly SQLiteAsyncConnection _conn;

        public LocalDB()
        {
            // caso queira recriar o BD execute o codigo abaixo             
            // APENAS MANTER EM AMBIENTES DE TESTE !!!
            //
            // string dbPath = Path.Combine(FileSystem.AppDataDirectory, KomodoPassDB);
            // if (File.Exists(dbPath))
            // {
            //    File.Delete(dbPath); // Remove o banco de dados existente
            // }
            //
            //APENAS MANTER EM AMBIENTES DE TESTE !!!
            //caso queira recriar o BD execute o codigo acima 


            _conn = new SQLiteAsyncConnection(Path.Combine(FileSystem.AppDataDirectory, KomodoPassDB));
            _conn.CreateTableAsync<KomodoPassword>();
            _conn.CreateTableAsync<MasterPassword>();      
        }


        // Relacionado ao CRUD das senhas 

       public async Task<List<KomodoPassword>> GetKomodoPasswords()
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

        //Relacionado a senha mestra
       
            string pepper = "Peeper K0m0d0P455";
            private const int _Memory_size = 65536;
            private const int _Degree_of_parallelism = 4;
            private const int _Iterations = 4;
            private const int _Hash_size = 32;
            
            public byte[] CreateSalt()
            {
                return RandomNumberGenerator.GetBytes(16);
            }

            public string HashPassword(string Password, byte[] salt)
            {
                String PasswordWithPeeper = Password + pepper;
                byte[] passwordBytes = Encoding.UTF8.GetBytes(PasswordWithPeeper);


                using var argon2 = new Argon2id(passwordBytes)
                {
                    Salt = salt,
                    DegreeOfParallelism = _Degree_of_parallelism,
                    Iterations = _Iterations,
                    MemorySize = _Memory_size,

                };
                byte[] hash = argon2.GetBytes(_Hash_size);
                return Convert.ToBase64String(hash);
            }
            // retirar do codigo/ o sistema de criação de user vira
            // com um user root definido que depois sera atualizado, para evitar que
            // outros users sejam criardos
            public async Task CreateMaster(MasterPassword password)
            {               
                await _conn.InsertAsync(password);
            }
            public async Task UpdateMaster(MasterPassword password)
            {
                await _conn.UpdateAsync(password);
            }
        //


    }
}


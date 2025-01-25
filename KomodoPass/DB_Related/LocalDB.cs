
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
        // SALT
        public byte[] CreateSalt()
        {
            return RandomNumberGenerator.GetBytes(16);
        }
        // passar funçao da chave quando for criptografar e descriptografar as senhas
        public async Task<byte[]> GetOrCreateKeyAsync()
        {
            const string KeyName = "AESKey";

            var storedKey = await SecureStorage.GetAsync(KeyName);
            if (!string.IsNullOrEmpty(storedKey))
            {
                // caso exista retorna a Key
                return Convert.FromBase64String(storedKey);
            }          
            var aesKey = RandomNumberGenerator.GetBytes(32);            
            await SecureStorage.SetAsync(KeyName, Convert.ToBase64String(aesKey));

            return aesKey;
        }
        // Relacionado ao CRUD das senhas         
        public string Encrypt(string password, byte[] aesKey)
        {
            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.Key = aesKey;
            aes.GenerateIV();

            using var msEncrypt = new MemoryStream();

            // Grava o IV no início dos dados criptografados
            msEncrypt.Write(aes.IV, 0, aes.IV.Length);

            using var encryptor = aes.CreateEncryptor();
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);

            byte[] passwordbytes = Encoding.UTF8.GetBytes(password);
            csEncrypt.Write(passwordbytes, 0, passwordbytes.Length);
            csEncrypt.FlushFinalBlock();

            // Retorna os dados criptografados (IV + dados)
            return Convert.ToBase64String(msEncrypt.ToArray());
        }
        public string Decrypt(string cipherText, byte[] aesKey)
        {
            byte[] combinedbytes = Convert.FromBase64String(cipherText);
            if (combinedbytes.Length < 16)
            {
                throw new ArgumentException("invalid cipher");
            }

            using var aes = Aes.Create();
            aes.Key = aesKey;

            // O IV está nos primeiros 16 bytes dos dados
            byte[] iv = new byte[16];
            Array.Copy(combinedbytes, 0, iv, 0, 16);
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();

            // Os dados criptografados começam a partir do byte 16
            using var msDecrypt = new MemoryStream(combinedbytes, 16, combinedbytes.Length - 16);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);

            return srDecrypt.ReadToEnd();
        }
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
        //Relacionado a senha mestra
        // funçao pra que exista apenas um usuario cadastrado 
        public async Task<MasterPassword?> GetFirstUser()
        {
            return await _conn.Table<MasterPassword>().OrderBy(x => x.Id).FirstOrDefaultAsync();
        }
        // Hash vars 
        private const int _Memory_size = 65536;
        private const int _Degree_of_parallelism = 4;
        private const int _Iterations = 4;
        private const int _Hash_size = 32;
        // Pepper
        string pepper = "Peeper_K0m0d0P455";
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
        public async Task<bool> PreUpdateMaster(string username, string password)
        {
            var onlyUser = await GetFirstUser();
            var user = await _conn.Table<MasterPassword>().Where(x => x.Username == username).FirstOrDefaultAsync();
            string hashedPassword = HashPassword(password, onlyUser.Salt);
            byte[] Hash = Encoding.UTF8.GetBytes(hashedPassword);
        if (onlyUser.Username == username|| onlyUser.PasswordHash == Hash)
        {
            return true;
        }           
            return false;              
        }
        public async Task CreateMaster(MasterPassword password)
        {
            await _conn.InsertAsync(password);
        }
        public async Task UpdateMaster(MasterPassword password)
        {
            await _conn.UpdateAsync(password);
        }
        // Login
        public async Task<bool> LoginProcess(string username, string password)
        {
            var onlyUser = await GetFirstUser();
            var user = await _conn.Table<MasterPassword>().Where(x => x.Username == username).FirstOrDefaultAsync();
            // não permite login de usuarios que nao sejam o primeiro usuario cadastrado ou com credenciais erradas
            if (onlyUser == null)
            {   
                return false;
            }
            else if(onlyUser.Username != username) 
            {                
                return false;
            }
            else
            {
                string hashedPassword = HashPassword(password, user.Salt);
                return hashedPassword == Convert.ToBase64String(user.PasswordHash);
            }
        }       
    }
}


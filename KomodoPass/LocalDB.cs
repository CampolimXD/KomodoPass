using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }
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

    }
}

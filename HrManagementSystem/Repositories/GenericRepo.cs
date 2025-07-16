using HrManagementSystem.Models;
using Microsoft.IdentityModel.Tokens.Experimental;

namespace HrManagementSystem.Repositories
{
    public class GenericRepo<T> where T : class
    {
        public HRContext con;
        public GenericRepo(HRContext context)
        {
            this.con = context;
        }
        public List<T> getAll()

        {
            return con.Set<T>().ToList();
        }
        public T getByID(int id)
        {
            return con.Set<T>().Find(id);
        }
        public void Add(T d)
        {
            con.Set<T>().Add(d);
        }
        public void Update(T s)
        {
            con.Entry(s).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        }
        public void Delete(int id)
        {
            con.Set<T>().Remove(getByID(id));
        }
        public void Save()
        {
            con.SaveChanges();
        }
    }
}

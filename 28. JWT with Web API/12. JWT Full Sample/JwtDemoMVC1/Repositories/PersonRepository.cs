using JwtDemoMVC.Data;
using JwtDemoMVC.Models;

namespace JwtDemoMVC.Repositories;

public class PersonRepository : Repository<Person>, IPersonRepository
{
    public PersonRepository(AppDbContext context) : base(context)
    {
    }
}

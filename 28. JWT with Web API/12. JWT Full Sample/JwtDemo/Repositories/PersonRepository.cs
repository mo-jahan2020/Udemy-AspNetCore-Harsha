using JwtDemo.Data;
using JwtDemo.Models;

namespace JwtDemo.Repositories;

public class PersonRepository : Repository<Person>, IPersonRepository
{
    public PersonRepository(AppDbContext context) : base(context)
    {
    }
}

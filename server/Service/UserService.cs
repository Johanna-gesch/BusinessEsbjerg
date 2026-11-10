using Infra.Entities;
using Infra.Repositories;

namespace Service;

public class UserService(IUserRepo repo)
{
    public async Task<List<User>> GetAll() {
        return await repo.GetAll();
    }
    
    

}

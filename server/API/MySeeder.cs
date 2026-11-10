using Infra;
using Infra.Entities;
using LinqToDB;
using LinqToDB.Data;

namespace API;

public class MySeeder(MyDatabaseConnection dc)
{
    public void Seed()
    {
        dc.CreateTable<User>(tableOptions: TableOptions.CreateIfNotExists);
        
        if (dc.Users.Count() == 0)
            for (int i = 1; i <= 10; i++)
            {
                dc.Insert(new User
                {
                    UserId = i.ToString(),
                    Username = $"User {i}",
                    IsAdmin = i == 1,
                    IsActive = true
                });
            }
    }
    
}
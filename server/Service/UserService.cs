using Infra;
using Infra.Entities;
using Infra.Entities.HolidayTracking.Salary;
using Service.DTOs.UserDTOs;
using Infra.Repositories.Interfaces;
using Service.HolidayServices;
using Service.Security;

namespace Service;

public class UserService(
    IUserRepo repo ,
    IPasswordHasher hasher,
    SalaryHistoryService salService,
    AccountService accountService)
{
    public async Task<List<User>> GetAll() {
        return await repo.GetAll();
    }

    public async Task<User> GetById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ValidationException("User id cannot be null or empty");
        
        return await repo.GetById(id)
            ?? throw new NotFoundException($"User with this id '{id}' not found");
    }

    public async Task Update(UpdateUserDto dto)
    {
        var user = await repo.GetById(dto.UserId)
            ?? throw new ValidationException("User not found");

        // Check whether salary has changed BEFORE updating user
        bool salaryChanged = 
            user.SalaryType != dto.SalaryType || 
            user.Salary != dto.Salary;
        
        if (!string.IsNullOrWhiteSpace(dto.Username))
            user.Username = dto.Username;
        
        user.SalaryType = dto.SalaryType;
        
        if (dto.Salary.HasValue)
            user.Salary = dto.Salary.Value;
        
        await repo.Update(user);

        if (salaryChanged)
        {
            await salService.Create(new SalaryHistory
            {
                Id = Guid.NewGuid(),
                UserId = user.UserId,
                EffectiveFrom = DateOnly.FromDateTime(DateTime.Now),
                SalaryType = user.SalaryType,
                Salary = user.Salary,
            });
        }
    }

    
    public async Task Insert(CreateUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
            throw new ValidationException("Username and password are required");
        
        var newUser = new User
        {
            UserId = Guid.NewGuid().ToString(),
            Username = dto.Username,
            PasswordHash = hasher.HashAndSaltPassword(dto.Password),
            IsAdmin = dto.IsAdmin,
        };
        await repo.Insert(newUser);

        await accountService.CreateDefaultAccountsAsync(newUser.UserId);
    }

    public async Task Delete(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new Infra.ValidationException("User id cannot be null or empty");

        var user = await repo.GetById(userId) ?? throw new NotFoundException($"User with id '{userId}' not found");

        await repo.Delete(user);
    }
}

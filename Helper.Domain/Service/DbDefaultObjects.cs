using Helper.Domain.Entities;

namespace Helper.Domain.Service;

public class DbDefaultObjects
{
    public static void CreateDefaultObjects(HelperDbContext context)
    {
        if (!context.Categories.Any())
        {
            context.Categories.AddRange(
                               new Category { Title = "Дім", Description = "Роботи по дому та прибудинковій теритирії" },
                               new Category { Title = "Діти", Description = "Догляд за дітьми, репетиторство" },
                               new Category { Title = "Авто", Description = "Ремонт та обслуговування автомобіля" }
            );
        }

        if (!context.Users.Any())
        {
            var user = new User
            {
                Username = "admin",
                Password = PasswordService.HashPassword("qwerty"),
                Email = "adminmail@mail.com",
                City = "City17"
            };
            context.Users.Add(user);
            context.SaveChanges();
        }

        context.SaveChanges();
    }
}

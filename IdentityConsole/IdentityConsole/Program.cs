using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;

namespace IdentityConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var userName = "mdwasim.akram1@wipro.com";
            var password = "Password";

            //using default usermanager and store
            //var userstore = new userstore<identityuser>();
            //var usermanager = new usermanager<identityuser>(userstore);
           
            //use custom user and userstore
            var userstore = new CustomUserStore(new CustomuserDbContext());
            var userManager = new UserManager<CustomUser, int>(userstore);

            //Adding a user
            //var result = userManager.Create(new CustomUser {UserName=userName }, password);
            //Console.WriteLine("User created:" + result.Succeeded);

            //add claim to user
            var user = userManager.FindByName(userName);
            //var claimresult = userManager.AddClaim(user.Id, new System.Security.Claims.Claim("given_name", "wasim"));
            //Console.WriteLine("Claim added: " + claimresult.Succeeded);

            //check password
            var isMatch = userManager.CheckPassword(user, password);
            Console.WriteLine(isMatch);
        }

        public class CustomUser : IUser<int>
        {
            public int Id { get; set; }
            public string UserName { get; set;}
            public string passwordHash { get; set; }
        }

        public class CustomuserDbContext : DbContext
        {
            public CustomuserDbContext() : base("DefaultConnection") { }
            public DbSet<CustomUser> users { get; set; }
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                var user = modelBuilder.Entity<CustomUser>();
                user.ToTable("Users");
                user.HasKey(x => x.Id);
                user.Property(x => x.Id).IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
                user.Property(x => x.UserName).IsRequired().HasMaxLength(256).HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("UserName Index") { IsUnique = true }));
                base.OnModelCreating(modelBuilder);
            }
        }

        public class CustomUserStore : IUserPasswordStore<CustomUser, int>
        {
            private CustomuserDbContext _dbcontext;
            public CustomUserStore(CustomuserDbContext dbcontext)
            {
                _dbcontext = dbcontext;
            }
            public Task CreateAsync(CustomUser user)
            {
                _dbcontext.users.Add(user);
                return _dbcontext.SaveChangesAsync();
            }

            public Task DeleteAsync(CustomUser user)
            {
                _dbcontext.users.Remove(user);
                return _dbcontext.SaveChangesAsync();
            }

            public void Dispose()
            {
                _dbcontext.Dispose();
            }

            public Task<CustomUser> FindByIdAsync(int userId)
            {
                return _dbcontext.users.FirstOrDefaultAsync(x => x.Id == userId);
            }

            public Task<CustomUser> FindByNameAsync(string userName)
            {
                return _dbcontext.users.FirstOrDefaultAsync(x => x.UserName == userName);
            }

            public Task<string> GetPasswordHashAsync(CustomUser user)
            {
                return Task.FromResult(user.passwordHash);
            }

            public Task<bool> HasPasswordAsync(CustomUser user)
            {
                return Task.FromResult(user.passwordHash != null);
            }

            public Task SetPasswordHashAsync(CustomUser user, string passwordHash)
            {
                user.passwordHash = passwordHash;
                return Task.FromResult(0);
            }

            public Task UpdateAsync(CustomUser user)
            {
                _dbcontext.users.Attach(user);
                return _dbcontext.SaveChangesAsync();
            }
        }
    }
}

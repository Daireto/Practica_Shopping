using Microsoft.AspNetCore.Identity;
using Shopping.Data.Entities;
using Shopping.Models;

namespace Shopping.Helpers
{
    public interface IUserHelper
    {
        Task<IdentityResult> AddUserAsync(User user, string password); //Agrega el usuario
        //Task<User> AddUserAsync(AddUserViewModel model); //Agrega el usuario
        Task AddUserToRoleAsync(User user, string roleName); //Asigna un rol a un usuario
        Task<IdentityResult> ChangePasswordAsync(User user, string oldPassword, string newPassword); //Cambia la contraseña del usuario
        Task CheckRoleAsync(string roleName); //Verifica si un rol existe
        Task<User> GetUserAsync(string email); //Devuelve al usuario por su email
        //Task<User> GetUserAsync(Guid userId); //Devuelve al usuario por su Id
        Task<bool> IsUserInRoleAsync(User user, string roleName); //Verifica el rol de un usuario
        Task<SignInResult> LoginAsync(LoginViewModel model); //Loguea usuarios
        Task LogoutAsync(); //Desloguea usuarios
        Task<IdentityResult> UpdateUserAsync(User user); //Actualiza los datos del usuario

        //TODO: Implement AddUserViewModel
        //TODO: Implement GetUserAsync by Guid
    }
}

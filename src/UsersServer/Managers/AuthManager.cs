using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using UsersServer.Contracts;
using UsersServer.Cryptography;
using UsersServer.Cryptography.Services;
using UsersServer.Data;
using UsersServer.Dtos;
using UsersServer.Models;

namespace UsersServer.Managers
{
    public class AuthManager : IAuthManager
    {
        private readonly SignInManager _signInManager;
        private readonly UserManager _userManager;
        private readonly UsersRepo _usersRepo;
        private readonly IHasher _hasher;
        private readonly IMapper _mapper;
        private readonly IJwtFactory _jwtFactory;
        
        public AuthManager(SignInManager signInManager, UserManager userManager, IHasher hasher, IMapper mapper
        , UsersRepo usersRepo, IJwtFactory jwtFactory)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _hasher = hasher;
            _mapper = mapper;
            _usersRepo = usersRepo;
            _jwtFactory = jwtFactory;
        }
        
        public async Task<AccessToken> GetToken(UserLoginDto message)
        {
            if (!string.IsNullOrEmpty(message.Email) && !string.IsNullOrEmpty(message.Password))
            {
                // ensure we have a user with the given user name
                var user = await _userManager.FindByEmailAsync(message.Email);
                if (user != null)
                {
                    // validate password
                    if (await LoginAsync(message.Email,message.Password))
                    {
                        // Todo generate refresh token
                        // Todo Add refresh token
                        await _usersRepo.UpdateAsync(user, CancellationToken.None);
                        var token = await _jwtFactory.GenerateEncodedToken(user.Id, user.UserName);
                        await _userManager.SetAuthenticationTokenAsync(user, "Income", "ApiUser", token.Token);

                        return token;
                    }
                }
            }

            return null;
        }
        
        public async Task<bool> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return false; // return error user doesn't exist
            }

            return await _userManager.CheckPasswordAsync(user, password);
            
            // return await GenerateAuthenticationResultForUserAsync(user);
        }

        private  UserCreateDto FromSignupToUser(UserSignupDto signupDto)
        {
            var pwd = signupDto.Password;
            var hash = _hasher.CreateHash(pwd, BaseCryptoItem.HashAlgorithm.SHA3_512);

            UserCreateDto userToCreate = new UserCreateDto
            {
                Email = signupDto.Email,
                PasswordHash = hash,
                UserName = signupDto.UserName
            };

            return userToCreate;
        }
        
        public async Task<IdentityResult> CreateUserAsync(UserSignupDto userSignupDto)
        {
            UserCreateDto userToCreate = FromSignupToUser(userSignupDto);
            
            var userMdl = _mapper.Map<User>(userToCreate);
            
            if(userMdl == null) return IdentityResult.Failed(new IdentityError
            {
                Code = "1",
                Description = "An error occured processing your request"
            });
            
            return await _userManager.CreateAsync(userMdl);
        }
        


    }
}
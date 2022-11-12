﻿using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WSBLearn.Application.Dtos;
using WSBLearn.Application.Interfaces;
using WSBLearn.Dal.Persistence;
using WSBLearn.Domain.Entities;
using AutoMapper;
using WSBLearn.Application.Exceptions;
using WSBLearn.Application.Validators;
using WSBLearn.Application.Requests.User;

namespace WSBLearn.Application.Services
{
    public class UserService : IUserService
    {
        private readonly WsbLearnDbContext _dbContext;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IValidator<CreateUserRequest> _createUserRequestValidator;
        private readonly IValidator<UpdateUserRequest> _updateUserRequestValidator;
        private readonly IValidator<UpdateUserPasswordRequest> _updateUserPasswordRequestValidator;
        private readonly JwtAuthenticationSettings _authenticationSettings;
        private readonly IMapper _mapper;

        public UserService(WsbLearnDbContext dbContext, IPasswordHasher<User> passwordHasher, 
            IValidator<CreateUserRequest> createUserRequestValidator,
            IValidator<UpdateUserRequest> updateUserRequestValidator,
            IValidator<UpdateUserPasswordRequest> updateUserPasswordRequestValidator,
        JwtAuthenticationSettings authenticationSettings,
            IMapper mapper)
        {
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;
            _createUserRequestValidator = createUserRequestValidator;
            _updateUserRequestValidator = updateUserRequestValidator;
            _updateUserPasswordRequestValidator = updateUserPasswordRequestValidator;
            _authenticationSettings = authenticationSettings;
            _mapper = mapper;
        }

        public void Register(CreateUserRequest createUserRequest)
        {
            ValidationResult validationResult = _createUserRequestValidator.Validate(createUserRequest);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors[0].ToString());
            }

            var user = new User()
            {
                Username = createUserRequest.Username,
                EmailAddress = createUserRequest.EmailAddress,
                RoleId = createUserRequest.RoleId,
                ProfilePictureUrl = createUserRequest.ProfilePictureUrl
            };
            user.Password = _passwordHasher.HashPassword(user, createUserRequest.Password);

            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();
        }

        public string Login(LoginDto loginDto)
        {
            var user = _dbContext.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.EmailAddress == loginDto.Login || u.Username == loginDto.Login);
            if (user is null)
                throw new BadHttpRequestException("Invalid username or password");

            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, loginDto.Password);
            if (result == PasswordVerificationResult.Failed)
                throw new BadHttpRequestException("Invalid username or password");

            return GenerateToken(user);
        }

        public IEnumerable<UserDto> GetAll()
        {
            IEnumerable<User> users = _dbContext.Users.Include(u => u.Role).AsEnumerable();
            var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);

            return userDtos;
        }

        public UserDto GetById(int id)
        {
            var user = _dbContext.Users.Include(u => u.Role).FirstOrDefault(u => u.Id == id);
            if (user is null)
                throw new NotFoundException("User with given id not found");
            var userDto = _mapper.Map<UserDto>(user);

            return userDto;
        }

        public void Delete(int id)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == id);
            if (user is null)
                throw new NotFoundException("User with given id not found");

            _dbContext.Users.Remove(user);
            _dbContext.SaveChanges();
        }

        public UserDto Update(int id, UpdateUserRequest updateUserRequest)
        {
            var user = _dbContext.Users.Include(u => u.Role).FirstOrDefault(u => u.Id == id);
            if (user is null)
                throw new NotFoundException("User with given id not found");

            ValidationResult validationResult = _updateUserRequestValidator.Validate(updateUserRequest);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors[0].ToString());
            }

            if (!string.IsNullOrEmpty(updateUserRequest.Username))
                user.Username = updateUserRequest.Username;
            if (!string.IsNullOrEmpty(updateUserRequest.EmailAddress))
                user.EmailAddress = updateUserRequest.EmailAddress;
            if (!string.IsNullOrEmpty(updateUserRequest.Password))
                user.Password = _passwordHasher.HashPassword(user, updateUserRequest.Password);
            if (!string.IsNullOrEmpty(updateUserRequest.ProfilePictureUrl))
                user.ProfilePictureUrl = updateUserRequest.ProfilePictureUrl;
            _dbContext.SaveChanges();
            var userDto = _mapper.Map<UserDto>(user);

            return userDto;
        }

        public UserDto UpdateUserRole(int id, int roleId)
        {
            var user = _dbContext.Users.Include(u => u.Role).FirstOrDefault(u => u.Id == id);
            if (user is null)
                throw new NotFoundException("User with given id not found");
            var role = _dbContext.Roles.FirstOrDefault(r => r.Id == roleId);
            if (role is null)
                throw new NotFoundException("Role with given id not found");

            user.RoleId = roleId;
            _dbContext.SaveChanges();

            var userDto = _mapper.Map<UserDto>(user);
            return userDto;
        }

        public void UpdateUserPassword(int id, UpdateUserPasswordRequest updateUserPasswordRequest)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == id);
            if (user is null)
                throw new NotFoundException("User with given id not found");
            var passwordVerification = _passwordHasher.VerifyHashedPassword(user, user.Password, updateUserPasswordRequest.OldPassword);
            if (passwordVerification == PasswordVerificationResult.Failed)
                throw new BadHttpRequestException("Invalid Password");
            var validationResult = _updateUserPasswordRequestValidator.Validate(updateUserPasswordRequest);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors[0].ToString());

            user.Password = _passwordHasher.HashPassword(user, updateUserPasswordRequest.NewPassword);
            _dbContext.SaveChanges();

            return;
        }

        private string GenerateToken(User user)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.EmailAddress),
                new Claim(ClaimTypes.Role, user.Role.Name)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authenticationSettings.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(_authenticationSettings.ExpireDays);

            var token = new JwtSecurityToken(
                issuer: _authenticationSettings.Issuer,
                audience: _authenticationSettings.Issuer,
                claims,
                expires: expires,
                signingCredentials: credentials);

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }
    }
}

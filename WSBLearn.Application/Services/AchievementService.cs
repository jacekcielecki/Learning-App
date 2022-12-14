using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using WSBLearn.Application.Dtos;
using WSBLearn.Application.Exceptions;
using WSBLearn.Application.Interfaces;
using WSBLearn.Application.Requests.Achievement;
using WSBLearn.Dal.Persistence;
using WSBLearn.Domain.Entities;

namespace WSBLearn.Application.Services
{

    public class AchievementService : IAchievementService
    {
        private readonly WsbLearnDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateAchievementRequest> _createAchievementRequest;
        private readonly IValidator<UpdateAchievementRequest> _updateAchievementRequest;

        public AchievementService(WsbLearnDbContext dbContext, IMapper mapper, IValidator<CreateAchievementRequest> createAchievementRequest, IValidator<UpdateAchievementRequest> updateAchievementRequest)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _createAchievementRequest = createAchievementRequest;
            _updateAchievementRequest = updateAchievementRequest;
        }

        public async Task<List<AchievementDto>> GetAllAsync()
        {
            var entities = await _dbContext.Achievements.ToListAsync();

            return _mapper.Map<List<AchievementDto>>(entities);
        }

        public async Task<AchievementDto> CreateAsync(CreateAchievementRequest request)
        {
            var validationResult = await _createAchievementRequest.ValidateAsync(request);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors[0].ToString());
            var entity = _mapper.Map<Achievement>(request);
            _dbContext.Achievements.Add(entity);
            await _dbContext.SaveChangesAsync();

            return _mapper.Map<AchievementDto>(entity);
        }

        public async Task<AchievementDto> UpdateAsync(int id, UpdateAchievementRequest request)
        {
            var validationResult = await _updateAchievementRequest.ValidateAsync(request);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors[0].ToString());
            var entity = await _dbContext.Achievements.FindAsync(id);
            if (entity is null)
                throw new NotFoundException("Achievement with given id not found");

            entity.Name = request.Name;
            entity.Description = request.Description;
            await _dbContext.SaveChangesAsync();

            return _mapper.Map<AchievementDto>(entity);
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _dbContext.Achievements.FindAsync(id);
            if (entity is null)
                throw new NotFoundException("Achievement with given id not found");

            _dbContext.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }
    }
}

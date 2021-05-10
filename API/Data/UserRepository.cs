using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {

        #region Private Fields
        private readonly DataContext _context;
        private readonly IMapper _mapper;            
        #endregion

        public UserRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;

        }

        public async Task<MemberDto> GetMemberByUsernameAsync(string username)
        {
            // return await _context.Users
            //     .Where<AppUser>(u => u.UserName == username)
            //     .Select(
            //         user => new MemberDto
            //         {
            //             // Without AutoMapper, manual mapping goest here...
            //         }
            //     )
            //     .SingleOrDefaultAsync();

            return await _context.Users
                .Where<AppUser>(u => u.UserName == username)
                .ProjectTo<MemberDto>(_mapper.ConfigurationProvider) // in this way, we use mapper in repository, instead of controller, and needn't query unnecessary columns from DB Context
                .SingleOrDefaultAsync();
        }

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
            var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

            // get an IQueryable object from DBContext, to act as query source to generate PagedList<T> result, a list of MemberDto with pagination info
            var query =  _context.Users.AsQueryable()
                // .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                // .AsNoTracking(); // since this is just a Read Operation, we could take off tracking
                .Where(u => u.UserName != userParams.CurrentUsername)
                .Where(u => u.Gender == userParams.Gender)
                .Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
            
            query = userParams.OrderBy switch
            {
                "created" => query.OrderByDescending(u => u.Created),
                _ => query.OrderByDescending(u => u.LastActive)
            };

            return await PagedList<MemberDto>.CreateAsync(
                query.ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                .AsNoTracking(), 
                userParams.PageNumber, 
                userParams.PageSize
            );
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _context.Users
                .FindAsync(id);
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.Photos)
                .SingleOrDefaultAsync<AppUser>(u => u.UserName == username);
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await _context.Users
                .Include(u => u.Photos)
                .ToListAsync<AppUser>();
        }


        public void UpdateUser(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }
    }
}